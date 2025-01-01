using System;
using ExileCore2;
using StashMan.Events.EventHandlers;
using StashMan.Managers;
using System.Threading;
using System.Threading.Tasks;
using StashMan.Infrastructure;
using StashMan.UI;

namespace StashMan
{
    /// <summary>
    /// Main plugin entry point. Manages initialization, plugin lifecycle,
    /// and ties together the managers/services.
    /// </summary>
    public class StashManCore : BaseSettingsPlugin<PluginConfig>
    {
        public static StashManCore Main { get; private set; }

        // Some references to our managers
        private StashManager _stashManager;
        private ItemManager _itemManager;
        private StashUpdater _stashUpdater;

        // If we want to periodically refresh stash data
        private const string StashRefreshTaskName = "StashMan_RefreshStash";

        public StashManCore()
        {
            Name = "StashMan";
        }

        public override bool Initialise()
        {
            Main = this;
            LogMessage("StashManCore initializing...", 5);

            _stashManager = new StashManager(Settings.StashData);
            _itemManager = new ItemManager();
            _stashUpdater = new StashUpdater(_stashManager, _itemManager);

            LoggingEventHandler.Register();
            PriceEventHandler.Register();

            Settings.Enable.OnValueChanged += (sender, enabled) =>
            {
                if (enabled && IsTownOrHideout())
                {
                    TaskRunner.Run(StashRefreshLoop, StashRefreshTaskName);
                }
                else
                {
                    StopTasksAndUnload();
                }
            };

            if (Settings.Enable && IsTownOrHideout())
            {
                TaskRunner.Run(StashRefreshLoop, StashRefreshTaskName);
            }

            LogMessage("StashManCore initialized.", 5);
            return true;
        }

        private static bool IsTownOrHideout()
        {
            return Main.GameController.IngameState.Data.CurrentWorldArea.IsTown ||
                   Main.GameController.IngameState.Data.CurrentWorldArea.IsHideout;
        }

        private void StopTasksAndUnload()
        {
            TaskRunner.Stop(StashRefreshTaskName);
            LogMessage("StashManCore unloaded.", 5);
        }

        /// <summary>
        /// A simple loop that calls _stashUpdater.RefreshStashData()
        /// every few seconds in the background, as long as the stash panel is open, etc.
        /// </summary>
        private async Task StashRefreshLoop(CancellationToken token)
        {
            Settings.ThreadStarted = DateTime.Now;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    Main.LogMessage("StashRefresh Pulse... Started at: " + Settings.ThreadStarted);

                    if (Main.GameController.IngameState.IngameUi.StashElement.IsVisible)
                    {
                        try
                        {
                            _stashUpdater.RefreshStashData();
                        }
                        catch (Exception e)
                        {
                            LogError($"Error in StashRefreshLoop: {e}");
                        }
                    }

                    // Sleep for a few seconds between refreshes, honoring cancellation
                    await Task.Delay(3000, token);
                }
            }
            catch (OperationCanceledException)
            {
                LogMessage("StashRefreshLoop was canceled.");
            }
            catch (Exception ex)
            {
                LogError($"Unexpected error in StashRefreshLoop: {ex}");
            }
        }

        public override void Render()
        {
            if (!Settings.Enable) return;

            StashManPanel.DrawPanel(Settings.StashData);

            base.Render();
        }

        public override void DrawSettings()
        {
            // Place plugin settings UI code here if needed
            // e.g., toggles, numeric inputs for refresh rate, etc.
            base.DrawSettings();
        }

        public override void AreaChange(AreaInstance area)
        {
            if (area.IsTown || area.IsHideout)
            {
                TaskRunner.Run(StashRefreshLoop, StashRefreshTaskName);
            }
            else
            {
                TaskRunner.Stop(StashRefreshTaskName);
            }

            base.AreaChange(area);
        }

        public override void ReceiveEvent(string eventId, object args)
        {
            // Handle certain event IDs from ExileCore or other plugins
            LogMessage($"Received event: {eventId}", 2);

            base.ReceiveEvent(eventId, args);
        }

        public override void OnUnload()
        {
            StopTasksAndUnload();
            base.OnUnload();
        }

        public override void OnPluginDestroyForHotReload()
        {
            StopTasksAndUnload();
            base.OnPluginDestroyForHotReload();
        }

        /// <summary>
        /// A convenience method to log from anywhere, if you prefer not to use the base plugin's method.
        /// </summary>
        public void DebugLog(string message, int color = 2)
        {
            LogMessage($"[StashMan] {message}", color);
        }
    }
}
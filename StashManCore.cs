using System;
using ExileCore2; 
using StashMan.Events.EventHandlers;
using StashMan.Managers;
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

            // 1) Set up our managers
            //    We keep StashData in PluginConfig, so let's pass it here.
            //    This ensures the same Stash instance is used across managers.
            _stashManager = new StashManager(Settings.StashData);
            _itemManager = new ItemManager();
            _stashUpdater = new StashUpdater(_stashManager, _itemManager);

            // 2) (Optional) Register event handlers (e.g. Logging, Price)
            LoggingEventHandler.Register();
            PriceEventHandler.Register();

            // 3) Optionally start a TaskRunner that periodically refreshes stash data
            //    For demonstration, let's do a background refresh every 3 seconds.
            TaskRunner.Run(StashRefreshLoop, StashRefreshTaskName);

            LogMessage("StashManCore initialized.", 5);
            return true;
        }

        /// <summary>
        /// A simple loop that calls _stashUpdater.RefreshStashDataAsync()
        /// every few seconds in the background, as long as the stash panel is open, etc.
        /// </summary>
        private async Task StashRefreshLoop()
        {
            while (true)
            {
                try
                {
                    // You could check if stash is visible or if the user is in town/hideout
                    // or just blindly refresh each loop:

                    _stashUpdater.RefreshStashData();
                }
                catch (Exception e)
                {
                    LogError($"Error in StashRefreshLoop: {e}");
                }

                // Sleep for a few seconds between refreshes
                await Task.Delay(3000);
            }
            // ReSharper disable once FunctionNeverReturns
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
            // e.g. toggles, numeric inputs for refresh rate, etc.
            base.DrawSettings();
        }

        public override void AreaChange(AreaInstance area)
        {
            // If you only want to update stash data in town/hideout, you could do:
            // if (area.IsTown || area.IsHideout)
            //     TaskRunner.Run(StashRefreshLoop, StashRefreshTaskName);
            // else
            //     TaskRunner.Stop(StashRefreshTaskName);

            base.AreaChange(area);
        }

        public override void ReceiveEvent(string eventId, object args)
        {
            // If you handle certain event IDs from ExileCore or other plugins, do it here
            LogMessage($"Received event: {eventId}", 2);

            base.ReceiveEvent(eventId, args);
        }

        public override void OnUnload()
        {
            // Stop any ongoing tasks
            TaskRunner.Stop(StashRefreshTaskName);

            // Cleanup or disposal logic
            LogMessage("StashManCore unloaded.", 5);
            base.OnUnload();
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

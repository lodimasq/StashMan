using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using StashMan.Infrastructure;
using StashMan.Models;

namespace StashMan.UI
{
    /// <summary>
    /// A sample ImGui-based UI for debugging or displaying stash info.
    /// You might call this in the plugin's Render() method.
    /// </summary>
    public static class StashManPanel
    {
        private static bool _pOpen;
        private static bool IsVisible { get; set; } = true;

        public static void DrawPanel(List<StashTab> stashTabs)
        {
            _pOpen = IsVisible;
            if (_pOpen)

                ImGui.Begin("StashMan Debug Panel", ref _pOpen, ImGuiWindowFlags.AlwaysAutoResize);

            // Display stash data
            ImGui.Text("Stash Data:");
            foreach (var tab in stashTabs.ToList())
            {
                ImGui.Text($"Tab: {tab.Name}, Type: {tab.Type}");
                foreach (var item in tab.Items)
                {
                    ImGui.Text($"  Item: {item.BaseName}, Quantity: {item.Quantity}");
                }
            }

            // Display running tasks
            ImGui.Separator();
            ImGui.Text("Running Tasks:");
            foreach (var task in TaskRunner.ActiveTasks)
            {
                ImGui.Text(
                    $"Task: {task.Key}, Status: {(task.Value.Token.IsCancellationRequested ? "Cancelling" : "Running")}   ");
            }

            ImGui.End();
        }
    }
}
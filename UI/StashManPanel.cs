using System.Numerics;
using ImGuiNET;
using StashMan.Models;

namespace StashMan.UI
{
    /// <summary>
    /// A sample ImGui-based UI for debugging or displaying stash info.
    /// You might call this in the plugin's Render() method.
    /// </summary>
    public static class StashManPanel
    {
        public static bool IsVisible { get; set; } = true;

        public static void DrawPanel(Stash stashData)
        {
            var pOpen = IsVisible;
            if (!pOpen) return;

            ImGui.SetNextWindowSize(new Vector2(600, 400), ImGuiCond.FirstUseEver);
            if (ImGui.Begin("StashMan Debug Panel", ref pOpen))
            {
                ImGui.Text("Stash Tabs:");
                if (ImGui.BeginTable("StashTabsTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg))
                {
                    ImGui.TableSetupColumn("Index");
                    ImGui.TableSetupColumn("Name");
                    ImGui.TableSetupColumn("Type");
                    ImGui.TableSetupColumn("Items Count");
                    ImGui.TableHeadersRow();

                    foreach (var tab in stashData.Tabs)
                    {
                        ImGui.TableNextRow();
                        ImGui.TableSetColumnIndex(0);
                        ImGui.Text(tab.Index.ToString());
                        ImGui.TableSetColumnIndex(1);
                        ImGui.Text(tab.Name);
                        ImGui.TableSetColumnIndex(2);
                        ImGui.Text(tab.Type);
                        ImGui.TableSetColumnIndex(3);
                        ImGui.Text(tab.Items.Count.ToString());
                    }

                    ImGui.EndTable();
                }
            }
            ImGui.End();
        }
    }
}
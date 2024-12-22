using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ExileCore2;
using ExileCore2.Shared;
using ExileCore2.Shared.Enums;
using StashMan.Classes;
using static StashMan.StashManCore;

namespace StashMan.Compartments;

internal class ActionsHandler
{
    public static int GetIndexOfCurrentVisibleTab()
    {
        return Main.GameController.Game.IngameState.IngameUi.StashElement.IndexVisibleStash;
    }

    public static void CleanUp()
    {
        Input.KeyUp(Keys.LControlKey);
        Input.KeyUp(Keys.Shift);
    }

    public static void HandleSwitchToTabEvent(object tab)
    {
        // Func<SyncTask<bool>> task = null;
        // switch (tab)
        // {
        //     case int index:
        //         task = () => ActionCoRoutine.ProcessSwitchToTab(index);
        //         break;
        //
        //     case string name:
        //         if (!RenamedAllStashNames.Contains(name))
        //         {
        //             DebugWindow.LogMsg($"{Main.Name}: can't find tab with name '{name}'.");
        //             break;
        //         }
        //
        //         var tempIndex = RenamedAllStashNames.IndexOf(name);
        //         task = () => ActionCoRoutine.ProcessSwitchToTab(tempIndex);
        //         DebugWindow.LogMsg($"{Main.Name}: Switching to tab with index: {tempIndex} ('{name}').");
        //         break;
        //
        //     default:
        //         DebugWindow.LogMsg("The received argument is not a string or an integer.");
        //         break;
        // }
        //
        // if (task != null) TaskRunner.Run(task, CoroutineName);
    }
    

    public static async SyncTask<bool> Delay(int ms = 0)
    {
        await Task.Delay(500);
        return true;
    }

    public static InventoryType GetTypeOfCurrentVisibleStash()
    {
        var stashPanelVisibleStash = Main.GameController.Game.IngameState.IngameUi?.StashElement?.VisibleStash;
        return stashPanelVisibleStash?.InvType ?? InventoryType.InvalidInventory;
    }
}
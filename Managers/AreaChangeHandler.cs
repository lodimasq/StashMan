using System;
using ExileCore2; // or wherever AreaInstance is located

namespace StashMan.Managers
{
    /// <summary>
    /// An optional place to handle area changes if you want to do logic 
    /// like stopping certain tasks or refreshing the stash only in hideout/town.
    /// </summary>
    public static class AreaChangeHandler
    {
        public static void HandleAreaChange(AreaInstance newArea)
        {
            // Example logic:
            // if (newArea.IsHideout || newArea.IsTown)
            // {
            //    // We might want to start the stash update co-routine
            //    TaskRunner.Run(StashUpdaterRoutine, "StashUpdater");
            // }
            // else
            // {
            //    // Stop the stash update co-routine
            //    TaskRunner.Stop("StashUpdater");
            // }

            Console.WriteLine($"[AreaChangeHandler] Moved to area: {newArea.Name}");
        }
    }
}
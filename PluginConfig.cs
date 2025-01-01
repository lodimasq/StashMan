using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using StashMan.Models;

namespace StashMan
{
    /// <summary>
    /// Configuration data for your plugin. 
    /// Typically includes user settings or toggles.
    /// </summary>
    public class PluginConfig : ISettings
    {
        public ToggleNode Enable { get; set; } = new (false);
        
        // Example: if we have a "DuplicateStashError" setting or flags
        [IgnoreMenu]
        public bool DuplicateStashError { get; set; } = false;

        // If you want to store stash data in config, 
        // you might do so here, or keep it purely in memory.
        [IgnoreMenu]
        public Stash StashData { get; set; } = new Stash();

        // public PluginConfig()
        // {
        //     Enable = new ToggleNode(true);
        // }
    }
}
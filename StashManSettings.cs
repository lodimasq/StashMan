using System.Collections.Generic;
using System.Text.Json.Serialization;
using System.Windows.Forms;
using ExileCore2.Shared.Attributes;
using ExileCore2.Shared.Interfaces;
using ExileCore2.Shared.Nodes;
using StashMan.Classes;

namespace StashMan;

public class StashManSettings : ISettings
{
    public Stash StashData { get; set; } = new();
    public bool DuplicateStashError { get; set; } = false;
   
    public ToggleNode Enable { get; set; } = new(false);
}
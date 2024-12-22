using System;
using System.Collections.Generic;
using System.IO;
using StashMan.Classes;
using static ExileCore2.PoEMemory.MemoryObjects.ServerInventory;

namespace StashMan.Compartments;

internal class Utility
{
    public static void SaveDefaultConfigsToDisk()
    {
        WriteToNonExistentFile($"{StashManCore.Main.ConfigDirectory}\\example filter.txt",
            "https://github.com/DetectiveSquirrel/StashMan/blob/master/Example%20Filter/Example.json");
    }

    public static void WriteToNonExistentFile(string path, string content)
    {
        if (File.Exists(path))
            return;

        if (path == null)
            return;

        using var streamWriter = new StreamWriter(path, true);
        streamWriter.Write(content);
        streamWriter.Close();
    }


    public static bool CheckIgnoreCells(InventSlotItem inventItem, (int Width, int Height) containerSize,
        int[,] ignoredCells)
    {
        var inventPosX = inventItem.PosX;
        var inventPosY = inventItem.PosY;

        if (inventPosX < 0 || inventPosX >= containerSize.Width)
            return true;

        if (inventPosY < 0 || inventPosY >= containerSize.Height)
            return true;

        return ignoredCells[inventPosY, inventPosX] != 0; //No need to check all item size
    }
}
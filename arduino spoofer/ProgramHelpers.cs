using System;
using System.Collections.Generic;

public static class ProgramHelpers
{
    public static void AddMouseDevice(
        this List<(string Name, string VID, string PID)> list,
        string name,
        string vid,
        string pid)
    {
        list.Add((name ?? "Unknown", vid, pid));
    }
}

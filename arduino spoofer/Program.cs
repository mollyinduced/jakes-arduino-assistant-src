using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text.RegularExpressions;
using System.Threading;
using Colorful;
using Console = Colorful.Console;

internal class Program
{
    private static readonly Regex vidPattern = new Regex("leonardo\\.vid(\\.\\d+)?=0x[0-9A-F]+");
    private static readonly Regex pidPattern = new Regex("leonardo\\.pid(\\.\\d+)?=0x[0-9A-F]+");
    private static readonly Regex namePattern = new Regex("leonardo\\.name=.*");
    private static readonly Regex usbProductPattern = new Regex("leonardo\\.build\\.usb_product=.*");
    private static readonly Regex buildVidPattern = new Regex("leonardo\\.build\\.vid=0x[0-9A-F]+");
    private static readonly Regex buildPidPattern = new Regex("leonardo\\.build\\.pid=0x[0-9A-F]+");
    private static readonly Regex extraFlagsPattern = new Regex("leonardo\\.build\\.extra_flags=\\{build\\.usb_flags\\}");

    private static void Main(string[] args)
    {
        Program.EnsureRunningAsAdmin();
        for (; ; )
        {
            Program.DisplayMenu();
            switch (Program.GetMenuSelection(3))
            {
                case 1:
                    {
                        Action action;
                        if ((action = Program.SpoofArduino) == null)
                        {
                            Program.SpoofArduino();
                        }
                        Program.SafeAction(action);
                        break;
                    }
                case 2:
                    {
                        Action action2;
                        if ((action2 = Program.UndoSpoof) == null)
                        {
                            Program.UndoSpoof();
                        }
                        Program.SafeAction(action2);
                        break;
                    }
                case 3:
                    return;
            }
        }
    }

    // Token: 0x06000002 RID: 2 RVA: 0x000020C8 File Offset: 0x000002C8
    private static void EnsureRunningAsAdmin()
    {
        if (!Program.IsUserAdministrator())
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = Process.GetCurrentProcess().MainModule.FileName,
                Verb = "runas"
            };
            try
            {
                Process.Start(startInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Program.LogErrorAndExit("This program requires administrator privileges to run.", ex);
            }
        }
    }

    // Token: 0x06000003 RID: 3 RVA: 0x00002138 File Offset: 0x00000338
    private static bool IsUserAdministrator()
    {
        return new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
    }

    // Token: 0x06000004 RID: 4 RVA: 0x00002150 File Offset: 0x00000350
    private static void DisplayMenu()
    {
        Console.Clear();
        Program.DisplayTitle();
        Program.DisplayDisclaimer();
        Console.WriteLine("[1] Spoof Arduino", Color.Cyan);
        Console.WriteLine("[2] Undo Spoof", Color.Cyan);
        Console.WriteLine("[3] Exit", Color.Cyan);
        Console.Write("Select an option: ", Color.Yellow);
    }

    // Token: 0x06000005 RID: 5 RVA: 0x000021A8 File Offset: 0x000003A8
    private static int GetMenuSelection(int maxOption)
    {
        int num;
        if (int.TryParse(Console.ReadLine(), out num) && num > 0 && num <= maxOption)
        {
            return num;
        }
        Program.DisplayInvalidOptionMessage();
        return Program.GetMenuSelection(maxOption);
    }

    // Token: 0x06000006 RID: 6 RVA: 0x000021D8 File Offset: 0x000003D8
    private static void DisplayTitle()
    {
        Console.WriteLine("\n  888888          888              d8b                                      \n    \"88b          888              88P                                      \n     888          888              8P                                       \n     888  8888b.  888  888  .d88b. \"  .d8888b                               \n     888     \"88b 888 .88P d8P  Y8b   88K                                   \n     888 .d888888 888888K  88888888   \"Y8888b.                              \n     88P 888  888 888 \"88b Y8b.            X88                              \n     888 \"Y888888 888  888  \"Y8888     88888P'                              \n   .d88d8888             888          d8b                                   \n .d88Pd88888             888          Y8P                                   \n888P\"d88P888             888                                                \n    d88P 888 888d888 .d88888 888  888 888 88888b.   .d88b.                  \n   d88P  888 888P\"  d88\" 888 888  888 888 888 \"88b d88\"88b                 \n  d88P   888 888    888  888 888  888 888 888  888 888  888                 \n d8888888888 888    Y88b 888 Y88b 888 888 888  888 Y88..88P                 \nd88P   d8888 888     \"Y88888  \"d8b888 888 88888888  \"Y88P\"           888    \n      d88888                   Y8P          888                      888    \n     d88P888                                888                      888    \n    d88P 888 .d8888b  .d8888b  888 .d8888b  888888  8888b.  88888b.  888888 \n   d88P  888 88K      88K      888 88K      888        \"88b 888 \"88b 888    \n  d88P   888 \"Y8888b. \"Y8888b. 888 \"Y8888b. 888    .d888888 888  888 888    \n d8888888888      X88      X88 888      X88 Y88b.  888  888 888  888 Y88b.  \nd88P     888  88888P'  88888P' 888  88888P'  \"Y888 \"Y888888 888  888  \"Y888 \n                                                                            \n                                                                            \n                                                                            ", Color.Magenta);
    }

    // Token: 0x06000007 RID: 7 RVA: 0x000021E9 File Offset: 0x000003E9
    private static void DisplayDisclaimer()
    {
        Console.WriteLine("If you paid for this software, you were scammed!", Color.Red);
        Console.WriteLine();
    }

    // Token: 0x06000008 RID: 8 RVA: 0x000021FF File Offset: 0x000003FF
    private static void DisplayInvalidOptionMessage()
    {
        Console.Clear();
        Console.WriteLine("[+] Jake's Arduino Spoofer [+]", Color.Cyan);
        Console.WriteLine("Invalid response... please pick again!", Color.Red);
        Thread.Sleep(2000);
    }

    // Token: 0x06000009 RID: 9 RVA: 0x00002230 File Offset: 0x00000430
    private static void UndoSpoof()
    {
        Console.Clear();
        Console.WriteLine("[+] Reverting Arduino to original settings... [+]", Color.Blue);
        string[] array = new string[]
        {
            Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%/Arduino15/packages/arduino/hardware/avr/1.8.6/boards.txt"),
            Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%/Arduino15/packages/arduino/hardware/avr/1.8.5/boards.txt"),
            Environment.ExpandEnvironmentVariables("%programfiles(x86)%/Arduino/hardware/arduino/avr/boards.txt")
        };
        string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "boards.txt");
        if (!File.Exists(path))
        {
            Program.LogErrorAndExit("Couldn't find the old boards.txt... Please redownload the script.", null);
        }
        try
        {
            string contents = File.ReadAllText(path);
            foreach (string text in array)
            {
                if (File.Exists(text))
                {
                    try
                    {
                        Program.SetFileWritable(text);
                        File.WriteAllText(text, contents);
                        Program.SetFileReadOnly(text);
                        Program.LogAndDisplayMessage("Successfully restored " + text + " to original.", Color.Green);
                    }
                    catch (Exception ex)
                    {
                        Program.LogAndDisplayMessage("Error restoring " + text + " to original: " + ex.Message, Color.Red);
                    }
                }
            }
            Program.LogAndDisplayMessage("Reverting complete.", Color.Blue);
        }
        catch (Exception ex2)
        {
            Program.LogErrorAndExit("Error reading boards.txt file: " + ex2.Message, ex2);
        }
    }

    // Token: 0x0600000A RID: 10 RVA: 0x00002374 File Offset: 0x00000574
    private static void SpoofArduino()
    {
        Console.Clear();
        Console.WriteLine("[+] Jake's Arduino Spoofer [+]", Color.Cyan);
        Console.WriteLine("\nSelect your mouse...", Color.Yellow);
        Console.WriteLine("Don't know which one's your mouse? Open Control Panel, click View devices and printers, right-click your mouse, go to properties, hardware, properties again, details tab, and check the Device instance path to see the PID and VID.", Color.Green);
        List<Program.MouseDevice> list = Program.ListMouseDevices();
        if (!list.Any<Program.MouseDevice>())
        {
            Program.LogAndDisplayMessage("No mouse devices found. Exiting...", Color.Red);
            return;
        }
        Program.DisplayMouseList(list);
        int menuSelection = Program.GetMenuSelection(list.Count);
        Program.MouseDevice selectedMouse = list[menuSelection - 1];
        string mouseName = selectedMouse.Name;
        Console.Write("Disable COM port? (Recommended) Y/N: ", Color.Yellow);
        string comChoice = Console.ReadLine().Trim().ToUpper();
        Program.SafeAction(delegate
        {
            Program.ReplaceAndSaveBoardsTxt("0x" + selectedMouse.Vid, "0x" + selectedMouse.Pid, mouseName, comChoice);
        });
    }

    // Token: 0x0600000B RID: 11 RVA: 0x00002440 File Offset: 0x00000640
    private static void DisplayMouseList(List<Program.MouseDevice> mice)
    {
        for (int i = 0; i < mice.Count; i++)
        {
            DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(22, 4);
            defaultInterpolatedStringHandler.AppendFormatted<int>(i + 1);
            defaultInterpolatedStringHandler.AppendLiteral(": VID: ");
            defaultInterpolatedStringHandler.AppendFormatted(mice[i].Vid);
            defaultInterpolatedStringHandler.AppendLiteral(", PID: ");
            defaultInterpolatedStringHandler.AppendFormatted(mice[i].Pid);
            defaultInterpolatedStringHandler.AppendLiteral(", Name: ");
            defaultInterpolatedStringHandler.AppendFormatted(mice[i].Name);
            Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear());
        }
        Console.Write("Select an option: ", Color.Yellow);
    }

    // Token: 0x0600000C RID: 12 RVA: 0x000024F0 File Offset: 0x000006F0
    private static List<Program.MouseDevice> ListMouseDevices()
    {
        List<Program.MouseDevice> list = new List<Program.MouseDevice>();
        foreach (ManagementBaseObject managementBaseObject in new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity").Get())
        {
            ManagementObject managementObject = (ManagementObject)managementBaseObject;
            object obj = managementObject["DeviceID"];
            string text = Program.ExtractId((obj != null) ? obj.ToString() : null, "VID_");
            object obj2 = managementObject["DeviceID"];
            string text2 = Program.ExtractId((obj2 != null) ? obj2.ToString() : null, "PID_");
            object obj3 = managementObject["Name"];
            string text3 = (obj3 != null) ? obj3.ToString() : null;
            if (text != null && text2 != null && text3 != null && text3.ToLower().Contains("mouse"))
            {
                list.Add(new Program.MouseDevice
                {
                    Vid = text,
                    Pid = text2,
                    Name = text3
                });
            }
        }
        return list;
    }

    // Token: 0x0600000D RID: 13 RVA: 0x000025EC File Offset: 0x000007EC
    private static string ExtractId(string deviceId, string idPrefix)
    {
        if (deviceId == null)
        {
            return null;
        }
        int num = deviceId.IndexOf(idPrefix);
        if (num == -1)
        {
            return null;
        }
        int num2 = deviceId.IndexOf("&", num + idPrefix.Length);
        if (num2 == -1)
        {
            num2 = deviceId.Length;
        }
        return deviceId.Substring(num + idPrefix.Length, num2 - num - idPrefix.Length);
    }

    // Token: 0x0600000E RID: 14 RVA: 0x00002644 File Offset: 0x00000844
    private static void ReplaceAndSaveBoardsTxt(string mouseVid, string mousePid, string mouseName, string comChoice)
    {
        DefaultInterpolatedStringHandler defaultInterpolatedStringHandler = new DefaultInterpolatedStringHandler(54, 3);
        defaultInterpolatedStringHandler.AppendLiteral("Configuring Arduino spoofing with VID: ");
        defaultInterpolatedStringHandler.AppendFormatted(mouseVid);
        defaultInterpolatedStringHandler.AppendLiteral(", PID: ");
        defaultInterpolatedStringHandler.AppendFormatted(mousePid);
        defaultInterpolatedStringHandler.AppendLiteral(", Name: ");
        defaultInterpolatedStringHandler.AppendFormatted(mouseName);
        Console.WriteLine(defaultInterpolatedStringHandler.ToStringAndClear(), Color.Blue);
        string[] locations = new string[]
        {
            Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%/Arduino15/packages/arduino/hardware/avr/1.8.6/boards.txt"),
            Environment.ExpandEnvironmentVariables("%LOCALAPPDATA%/Arduino15/packages/arduino/hardware/avr/1.8.5/boards.txt"),
            Environment.ExpandEnvironmentVariables("%programfiles(x86)%/Arduino/hardware/arduino/avr/boards.txt")
        };
        string backupDirectory = "backup";
        Program.SafeAction(delegate
        {
            Program.BackupOriginalFiles(locations, backupDirectory);
        });
        foreach (string text in locations)
        {
            if (File.Exists(text))
            {
                try
                {
                    Program.SetFileWritable(text);
                    List<string> list = File.ReadAllLines(text).ToList<string>();
                    for (int j = 0; j < list.Count; j++)
                    {
                        if (Program.vidPattern.IsMatch(list[j]))
                        {
                            list[j] = "leonardo.vid=" + mouseVid;
                        }
                        else if (Program.pidPattern.IsMatch(list[j]))
                        {
                            list[j] = "leonardo.pid=" + mousePid;
                        }
                        else if (Program.buildVidPattern.IsMatch(list[j]))
                        {
                            list[j] = "leonardo.build.vid=" + mouseVid;
                        }
                        else if (Program.buildPidPattern.IsMatch(list[j]))
                        {
                            list[j] = "leonardo.build.pid=" + mousePid;
                        }
                        else if (Program.namePattern.IsMatch(list[j]))
                        {
                            list[j] = "leonardo.name=" + mouseName;
                        }
                        else if (Program.usbProductPattern.IsMatch(list[j]))
                        {
                            list[j] = "leonardo.build.usb_product=\"" + mouseName + "\"";
                        }
                        else if (Program.extraFlagsPattern.IsMatch(list[j]))
                        {
                            list[j] = ((comChoice == "Y") ? "leonardo.build.extra_flags={build.usb_flags} -DCDC_DISABLED" : "leonardo.build.extra_flags={build.usb_flags}");
                        }
                    }
                    File.WriteAllLines(text, list);
                    Program.SetFileReadOnly(text);
                    Program.LogAndDisplayMessage("Successfully modified: " + text, Color.Green);
                    Thread.Sleep(2000);
                }
                catch (Exception ex)
                {
                    Program.LogAndDisplayMessage("Error modifying " + text + ": " + ex.Message, Color.Red);
                    Thread.Sleep(2000);
                }
            }
        }
        Program.LogAndDisplayMessage("Spoofing complete. Now continue the tutorial.", Color.Blue);
        Thread.Sleep(10000);
    }

    // Token: 0x0600000F RID: 15 RVA: 0x0000293C File Offset: 0x00000B3C
    private static void BackupOriginalFiles(string[] filePaths, string backupDirectory)
    {
        Directory.CreateDirectory(backupDirectory);
        foreach (string text in filePaths)
        {
            if (File.Exists(text))
            {
                try
                {
                    string destFileName = Path.Combine(backupDirectory, Path.GetFileName(text));
                    File.Copy(text, destFileName, true);
                }
                catch (Exception ex)
                {
                    Program.LogAndDisplayMessage("Error creating backup for " + text + ": " + ex.Message, Color.Red);
                    Thread.Sleep(2000);
                }
            }
        }
    }

    // Token: 0x06000010 RID: 16 RVA: 0x000029C4 File Offset: 0x00000BC4
    private static void SetFileWritable(string filePath)
    {
        FileAttributes attributes = File.GetAttributes(filePath);
        if (attributes.HasFlag(FileAttributes.ReadOnly))
        {
            File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
        }
    }

    // Token: 0x06000011 RID: 17 RVA: 0x000029F5 File Offset: 0x00000BF5
    private static void SetFileReadOnly(string filePath)
    {
        File.SetAttributes(filePath, File.GetAttributes(filePath) | FileAttributes.ReadOnly);
    }

    // Token: 0x06000012 RID: 18 RVA: 0x00002A08 File Offset: 0x00000C08
    private static void SafeAction(Action action)
    {
        try
        {
            action();
        }
        catch (Exception ex)
        {
            Program.LogErrorAndExit("An unexpected error occurred.", ex);
            Thread.Sleep(2000);
        }
    }

    // Token: 0x06000013 RID: 19 RVA: 0x00002A48 File Offset: 0x00000C48
    private static void LogAndDisplayMessage(string message, Color color)
    {
        Console.WriteLine(message, color);
    }

    // Token: 0x06000014 RID: 20 RVA: 0x00002A51 File Offset: 0x00000C51
    private static void LogErrorAndExit(string message, Exception ex = null)
    {
        Console.WriteLine(message, Color.Red);
        if (ex != null)
        {
            Console.WriteLine(ex.Message, Color.Red);
        }
        Thread.Sleep(2000);
        Environment.Exit(1);
    }

    private class MouseDevice
    {
        public string Vid { get; set; }
        public string Pid { get; set; }
        public string Name { get; set; }
    }
}
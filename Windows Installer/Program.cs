using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Windows_Installer
{
    internal class Program
    {
        static Process command = new Process();
        static string Bootloader, ThisLocation, ThisDrive;
        static void Main(string[] args)
        {
            Console.Write("Are you ready to install: (Y/N)  ");
            string Ready = Console.ReadLine();
            if (Ready == "N")
            {
               Environment.Exit(0);
            }
            DataCollecting();
            Install();
            Console.Clear();
            Console.WriteLine("Kész!");
            Console.ReadKey();
        }
        static void CommandLineMode()
        {

        }

        static void DataCollecting()
        {
            using (command = new Process())
            {
                command.StartInfo.FileName = "cmd.exe";
                command.StartInfo.UseShellExecute = false;
                command.StartInfo.RedirectStandardInput = true;
                command.Start();
                StreamWriter myStreamWriter = command.StandardInput;
                string inputText;
                int numLines = 0;
                while (numLines < 4)
                {
                    inputText = DataCommands[numLines];
                    numLines++;
                    myStreamWriter.WriteLine(inputText);
                }
                myStreamWriter.Close();
                command.WaitForExit();
            }
            Console.WriteLine("\n\nData collecting 1/3");
            Console.Write("Number of disk to install:    ");
            DiskCommands[0] = "sel disk " + Console.ReadLine();
            using (command = new Process())
            {
                command.StartInfo.FileName = "cmd.exe";
                command.StartInfo.UseShellExecute = false;
                command.StartInfo.RedirectStandardInput = true;
                command.Start();
                StreamWriter myStreamWriter = command.StandardInput;
                if (File.Exists("install.wim"))
                {
                    myStreamWriter.WriteLine("dism /get-wiminfo /wimfile:install.wim");
                    ComCommands[0] = "dism /apply-image /imagefile:install.wim";
                }
                if (File.Exists("install.esd"))
                {
                    myStreamWriter.WriteLine("dism /get-wiminfo /wimfile:install.esd");
                    ComCommands[0] = "dism /apply-image /imagefile:install.esd";
                }
                myStreamWriter.Close();
                command.WaitForExit();
            }
            Console.WriteLine("\n\nData collecting 2/3");
            Console.Write("Index of windows edition to install:    ");
            ComCommands[0] += " /index:" + int.Parse(Console.ReadLine()) +" /applydir:V:\\";
            Console.WriteLine("\n\nDataCollecting 3/3");
            Console.WriteLine("If you don't know choose \"All\"");
            Console.Write("Bootloader UEFI or BIOS or All:         ");
            Bootloader = Console.ReadLine().ToUpper();
            if (Bootloader == "ALL")
            {
                ComCommands[7] = "bcdboot V:\\Windows /s W: /f All";
            }
            if (Bootloader == "UEFI")
            {
                DiskCommands[2] = "cre par efi size=500";
                DiskCommands[3] = "format fs=fat32 quick";
                ComCommands[7] = "bcdboot V:\\Windows /s W: /f UEFI";
            }
            if (Bootloader == "BIOS")
            {
                ComCommands[7] = "bcdboot V:\\Windows /s W: /f BIOS";
            }
            DiskCommands[3] += " label=\"System Reserved\"";
            Console.Write("\n\nContinue? (Y/N)    ");
            string Continue = Console.ReadLine();
            if (Continue == "N")
            {
                Console.Clear();
                DataCollecting();
            }
        }
        static List<string> DataCommands = new List<string>() {
            "echo off",
            "cls",
            "diskpart",
            "list disk",};

        static void Install()
        {
            Diskpart();
            Command();
        }

        static void Diskpart()
        {
            using (command = new Process())
            {
                command.StartInfo.FileName = "diskpart.exe";
                command.StartInfo.UseShellExecute = false;
                command.StartInfo.RedirectStandardInput = true;
                command.Start();
                StreamWriter myStreamWriter = command.StandardInput;
                string inputText;
                int numLines = 0;
                while (numLines < DiskCommands.Count)
                {
                   if (numLines == 5 && Bootloader == "UEFI")
                    {
                        numLines++;
                    }
                    inputText = DiskCommands[numLines];
                    if (numLines == 1 && Bootloader == "UEFI")
                    {
                        myStreamWriter.WriteLine("conv gpt");
                    }
                    if (numLines == 1 && Bootloader == "BIOS")
                    {
                        myStreamWriter.WriteLine("conv mbr");
                    }
                    numLines++;
                    myStreamWriter.WriteLine(inputText);
                }
                myStreamWriter.Close();
                command.WaitForExit();
            }
        }
        static List<string> DiskCommands = new List<string>() {
            "sel disk",
            "clean",
            "cre par pri size=500",
            "format fs=NTFS quick",
            "assign letter W",
            "active",
            "cre par pri",
            "format fs=ntfs quick",
            "assign letter V",
            "exit"};

        static void Command()
        {
            using (command = new Process())
            {
                command.StartInfo.FileName = "cmd.exe";
                command.StartInfo.UseShellExecute = false;
                command.StartInfo.RedirectStandardInput = true;
                command.Start();
                StreamWriter myStreamWriter = command.StandardInput;
                string inputText;
                int numLines = 0;
                while (numLines < ComCommands.Count)
                {
                    inputText = ComCommands[numLines];
                    numLines++;
                    myStreamWriter.WriteLine(inputText);
                }
                myStreamWriter.Close();
                command.WaitForExit();
            }
        }
        static List<string> ComCommands = new List<string>() {
            "dism /apply-image /imagefile:install.wim or esd /index:num /applydir:V:\\",
            "md W:\\efi\\microsoft\\boot\\",
            "copy bootrec.exe W:\\efi\\microsoft\\boot\\bootrec.exe",
            "W:",
            "cd /d W:\\efi\\microsoft\\boot\\",
            "bootrec.exe /fixmbr",
            "del W:\\efi\\microsoft\\boot\\bootrec.exe /s /q",
            "bcdboot V:\\Windows /s W: /f All (BIOS, UEFI)",};
    }
}

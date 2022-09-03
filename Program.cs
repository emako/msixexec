using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace msixexec
{
    internal class Program
    {
        /// <summary>
        /// Call AppInstaller to install msix-like package but not a cli installer.
        /// Support msix-like format: msix/msixbundle/appx/appxbundle.
        /// ---
        /// ExitCode Meaning
        /// -1: No msix-like path
        /// 0: AppInstaller Exited
        /// 1: AppInstaller Not found
        /// </summary>
        /// <param name="args">path of msix</param>
        public static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("No msix-like path");
                Environment.Exit(-1);
            }
            
            // Startup the installer provided by system
            Process.Start(new ProcessStartInfo()
            {
                FileName = "cmd.exe",
                Arguments = $"/c \"{args.First()}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
            }).WaitForExit();

            // Waitting for chattering
            Thread.Sleep(3000);

            int? pid = null;

            try
            {
                // Search program such as following
                // C:\Program Files\WindowsApps\Microsoft.DesktopAppInstaller_1.18.2091.0_x64__8wekyb3d8bbwe\AppInstaller.exe
                Process p = Process.GetProcessesByName("AppInstaller").FirstOrDefault();

                if (p != null)
                {
                    // Make sure program found is published by microsoft
                    if (p.MainModule.FileVersionInfo.CompanyName == "Microsoft Corporation")
                    {
                        pid = p.Id;
                        Console.WriteLine($"AppInstaller found, pid={pid}");
                    }
                }
            }
            catch
            {
            }

            if (pid != null)
            {
                try
                {
                    // Waitting for AppInstaller exited
                    while (Process.GetProcessById(pid ?? default) != null)
                    {
                        Thread.Sleep(1000);
                    }
                }
                catch
                {
                }
            }
            else
            {
                Console.WriteLine("AppInstaller Not found");
                Environment.Exit(1);
            }

            Console.WriteLine("AppInstaller Exited");
            Environment.Exit(0);
        }
    }
}

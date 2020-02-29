using System;
using System.Diagnostics;

namespace NuPut
{
    public static class CommandLine
    {
        public static string Cmd(string cmd)
        {
            var process = new Process
            {
                StartInfo =
                {
                    FileName = GetProcessFileName(),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    CreateNoWindow = false,
                    UseShellExecute = false,
                }
            };

            process.Start();

            process.StandardInput.WriteLine(cmd);
            process.StandardInput.Flush();
            process.StandardInput.Close();
            process.WaitForExit();

            return process.StandardOutput.ReadToEnd();
        }

        private static string GetProcessFileName()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                case PlatformID.MacOSX:
                {
                    return "/bin/bash";
                }

                case PlatformID.Xbox:
                {
                    Console.WriteLine("Xbox not supported");
                    Environment.Exit(0);
                    throw new Exception();
                }

                default: return "powershell.exe";
            }
        }
    }
}
using System;
using NuGet.Common;

namespace NuPut
{
    
    public class Logger : ILogger
    {
        public void LogDebug(string data)
        {
            Console.WriteLine(data);
        }

        public void LogVerbose(string data)
        {
            Console.WriteLine(data);
        }

        public void LogInformation(string data)
        {
            Console.WriteLine(data);
        }

        public void LogMinimal(string data)
        {
            Console.WriteLine(data);
        }

        public void LogWarning(string data)
        {
            Console.WriteLine(data);
        }

        public void LogError(string data)
        {
            Console.WriteLine(data);
        }

        public void LogInformationSummary(string data)
        {
            Console.WriteLine(data);
        }

        public void LogErrorSummary(string data)
        {
            Console.WriteLine(data);
        }
    }
}
using CommandLine;

namespace NuPut
{
    public class Options
    {
        [Option(
            'h',
            "host",
            Required = true,
            HelpText = "Url for NuGet.Server")]
        public string Host { get; set; }

        [Option('d', "dir", Required = true, HelpText = "Solution directory")]
        public string WorkingDir { get; set; }

        public Options()
        {
            Host = string.Empty;
            WorkingDir = string.Empty;
        }
    }
}
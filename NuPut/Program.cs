﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommandLine;
using ConsoleTables;
using NuGet.Common;

namespace NuPut
{
    internal class Program
    {
        private static ILogger _logger;

        static Program()
        {
            _logger = new Logger();
        }

        private static void ParseError(IEnumerable<Error> errors)
        {
            foreach (var error in errors)
            {
                Console.WriteLine(error.ToString());
            }

            throw new Exception("Parsing errors");
        }

        internal static async Task Main(string[] args)
        {
            var host = string.Empty;
            var workingDir = string.Empty;
            try
            {
                Parser.Default.ParseArguments<Options>(args)
                    .WithParsed(o =>
                    {
                        host = o.Host;
                        workingDir = o.WorkingDir;
                        Console.WriteLine(
                            $"Enabling with settings:\r\n" +
                            $"Host: {host}\r\n" +
                            $"Working dir: {workingDir}\r\n");
                    })
                    .WithNotParsed(ParseError);
            }
            catch (Exception)
            {
                Environment.Exit(1);
            }

            CheckProceed();

            var packages = new List<Package>();

            try
            {
                var connection = new NuGetConnection(host, _logger);

                var projects = CheckProjects(workingDir);
                
                await projects.ForEachAsync(async x =>
                {
                    var projectName = Path.GetFileNameWithoutExtension(x.Name);
                    var metadatas = await connection.GetMetadataAsync(projectName);
                    var package = new Package(x, metadatas);
                    packages.Add(package);
                });

                packages = packages.OrderBy(x => x.Name).ToList();
                
                packages.PrintPackages();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Something went terribly wrong");
                Environment.Exit(1);
            }

            Console.WriteLine();

            if (!packages.Any())
            {
                Console.WriteLine($"No candidates");
                Environment.Exit(0);
            }

            Console.WriteLine("Proceed with candidates: ");
            Console.WriteLine("[hit enter for all or digits divided with space for specific (\"0 3 6\")]");

            var candidatesToProceedRange = Console.ReadLine();

            if (!string.IsNullOrEmpty(candidatesToProceedRange))
            {
                var candidatesIdsToKeep = new List<int>();

                var split = candidatesToProceedRange
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var candidateIdString in split)
                {
                    var parseSuccess = int.TryParse(candidateIdString, out var candidateId);

                    if (!parseSuccess)
                    {
                        Console.WriteLine($"Error parsing {candidateIdString}");
                        Environment.Exit(0);
                    }

                    if (candidateId - 1 > packages.Count - 1 || candidateId < 0)
                    {
                        Console.WriteLine($"No candidate {candidateId} found");
                        Environment.Exit(0);
                    }

                    candidatesIdsToKeep.Add(candidateId - 1);
                }

                Console.WriteLine($"Keeping [{string.Join(' ', candidatesIdsToKeep.Select(x => x + 1))}]");
                var candidatesToKeep = packages.Where((_, i) => candidatesIdsToKeep.Contains(i));

                packages = candidatesToKeep.ToList();
            }

            packages.PrintPackages();

            CheckProceed();

            var table = new ConsoleTable("Index", "VersionSection");
            table
                .AddRow(0, VersionSection.PATCH)
                .AddRow(1, VersionSection.MINOR)
                .AddRow(2, VersionSection.MAJOR);

            foreach (var package in packages)
            {
                Console.WriteLine($"Select version section to increase for project: {package.Name}");

                table.Write();
                var versionParsed = int.TryParse(Console.ReadLine(), out var versionInt);
                VersionSection versionSection;
                try
                {
                    versionSection = versionParsed ? (VersionSection) versionInt : VersionSection.PATCH;
                }
                catch (Exception)
                {
                    versionSection = VersionSection.PATCH;
                }
                
                package.IncreaseVersion(versionSection);
                package.Build();
                package.Pack();

                Console.WriteLine($"{package.Name} is ready to be pushed");

                CheckProceed();

                package.Push(host);
            }

            Environment.Exit(0);
        }

        private static IEnumerable<FileInfo> CheckProjects(string workingDir)
        {
            return Directory
                .EnumerateFiles(workingDir, "*.csproj", SearchOption.AllDirectories)
                .Select(x => new FileInfo(x));
        }

        private static void CheckProceed()
        {
            var ask = true;

            while (ask)
            {
                Console.WriteLine();
                Console.Write("Proceed [y/n]: ");

                var answer = Console.ReadLine();

                if (answer == null) continue;

                answer = answer.ToLowerInvariant();

                if (answer == "n" || answer == "no") Environment.Exit(0);
                if (answer == "yes" || answer == "y" || answer == string.Empty) ask = false;
            }
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTables;
using NuGet.Versioning;

namespace NuPut
{
    public static class Extensions
    {
        public static Task ForEachAsync<T>(
            this IEnumerable<T> source,
            Func<T, Task> body,
            int maxDegreeOfParallelism = -1,
            bool throwOnException = true)
        {
            var asArray = source.ToArray();
            if (!asArray.Any()) return Task.CompletedTask;

            if (maxDegreeOfParallelism <= 0 && maxDegreeOfParallelism != -1) throw new Exception();

            var tasks = Partitioner.Create(asArray)
                .GetPartitions(maxDegreeOfParallelism == -1 ? asArray.Length : maxDegreeOfParallelism)
                .Select(partition => Task.Run((Func<Task>) (async () =>
                {
                    using (partition)
                        while (partition.MoveNext())
                            await body(partition.Current)
                                .ContinueWith(t =>
                                {
                                    if (t.Status != TaskStatus.Faulted) return;
                                    if (throwOnException && t.Exception != null) throw t.Exception;
                                });
                })));

            return Task.WhenAll(tasks);
        }

        public static NuGetVersion IncreaseVersion(
            this NuGetVersion version,
            VersionSection versionSection = VersionSection.PATCH)
        {
            return versionSection switch
            {
                VersionSection.PATCH => new NuGetVersion(version.Major, version.Minor, version.Patch + 1),
                VersionSection.MINOR => new NuGetVersion(version.Major, version.Minor + 1, 0),
                VersionSection.MAJOR => new NuGetVersion(version.Major + 1, 0, 0),
                _ => new NuGetVersion(version.Major, version.Minor, version.Patch + 1)
            };
        }

        public static void PrintPackages(this IEnumerable<Package> packages)
        {
            var index = 0;
            ConsoleTable.From(packages.Select(x => new
                {
                    Index = index += 1,
                    x.Name,
                    x.LatestRemoteVersion
                }))
                .Write();
        }
    }
}
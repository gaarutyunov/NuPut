using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;

namespace NuPut
{
    public class Package
    {
        private IEnumerable<IPackageSearchMetadata> RemoteMetadata { get; set; }
        private IEnumerable<NuGetVersion> RemoteVersions => RemoteMetadata.Select(x => x.Identity.Version);
        public NuGetVersion LatestRemoteVersion => RemoteVersions.Max();
        private NuGetVersion NewVersion { get; set; }

        private FileInfo ProjectInfo { get; set; }
        public string Name { get; set; }
        public Package(FileInfo projectInfo, IEnumerable<IPackageSearchMetadata> remoteMetadata)
        {
            var packageSearchMetadatas = remoteMetadata.ToList();
            RemoteMetadata = packageSearchMetadatas;
            ProjectInfo = projectInfo;
            Name = Path.GetFileNameWithoutExtension(projectInfo.Name);
            NewVersion = packageSearchMetadatas.Any() ? LatestRemoteVersion.IncreaseVersion() : new NuGetVersion(1, 0, 0);
        }

        public void IncreaseVersion(VersionSection versionSection)
        {
            if (LatestRemoteVersion != null)
            { 
                NewVersion = LatestRemoteVersion.IncreaseVersion(versionSection);
            }
        }
        
        public void Push(string host)
        {
            if (ProjectInfo?.Directory == null)
            {
                throw new ArgumentNullException(nameof(ProjectInfo));
            }
            var nupkg = ProjectInfo.Directory
                .EnumerateFiles($"{Name}.{NewVersion}.nupkg", SearchOption.AllDirectories)
                .FirstOrDefault();
            if (nupkg == null) throw new ArgumentNullException(nameof(nupkg));
            Console.WriteLine(CommandLine.Cmd($"nuget push {nupkg} -Source {host}"));
            File.Delete(nupkg.FullName);
        }

        public void Pack()
        {
            var command = $"dotnet pack {ProjectInfo.FullName} -p:PackageVersion={NewVersion}";
            Console.WriteLine(CommandLine.Cmd(command));
        }

        public void Build()
        {
            var command = $"dotnet build {ProjectInfo.FullName}";
            Console.WriteLine(CommandLine.Cmd(command));
        }
    }
}
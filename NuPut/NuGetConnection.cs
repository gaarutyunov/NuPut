using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NuGet.Common;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuPut
{
    public class NuGetConnection
    {
        private readonly SourceRepository _sourceRepository;
        private readonly ILogger _logger;

        public NuGetConnection(string host, ILogger logger)
        {
            _logger = logger;
            var providers = new List<Lazy<INuGetResourceProvider>>();
            providers.AddRange(Repository.Provider.GetCoreV3());
            try
            {
                var packageSource = new PackageSource(host, string.Empty, true);
                _sourceRepository = new SourceRepository(packageSource, providers);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<IEnumerable<IPackageSearchMetadata>> GetMetadataAsync(string projectId)
        {
            var packageMetadataResource = await _sourceRepository.GetResourceAsync<PackageMetadataResource>();
            return await packageMetadataResource.GetMetadataAsync(
                projectId,
                true,
                true,
                _logger,
                CancellationToken.None);
        }
    }
}
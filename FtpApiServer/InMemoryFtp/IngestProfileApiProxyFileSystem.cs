// <copyright file="InMemoryFileSystem.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FubarDev.FtpServer.BackgroundTransfer;
using FubarDev.FtpServer.FileSystem;
using Kaltura.Services;
using Kaltura.Request;
using Microsoft.Extensions.Logging;
using KLogMonitor;
using System.Reflection;
using Kaltura.Types;
using FubarDev.FtpServer;
using FtpApiServer.Authentication;

namespace FtpApiServer.InMemoryFtp
{
    public class IngestProfileApiProxyFileSystem : IUnixFileSystem
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly AuthenticatedKalturaOttUser _AuthenticatedKalturaOttUser;
        private readonly Kaltura.Client _KalturaClient;

        public bool SupportsAppend => false;
        public bool SupportsNonEmptyDirectoryDelete => false;
        public StringComparer FileSystemEntryComparer => StringComparer.OrdinalIgnoreCase;
        public IUnixDirectoryEntry Root { get; }

        public IngestProfileApiProxyFileSystem(AuthenticatedKalturaOttUser authenticatedKalturaOttUser, Kaltura.Client kalturaClient)
        {
            _KalturaClient = kalturaClient;
            _AuthenticatedKalturaOttUser = authenticatedKalturaOttUser;
            var ingestProfiles = GetIngestProfiles(authenticatedKalturaOttUser.Ks);
            var rootFolder = new IngestProfileDirectoryEntry(this);
            rootFolder.Children = ingestProfiles.Select(p => (IUnixFileSystemEntry)new IngestProfileDirectoryEntry(this, p, rootFolder)).ToList();
            Root = rootFolder;
        }

        private IList<IngestProfile> GetIngestProfiles(string authenticateduserKs)
        {
            try
            {
                var ingestProfiles = IngestProfileService.List().WithKs(authenticateduserKs).ExecuteAndWaitForResponse(_KalturaClient)?.Objects;
                if (ingestProfiles == null)
                {
                    _Logger.Error($"Could not find any ingest profiles");
                    return new List<IngestProfile>();
                }
                return ingestProfiles;
            }
            catch (Exception e)
            {
                _Logger.Error($"KalturaIngestProfileApiProxyFileSystem > Error while trying to list ingest profiles", e);
                return new List<IngestProfile>();
            }
        }

        public Task<IReadOnlyList<IUnixFileSystemEntry>> GetEntriesAsync(IUnixDirectoryEntry directoryEntry, CancellationToken cancellationToken)
        {
            var entry = (IngestProfileDirectoryEntry)directoryEntry;
            var children = entry.Children.ToList();
            return Task.FromResult<IReadOnlyList<IUnixFileSystemEntry>>(children);
        }

        public Task<IUnixFileSystemEntry> GetEntryByNameAsync(IUnixDirectoryEntry directoryEntry, string name, CancellationToken cancellationToken)
        {
            var entry = (IngestProfileDirectoryEntry)directoryEntry;
            var childEntry = entry.Children.FirstOrDefault(e => e.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (childEntry != null)
            {
                return Task.FromResult(childEntry);
            }

            return Task.FromResult<IUnixFileSystemEntry>(null);
        }

        public Task<IUnixFileSystemEntry> MoveAsync(IUnixDirectoryEntry parent, IUnixFileSystemEntry source, IUnixDirectoryEntry target, string fileName, CancellationToken cancellationToken)
        {
            throw new UnauthorizedAccessException("Directory creation is no allowed.");
        }

        public Task UnlinkAsync(IUnixFileSystemEntry entry, CancellationToken cancellationToken)
        {
            throw new UnauthorizedAccessException("Directory creation is no allowed.");
        }

        public Task<IUnixDirectoryEntry> CreateDirectoryAsync(IUnixDirectoryEntry targetDirectory, string directoryName, CancellationToken cancellationToken)
        {
            throw new UnauthorizedAccessException("Directory creation is no allowed.");
        }

        public Task<Stream> OpenReadAsync(IUnixFileEntry fileEntry, long startPosition, CancellationToken cancellationToken)
        {
            // TODO: unwrap entry with the s3 url of the source file
            var entry = (InMemoryFileEntry)fileEntry;
            var stream = new MemoryStream(entry.Data)
            {
                Position = startPosition,
            };

            return Task.FromResult<Stream>(stream);
        }

        public async Task<IBackgroundTransfer> AppendAsync(IUnixFileEntry fileEntry, long? startPosition, Stream data, CancellationToken cancellationToken)
        {
            return null;
        }

        public async Task<IBackgroundTransfer> CreateAsync(IUnixDirectoryEntry targetDirectory, string fileName, Stream data, CancellationToken cancellationToken)
        {
            //var inMemoryFileStream = new MemoryStream();
            //await data.CopyToAsync(inMemoryFileStream, 81920, cancellationToken)
            //    .ConfigureAwait(false);
            
            // TODO: Stream is not sending well, cant tell why, phoenix throws an error in parser because stream is empty
            var targetEntry = (IngestProfileDirectoryEntry)targetDirectory;
            var profileId = targetEntry.IngestProfileData.Id;
            var assetData = new BulkUploadProgramAssetData { TypeId = 0 };
            var jobData = new BulkUploadIngestJobData { IngestProfileId = profileId };
            var bulkUploadResponse = await AssetService.AddFromBulkUpload(data, jobData, assetData)
                .WithKs(_AuthenticatedKalturaOttUser.Ks)
                .ExecuteAsync(_KalturaClient).ConfigureAwait(false);

            



            // TODO: Allow downloading uploaded files by calling a phoenix url to download the source from s3
            var entry = new InMemoryFileEntry(this, targetEntry, fileName, new byte[0]);
            targetEntry.Children.Add(entry);

            var now = DateTimeOffset.Now;
            targetEntry.SetLastWriteTime(now);

            entry
                .SetLastWriteTime(now)
                .SetCreateTime(now);

            return null;
        }

        public async Task<IBackgroundTransfer> ReplaceAsync(IUnixFileEntry fileEntry, Stream data, CancellationToken cancellationToken)
        {
            var temp = new MemoryStream();
            await data.CopyToAsync(temp, 81920, cancellationToken)
                .ConfigureAwait(false);

            var entry = (InMemoryFileEntry)fileEntry;
            entry.Data = temp.ToArray();

            var now = DateTimeOffset.Now;
            entry.SetLastWriteTime(now);

            return null;
        }

        public Task<IUnixFileSystemEntry> SetMacTimeAsync(IUnixFileSystemEntry entry, DateTimeOffset? modify, DateTimeOffset? access, DateTimeOffset? create, CancellationToken cancellationToken)
        {
            var fsEntry = (InMemoryFileSystemEntry)entry;

            if (modify != null)
            {
                fsEntry.SetLastWriteTime(modify.Value);
            }

            if (create != null)
            {
                fsEntry.SetCreateTime(create.Value);
            }

            return Task.FromResult(entry);
        }

        public void Dispose()
        {
        }
    }
}

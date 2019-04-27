using System.Collections.Generic;
using FubarDev.FtpServer.FileSystem;
using FubarDev.FtpServer.FileSystem.Generic;
using Kaltura.Types;

namespace FtpApiServer.InMemoryFtp
{
    public class IngestProfileDirectoryEntry : InMemoryFileSystemEntry, IUnixDirectoryEntry
    {
        private static readonly IUnixPermissions _defaultPermissions = new GenericUnixPermissions(
            new GenericAccessMode(true, true, true),
            new GenericAccessMode(true, true, true),
            new GenericAccessMode(true, false, true));

        public IngestProfileDirectoryEntry(IUnixFileSystem fileSystem, IngestProfile ingestProfileData = null, IngestProfileDirectoryEntry parent = null) 
            : base(fileSystem, parent, ingestProfileData?.ExternalId, _defaultPermissions)
        {
            IngestProfileData = ingestProfileData;
            Children = new List<IUnixFileSystemEntry>();
        }

        public bool IsRoot => Parent is null;

        public bool IsDeletable => !IsRoot;

        public IList<IUnixFileSystemEntry> Children { get; set; }
        public IngestProfile IngestProfileData { get; }
    }
}

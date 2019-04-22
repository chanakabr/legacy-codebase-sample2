// <copyright file="InMemoryFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using Microsoft.Extensions.Options;

namespace FtpApiServer.InMemoryFtp
{
    /// <summary>
    /// An implementation of an in-memory file system.
    /// </summary>
    public class InMemoryFileSystemProvider : IFileSystemClassFactory
    {
        private Action<string, string, Stream> _OnFileUploaded;

        /// <summary>
        /// Initializes a new instance of the <see cref="InMemoryFileSystemProvider"/> class.
        /// </summary>
        /// <param name="options">The provider options.</param>
        public InMemoryFileSystemProvider(IOptions<InMemoryFileSystemOptions> options)
        {
            _OnFileUploaded = options.Value.OnFileUpload;
        }

      
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            // TODO: Safly authenticate and throw an error if wrong creds
            var userParams = userId.Split('/');
            var ottUsername = userParams[0];
            var groupId = int.Parse(userParams[1]);

            var fileSystem = new InMemoryFileSystem(groupId, _OnFileUploaded);
            return Task.FromResult<IUnixFileSystem>(fileSystem);
        }
    }
}

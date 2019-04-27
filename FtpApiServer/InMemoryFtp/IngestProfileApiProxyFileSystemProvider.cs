// <copyright file="InMemoryFileSystemProvider.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using FtpApiServer.Authentication;
using FubarDev.FtpServer;
using FubarDev.FtpServer.AccountManagement;
using FubarDev.FtpServer.FileSystem;
using Kaltura;
using Microsoft.Extensions.Options;

namespace FtpApiServer.InMemoryFtp
{
    public class IngestProfileApiProxyFileSystemProvider : IFileSystemClassFactory
    {
        private Action<string, string, Stream> _OnFileUploaded;
        private readonly IFtpConnectionAccessor _ConnectionAccessor;
        private readonly Client _KalturaClient;
        private readonly IFtpUser _User;

      
        public IngestProfileApiProxyFileSystemProvider(IOptions<InMemoryFileSystemOptions> options, 
            Kaltura.Client kalturaClient, 
            IFtpConnectionAccessor connectionAccessor
            )
        {
            _OnFileUploaded = options.Value.OnFileUpload;
            _ConnectionAccessor = connectionAccessor;
            _KalturaClient = kalturaClient;
        }

      
        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var authenticatedOttUser = (AuthenticatedKalturaOttUser)_ConnectionAccessor.FtpConnection.Data.User;

            var fileSystem = new IngestProfileApiProxyFileSystem(authenticatedOttUser, _OnFileUploaded, _KalturaClient);
            return Task.FromResult<IUnixFileSystem>(fileSystem);
        }
    }
}

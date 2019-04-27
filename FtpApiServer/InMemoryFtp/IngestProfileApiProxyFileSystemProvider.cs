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
        private readonly IFtpConnectionAccessor _ConnectionAccessor;
        private readonly Client _KalturaClient;


        public IngestProfileApiProxyFileSystemProvider(Client kalturaClient, IFtpConnectionAccessor connectionAccessor)
        {
            _ConnectionAccessor = connectionAccessor;
            _KalturaClient = kalturaClient;
        }


        public Task<IUnixFileSystem> Create(string userId, bool isAnonymous)
        {
            var authenticatedOttUser = (AuthenticatedKalturaOttUser)_ConnectionAccessor.FtpConnection.Data.User;

            var fileSystem = new IngestProfileApiProxyFileSystem(authenticatedOttUser, _KalturaClient);
            return Task.FromResult<IUnixFileSystem>(fileSystem);
        }
    }
}

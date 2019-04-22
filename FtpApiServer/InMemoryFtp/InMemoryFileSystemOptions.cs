// <copyright file="InMemoryFileSystemOptions.cs" company="Fubar Development Junker">
// Copyright (c) Fubar Development Junker. All rights reserved.
// </copyright>

using System;
using System.IO;

namespace FtpApiServer.InMemoryFtp
{
    /// <summary>
    /// In-memory file system options.
    /// </summary>
    public class InMemoryFileSystemOptions
    {
       /// <summary>
       /// The action to invoke once a new file have been uploaded.
       /// arg1: folderName
       /// arg2: fileName
       /// arg3: fileDataStream
       /// </summary>
        public Action<string, string, Stream> OnFileUpload { get; set; }
    }
}

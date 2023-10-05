using System;
using Amazon.S3.Transfer;
using System.IO;
using System.Net;

namespace ApiLogic.Catalog
{
    public abstract class OTTBasicFile
    {
        public bool ShouldDeleteSourceFile { get; protected set; }
        public string Name { get; set; }
        public abstract TransferUtilityUploadRequest GetTransferUtilityUploadRequest();
        public abstract void SaveToFileSystem(string destPath, bool shouldDelete);
        public abstract FileInfo GetFileInfo();
    }

    public class OTTStreamFile : OTTBasicFile
    {
        private Stream file;

        public OTTStreamFile(Stream formFile, string fileName)
        {
            file = formFile;            
            Name = fileName;
            ShouldDeleteSourceFile = false;
        }

        public override FileInfo GetFileInfo()
        {
            return new FileInfo(Name);
        }

        public Stream GetStream()
        {
            return this.file;
        }
        
        public override TransferUtilityUploadRequest GetTransferUtilityUploadRequest()
        {
            return new TransferUtilityUploadRequest { InputStream = this.file };
        }

        public override void SaveToFileSystem(string destPath, bool shouldDelete)
        {
            using (Stream tempFile = File.Create(destPath))
            {
                this.file.CopyTo(tempFile);
            }
        }
    }

    public class OTTFile: OTTBasicFile
    {
        private string Path;
        public OTTFile(string filePath, string fileName, bool shouldDeleteSourceFile = true)
        {
            Path = filePath;
            Name = fileName;
            ShouldDeleteSourceFile = shouldDeleteSourceFile;
        }

        public override FileInfo GetFileInfo()
        {
            var fi = new FileInfo(Path);
            if (fi.Exists)
            {
                return fi;
            }

            var _path = Path;
            int place = _path.LastIndexOf("/");
    
            if (place == -1)
                return fi;
    
            _path =  _path.Remove(place, "/".Length).Insert(place, "\\");
            return new FileInfo(_path);
        }

        public override TransferUtilityUploadRequest GetTransferUtilityUploadRequest()
        {
            var fileInfo = GetFileInfo();
            try
            {
                var fileStream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                byte[] buffer;
                int sum = 0;                          // total number of bytes read

                int length = (int)fileStream.Length;  // get file length
                buffer = new byte[length];            // create buffer
                int count;                            // actual number of bytes read

                // read until Read method returns 0 (end of the stream has been reached)
                while ((count = fileStream.Read(buffer, sum, length - sum)) > 0)
                    sum += count;  // sum is a buffer offset for next reading

                if (sum > 0)
                {
                    return new TransferUtilityUploadRequest { InputStream = fileStream};
                }
            }
            catch (Exception)
            {
                // ignored
            }

            return new TransferUtilityUploadRequest { FilePath = fileInfo.FullName };
        }

        public override void SaveToFileSystem(string destPath, bool shouldDelete)
        {
            var fullName = GetTransferUtilityUploadRequest().FilePath;
            if (shouldDelete)
                File.Move(fullName, destPath);
            else
                File.Copy(fullName, destPath);
        }
    }
}

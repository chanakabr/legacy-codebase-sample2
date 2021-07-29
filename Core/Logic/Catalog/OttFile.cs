using Amazon.S3.Transfer;
using System.IO;

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
            return new FileInfo(Path);
        }

        public override TransferUtilityUploadRequest GetTransferUtilityUploadRequest()
        {
            var fileInfo = new FileInfo(Path);
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

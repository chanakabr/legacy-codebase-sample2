
using Amazon.S3;
using Amazon.S3.Transfer;
using ApiObjects.Response;
using ConfigurationManager;
using ConfigurationManager.Types;
using KLogMonitor;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ApiLogic
{
    public abstract class FileHandler
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected abstract void Initialize();
        protected abstract GenericResponse<string> Save(string fileName, FileInfo fileInfo, string subDir);
        protected abstract GenericResponse<string> GetSubDir(string id, string typeName);
        protected abstract GenericResponse<string> GetSubDir(long id, string typeName);
        protected abstract string GetUrl(string subDir, string fileName);
        protected abstract Status Delete(string fileURL);
        protected virtual Status ValidateFileContent(FileInfo file, string filePath) { return Status.Ok; }

        public bool ShouldDeleteSourceFile { get; protected set; }
        private const string KALTURA = "Kaltura";

        private static FileHandler _instance;
        public static FileHandler Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetFileHandlerImpl();
                }
                return _instance;
            }
        }

        protected FileHandler()
        {
            initialize();
            Initialize();
        }

        private void initialize()
        {
            ShouldDeleteSourceFile = ApplicationConfiguration.Current.FileUpload.ShouldDeleteSourceFile.Value;
        }

        private static FileHandler GetFileHandlerImpl()
        {

            switch (ApplicationConfiguration.Current.FileUpload.Type.Value)
            {
                case eFileUploadType.FileSystem:
                    return new FileSystemHandler();
                case eFileUploadType.S3:
                    return new S3FileHandler();
            }

            return null;
        }

        public Status DeleteFile(string fileUrl)
        {
            return Delete(fileUrl);
        }

        public GenericResponse<string> SaveFile(string id, FileInfo fileInfo, string objectTypeName, string filePath = "")
        {
            GenericResponse<string> saveFileResponse = new GenericResponse<string>();
            var validationResponse = Validate(objectTypeName, fileInfo, filePath);
            if (validationResponse.HasObject())
            {
                saveFileResponse = GetSubDir(id, validationResponse.Object);
                if (saveFileResponse.HasObject())
                {
                    saveFileResponse = Save(GetFileName(id.ToString(), fileInfo.Extension), fileInfo, saveFileResponse.Object);
                }
            }
            else
            {
                saveFileResponse.SetStatus(validationResponse.Status);
            }

            return saveFileResponse;
        }

        public GenericResponse<string> SaveFile(long id, FileInfo fileInfo, string objectTypeName, string filePath = "")
        {
            GenericResponse<string> saveFileResponse = new GenericResponse<string>();
            var validationResponse = Validate(objectTypeName, fileInfo, filePath);
            if (validationResponse.HasObject())
            {
                saveFileResponse = GetSubDir(id, validationResponse.Object);
                if (saveFileResponse.HasObject())
                {
                    saveFileResponse = Save(GetFileName(id.ToString(), fileInfo.Extension), fileInfo, saveFileResponse.Object);
                }
            }
            else
            {
                saveFileResponse.SetStatus(validationResponse.Status);
            }

            return saveFileResponse;
        }

        private string GetFileName(string id, string fileExtension)
        {
            return (id + fileExtension);
        }

        private GenericResponse<string> Validate(string objectTypeName, FileInfo fileInfo = null, string filePath = "")
        {
            var validationStatus = new GenericResponse<string>();
            if (fileInfo != null && !fileInfo.Exists)
            {
                validationStatus.SetStatus(eResponseStatus.FileDoesNotExists, string.Format("file:{0} does not exists.", fileInfo.Name));
                return validationStatus;
            }

            validationStatus = GetFileObjectTypeName(objectTypeName);

            if (validationStatus.IsOkStatusCode())
            {
                validationStatus.SetStatus(ValidateFileContent(fileInfo, filePath));
            }

            return validationStatus;
        }

        public GenericResponse<string> GetFileObjectTypeName(string objectTypeName)
        {
            var response = new GenericResponse<string>();

            if (string.IsNullOrEmpty(objectTypeName))
            {
                response.SetStatus(eResponseStatus.InvalidFileType, "File's objectType name cannot be empty");
                return response;
            }

            if (!objectTypeName.StartsWith(KALTURA))
            {
                response.SetStatus(eResponseStatus.InvalidFileType, string.Format("File's objectType value must be type of KalturaOTTObject. objectType.Name={0}", objectTypeName));
                return response;
            }

            response.Object = objectTypeName.Substring(KALTURA.Length);
            if (string.IsNullOrEmpty(response.Object))
            {
                response.SetStatus(eResponseStatus.InvalidFileType, string.Format("File's objectType.Name minimum length is {0}. objectType.Name={1}", KALTURA.Length + 1, objectTypeName));
                return response;
            }

            response.SetStatus(eResponseStatus.OK);
            return response;
        }
    }

    public class S3FileHandler : FileHandler
    {
        public int NumberOfRetries { get; private set; }
        public string Region { get; private set; }
        public string BucketName { get; private set; }
        public string Path { get; private set; }

        private const int m = 1024 * 1024;//Byte to Mb
        private const int _maxFileSize = 15; //Max upload file size : 15MB
        private static List<string> _fileExtensions = new List<string> { "jpeg", "jpg", "png", "tif", "gif", "xls", "xlsx", "csv", "xslm" }
                .Select(x => x.Replace(".", string.Empty)).ToList();//Supported file types

        protected override void Initialize()
        {
            Region = ApplicationConfiguration.Current.FileUpload.S3.Region.Value;
            BucketName = ApplicationConfiguration.Current.FileUpload.S3.BucketName.Value;
            NumberOfRetries = ApplicationConfiguration.Current.FileUpload.S3.NumberOfRetries.Value;
            Path = ApplicationConfiguration.Current.FileUpload.S3.Path.Value;
        }

        protected override GenericResponse<string> Save(string fileName, FileInfo fileInfo, string subDir)
        {
            GenericResponse<string> saveResponse = new GenericResponse<string>();
            for (int i = 0; i < NumberOfRetries; i++)
            {
                using (var client = new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(Region)))
                {
                    try
                    {
                        var filePath = GetRelativeFilePath(subDir, fileName);
                        using (var fileTransferUtility = new TransferUtility(client))
                        {
                            var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                            {
                                BucketName = BucketName,
                                FilePath = fileInfo.FullName,
                                Key = filePath
                            };
                            fileTransferUtility.Upload(fileTransferUtilityRequest);

                            if (ShouldDeleteSourceFile)
                                File.Delete(fileInfo.FullName);

                            saveResponse.Object = GetUrl(subDir, fileName);
                            saveResponse.SetStatus(eResponseStatus.OK);
                            return saveResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("An Exception was occurred in Save file to S3, attempt: {0}/{1}. fileName:{2}, fileInfo.FullName:{3}, subDir:{4}.",
                                                i + 1, NumberOfRetries, fileName, fileInfo.FullName, subDir), ex);
                        saveResponse.SetStatus(eResponseStatus.ErrorSavingFile, string.Format("Error while save file:{0} to S3", fileName));
                        return saveResponse;
                    }
                }
            }

            saveResponse.SetStatus(eResponseStatus.ErrorSavingFile, string.Format("Could not save file:{0} to S3", fileName));
            return saveResponse;
        }

        protected override Status Delete(string fileURL)
        {
            var deleteResponse = new Status((int)eResponseStatus.Error);
            for (int i = 0; i < NumberOfRetries; i++)
            {
                using (var client = new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(Region)))
                {
                    try
                    {
                        using (var fileTransferUtility = new TransferUtility(client))
                        {
                            File.Delete(fileURL);
                            deleteResponse.Set(eResponseStatus.OK);
                            return deleteResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error(string.Format("An Exception was occurred in Delete file from S3, attempt: {0}/{1}. fileURL:{2}",
                                                i + 1, NumberOfRetries, fileURL), ex);
                        deleteResponse.Set(eResponseStatus.Error, string.Format("Error while Delete file:{0} from S3", fileURL));
                        return deleteResponse;
                    }
                }
            }

            deleteResponse.Set(eResponseStatus.Error, string.Format("Could not Delete file:{0} from S3", fileURL));
            return deleteResponse;
        }

        protected override GenericResponse<string> GetSubDir(string id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>()
            {
                Object = typeName
            };
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        protected override GenericResponse<string> GetSubDir(long id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>()
            {
                Object = typeName
            };
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        protected override string GetUrl(string subDir, string fileName)
        {
            return string.Format("{0}{1}", Path, GetRelativeFilePath(subDir, fileName));
        }

        private string GetRelativeFilePath(string subDir, string fileName)
        {
            return string.Format("{0}_{1}", subDir, fileName);
        }

        protected override Status ValidateFileContent(FileInfo file, string filePath)
        {
            var status = Status.Ok;
            try
            {
                if (file.Length > _maxFileSize * m)//check size in bytes
                {
                    log.Debug($"Failed file size validation, file size: {file.Length * m} mb");
                    status.Set(eResponseStatus.FileExceededMaxSize, "File Exceeded Max Size");
                    return status;
                }
                var fileArray = File.ReadAllBytes(filePath);
                var fileMime = MimeTypeManager.GetMimeType(fileArray, file.Name);
                var matchingExtension = MimeTypeManager.GetMimeByExtention(file.Extension);

                if (!_fileExtensions.Contains(file.Extension.Replace(".", string.Empty)))
                {
                    log.Debug($"Failed file extension validation, file extension: {file.Extension}");
                    status.Set(eResponseStatus.FileExtensionNotSupported, "File Extension Not Supported");
                    return status;
                }
                else if (string.IsNullOrEmpty(fileMime) || string.IsNullOrEmpty(matchingExtension) || matchingExtension.ToLower() != fileMime.ToLower())
                {
                    log.Debug($"Failed file mime/content-type validation, expected: {matchingExtension}, actual: {fileMime}");
                    status.Set(eResponseStatus.FileMimeDifferentThanExpected, "File Mime Different Than Expected");
                    return status;
                }
                return status;
            }
            catch (FileNotFoundException ex)
            {
                log.Error($"File not found: {ex}");
                status.Set(eResponseStatus.FileDoesNotExists, "File Does Not Exists");
                return status;
            }
        }
    }

    public class FileSystemHandler : FileHandler
    {
        public string Destination { get; private set; }
        public string PublicUrl { get; private set; }

        protected override void Initialize()
        {
            Destination = ApplicationConfiguration.Current.FileUpload.FileSystem.DestPath.Value;
            PublicUrl = ApplicationConfiguration.Current.FileUpload.FileSystem.PublicUrl.Value;
        }

        protected override GenericResponse<string> Save(string fileName, FileInfo fileInfo, string subDir)
        {
            GenericResponse<string> saveResponse = new GenericResponse<string>();
            var destDir = Path.Combine(Destination, subDir);
            CreateSubDir(destDir);

            var destPath = Path.Combine(destDir, fileName);

            if (File.Exists(destPath))
            {
                saveResponse.SetStatus(eResponseStatus.FileAlreadyExists, string.Format("file:{0} already exists.", fileInfo.Name));
                return saveResponse;
            }

            try
            {
                if (ShouldDeleteSourceFile)
                    File.Move(fileInfo.FullName, destPath);
                else
                    File.Copy(fileInfo.FullName, destPath);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in Save file to FileSystem. fileName:{0}, fileInfo.FullName:{1}, subDir:{2}.",
                                        fileName, fileInfo.FullName, subDir), ex);
                saveResponse.SetStatus(eResponseStatus.ErrorSavingFile, string.Format("Error while save file:{0} to FileSystem", fileName));
                return saveResponse;
            }

            saveResponse.Object = GetUrl(subDir, fileName);
            saveResponse.SetStatus(eResponseStatus.OK);
            return saveResponse;
        }

        protected override Status Delete(string fileURL)
        {
            var deleteResponse = new Status((int)eResponseStatus.Error);

            if (!File.Exists(fileURL))
            {
                deleteResponse.Set(eResponseStatus.FileDoesNotExists, string.Format("file:{0} does not exists.", fileURL));
                return deleteResponse;
            }

            try
            {
                File.Delete(fileURL);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in Save file to FileSystem. fileURL:{0}.", fileURL));
                deleteResponse.Set(eResponseStatus.Error, string.Format("Error while delete file:{0} from FileSystem", fileURL));
                return deleteResponse;
            }

            deleteResponse.Set(eResponseStatus.OK);
            return deleteResponse;
        }

        private static void CreateSubDir(string destDir)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
        }

        protected override GenericResponse<string> GetSubDir(string id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>();
            if (id.Length < 6)
            {
                subDirResponse.SetStatus(eResponseStatus.FileIdNotInCorrectLength, string.Format("file id length is too short, the minimum length is 6. id.Length:{0}", id.Length));
                return subDirResponse;
            }

            subDirResponse.Object = Path.Combine(typeName, id.Substring(0, 3), id.Substring(3, 3));
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        protected override GenericResponse<string> GetSubDir(long id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>();
            if (id < 1)
            {
                subDirResponse.SetStatus(eResponseStatus.FileIdNotInCorrectLength, string.Format("file id value is too small, the value is 1. id:{0}", id));
                return subDirResponse;
            }
            subDirResponse.Object = Path.Combine(typeName, (id / 1000000).ToString(), (id / 1000).ToString());
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        protected override string GetUrl(string subDir, string fileName)
        {
            return string.Format("{0}{1}/{2}", PublicUrl, subDir.Replace("\\", "/"), fileName);
        }
    }
}
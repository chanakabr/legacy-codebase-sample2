using Amazon.S3;
using Amazon.S3.Transfer;
using ApiObjects.Response;
using ConfigurationManager;
using ConfigurationManager.Types;
using KLogMonitor;
using System;
using System.IO;
using System.Reflection;

namespace TVinciShared
{
    public abstract class FileHandler
    {
        protected static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected abstract void Initialize();
        protected abstract GenericResponse<string> Save(string fileName, FileInfo fileInfo, string subDir);
        protected abstract GenericResponse<string> GetSubDir(string id, string typeName);
        protected abstract GenericResponse<string> GetSubDir(long id, string typeName);
        protected abstract string GetUrl(string subDir, string fileName);

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
            ShouldDeleteSourceFile = ApplicationConfiguration.FileUpload.ShouldDeleteSourceFile.Value;
        }

        private static FileHandler GetFileHandlerImpl()
        {
            switch (ApplicationConfiguration.FileUpload.UploadType)
            {
                case FileUploadConfiguration.eFileUploadType.FileSystem:
                    return new FileSystemHandler();
                case FileUploadConfiguration.eFileUploadType.S3:
                    return new S3FileHandler();
            }

            return null;
        }

        public GenericResponse<string> SaveFile(string id, FileInfo fileInfo, Type objectType)
        {
            GenericResponse<string> saveFileResponse = new GenericResponse<string>();
            var validationResponse = Validate(objectType, fileInfo);
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

        public GenericResponse<string> SaveFile(long id, FileInfo fileInfo, Type objectType)
        {
            GenericResponse<string> saveFileResponse = new GenericResponse<string>();
            var validationResponse = Validate(objectType, fileInfo);
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

        private GenericResponse<string> Validate(Type objectType, FileInfo fileInfo = null) 
        {
            var validationStatus = new GenericResponse<string>();
            if (fileInfo != null && !fileInfo.Exists)
            {
                validationStatus.SetStatus(eResponseStatus.FileDoesNotExists, string.Format("file:{0} does not exists.", fileInfo.Name));
                return validationStatus;
            }

            if (objectType == null || !objectType.IsClass || objectType.IsAbstract)
            {
                validationStatus.SetStatus(eResponseStatus.InvalidFileType);
                return validationStatus;
            }

            validationStatus = GetFileObjectTypeName(objectType.Name);
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
        public string AccessKey { get; private set; }
        public string SecretKey { get; private set; }
        public string Region { get; private set; }
        public string BucketName { get; private set; }
        public string Path { get; private set; }
        
        protected override void Initialize()
        {
            AccessKey = ApplicationConfiguration.FileUpload.S3.AccessKey.Value;
            SecretKey = ApplicationConfiguration.FileUpload.S3.SecretKey.Value;
            Region = ApplicationConfiguration.FileUpload.S3.Region.Value;
            BucketName = ApplicationConfiguration.FileUpload.S3.BucketName.Value;
            NumberOfRetries = ApplicationConfiguration.FileUpload.S3.NumberOfRetries.IntValue;
            Path = ApplicationConfiguration.FileUpload.S3.Path.Value;
        }

        protected override GenericResponse<string> Save(string fileName, FileInfo fileInfo, string subDir)
        {
            GenericResponse<string> saveResponse = new GenericResponse<string>();
            for (int i = 0; i < NumberOfRetries; i++)
            {
                using (var client = new AmazonS3Client(AccessKey, SecretKey, Amazon.RegionEndpoint.GetBySystemName(Region)))
                {
                    try
                    {
                        using (var fileTransferUtility = new TransferUtility(client))
                        {
                            var fileTransferUtilityRequest = new TransferUtilityUploadRequest
                            {
                                BucketName = BucketName,
                                FilePath = fileInfo.FullName,
                                Key = fileName
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
            return string.Format("{0}{1}_{2}", Path, subDir, fileName);
        }
    }

    public class FileSystemHandler : FileHandler
    {
        public string Destination { get; private set; }
        public string PublicUrl { get; private set; }
        
        protected override void Initialize()
        {
            Destination = ApplicationConfiguration.FileUpload.FileSystem.DestPath.Value;
            PublicUrl = ApplicationConfiguration.FileUpload.FileSystem.PublicUrl.Value;
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
                subDirResponse.SetStatus(eResponseStatus.FileIdNotInTheRightLength, string.Format("file id length is too short, the minimum length is 6. id.Length:{0}", id.Length));
                return subDirResponse;
            }

            subDirResponse.Object = string.Format("{0}\\{1}\\{2}", typeName, id.Substring(0, 3), id.Substring(3, 3));
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        protected override GenericResponse<string> GetSubDir(long id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>();
            if (id < 1)
            {
                subDirResponse.SetStatus(eResponseStatus.FileIdNotInTheRightLength, string.Format("file id value is too small, the value is 1. id:{0}", id));
                return subDirResponse;
            }
            subDirResponse.Object = string.Format("{0}\\{1}\\{2}", typeName, (long)(id / 1000000), (long)(id / 1000));
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        protected override string GetUrl(string subDir, string fileName)
        {
            return string.Format("{0}{1}/{2}", PublicUrl, subDir.Replace("\\", "/"), fileName);
        }
    }
}
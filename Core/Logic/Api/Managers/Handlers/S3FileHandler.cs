using Amazon.S3;
using Amazon.S3.Transfer;
using ApiLogic.Catalog;
using ApiObjects.Response;
using Phx.Lib.Appconfig.Types;
using Phx.Lib.Log;
using Core.Catalog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using TVinciShared;

namespace ApiLogic.Api.Managers.Handlers
{
    public class S3FileHandler : IFileHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int NumberOfRetries;
        private readonly string Region;
        private readonly string BucketName;
        private readonly string Path;
        private readonly bool ShouldDeleteSourceFile;
        private readonly Func<IAmazonS3> GetAmazonS3Client;
        private readonly Func<IAmazonS3, ITransferUtility> GetTransferUtility;


        private const int m = 1024 * 1024;//Byte to Mb
        private const int _maxFileSize = 15; //Max upload file size : 15MB
        private static List<string> _fileExtensions = new List<string> { "jpeg", "jpg", "png", "tif", "gif", "xls", "xlsx", "csv", "xslm" }
                .Select(x => x.Replace(".", string.Empty)).ToList();//Supported file types

        public S3FileHandler(S3Configuration config, bool shouldDeleteSourceFile, Func<IAmazonS3> getAmazonS3Client = null, Func<IAmazonS3, ITransferUtility> getTransferUtility = null)
        {
            Region = config.Region.Value;
            BucketName = config.BucketName.Value;
            NumberOfRetries = config.NumberOfRetries.Value;
            Path = config.Path.Value;
            ShouldDeleteSourceFile = shouldDeleteSourceFile;

            if (getAmazonS3Client == null)
            {
                GetAmazonS3Client = () => new AmazonS3Client(Amazon.RegionEndpoint.GetBySystemName(Region));
            }
            else
            {
                GetAmazonS3Client = getAmazonS3Client;
            }
            if (getTransferUtility == null)
            {
                GetTransferUtility = GetAmazonS3Client => new TransferUtility(GetAmazonS3Client);
            }
            else
            {
                GetTransferUtility = getTransferUtility;
            }
        }

        public GenericResponse<string> Save(string fileName, OTTBasicFile file, string subDir, string prefix = "")
        {
            GenericResponse<string> saveResponse = new GenericResponse<string>();
            var filePath = prefix.IsNullOrEmpty() ? GetRelativeFilePath(subDir, fileName) : $"{prefix}/{GetRelativeFilePath(subDir, fileName)}";
            for (var i = 0; i < NumberOfRetries; i++)
            {
                using (var client = GetAmazonS3Client())
                {
                    try
                    {
                        using (var fileTransferUtility = GetTransferUtility(client))
                        {
                            var fileTransferUtilityRequest = file.GetTransferUtilityUploadRequest();
                            fileTransferUtilityRequest.BucketName = BucketName;

                            //https://docs.aws.amazon.com/AmazonS3/latest/userguide/object-keys.html
                            if (filePath.Contains('\\'))
                                filePath = filePath.Replace('\\', '/');
                            
                            fileTransferUtilityRequest.Key = filePath;
                            if (fileTransferUtilityRequest.InputStream != null)
                            {
                                fileTransferUtility.Upload(fileTransferUtilityRequest.InputStream, BucketName, filePath);    
                            }
                            else
                            {
                                fileTransferUtility.Upload(fileTransferUtilityRequest);
                            }

                            if (file.ShouldDeleteSourceFile && ShouldDeleteSourceFile)
                                File.Delete(fileTransferUtilityRequest.FilePath);

                            saveResponse.Object = GetUrl(subDir, fileName);
                            
                            log.Warn($"*** The file was saved at S3 in the following path: {saveResponse.Object} ***");
                            
                            saveResponse.SetStatus(eResponseStatus.OK);
                            return saveResponse;
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Warn(string.Format("An Exception was occurred in Save file to S3, attempt: {0}/{1}. fileName: {2}, fileInfo.FullName: {3}, subDir: {4}, bucketName: {5}.",
                                                i + 1, NumberOfRetries, fileName, filePath, subDir, BucketName), ex);
                    }
                }
            }

            log.Error($"Could not save file: {fileName} to S3, all retries failed ({NumberOfRetries})");
            saveResponse.SetStatus(eResponseStatus.ErrorSavingFile, $"Could not save file: {fileName} to S3");
            return saveResponse;
        }

        public GenericResponse<byte[]> Get(string fileName, string fileUrl, string subDir, int groupId = 0, Image image = null)
        {
            log.Debug($"Start download file: [{fileName}] from S3");
            var response = new GenericResponse<byte[]>();
            for (int i = 0; i < NumberOfRetries; i++)
            {
                try
                {
                    using (var client = GetAmazonS3Client())
                    {
                        var filePath = string.Empty;
                        if (image != null)
                        {
                            filePath = fileUrl.Split('/')?.Last();
                        }
                        else//for file
                        {
                            filePath = GetRelativeFilePath(subDir, fileName);
                        }

                        var getObjectRequest = new Amazon.S3.Model.GetObjectRequest
                        {
                            BucketName = BucketName,
                            Key = filePath
                        };

                        using (var _response = client.GetObjectAsync(getObjectRequest).Result)
                        using (Stream stream = _response.ResponseStream)
                        using (var memoryStream = new MemoryStream())
                        {
                            if (_response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                            {
                                response.SetStatus(eResponseStatus.Error, $"Error while download file:{fileName} from S3");
                                return response;
                            }

                            stream.CopyTo(memoryStream);
                            memoryStream.Position = 0;

                            response.SetStatus(eResponseStatus.OK);
                            response.Object = memoryStream.ToArray();
                        }
                    }
                    return response;
                }
                catch (AmazonS3Exception e)
                {
                    // If bucket or object does not exist
                    log.Error($"'AmazonS3Exception': Error encountered, ex:'{e}' when reading object");
                    response.SetStatus(eResponseStatus.Error, $"Error while download file: {fileName} from S3");
                }
                catch (Exception ex)
                {
                    log.Error(string.Format("An Exception was occurred in Get file from S3, attempt: {0}/{1}. fileName:{2}, fileInfo.FullName:{3}, subDir:{4}.",
                                            i + 1, NumberOfRetries, fileName, fileUrl, subDir), ex);
                    response.SetStatus(eResponseStatus.Error, $"Error while download file: {fileName} from S3");
                }
            }
            response.SetStatus(eResponseStatus.Error, $"Could not download file:{fileName} from S3");
            return response;
        }

        public Status Delete(string fileURL)
        {
            var deleteResponse = new Status((int)eResponseStatus.Error);
            for (int i = 0; i < NumberOfRetries; i++)
            {
                using (var client = GetAmazonS3Client())
                {
                    try
                    {
                        using (var fileTransferUtility = GetTransferUtility(client))
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

        public GenericResponse<string> GetSubDir(string id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>()
            {
                Object = typeName
            };
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        public GenericResponse<string> GetSubDir(long id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>()
            {
                Object = typeName
            };
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        public string GetUrl(string subDir, string fileName)
        {
            var withSlash = Path.EndsWith("/") ? "" : "/";
            return $"{Path}{withSlash}{GetRelativeFilePath(subDir, fileName)}";
        }

        protected string GetRelativeFilePath(string subDir, string fileName)
        {
            return $"{subDir}_{fileName}";
        }

        public Status ValidateFileContent(FileInfo file, string filePath)
        {
            var status = Status.Ok;
            try
            {
                if (file.Length > _maxFileSize * m)//check size in bytes
                {
                    log.Warn($"Failed file size validation, file size: {file.Length * m} mb");
                    status.Set(eResponseStatus.FileExceededMaxSize, "File Exceeded Max Size");
                    return status;
                }
                var fileArray = File.ReadAllBytes(filePath);
                var fileMime = MimeTypeManager.GetMimeType(fileArray, file.Name);
                var matchingExtension = MimeTypeManager.GetMimeByExtention(file.Extension);

                if (!_fileExtensions.Contains(file.Extension.Replace(".", string.Empty)))
                {
                    log.Warn($"Failed file extension validation, file extension: {file.Extension}");
                    status.Set(eResponseStatus.FileExtensionNotSupported, "File Extension Not Supported");
                    return status;
                }
                else if (string.IsNullOrEmpty(fileMime) || string.IsNullOrEmpty(matchingExtension) || matchingExtension.ToLower() != fileMime.ToLower())
                {
                    log.Warn($"Failed file mime/content-type validation, expected: {matchingExtension}, actual: {fileMime}");
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
}

using Amazon.S3;
using Amazon.S3.Transfer;
using ApiLogic.Catalog;
using ApiObjects.Response;
using ConfigurationManager;
using ConfigurationManager.Types;
using KLogMonitor;
using Core.Catalog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiLogic.Api.Managers.Handlers;
using TVinciShared;

namespace ApiLogic
{
    public class FileManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<FileManager> LazyInstance = new Lazy<FileManager>(() => new FileManager(GetFileHandlerImpl()), LazyThreadSafetyMode.PublicationOnly);
        public static FileManager Instance => LazyInstance.Value;
        private const string KALTURA = "Kaltura";
        private readonly IFileHandler _handler;

        public FileManager(IFileHandler handler)
        {
            _handler = handler;
        }

        private static IFileHandler GetFileHandlerImpl()
        {
            switch (ApplicationConfiguration.Current.FileUpload.Type.Value)
            {
                case eFileUploadType.FileSystem:
                    return new FileSystemHandler();
                case eFileUploadType.S3:
                    return new S3FileHandler(ApplicationConfiguration.Current.FileUpload.S3, ApplicationConfiguration.Current.FileUpload.ShouldDeleteSourceFile.Value);
            }

            log.Warn($"Couldnt create a FileManager, FileUpload.Type is not configured FileUpload.Type: {ApplicationConfiguration.Current.FileUpload.Type.Value}");
            return null;
        }

        public Status DeleteFile(string fileUrl)
        {
            return _handler.Delete(fileUrl);
        }

        public GenericResponse<string> SaveFile(string id, OTTBasicFile file, string objectTypeName, string prefix = "", string optionalFileName = "")
        {
            var saveFileResponse = new GenericResponse<string>();
            if (file == null)
            {
                log.Warn($"OTTBasicFile is null and can't be used while trying to save file of {objectTypeName} task, taskId {id}");
                saveFileResponse.SetStatus(eResponseStatus.Error, $"OTTBasicFile is null and can't be used");
                return saveFileResponse;
            }

            var fileInfo = file.GetFileInfo();
            var validationResponse = GetFileObjectTypeName(objectTypeName); 

            if (file is OTTFile)
            {
                Validate(file.ShouldDeleteSourceFile, fileInfo, ref validationResponse);
            }
            
            if (validationResponse.HasObject() && validationResponse.IsOkStatusCode())
            {
                saveFileResponse = _handler.GetSubDir(id, validationResponse.Object);
                if (saveFileResponse.HasObject())
                {
                    saveFileResponse = optionalFileName.IsNullOrEmpty() ? _handler.Save(GetFileName(id.ToString(), fileInfo.Extension), file, saveFileResponse.Object, prefix) : 
                                                                          _handler.Save(optionalFileName, file, saveFileResponse.Object, prefix);
                }
            }
            else
            {
                saveFileResponse.SetStatus(validationResponse.Status);
            }

            return saveFileResponse;
        }

        public GenericResponse<string> SaveFile(long id, OTTBasicFile file, string objectTypeName, string prefix = "", string optionalFileName = "")
        {
            var saveFileResponse = new GenericResponse<string>();
            if (file == null)
            {
                log.Warn($"OTTBasicFile is null and can't be used while trying to save file of {objectTypeName} task, taskId {id}");
                saveFileResponse.SetStatus(eResponseStatus.Error, $"OTTBasicFile is null and can't be used");
                return saveFileResponse;
            }

            var fileInfo = file.GetFileInfo();
            var validationResponse = GetFileObjectTypeName(objectTypeName);

            if (file is OTTFile)
            {
                Validate(file.ShouldDeleteSourceFile, fileInfo, ref validationResponse);
            }

            if (validationResponse.HasObject() && validationResponse.IsOkStatusCode())
            {
                saveFileResponse = _handler.GetSubDir(id, validationResponse.Object);
                if (saveFileResponse.HasObject())
                {
                    saveFileResponse = optionalFileName.IsNullOrEmpty() ? _handler.Save(GetFileName(id.ToString(), fileInfo.Extension), file, saveFileResponse.Object, prefix) :
                                                                          _handler.Save(optionalFileName, file, saveFileResponse.Object, prefix);
                }
            }
            else
            {
                saveFileResponse.SetStatus(validationResponse.Status);
            }

            return saveFileResponse;
        }

        public GenericResponse<byte[]> DownloadImage(int groupId, string url, string contentId, Image image)
        {
            log.Debug($"Starting Image Download, Id: {image.Id}, Image Content Id: {image.ContentId}, url: {url}");
            return _handler.Get(contentId, url ?? image.Url, string.Empty, groupId, image);
        }

        public GenericResponse<byte[]> DownloadFile(long id, string fileUrl, string objectTypeName = "KalturaBulkUpload")
        {
            log.Debug($"Starting File Download: {fileUrl}, type: {objectTypeName}");
            var fileResponse = new GenericResponse<byte[]>();
            var validationStatus = GetFileObjectTypeName(objectTypeName);
            if (validationStatus.HasObject())
            {
                var subDir = _handler.GetSubDir(id, validationStatus.Object);
                if (subDir.HasObject())
                {
                    var _extension = $".{fileUrl.Substring(fileUrl.LastIndexOf('.') + 1)}";
                    fileResponse = _handler.Get(GetFileName(id.ToString(), _extension), fileUrl, subDir.Object);
                }
            }
            else
            {
                fileResponse.SetStatus(validationStatus.Status);
            }
            
            return fileResponse;
        }

        private string GetFileName(string id, string fileExtension)
        {
            return $"{id}{fileExtension}";
        }

        private void Validate(bool shouldValidateContent, FileInfo fileInfo, ref GenericResponse<string> validationStatus)
        {
            if (fileInfo != null && !fileInfo.Exists)
            {
                validationStatus.SetStatus(eResponseStatus.FileDoesNotExists, string.Format("file:{0} does not exists.", fileInfo.Name));
                return;
            }

            if (shouldValidateContent && validationStatus.IsOkStatusCode())
            {
                validationStatus.SetStatus(_handler.ValidateFileContent(fileInfo, fileInfo.FullName));
            }
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
}
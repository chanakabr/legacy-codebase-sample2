using ApiLogic.Catalog;
using ApiObjects.Response;
using Phx.Lib.Appconfig;
using Phx.Lib.Log;
using Core.Catalog;
using System;
using System.IO;
using System.Reflection;

namespace ApiLogic.Api.Managers.Handlers
{
    public class FileSystemHandler : IFileHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private readonly string Destination = ApplicationConfiguration.Current.FileUpload.FileSystem.DestPath.Value;
        private readonly string PublicUrl = ApplicationConfiguration.Current.FileUpload.FileSystem.PublicUrl.Value;
        private readonly bool ShouldDeleteSourceFile = ApplicationConfiguration.Current.FileUpload.ShouldDeleteSourceFile.Value;

        public FileSystemHandler()
        {
        }

        public Status ValidateFileContent(FileInfo file, string filePath) { return Status.Ok; }

        public GenericResponse<byte[]> Get(string fileName, string fileUrl, string subDir, int groupId = 0, Image image = null)
        {
            log.Debug($"Start file download: [{fileName}] from FileSystem. (url: {fileUrl})");
            var response = new GenericResponse<byte[]>();
            try
            {
                using (var webClient = new System.Net.WebClient())
                {
                    byte[] fileBytes = webClient.DownloadData(fileUrl);

                    if (fileBytes == null || fileBytes.Length == 0)
                    {
                        response.SetStatus(eResponseStatus.FileDoesNotExists);
                        return response;
                    }
                    response.SetStatus(eResponseStatus.OK);
                    response.Object = fileBytes;
                }
            }
            catch (Exception ex)
            {
                log.Error($"Exception downloading file from 'FileSystemHandler', url: {fileUrl}, error: {ex}");
                response.SetStatus(eResponseStatus.Error, "Error while downloading file");
            }
            return response;
        }

        public Status Delete(string fileURL)
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
                log.Error($"An Exception was occurred in Delete file to FileSystem. fileURL:{fileURL}, error: {ex}");
                deleteResponse.Set(eResponseStatus.Error, $"Error while delete file:{fileURL} from FileSystem, error: {ex}");
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

        public GenericResponse<string> GetSubDir(string id, string typeName)
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

        public GenericResponse<string> GetSubDir(long id, string typeName)
        {
            GenericResponse<string> subDirResponse = new GenericResponse<string>();
            if (id < 1)
            {
                subDirResponse.SetStatus(eResponseStatus.FileIdNotInCorrectLength, $"file id value is too small, need id value bigger than 1. id:{id}");
                return subDirResponse;
            }
            subDirResponse.Object = Path.Combine(typeName, (id / 1000000).ToString(), (id / 1000).ToString());
            subDirResponse.SetStatus(eResponseStatus.OK);
            return subDirResponse;
        }

        public string GetUrl(string subDir, string fileName)
        {
            return string.Format("{0}{1}/{2}", PublicUrl, subDir.Replace("\\", "/"), fileName);
        }

        public GenericResponse<string> Save(string fileName, OTTBasicFile file, string subDir, string prefix = "")
        {
            GenericResponse<string> saveResponse = new GenericResponse<string>();
            var destDir = Path.Combine(Destination, subDir);

            try
            {
                CreateSubDir(destDir);
            }
            catch (Exception ex)
            {
                log.Error($"Save file: {fileName}, destDir: {destDir} error: {ex}", ex);
            }

            var destPath = Path.Combine(destDir, fileName);

            if (File.Exists(destPath))
            {
                saveResponse.SetStatus(eResponseStatus.FileAlreadyExists, string.Format("file:{0} already exists.", file.Name));
                return saveResponse;
            }

            try
            {
                file.SaveToFileSystem(destPath, ShouldDeleteSourceFile);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("An Exception was occurred in Save file to FileSystem. fileName:{0}, subDir:{1}.",
                                        fileName, subDir), ex);
                saveResponse.SetStatus(eResponseStatus.ErrorSavingFile, string.Format("Error while save file:{0} to FileSystem", fileName));
                return saveResponse;
            }

            saveResponse.Object = GetUrl(subDir, fileName);
            
            log.Warn($"*** The file was saved at File system in the following path: {saveResponse.Object} ***");

            saveResponse.SetStatus(eResponseStatus.OK);
            return saveResponse;
        }
    }
}

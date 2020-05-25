using KLogMonitor;
using System.IO;
using System.Reflection;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Upload;
using ApiLogic;
using System.Collections.Generic;
using ApiObjects.Response;
using System;
using System.Linq;

namespace WebAPI.Managers
{
    public class UploadTokenManager
    {
        private const string CB_SECTION_NAME = "tokens";
        private const string UPLOAD_TOKEN_KEY_FORMAT = "upload_token_{0}";
        private const int m = 1024 * 1024;//Byte  to Mb
        private const int _maxFileSize = 15; //Max upload file size : 15MB
        private static List<string> _fileExtensions = new List<string> { "jpeg", "jpg", "png", "tif", "gif", "xls", "xlsx", "csv", "xslm" }
                .Select(x => x.Replace(".", string.Empty)).ToList();//Supported file types

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static CouchbaseManager.CouchbaseManager cbManager = new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);

        internal static KalturaUploadToken AddUploadToken(int groupId)
        {
            Group group = GroupsManager.GetGroup(groupId);
            string UploadTokenKeyFormat = UPLOAD_TOKEN_KEY_FORMAT;
            int UploadTokenExpirySeconds = 86400;
            if (!string.IsNullOrEmpty(group.UploadTokenKeyFormat))
            {
                UploadTokenKeyFormat = group.UploadTokenKeyFormat;
            }

            if (group.UploadTokenExpirySeconds > 0)
            {
                UploadTokenExpirySeconds = group.UploadTokenExpirySeconds;
            }

            // save in CB
            UploadToken cbUploadToken = new UploadToken(groupId);
            string uploadTokenCbKey = string.Format(UploadTokenKeyFormat, cbUploadToken.UploadTokenId);
            if (!cbManager.Add(uploadTokenCbKey, cbUploadToken, (uint)UploadTokenExpirySeconds, true))
            {
                log.Error("AddUploadToken: Failed to store upload token");
                throw new InternalServerErrorException();
            }

            return new KalturaUploadToken(cbUploadToken);
        }

        internal static UploadToken GetUploadToken(string id, int groupId)
        {
            Group group = GroupsManager.GetGroup(groupId);

            string UploadTokenKeyFormat = UPLOAD_TOKEN_KEY_FORMAT;
            if (!string.IsNullOrEmpty(group.UploadTokenKeyFormat))
            {
                UploadTokenKeyFormat = group.UploadTokenKeyFormat;
            }

            string uploadTokenCbKey = string.Format(UploadTokenKeyFormat, id);
            UploadToken cbUploadToken = cbManager.Get<UploadToken>(uploadTokenCbKey, true);
            if (cbUploadToken == null)
            {
                log.ErrorFormat("GetUploadToken: failed to get UploadToken from CB, key = {0}", uploadTokenCbKey);
                throw new NotFoundException(NotFoundException.OBJECT_ID_NOT_FOUND, "upload-token", id);
            }
            else
            {
                log.DebugFormat("GetUploadToken: FileUrl: {0}, FileSize: {1}, UploadTokenId: {2}", cbUploadToken.FileUrl, cbUploadToken.FileSize, cbUploadToken.UploadTokenId);
            }

            return cbUploadToken;
        }

        internal static KalturaUploadToken UploadUploadToken(string id, string path, int groupId)
        {
            log.DebugFormat("UploadUploadToken function params -> Id: {0}, Path: {1}, GroupId: {2}", id, path, groupId);

            UploadToken cbUploadToken = GetUploadToken(id, groupId);

            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo == null)
            {
                log.Error("UploadUploadToken: Failed to create file info, Path: " + path);
                throw new InternalServerErrorException();
            }

            cbUploadToken.FileSize = fileInfo.Length;

            ValidateFile(fileInfo, path);

            var saveFileResponse = FileHandler.Instance.SaveFile(id, fileInfo, "KalturaUploadToken");
            if (saveFileResponse == null)
            {
                log.Error("UploadUploadToken: Failed to get saveFileResponse");
                throw new InternalServerErrorException();
            }

            if (!saveFileResponse.HasObject())
            {
                throw new ClientException(saveFileResponse.Status);
            }

            log.DebugFormat("UploadUploadToken save file response -> Object: {0}", saveFileResponse.Object);

            cbUploadToken.FileUrl = saveFileResponse.Object;
            cbUploadToken.Status = KalturaUploadTokenStatus.FULL_UPLOAD;

            Group group = GroupsManager.GetGroup(groupId);

            string UploadTokenKeyFormat = UPLOAD_TOKEN_KEY_FORMAT;
            int UploadTokenExpirySeconds = 86400;
            if (!string.IsNullOrEmpty(group.UploadTokenKeyFormat))
            {
                UploadTokenKeyFormat = group.UploadTokenKeyFormat;
            }

            if (group.UploadTokenExpirySeconds > 0)
            {
                UploadTokenExpirySeconds = group.UploadTokenExpirySeconds;
            }

            // save in CB
            string uploadTokenCbKey = string.Format(UploadTokenKeyFormat, cbUploadToken.UploadTokenId);
            if (!cbManager.Set(uploadTokenCbKey, cbUploadToken, (uint)UploadTokenExpirySeconds, true))
            {
                log.Error("UploadUploadToken: Failed to store upload token");
                throw new InternalServerErrorException();
            }

            return new KalturaUploadToken(cbUploadToken);
        }

        private static void ValidateFile(FileInfo file, string filePath)
        {
            try
            {
                var fileArray = File.ReadAllBytes(filePath);
                var fileMime = MimeType.GetMimeType(fileArray, file.Name);
                var matchingExtension = MimeType.GetMimeByExtention(file.Extension);

                if (file.Length > _maxFileSize * m)//check size in bytes
                {
                    log.Debug($"Failed file size validation, file size: {file.Length * m} mb");
                    throw new ClientException((int)eResponseStatus.FileExceededMaxSize, "File Exceeded Max Size");
                }
                else if (!_fileExtensions.Contains(file.Extension.Replace(".", string.Empty)))
                {
                    log.Debug($"Failed file extension validation, file extension: {file.Extension}");
                    throw new ClientException((int)eResponseStatus.FileExtensionNotSupported, "File Extension Not Supported");
                }
                else if (string.IsNullOrEmpty(fileMime) || matchingExtension.ToLower() != fileMime.ToLower())
                {
                    log.Debug($"Failed file mime/content-type validation, expected: {matchingExtension}, actual: {fileMime}");
                    throw new ClientException((int)eResponseStatus.FileMimeDifferentThanExpected, "File Mime Different Than Expected");
                }
            }
            catch (FileNotFoundException ex)
            {
                log.Error($"File not found: {ex}");
                throw ex;
            }
            catch (Exception ex)
            {
                log.Error($"Error while validating File: {file} ex: {ex}");
                throw ex;
            }
        }
    }
}
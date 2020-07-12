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
using WebAPI.ObjectsConvertor;
using ApiLogic.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Managers
{
    public class UploadTokenManager
    {
        private const string CB_SECTION_NAME = "tokens";
        private const string UPLOAD_TOKEN_KEY_FORMAT = "upload_token_{0}";
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

        internal static KalturaUploadToken UploadUploadToken(string id, KalturaOTTFile fileData, int groupId)
        {
            log.DebugFormat("UploadUploadToken function params -> Id: {0}, filename: {1}, GroupId: {2}", id, fileData.name, groupId);

            UploadToken cbUploadToken = GetUploadToken(id, groupId);
            OTTBasicFile file = fileData.ConvertToOttFileType();       

            long.TryParse(id, out long _id);
            
            if (file is OTTFile)
            {
                cbUploadToken.FileSize = new FileInfo(fileData.path).Length;
            }
            var saveFileResponse = file.SaveFile(_id, "KalturaUploadToken");            
                                  
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
    }
}
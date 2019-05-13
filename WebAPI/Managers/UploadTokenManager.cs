using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using ConfigurationManager;
using ConfigurationManager.Types;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Upload;
using Exception = System.Exception;
using UriBuilder = System.UriBuilder;
using WebAPI.Utils;
using ApiLogic;

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
            if (!cbManager.Add(uploadTokenCbKey, cbUploadToken, (uint) UploadTokenExpirySeconds, true))
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

            return cbUploadToken;
        }

        internal static KalturaUploadToken UploadUploadToken(string id, string path, int groupId)
        {
            UploadToken cbUploadToken = GetUploadToken(id, groupId);
            FileInfo fileInfo = new FileInfo(path);
            cbUploadToken.FileSize = fileInfo.Length;
            
            var saveFileResponse = FileHandler.Instance.SaveFile(id, fileInfo, "KalturaUploadToken");
            if (!saveFileResponse.HasObject())
            {
                throw new ClientException(saveFileResponse.Status.Code, saveFileResponse.Status.Message);
            }

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
            if (!cbManager.Set(uploadTokenCbKey, cbUploadToken, (uint) UploadTokenExpirySeconds, true))
            {
                log.Error("UploadUploadToken: Failed to store upload token");
                throw new InternalServerErrorException();
            }

            return new KalturaUploadToken(cbUploadToken);
        }
    }
}
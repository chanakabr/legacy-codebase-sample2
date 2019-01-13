using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using ConfigurationManager;
using WebAPI.ClientManagers;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Upload;
using UriBuilder = System.UriBuilder;

namespace WebAPI.Managers
{
    public class UploadManager
    {
        private const string CB_SECTION_NAME = "tokens";
        private const string UPLOAD_TOKEN_KEY_FORMAT = "upload_token_{0}";

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static CouchbaseManager.CouchbaseManager cbManager =
            new CouchbaseManager.CouchbaseManager(CB_SECTION_NAME);

        internal static KalturaUploadToken AddUploadToken(KalturaUploadToken uploadToken, int groupId)
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
                log.ErrorFormat("AddUploadToken: Failed to store upload token");
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

            if (ShouldWriteToS3())
                cbUploadToken.FileUrl = S3Uploader.Instance.UploadFile(fileInfo, id);
            else
                cbUploadToken.FileUrl = FileSystemUploader.Instance.UploadFile(fileInfo, id);

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
                log.ErrorFormat("UploadUploadToken: Failed to store upload token");
                throw new InternalServerErrorException();
            }

            return new KalturaUploadToken(cbUploadToken);
        }

        internal static bool ShouldWriteToS3()
        {
            // get from TCM
            return ApplicationConfiguration.UploadFilesToS3.Value;
        }
    }

    public abstract class BaseUploader
    {
        protected abstract void Initialize();
        protected abstract string Upload(FileInfo fileInfo, string id);

        protected BaseUploader()
        {
            Initialize();
        }

        public string UploadFile(FileInfo fileInfo, string id)
        {
            if (!fileInfo.Exists)
                throw new InternalServerErrorException();

            return Upload(fileInfo, id);
        }
    }

    public class S3Uploader : BaseUploader
    {
        private static S3Uploader _instance;

        private S3Uploader()
            :base()
        {
            
        }

        public static S3Uploader Instance
        {
            get { return _instance ?? (_instance = new S3Uploader()); }
        }

        protected override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override string Upload(FileInfo fileInfo, string id)
        {
            throw new NotImplementedException();
        }
    }

    public class FileSystemUploader : BaseUploader
    {
        private static FileSystemUploader _instance;

        public string Destination { get; set; }
        public string PublicUrl { get; set; }

        private FileSystemUploader()
            : base()
        {
        }

        public static FileSystemUploader Instance
        {
            get { return _instance ?? (_instance = new FileSystemUploader()); }
        }

        protected override void Initialize()
        {
            Destination = ApplicationConfiguration.FileSystemUploader.DestPath.Value;
            PublicUrl = ApplicationConfiguration.FileSystemUploader.PublicUrl.Value;
        }

        protected override string Upload(FileInfo fileInfo, string id)
        {
            var subDir = GetSubDir(id);
            var destDir = Path.Combine(Destination, subDir);
            CreateSubDir(destDir);

            var fileName = id + fileInfo.Extension;
            var destPath = Path.Combine(destDir, fileName);

            if (File.Exists(destPath))
                throw new InternalServerErrorException();

            try
            {
                File.Move(fileInfo.FullName, destPath);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException();
            }

            return new Uri(new Uri(PublicUrl), fileName).AbsoluteUri;
        }

        private static void CreateSubDir(string destDir)
        {
            if (!Directory.Exists(destDir))
                Directory.CreateDirectory(destDir);
        }

        private static string GetSubDir(string id)
        {
            const int CharacterNumber = 8;

            if (id.Length < CharacterNumber)
                throw new InternalServerErrorException();

            var sb = new StringBuilder(CharacterNumber);

            for (int i = 0; i < CharacterNumber; i = i + 2)
            {
                sb.Append(id.Substring(i, 2) + "\\");
            }

            return sb.ToString();
        }
    }
}
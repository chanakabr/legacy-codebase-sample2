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
            

            BaseUploader uploader = null;
            if (ShouldWriteToS3())
                uploader = new S3Uploader(fileInfo);
            else
                uploader = new FileSystemUploader(fileInfo);

            var fileUrl = uploader.UploadFile(id);

            cbUploadToken.FileUrl = fileUrl;
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
            return false;
        }
    }

    public abstract class BaseUploader
    {
        protected abstract void Initialize();
        protected abstract string Upload(string id);

        protected FileInfo SourceFile { get; private set; }

        protected string SourceFullPath
        {
            get { return SourceFile.FullName; }
        }

        protected string FileExtension
        {
            get { return SourceFile.Extension; }
        }

        protected BaseUploader(FileInfo fileInfo)
        {
            SourceFile = fileInfo;
            Initialize();
        }

        public string UploadFile(string id)
        {
            if (!SourceFile.Exists)
                throw new InternalServerErrorException();

            return Upload(id);
        }
    }

    public class S3Uploader : BaseUploader
    {
        protected string DirectoryPath
        {
            get { return string.Empty; }
        }

        public S3Uploader(FileInfo fileInfo)
            : base(fileInfo)
        {
        }

        public static string BuildPublicUrl(string resource2Token)
        {
            throw new NotImplementedException();
        }

        protected override void Initialize()
        {
            throw new NotImplementedException();
        }

        protected override string Upload(string id)
        {
            throw new NotImplementedException();
        }
    }

    public class FileSystemUploader : BaseUploader
    {
        public static string Destination { get; set; }
        public static string DestinationUrl { get; set; }

        public FileSystemUploader(FileInfo fileInfo)
            : base(fileInfo)
        {
        }

        protected override void Initialize()
        {
            Destination = @"C:\temp\dest";
            DestinationUrl = "http://localhost/pics/";
        }

        protected override string Upload(string id)
        {
            if (!Directory.Exists(Destination))
                throw new InternalServerErrorException();

            var fileName = id + FileExtension;
            var destPath = Path.Combine(Destination, fileName);

            if (File.Exists(destPath))
                throw new InternalServerErrorException();

            try
            {
                File.Copy(SourceFullPath, destPath);
            }
            catch (Exception e)
            {
                throw new InternalServerErrorException();
            }

            return new Uri(new Uri(DestinationUrl), fileName).AbsoluteUri;
        }
    }
}
using ApiObjects.Billing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class ImageUploadData : BaseCeleryData
    {
        public const string TASK = "distributed_tasks.process_image_upload";

        private string imageId;
        private int version;
        private string sourcePath;
        private long rowId;
        private string imageServerUrl;
        private eMediaType mediaType;

        public ImageUploadData(int groupId, string imageId, int version, string sourcePath, long rowId, string imageServerUrl, eMediaType mediaType) :
            base(// id = guid
                 Guid.NewGuid().ToString(),
                // task = const
                 TASK)
        {
            // Basic member initialization
            this.GroupId = groupId;
            this.imageId = imageId;
            this.version = version;
            this.sourcePath = sourcePath;
            this.rowId = rowId;
            this.imageServerUrl = imageServerUrl;
            this.mediaType = mediaType;
            this.RecoveryMessageId = BuildMessageRecoveryKey(mediaType, rowId, version);

            this.args = new List<object>()
            {
                groupId,
                imageId,
                version,
                sourcePath,
                rowId,
                imageServerUrl,
                (int)mediaType
            };
        }

        // method to build the recovery key (others need to build the key as well)
        public static string BuildMessageRecoveryKey(eMediaType mediaType, long rowId, int version)
        {
            return (int)mediaType + "_" + rowId + "_" + version;
        }
    }
}

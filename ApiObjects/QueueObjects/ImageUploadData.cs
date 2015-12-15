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

        public ImageUploadData(int groupId, string imageId, int version, string sourcePath) :
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

            this.args = new List<object>()
            {
                groupId,
                imageId,
                version,
                sourcePath
            };
        }
    }
}

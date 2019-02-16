using ApiObjects;
using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog.CatalogManagement
{
    public class Image
    {
        public long Id { get; set; }

        public int Version { get; set; }

        public long ImageTypeId { get; set; }

        public long ImageObjectId { get; set; }

        public eAssetImageType ImageObjectType { get; set; }

        public eTableStatus Status { get; set; }

        public string Url { get; set; }

        public string ContentId { get; set; }

        public bool? IsDefault { get; set; }

        /// <summary>
        /// only used for backward compatibility of pic sizes
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// only used for backward compatibility of pic sizes
        /// </summary>
        public int Width { get; set; }

        public string RatioName { get; set; }

        public ImageReferenceTable ReferenceTable { get; set; }

        public long ReferenceId { get; set; }

        public Image() { }

        public Image(Image imageToCopy)
        {
            this.Id = imageToCopy.Id;
            this.ContentId = imageToCopy.ContentId;
            this.Height = imageToCopy.Height;
            this.ImageObjectId = imageToCopy.ImageObjectId;
            this.ImageObjectType = imageToCopy.ImageObjectType;
            this.ImageTypeId = imageToCopy.ImageTypeId;
            this.IsDefault = imageToCopy.IsDefault;
            this.RatioName = imageToCopy.RatioName;
            this.ReferenceId = imageToCopy.ReferenceId;
            this.ReferenceTable = imageToCopy.ReferenceTable;
            this.Status = imageToCopy.Status;
            this.Url = imageToCopy.Url;
            this.Version = imageToCopy.Version;
            this.Width = imageToCopy.Width;
        }
    }
}

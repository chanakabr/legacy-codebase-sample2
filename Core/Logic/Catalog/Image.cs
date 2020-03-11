using ApiObjects;
using ApiObjects.Response;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog
{
    [Serializable]
    public class Image
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Version")]
        public int Version { get; set; }

        [JsonProperty("ImageTypeId")]
        public long ImageTypeId { get; set; }

        [JsonProperty("ImageObjectId")]
        public long ImageObjectId { get; set; }

        [JsonProperty("ImageObjectType")]
        public eAssetImageType ImageObjectType { get; set; }

        [JsonProperty("Status")]
        public eTableStatus Status { get; set; }

        [JsonProperty("Url")]
        public string Url { get; set; }

        [JsonProperty("ContentId")]
        public string ContentId { get; set; }

        [JsonProperty("IsDefault")]
        public bool? IsDefault { get; set; }

        /// <summary>
        /// only used for backward compatibility of pic sizes
        /// </summary>
        [JsonProperty("Height")]
        public int Height { get; set; }

        /// <summary>
        /// only used for backward compatibility of pic sizes
        /// </summary>
        [JsonProperty("Width")]
        public int Width { get; set; }

        [JsonProperty("RatioName")]
        public string RatioName { get; set; }

        [JsonProperty("ReferenceTable")]
        public ImageReferenceTable ReferenceTable { get; set; }

        [JsonProperty("ReferenceId")]
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
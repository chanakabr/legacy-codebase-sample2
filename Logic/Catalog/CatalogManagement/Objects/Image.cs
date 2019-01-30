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
        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public long Id { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public int Version { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public long ImageTypeId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public long ImageObjectId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public eAssetImageType ImageObjectType { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public eTableStatus Status { get; set; }

        [ExcelTemplateAttribute(PropertyValueRequired = true)]
        public string Url { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public string ContentId { get; set; }

        [ExcelTemplateAttribute(IgnoreWhenGeneratingTemplate = true)]
        public bool? IsDefault { get; set; }

        // only used for backward compatibility of pic sizes
        public int Height { get; set; }

        // only used for backward compatibility of pic sizes
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

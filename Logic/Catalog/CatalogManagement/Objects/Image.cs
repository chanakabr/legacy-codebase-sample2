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
    }
}

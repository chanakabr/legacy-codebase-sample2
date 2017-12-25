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
    }
}

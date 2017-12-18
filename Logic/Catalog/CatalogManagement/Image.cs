using ApiObjects;
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

        public string Version { get; set; }

        public long ImageTypeId { get; set; }

        public long ImageObjectId { get; set; }

        public ImageObjectType ImageObjectType { get; set; }

        public ImageStatus Status { get; set; }

        public string Url { get; set; }

        public string SystemName { get; set; }
    }
}

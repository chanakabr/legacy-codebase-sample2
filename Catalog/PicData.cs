using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Catalog
{
    public class PicData
    {
        public long PicId { get; set; }

        public int Version { get; set; }

        public int RatioId { get; set; }

        public string BaseUrl { get; set; }
    }
}

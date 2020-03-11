using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Catalog
{
    public class PicData
    {
        public long PicId { get; set; }

        public int Version { get; set; }

        public string Ratio { get; set; }

        public int RatioId { get; set; }

        public string BaseUrl { get; set; }

        public int GroupId { get; set; }
    }
}

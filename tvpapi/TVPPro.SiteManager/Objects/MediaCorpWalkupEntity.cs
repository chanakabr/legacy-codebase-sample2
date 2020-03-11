using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
   public class MediaCorpWalkupEntity
    {
        public string PostalCode { get; set; }
        public string BuildingNumber { get; set; }
        public string StreetKey { get; set; }
        public char WalkupIndicator { get; set; }
    }
}

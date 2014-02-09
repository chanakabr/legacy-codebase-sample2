using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    public class MediaCorpAddressEntity
    {
        public string BuildingNumber { get; set; }
        public string StreetName { get; set; }
        public string BuildingName { get; set; }
        public char AddressType { get; set; }
        public char WalkupIndicator { get; set; }
        public MediaCorpWalkupEntity[] WalkupArray { get; set; }

        public MediaCorpAddressEntity()
        {
            BuildingNumber = string.Empty;
            StreetName = string.Empty;
            BuildingName = string.Empty;
            
        }
    }
}

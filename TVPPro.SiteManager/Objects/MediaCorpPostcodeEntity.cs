using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPPro.SiteManager.Objects
{
    public class MediaCorpPostcodeEntity
    {
        public string PostalCode { get; set; }
        public char AddressType { get; set; }
        public string BuildingNumber { get; set; }
        public string StreetKey { get; set; }
        public string BuildingKey { get; set; }
    }
}

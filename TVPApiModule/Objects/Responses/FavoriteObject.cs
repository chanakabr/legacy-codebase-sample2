using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FavoriteObject
    {
        public string deviceUDID { get; set; }

        public string type { get; set; }

        public string itemCode { get; set; }

        public string siteUserGUID { get; set; }

        public DateTime updateDate { get; set; }

        public string extraData { get; set; }

        public int id { get; set; }

        public string deviceName { get; set; }

        public int domainID { get; set; }

        public int is_channel { get; set; }

    }
}

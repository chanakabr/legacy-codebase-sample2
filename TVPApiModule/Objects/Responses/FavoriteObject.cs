using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class FavoriteObject
    {
        public string device_udid { get; set; }

        public string type { get; set; }

        public string item_code { get; set; }

        public string site_user_guid { get; set; }

        public DateTime update_date { get; set; }

        public string extra_data { get; set; }

        public int id { get; set; }

        public string device_name { get; set; }

        public int domain_id { get; set; }

        public int is_channel { get; set; }

    }
}

using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class PermittedMediaContainer
    {
        public int media_id { get; set; }

        public int media_file_id { get; set; }

        public int max_uses { get; set; }

        public int current_uses { get; set; }

        public DateTime end_date { get; set; }

        public DateTime current_date { get; set; }

        public DateTime purchase_date { get; set; }

        public PaymentMethod purchase_method { get; set; }

        public string device_udid { get; set; }

        public string device_name { get; set; }

        public bool is_cancel_window { get; set; }
    }
}

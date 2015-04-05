using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class PermittedCollectionContainer
    {
        public string collection_code { get; set; }

        public System.DateTime end_date { get; set; }

        public System.DateTime current_date { get; set; }

        public System.DateTime last_view_date { get; set; }

        public System.DateTime purchase_date { get; set; }

        public int collection_purchase_id { get; set; }

        public PaymentMethod payment_method { get; set; }

        public string device_udid { get; set; }

        public string device_name { get; set; }

        public bool is_cancel_window { get; set; }
    }
}

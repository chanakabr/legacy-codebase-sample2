using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class PermittedSubscriptionContainer
    {
        public string subscription_code { get; set; }

        public int max_uses { get; set; }

        public int current_uses { get; set; }

        public DateTime end_date { get; set; }

        public DateTime current_date { get; set; }

        public DateTime last_view_date { get; set; }

        public DateTime purchase_date { get; set; }

        public DateTime next_renewal_date { get; set; }

        public bool recurring_status { get; set; }

        public bool is_sub_renewable { get; set; }

        public int subscription_purchase_id { get; set; }

        public PaymentMethod payment_method { get; set; }

        public string device_udid { get; set; }

        public string device_name { get; set; }

        public bool is_cancel_window { get; set; }
    }

}

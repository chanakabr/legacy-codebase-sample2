using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class Domain
    {
        public string name { get; set; }

        public string description { get; set; }

        public string co_guid { get; set; }

        public int domain_id { get; set; }

        public int group_id { get; set; }

        public int limit { get; set; }

        public int device_limit { get; set; }

        public int user_limit { get; set; }

        public int concurrent_limit { get; set; }

        public int status { get; set; }

        public int is_active { get; set; }

        public int[] users_ids { get; set; }

        public DeviceContainer[] device_families { get; set; }

        public int[] master_guids { get; set; }

        public int[] pending_users_ids { get; set; }

        public int[] default_users_ids { get; set; }

        public DomainStatus domain_status { get; set; }

        public int frequency_flag { get; set; }

        public System.DateTime next_action_freq { get; set; }

        public HomeNetwork[] home_networks { get; set; }

        public DomainRestriction domain_restriction { get; set; }

        public DateTime next_user_action_freq { get; set; }

        public int sso_operator_id { get; set; }
    }
}

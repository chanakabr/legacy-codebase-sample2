using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class User
    {
        public UserBasicData basic_data { get; set; }

        public UserDynamicData dynamic_data { get; set; }

        public string site_guid { get; set; }

        public int domain_id { get; set; }

        public bool is_domain_master { get; set; }

        public UserState user_state { get; set; }

        public int sso_operator_id { get; set; }
    }
}

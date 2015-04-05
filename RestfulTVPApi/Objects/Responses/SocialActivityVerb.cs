using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class SocialActivityVerb
    {
        public string social_action_id { get; set; }
        public int action_type { get; set; }
        public string action_name { get; set; }
        public int rate_value { get; set; }
        public List<ActionProperties> properties { get; set; }
    }
}

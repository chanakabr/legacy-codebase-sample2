using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class SocialActivitySubject
    {
        public string actor_site_guid { get; set; }
        public string actor_pic_url { get; set; }
        public string actor_tvinci_username { get; set; }
        public int group_id { get; set; }
        public string device_udid { get; set; }
    }
}

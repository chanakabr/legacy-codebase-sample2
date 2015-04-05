using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class SocialActivityDoc
    {
        public string id { get; set; }
        public string doc_owner_site_guid { get; set; }        
        public int social_platform { get; set; }
        public string doc_type { get; set; }
        public long create_date { get; set; }        
        public long last_update { get; set; }        
        public bool is_active { get; set; }        
        public bool permit_sharing { get; set; }
        public SocialActivityObject activity { get; set; }        
        public SocialActivitySubject subject { get; set; }        
        public SocialActivityVerb verbVerb { get; set; }
    }
}

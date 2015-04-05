using RestfulTVPApi.Objects.Responses.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RestfulTVPApi.Objects.Responses
{
    public class CampaignActionInfo
    {

        public VoucherReceipentInfo[] voucher_receipents { get; set; }
       
        public SocialInviteInfo social_invite_info { get; set; }
        
        public int media_id { get; set; }
        
        public int site_guid { get; set; }
       
        public string media_link { get; set; }
        
        public string sender_name { get; set; }

        public string sender_email { get; set; }
        
        public CampaignActionResult status { get; set; }
    }

}

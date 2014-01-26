using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class CampaignActionInfo
    {

        public VoucherReceipentInfo[] voucherReceipents { get; set; }
       
        public SocialInviteInfo socialInviteInfo { get; set; }
        
        public int mediaID { get; set; }
        
        public int siteGuid { get; set; }
       
        public string mediaLink { get; set; }
        
        public string senderName { get; set; }

        public string senderEmail { get; set; }
        
        public CampaignActionResult status { get; set; }
    }

}

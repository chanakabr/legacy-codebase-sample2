using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TVPApiModule.Objects.Responses
{
    public class CampaignActionInfo
    {

        public VoucherReceipentInfo[] m_voucherReceipents { get; set; }
       
        public SocialInviteInfo m_socialInviteInfo { get; set; }
        
        public int m_mediaID { get; set; }
        
        public int m_siteGuid { get; set; }
       
        public string m_mediaLink { get; set; }
        
        public string m_senderName { get; set; }

        public string m_senderEmail { get; set; }
        
        public CampaignActionResult m_status { get; set; }
    }


    public class VoucherReceipentInfo
    {
        public string m_emailAdd { get; set; }
       
        public string m_receipentName { get; set; }
    }


    public class SocialInviteInfo
    {
        public string m_hashCode { get; set; }
    }


    public enum CampaignActionResult
    {

        /// <remarks/>
        OK,

        /// <remarks/>
        ERROR,
    }
}

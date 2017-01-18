using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.ConditionalAccess
{
    public class CampaignActionInfo
    {

        public struct SocialInviteInfo
        {
            public string m_hashCode;
        }

        public struct VoucherReceipentInfo
        {
            public string m_emailAdd;
            public string m_receipentName;
            

            public VoucherReceipentInfo(string emailAdd, string receipentName)
            {
                m_emailAdd = emailAdd;
                m_receipentName = receipentName;
               
            }
        }

        public void AddReceipent(string name, string email)
        {
            if (m_voucherReceipents == null)
            {
                m_voucherReceipents = new List<VoucherReceipentInfo>();
            }
            m_voucherReceipents.Add(new VoucherReceipentInfo(name, email));
        }

        public List<VoucherReceipentInfo> m_voucherReceipents;
        public SocialInviteInfo m_socialInviteInfo;
        public int m_mediaID;
        public int m_siteGuid;
        public string m_mediaLink;
        public string m_senderName;
        public string m_senderEmail;
        public CampaignActionResult m_status;
    }
}

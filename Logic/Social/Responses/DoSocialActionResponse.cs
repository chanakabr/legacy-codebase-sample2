using ApiObjects.Social;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Social.Responses
{
    public class DoSocialActionResponse : BaseSocialResponse
    {
        public DoSocialActionResponse()
            : base()
        {
        }
        public DoSocialActionResponse(int nStatus) :
            base(nStatus)
        {
        }

        public DoSocialActionResponse(SocialActionResponseStatus nStatusIntern, SocialActionResponseStatus nStatusExten) 
        {
            m_eActionResponseStatusIntern = nStatusIntern;
            m_eActionResponseStatusExtern = nStatusExten;
        }

        public DoSocialActionResponse(int nStatus, SocialActionResponseStatus nStatusIntern, SocialActionResponseStatus nStatusExten) : base(nStatus)
        {
            m_eActionResponseStatusIntern = nStatusIntern;
            m_eActionResponseStatusExtern = nStatusExten;
        }

        public SocialActionResponseStatus m_eActionResponseStatusIntern { get; set; }
        public SocialActionResponseStatus m_eActionResponseStatusExtern { get; set; }
    }
}

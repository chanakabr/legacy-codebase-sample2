using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;

namespace Core.Social.Responses
{
    public class BaseSocialPrivacyReponse : BaseSocialResponse
    {
        public BaseSocialPrivacyReponse(int nStatus) :
            base(nStatus)
        {
        }

        public eSocialPrivacy m_ePrivacy { get; set; }
    }
}

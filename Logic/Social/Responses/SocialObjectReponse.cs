using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Social.Responses
{
    public class SocialObjectReponse : BaseSocialResponse
    {
        public SocialObjectReponse(int nStatus) :
            base(nStatus)
        {
        }

        public SocialObjectReponse() : base(0) { }

        public string sID { get; set; }
    }

}

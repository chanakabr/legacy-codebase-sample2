using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Social.Responses
{
    public class SocialActionQueryResponse : BaseSocialResponse
    {
        public SocialActionQueryResponse(int nStatus)
            : base(nStatus)
        {
            m_lUserActionObj = new List<ApiObjects.SocialActivityDoc>();
        }

        public List<ApiObjects.SocialActivityDoc> m_lUserActionObj { get; set; }
        public int TotalCount { get; set; }
    }
}

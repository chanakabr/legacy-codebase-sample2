using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Social.Responses;
using ApiObjects;

namespace Core.Social.Requests
{
    public interface IBaseSocialRequest
    {
        SocialPlatform m_eSocialPlatform { get; set; }
        BaseSocialResponse GetResponse(int nGroupID);
        string m_sFunctionName { get; set; }
    }
}

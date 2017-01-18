using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Social.Responses;
using ApiObjects;

namespace Core.Social.Requests
{

    public abstract class SocialBaseRequestWrapper : IBaseSocialRequest
    {
        public static readonly int STATUS_OK = 200;
        public static readonly int STATUS_FAIL = 400;

        public SocialBaseRequestWrapper() { }

        protected int m_nGroupID { get; set; }
        public SocialPlatform m_eSocialPlatform { get; set; }
        public abstract string m_sFunctionName { get; set; }

        public abstract BaseSocialResponse GetResponse(int nGroupID);

        public List<ApiObjects.KeyValuePair> m_oKeyValue { get; set; }
    }
}

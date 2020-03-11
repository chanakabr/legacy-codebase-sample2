using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Social.Responses
{
    public class BaseSocialResponse
    {
        public BaseSocialResponse() {
            m_nStatus = -1;
        }
        public BaseSocialResponse(int nStatus)
        {
        }

        public int m_nStatus { get; set; }
        public string m_sResponseID { get; set; }
        public string m_sError { get; set; }
    }
}

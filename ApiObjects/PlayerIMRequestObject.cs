using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class PlayerIMRequestObject
    {
        public PlayerIMRequestObject() 
        {
            m_sPalyerID = "";
            m_sPlayerKey = "";
        }

        public void Initialize(string sPalyerID, string sPlayerKey)
        {
            m_sPalyerID = sPalyerID;
            m_sPlayerKey = sPlayerKey;
        }

        public string m_sPalyerID;
        public string m_sPlayerKey;
    }
}

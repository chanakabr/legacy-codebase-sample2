using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for TagMetaPair
/// </summary>
/// 

namespace TVPApi
{
    //Holds information regarding metas and tags
    public struct TagMetaPair
    {
        private string m_Key;

        private string m_Value;

        public TagMetaPair(string key, string value)
        {
            m_Key = key;
            m_Value = value;
        }

        public string Key
        {
            get
            {
                return m_Key;
            }
            set
            {
                m_Key = value;
            }
        }

        public string Value
        {
            get
            {
                return m_Value;
            }
            set
            {
                m_Value = value;
            }
        }
    }
}

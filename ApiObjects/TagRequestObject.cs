using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class TagRequestObject
    {
        public TagRequestObject() 
        {
            m_sTagName = "";
            m_sOrderBy = "count";
            m_oRange = null;
        }

        public void Initialize(string sTagName, string sOrderBy, ApiObjects.StringRange oRange)
        {
            m_sTagName = sTagName;
            m_sOrderBy = sOrderBy;
            m_oRange = oRange;
        }

        public string m_sTagName;
        //can be "count" or other for value
        public string m_sOrderBy;
        public StringRange m_oRange;
    }
}

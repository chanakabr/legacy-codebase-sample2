using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class OneTimeObject
    {
        public OneTimeObject() 
        {
            m_sLink = "";
            m_sBlock = "";
        }

        public void Initialize(string sLink , string sBlock)
        {
            m_sLink = sLink;
            m_sBlock = sBlock;
        }

        public string m_sLink;
        public string m_sBlock;
    }
}

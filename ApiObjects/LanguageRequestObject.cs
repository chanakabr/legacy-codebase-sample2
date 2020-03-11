using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    public class LanguageRequestObject
    {
        public LanguageRequestObject() 
        {
            m_sFullName = "";
        }

        public void Initialize(string sFullName)
        {
            m_sFullName = sFullName;
        }

        public string m_sFullName;
    }
}

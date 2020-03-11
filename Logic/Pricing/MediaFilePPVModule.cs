using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    [Serializable]
    public class MediaFilePPVModule
    {
        public PPVModule[] m_oPPVModules;
        public Int32 m_nMediaFileID;
        public string m_sProductCode;

        public MediaFilePPVModule()
        {
            m_oPPVModules = null;
            m_nMediaFileID = 0;
            m_sProductCode = string.Empty;
        }

        public void Initialize(PPVModule[] oPPVModules, Int32 nMediaFileID, string sProductCode)
        {
            m_oPPVModules = oPPVModules;
            m_nMediaFileID = nMediaFileID;
            m_sProductCode = sProductCode;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("MediaFilePPVModule. ");
            sb.Append(String.Concat("MF ID: ", m_nMediaFileID));
            sb.Append(String.Concat(" Prd Cd: ", m_sProductCode));
            if (m_oPPVModules != null && m_oPPVModules.Length > 0)
            {
                for (int i = 0; i < m_oPPVModules.Length; i++)
                {
                    if (m_oPPVModules[i] != null)
                    {
                        sb.Append(String.Concat(" PPV Module at index: ", i, ": ", m_oPPVModules[i].ToString()));
                    }
                    else
                    {
                        sb.Append(String.Concat(" PPVModule at index: ", i, " is null. "));
                    }
                }
            }
            else
            {
                sb.Append("PPV Modules are null or empty. ");
            }

            return sb.ToString();
        }
    }
}

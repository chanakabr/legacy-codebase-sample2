using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Pricing
{
    /*
     * This class is used as a container for PPVModuleWithExpiry. It relates PPVModuleWithExpiry to a given MediaFile as in table ppv_modules_media_files
     */
    [Serializable]
    public class MediaFilePPVContainer
    {
        public PPVModuleWithExpiry[] m_oPPVModules;
        public Int32 m_nMediaFileID;
        public string m_sProductCode;

        public MediaFilePPVContainer()
        {
            m_oPPVModules = null;
            m_nMediaFileID = 0;
            m_sProductCode = string.Empty;
        }

        public void Initialize(PPVModuleWithExpiry[] oPPVModules, Int32 nMediaFileID, string sProductCode)
        {
            m_oPPVModules = oPPVModules;
            m_nMediaFileID = nMediaFileID;
            m_sProductCode = sProductCode;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("MediaFilePPVContainer. ");
            sb.Append(String.Concat(" MF ID: ", m_nMediaFileID));
            sb.Append(String.Concat(" Prd Cd: ", m_sProductCode));

            return sb.ToString();
        }
    }
}

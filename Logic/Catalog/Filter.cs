using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Core.Catalog
{
   
    [DataContract]
    public class Filter
    {
        [DataMember]
        public bool m_bUseStartDate;
        [DataMember]
        public bool m_bUseFinalDate;
        [DataMember]
        public bool m_bOnlyActiveMedia;
        [DataMember]
        public Int32 m_nLanguage;
        [DataMember]
        public string m_sDeviceId;
        [DataMember]
        public string m_sPlatform; //not yet in use    
        [DataMember]
        public bool m_noFileUrl = false;
        [DataMember]
        public int m_nUserTypeID;

        [DataMember]
        public int[] fileTypes;

        public Filter(bool bUseStartDate, bool bUseFinalDate, bool bOnlyActiveMedia, Int32 nLanguage, string sDeviceId, string sPlatform, bool noFileUrl)
        {
            m_bUseStartDate = bUseStartDate;
            m_bUseFinalDate = bUseFinalDate;
            m_bOnlyActiveMedia = bOnlyActiveMedia;
            m_nLanguage = nLanguage;
            m_sDeviceId = sDeviceId;
            m_sPlatform = sPlatform;//not yet in use
            m_noFileUrl = noFileUrl;
        }

        public Filter(bool bUseStartDate, bool bUseFinalDate, bool bOnlyActiveMedia, Int32 nLanguage, string sDeviceId, string sPlatform, Int32 nUserTypeID)
        {
            m_bUseStartDate = bUseStartDate;
            m_bUseFinalDate = bUseFinalDate;
            m_bOnlyActiveMedia = bOnlyActiveMedia;
            m_nLanguage = nLanguage;
            m_sDeviceId = sDeviceId;
            m_sPlatform = sPlatform;//not yet in use
            m_nUserTypeID = nUserTypeID;
        }

        public Filter()
        {
        }
    }
}

using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using Logger;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using System;


namespace Catalog
{
 
    public  class MediaPlayRequestData
    {
        [DataMember]
        public int m_nMediaID;
        [DataMember]
        public string m_sSiteGuid;
        [DataMember]
        public string m_sAction;
        [DataMember]
        public int m_nLoc;
        [DataMember]
        public string m_sUDID;
        [DataMember]
        public string m_sMediaDuration;
        [DataMember]
        public int m_nMediaFileID;
        [DataMember]
        public int m_nAvgBitRate;
        [DataMember]
        public int m_nTotalBitRate;
        [DataMember]
        public int m_nCurrentBitRate;
        
        public string m_sMediaTypeId;

        public MediaPlayRequestData()
        {


        }

    }
}

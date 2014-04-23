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
    public class MediaLastPositionRequestData
    {
        [DataMember]
        public int m_nMediaID;
        [DataMember]
        public int m_sSiteGuid;        
        [DataMember]
        public string m_sUDID;        
    }
}

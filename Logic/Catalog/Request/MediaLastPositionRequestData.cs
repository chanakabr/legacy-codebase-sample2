using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using System;
using System.Text;


namespace Core.Catalog.Request
{
    public class MediaLastPositionRequestData
    {
        [DataMember]
        public int m_nMediaID;
        [DataMember]
        public string m_sNpvrID;
        [DataMember]
        public string m_sSiteGuid;
        [DataMember]
        public string m_sUDID;

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("MediaLastPositionRequestData object. ");
            sb.Append(String.Concat("Media ID: ", m_nMediaID));
            sb.Append(String.Concat(" Site Guid: ", m_sSiteGuid ?? "null"));
            sb.Append(String.Concat(" UDID: ", m_sUDID ?? "null"));
            return sb.ToString();
        }
    }
}

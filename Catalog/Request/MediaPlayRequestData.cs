using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using System;
using System.Text;


namespace Catalog.Request
{
 
    public class MediaPlayRequestData
    {
        [DataMember]
        public int m_nMediaID;
        [DataMember]
        public string m_sNpvrID;
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
        [DataMember]
        public bool m_bIsEpg;
        
        public string m_sMediaTypeId;

        public MediaPlayRequestData()
        {


        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("MediaPlayRequestData obj: ");
            sb.Append(String.Concat("Media ID: ", m_nMediaID));
            sb.Append(String.Concat(" Site Guid: ", m_sSiteGuid ?? "null"));
            sb.Append(String.Concat(" Action: ", m_sAction ?? "null"));
            sb.Append(String.Concat(" Loc: ", m_nLoc));
            sb.Append(String.Concat(" UDID: ", m_sUDID ?? "null"));
            sb.Append(String.Concat(" Media Duration: ", m_sMediaDuration ?? "null"));
            sb.Append(String.Concat(" Media File ID: ", m_nMediaFileID));
            sb.Append(String.Concat(" Avg Bitrate: ", m_nAvgBitRate));
            sb.Append(String.Concat(" Total Bitrate: ", m_nTotalBitRate));
            sb.Append(String.Concat(" Current Bitrate: ", m_nCurrentBitRate));
            sb.Append(String.Concat(" Is Media EPG: ", m_bIsEpg));

            return sb.ToString();
        }

    }
}

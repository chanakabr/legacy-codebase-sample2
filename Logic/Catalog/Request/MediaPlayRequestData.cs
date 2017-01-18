using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using TVinciShared;
using DAL;
using Tvinci.Core.DAL;
using System;
using System.Text;
using ApiObjects;


namespace Core.Catalog.Request
{
 
    public class MediaPlayRequestData
    {
        [DataMember]
        public string m_sAssetID;
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
        public eAssetTypes m_eAssetType;

        public string m_sMediaTypeId;

        public MediaPlayRequestData()
        {


        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("MediaPlayRequestData obj: ");
            sb.Append(String.Concat("Asset ID: ", m_sAssetID));
            sb.Append(String.Concat(" Site Guid: ", m_sSiteGuid ?? "null"));
            sb.Append(String.Concat(" Action: ", m_sAction ?? "null"));
            sb.Append(String.Concat(" Loc: ", m_nLoc));
            sb.Append(String.Concat(" UDID: ", m_sUDID ?? "null"));
            sb.Append(String.Concat(" Media Duration: ", m_sMediaDuration ?? "null"));
            sb.Append(String.Concat(" Media File ID: ", m_nMediaFileID));
            sb.Append(String.Concat(" Avg Bitrate: ", m_nAvgBitRate));
            sb.Append(String.Concat(" Total Bitrate: ", m_nTotalBitRate));
            sb.Append(String.Concat(" Current Bitrate: ", m_nCurrentBitRate));
            sb.Append(String.Concat(" Asset Type: ", m_eAssetType));

            return sb.ToString();
        }

    }
}

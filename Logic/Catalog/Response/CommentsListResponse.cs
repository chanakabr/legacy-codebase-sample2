using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace Core.Catalog.Response
{
    [DataContract]
    public class CommentsListResponse : BaseResponse
    {
        [DataMember]
        public List<Comments> m_lComments;

        public CommentsListResponse()
        {
            m_lComments = new List<Comments>();
        }
    }


    [DataContract]
    [Serializable]
    [JsonObject("comment")]
    public class Comments
    {
        [DataMember]
        [JsonIgnore]
        public Int32 Id;
        [DataMember]
        [JsonProperty("media_id")]
        public Int32 m_nAssetID;
        [DataMember]
        [JsonProperty("writer")]
        public string m_sWriter;
        [DataMember]
        [JsonProperty("language_id")]
        public int m_nLang;
        [DataMember]
        [JsonIgnore]
        public string m_sLangName;
        [DataMember]
        [JsonProperty("header")]
        public string m_sHeader;
        [DataMember]
        [JsonProperty("sub_header")]
        public string m_sSubHeader;
        [DataMember]
        [JsonProperty("text")]
        public string m_sContentText;
        [DataMember]
        [JsonProperty("action_date")]
        [JsonConverter(typeof(ApiObjects.JsonSerializers.BaseTimeConverter))]
        public DateTime m_dCreateDate;
        [DataMember]
        [JsonProperty("site_guid")]
        public string m_sSiteGuid;
        [DataMember]
        [JsonIgnore]
        public string m_sUserPicURL;
        [JsonProperty("media_type")]
        public string m_sAssetType;
        [JsonProperty("action")]
        public string m_Action;
        [JsonProperty("group_id")]
        public int m_nGroupID;
        
        [DataMember]
        [JsonIgnore]
        public ApiObjects.eAssetType AssetType;

        public Comments()
        {
            m_Action = "comment"; //defult value - comment
        }
    }
}

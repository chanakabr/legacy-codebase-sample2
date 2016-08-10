using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Epg Comment Response
    /// </summary>
    [Serializable]

    public class KalturaAssetCommentListResponse : KalturaListResponse        
    {
        /// <summary>
        /// Assets
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaAssetComment> Objects { get; set; }

        //[DataMember(Name = "requestId")]
        //[JsonProperty(PropertyName = "requestId")]
        //[XmlElement("requestId", IsNullable = true)]
        //public string RequestId { get; set; }
    }
     /// <summary>
    /// Asset Comment
    /// </summary>
    [Serializable]
    public class KalturaAssetComment : KalturaOTTObject
    {
        /// <summary>
        /// Comment ID
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        public Int32 Id;

        /// <summary>
        /// Asset identifier
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty(PropertyName = "assetId")]
        [XmlElement(ElementName = "assetId")]
        public string AssetId { get; set; }

        /// <summary>
        /// Asset Type
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty(PropertyName = "assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaAssetType AssetType { get; set; }
        
        /// <summary>
        /// Language Id
        /// </summary>
        [DataMember(Name = "languageId")]
        [JsonProperty(PropertyName = "languageId")]
        [XmlElement(ElementName = "languageId")]
        public int LanguageId;

        /// <summary>
        /// Header
        /// </summary>
        [DataMember(Name = "header")]
        [JsonProperty(PropertyName = "header")]
        [XmlElement(ElementName = "header")]       
        public string Header;

        /// <summary>
        /// Sub Header
        /// </summary>
        [DataMember(Name = "subHeader")]
        [JsonProperty(PropertyName = "subHeader")]
        [XmlElement(ElementName = "subHeader")]     
        public string SubHeader;

        /// <summary>
        /// Text
        /// </summary>
        [DataMember(Name = "text")]
        [JsonProperty(PropertyName = "text")]
        [XmlElement(ElementName = "text")]     
        public string Text;

        /// <summary>
        /// CreateDate
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty(PropertyName = "createDate")]
        [XmlElement(ElementName = "createDate")]
        public long CreateDate;

        /// <summary>
        /// User Id
        /// </summary>
        [DataMember(Name = "userId")]
        [JsonProperty(PropertyName = "userId")]
        [XmlElement(ElementName = "userId")]
        public string UserId;

        /// <summary>
        /// Writer
        /// </summary>
        [DataMember(Name = "writer")]
        [JsonProperty(PropertyName = "writer")]
        [XmlElement(ElementName = "writer")]
        public string Writer;

        /// <summary>
        /// User Picture URL
        /// </summary>
        [DataMember(Name = "userPictureURL")]
        [JsonProperty(PropertyName = "userPictureURL")]
        [XmlElement(ElementName = "userPictureURL")]
        public string UserPictureURL;
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;

namespace WebAPI.Models.Social
{
    public class KalturaSocialAction : KalturaOTTObject
    {

        /// <summary>
        /// social action document id
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public string Id { get; set; }

        /// <summary>
        /// Action type
        /// </summary>
        [DataMember(Name = "actionType")]
        [JsonProperty("actionType")]
        [XmlElement(ElementName = "actionType")]
        public KalturaSocialActionType ActionType { get; set; }

        /// <summary>
        /// EPOC based timestamp for when the action occurred
        /// </summary>
        [DataMember(Name = "time")]
        [JsonProperty("time")]
        [XmlElement(ElementName = "time")]
        public long? Time { get; set; }

        /// <summary>
        /// ID of the asset that was acted upon
        /// </summary>
        [DataMember(Name = "assetId")]
        [JsonProperty("assetId")]
        [XmlElement(ElementName = "assetId")]
        public long? AssetId { get; set; }

        /// <summary>
        /// Type of the asset that was acted upon, currently only VOD (media)
        /// </summary>
        [DataMember(Name = "assetType")]
        [JsonProperty("assetType")]
        [XmlElement(ElementName = "assetType")]
        public KalturaAssetType AssetType { get; set; }

        public override string ToString()
        {
            string res = string.Format("actionType : {0}, Time :{1}, AssetId : {2}, AssetID :{3}, AssetType : {4}", ActionType.ToString(), Time, AssetId , AssetType);
            return res;  
        }
    }

    public class KalturaSocialActionRate : KalturaSocialAction
    {
        /// <summary>
        /// The value of the rating
        /// </summary>
        [DataMember(Name = "rate")]
        [JsonProperty("rate")]
        [XmlElement(ElementName = "rate")]
        public int Rate { get; set; }

        public KalturaSocialActionRate(int value)
        {
            ActionType = KalturaSocialActionType.RATE;
            Rate = value;
        }
        public KalturaSocialActionRate()
        {
            ActionType = KalturaSocialActionType.RATE;            
        }

        public override string ToString()
        {
            string res = string.Format("{0}, Rate Value = {1} ", base.ToString(), Rate);
            return res;
        }
    }
    
    public class KalturaSocialActionWatch : KalturaSocialAction
    {
        /// <summary>
        /// The value of the url
        /// </summary>
        [DataMember(Name = "url")]
        [JsonProperty("url")]
        [XmlElement(ElementName = "url")]
        [SchemeProperty(WriteOnly = true)]
        public string Url { get; set; }

        public KalturaSocialActionWatch(string value)
        {
            ActionType = KalturaSocialActionType.WATCH;
            Url = value;
        }
        public KalturaSocialActionWatch()
        {
            ActionType = KalturaSocialActionType.WATCH;
        }

        public override string ToString()
        {
            string res = string.Format("{0}, Watch url Value = {1} ", base.ToString(), Url);
            return res;
        }
    }

    public enum KalturaSocialActionType
    {
        LIKE,
        WATCH,
        RATE,        
        SHARE       
    }
}
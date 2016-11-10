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
        /// Action type
        /// </summary>
        [DataMember(Name = "actionType")]
        [JsonProperty("actionType")]
        [XmlElement(ElementName = "actionType")]
        public KalturaSocialActionType Type { get; set; }

        /// <summary>
        /// EPOC based timestamp for when the action occurred
        /// </summary>
        [DataMember(Name = "actionTime")]
        [JsonProperty("actionTime")]
        [XmlElement(ElementName = "actionTime")]
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
            string res = string.Format("actionType : {0}, Time :{1}, AssetId : {2}, AssetID :{3}, AssetType : {4}", Type.ToString(), Time, AssetId , AssetType);
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
            Type = KalturaSocialActionType.RATE;
            Rate = value;
        }

        public override string ToString()
        {
            string res = string.Format("{0}, Rate Value = {1} ", base.ToString(), Rate);
            return res;
        }
    }

    public enum KalturaSocialActionType
    {
        LIKE,
        WATCH,
        RATE,
        UNLIKE,
        SHARE       
    }
}
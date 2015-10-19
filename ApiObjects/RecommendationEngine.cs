using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;

namespace ApiObjects
{
    [Serializable]
    [JsonObject(Id = "id")]
    public class RecommendationEngine 
    {
        #region Properties

        [DataMember]
        [JsonProperty()]
        public int ID
        {
            get;
            set;
        }

        [DataMember]
        [JsonProperty()]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public bool IsDefault
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public bool IsActive
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public string AdapterUrl
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public string ExternalIdentifier
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public string SharedSecret
        {
            get;
            set;
        }

        [XmlIgnore]
        [JsonProperty()]
        [DataMember]
        public int Status
        {
            get;
            set;
        }
        
        [JsonProperty()]
        [DataMember]
        public List<RecommendationEngineSettings> Settings
        {
            get;
            set;
        }

        #endregion

        #region Ctor

        public RecommendationEngine()
        {

        }

        public RecommendationEngine(RecommendationEngine clone)
        {
            this.ID = clone.ID;
            this.Name = clone.Name;
            this.IsDefault = clone.IsDefault;
            this.IsActive = clone.IsActive;
            this.AdapterUrl = clone.AdapterUrl;
            this.Status = clone.Status;
            this.ExternalIdentifier = clone.ExternalIdentifier;
            this.SharedSecret = clone.SharedSecret;
            this.Settings = new List<RecommendationEngineSettings>(clone.Settings);
        }

        #endregion
    }
}

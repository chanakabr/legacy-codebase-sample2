using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace ApiObjects
{
    [Serializable]
    [JsonObject(Id = "id")]
    public class RecommendationEngine : RecommendationEngineBase
    {
        #region Properties

        [JsonProperty()]
        public bool IsDefault
        {
            get;
            set;
        }

        [JsonProperty()]
        public bool IsActive
        {
            get;
            set;
        }

        [JsonProperty()]
        public string AdapterUrl
        {
            get;
            set;
        }

        [JsonProperty()]
        public string ExternalIdentifier
        {
            get;
            set;
        }

        [JsonProperty()]
        public string SharedSecret
        {
            get;
            set;
        }

        [XmlIgnore]
        [JsonProperty()]
        public int Status
        {
            get;
            set;
        }
        
        [JsonProperty()]
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

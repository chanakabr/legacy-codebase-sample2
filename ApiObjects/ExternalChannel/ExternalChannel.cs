using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace ApiObjects
{
    [Serializable]
    [JsonObject(Id = "id")]
    public class ExternalChannel
    {
        #region Data Members

        [JsonProperty()]
        [DataMember]
        public int ID
        {
            get;
            set;
        }

        [JsonProperty()]
        [XmlIgnore]
        [DataMember]
        public int GroupId
        {
            get;
            set;
        }

        [JsonProperty()]
        [DataMember]
        public string Name
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
        public int RecommendationEngineId
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
        public List<ExternalRecommendationEngineEnrichment> Enrichments;

        /// <summary>
        /// KSQL expression with personalized filtering
        /// </summary>
        [JsonProperty()]
        [DataMember]
        public string FilterExpression
        {
            get;
            set;
        }

        #endregion

        public ExternalChannel()
        {

        }

        public ExternalChannel(ExternalChannel externalChannel)
        {
            this.ID = externalChannel.ID;
            this.Name = externalChannel.Name;
            this.ExternalIdentifier = externalChannel.ExternalIdentifier;
            this.RecommendationEngineId = externalChannel.RecommendationEngineId;
            this.FilterExpression = externalChannel.FilterExpression;
            this.Enrichments = externalChannel.Enrichments;
            this.IsActive = externalChannel.IsActive;
        }

    }
}

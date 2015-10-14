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
        public List<ExternalChannelEnrichment> Enrichments;

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

    public enum ExternalChannelEnrichment : int
    {
        ClientLocation = 1,
        UserId = 2,
        HouseholdId = 4,
        DeviceId = 8,
        DeviceType = 16,
        UTCOffset = 32,
        Language = 64,
        NPVRSupport = 128,
        Catchup = 256,
        Parental = 512,
        DTTRegion = 1024,
        AtHome = 2048
    }
}

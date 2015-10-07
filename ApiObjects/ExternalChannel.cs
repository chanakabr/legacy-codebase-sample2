using System.Collections.Generic;
using System.Xml.Serialization;

namespace ApiObjects
{
    public class ExternalChannel
    {
        public int ID { get; set; }
        [XmlIgnore]
        public int GroupId { get; set; }
        public string Name { get; set; }
        public string ExternalIdentifier { get; set; }
        public int RecommendationEngineId { get; set; }
        public bool IsActive { get; set; }
        public List<ExternalChannelEnrichment> Enrichments;

        /// <summary>
        /// KSQL expression with personalized filtering
        /// </summary>
        public string FilterExpression { get; set; }

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
        UTCOffset = 32
    }
}

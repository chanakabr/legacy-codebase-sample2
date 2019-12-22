using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Segmentation
{
    /// <summary>
    /// Base class that defines segment action
    /// </summary>
    public partial class KalturaBaseSegmentAction : KalturaOTTObject
    {
    }

    /// <summary>
    /// Asset order segment action
    /// </summary>
    public partial class KalturaAssetOrderSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// Action name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// Action values
        /// </summary>
        [DataMember(Name = "values")]
        [JsonProperty(PropertyName = "values")]
        [XmlElement(ElementName = "values")]
        public List<KalturaStringValue> Values { get; set; }
    }

    /// <summary>
    /// Segment action with ksql
    /// </summary>
    public abstract partial class KalturaKsqlSegmentAction : KalturaBaseSegmentAction
    {
        /// <summary>
        /// KSQL
        /// </summary>
        [DataMember(Name = "ksql")]
        [JsonProperty(PropertyName = "ksql")]
        [XmlElement(ElementName = "ksql")]
        public string KSQL { get; set; }
    }

    /// <summary>
    /// Asset filter action
    /// </summary>
    public abstract partial class KalturaSegmentAssetFilterAction : KalturaKsqlSegmentAction
    {
    }

    /// <summary>
    /// segment asset filter for segment action
    /// </summary>
    public partial class KalturaSegmentAssetFilterSegmentAction : KalturaSegmentAssetFilterAction
    {
    }

    /// <summary>
    /// segment asset filter for subscription action
    /// </summary>
    public partial class KalturaSegementAssetFilterSubscriptionAction : KalturaSegmentAssetFilterAction
    {
    }

    /// <summary>
    /// segment block subscription action
    /// </summary>
    public abstract partial class KalturaBlockSubscriptionSegmentAction : KalturaKsqlSegmentAction
    {
    }

    /// <summary>
    /// segment block subscription for playback action
    /// </summary>
    public partial class KalturaSegmentBlockPlaybackSubscriptionAction : KalturaBlockSubscriptionSegmentAction
    {
    }

    /// <summary>
    /// segment block subscription for cancel action
    /// </summary>
    public partial class KalturaSegmentBlockCancelSubscriptionAction : KalturaBlockSubscriptionSegmentAction
    {
    }

    /// <summary>
    /// segment block subscription for purchase action
    /// </summary>
    public partial class KalturaSegmentBlockPurchaseSubscriptionAction : KalturaBlockSubscriptionSegmentAction
    {
    }
}
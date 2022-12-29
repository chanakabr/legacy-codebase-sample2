namespace Phoenix.Generated.Api.Events.Crud.SegmentationType
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// An event used to reflect a crud operation in a segmentation type
    ///
    /// An event used to reflect a crud operation on a segmentation Type
    ///
    /// A base event for all CRUD events, contains common properties
    ///
    /// A base event for ALL EVENTS, currently does not contain anything, placeholder for the
    /// future
    ///
    /// A base schema for ALL SCHEMAS, contains common meta data about the schema
    /// </summary>
    public partial class SegmentationType
    {
        /// <summary>
        /// enum values - Created=0, Updated=1,Deleted=2
        /// </summary>
        [JsonProperty("operation")]
        public long Operation { get; set; }

        [JsonProperty("partnerId")]
        public long PartnerId { get; set; }

        [JsonProperty("schema", NullValueHandling = NullValueHandling.Ignore)]
        public Schema Schema { get; set; }

        [JsonProperty("actions", NullValueHandling = NullValueHandling.Ignore)]
        public Action[] Actions { get; set; }

        [JsonProperty("conditions", NullValueHandling = NullValueHandling.Ignore)]
        public Conditions Conditions { get; set; }

        [JsonProperty("createDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? CreateDate { get; set; }

        [JsonProperty("description", NullValueHandling = NullValueHandling.Ignore)]
        public string Description { get; set; }

        [JsonProperty("executionDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecutionDate { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("segmentsValues", NullValueHandling = NullValueHandling.Ignore)]
        public SegmentsValue[] SegmentsValues { get; set; }

        [JsonProperty("updateDate", NullValueHandling = NullValueHandling.Ignore)]
        public long? UpdateDate { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public Value Value { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public long? Version { get; set; }
    }

    public partial class Action
    {
        [JsonProperty("assetOrderSegmentAction", NullValueHandling = NullValueHandling.Ignore)]
        public AssetOrderSegmentAction AssetOrderSegmentAction { get; set; }

        [JsonProperty("ksqlSegmentAction", NullValueHandling = NullValueHandling.Ignore)]
        public KsqlSegmentAction KsqlSegmentAction { get; set; }
    }

    public partial class AssetOrderSegmentAction
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Values { get; set; }
    }

    public partial class KsqlSegmentAction
    {
        /// <summary>
        /// Values: SegmentBlockCancelSubscriptionAction, SegmentBlockPlaybackSubscriptionAction,
        /// SegmentBlockPurchaseSubscriptionAction, SegmentAssetFilterSegmentAction,
        /// SegmentAssetFilterSubscriptionAction
        /// </summary>
        [JsonProperty("actionType", NullValueHandling = NullValueHandling.Ignore)]
        public string ActionType { get; set; }

        [JsonProperty("ksql", NullValueHandling = NullValueHandling.Ignore)]
        public string Ksql { get; set; }
    }

    public partial class Conditions
    {
        [JsonProperty("contentScoreConditions", NullValueHandling = NullValueHandling.Ignore)]
        public ContentScoreCondition[] ContentScoreConditions { get; set; }

        [JsonProperty("monetizationConditions", NullValueHandling = NullValueHandling.Ignore)]
        public MonetizationCondition[] MonetizationConditions { get; set; }

        /// <summary>
        /// Values: And, Or
        /// </summary>
        [JsonProperty("operator", NullValueHandling = NullValueHandling.Ignore)]
        public string Operator { get; set; }

        [JsonProperty("userDataConditions", NullValueHandling = NullValueHandling.Ignore)]
        public UserDataCondition[] UserDataConditions { get; set; }
    }

    public partial class ContentScoreCondition
    {
        [JsonProperty("contentActions", NullValueHandling = NullValueHandling.Ignore)]
        public ContentAction[] ContentActions { get; set; }

        [JsonProperty("days", NullValueHandling = NullValueHandling.Ignore)]
        public long? Days { get; set; }

        [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; set; }

        [JsonProperty("maxScore", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxScore { get; set; }

        [JsonProperty("minScore", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinScore { get; set; }

        [JsonProperty("values", NullValueHandling = NullValueHandling.Ignore)]
        public string[] Values { get; set; }
    }

    public partial class ContentAction
    {
        /// <summary>
        /// Values: watch_linear, watch_vod, catchup, npvr, favorite, recording, social_action
        /// </summary>
        [JsonProperty("action", NullValueHandling = NullValueHandling.Ignore)]
        public string Action { get; set; }

        [JsonProperty("length", NullValueHandling = NullValueHandling.Ignore)]
        public long? Length { get; set; }

        /// <summary>
        /// Values: minutes, percentage
        /// </summary>
        [JsonProperty("lengthType", NullValueHandling = NullValueHandling.Ignore)]
        public string LengthType { get; set; }

        [JsonProperty("multiplier", NullValueHandling = NullValueHandling.Ignore)]
        public long? Multiplier { get; set; }
    }

    public partial class MonetizationCondition
    {
        [JsonProperty("businessModules", NullValueHandling = NullValueHandling.Ignore)]
        public long[] BusinessModules { get; set; }

        [JsonProperty("currencyCode", NullValueHandling = NullValueHandling.Ignore)]
        public string CurrencyCode { get; set; }

        [JsonProperty("days", NullValueHandling = NullValueHandling.Ignore)]
        public long? Days { get; set; }

        [JsonProperty("maxValue", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaxValue { get; set; }

        [JsonProperty("minValue", NullValueHandling = NullValueHandling.Ignore)]
        public long? MinValue { get; set; }

        /// <summary>
        /// Values: Any, PPV, Subscription, Boxset, PPVLive
        /// </summary>
        [JsonProperty("monetizationType", NullValueHandling = NullValueHandling.Ignore)]
        public string MonetizationType { get; set; }

        /// <summary>
        /// Values: Count, Sum, Avg
        /// </summary>
        [JsonProperty("operator", NullValueHandling = NullValueHandling.Ignore)]
        public string Operator { get; set; }
    }

    public partial class UserDataCondition
    {
        [JsonProperty("field", NullValueHandling = NullValueHandling.Ignore)]
        public string Field { get; set; }

        [JsonProperty("value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public partial class Schema
    {
        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("version", NullValueHandling = NullValueHandling.Ignore)]
        public string Version { get; set; }
    }

    public partial class SegmentsValue
    {
        [JsonProperty("Id")]
        public long Id { get; set; }

        [JsonProperty("Name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }

        [JsonProperty("Value", NullValueHandling = NullValueHandling.Ignore)]
        public string Value { get; set; }
    }

    public partial class Value
    {
        [JsonProperty("affectedHouseholds", NullValueHandling = NullValueHandling.Ignore)]
        public long? AffectedHouseholds { get; set; }

        [JsonProperty("affectedUsers", NullValueHandling = NullValueHandling.Ignore)]
        public long? AffectedUsers { get; set; }

        [JsonProperty("id", NullValueHandling = NullValueHandling.Ignore)]
        public long? Id { get; set; }
    }
}

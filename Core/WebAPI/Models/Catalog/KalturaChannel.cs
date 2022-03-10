using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog.Ordering;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;

namespace WebAPI.Models.Catalog
{
    /// <summary>
    /// Channel details
    /// </summary>
    public partial class KalturaChannel : KalturaBaseChannel
    {
        private const string OPC_MERGE_VERSION = "5.0.0.0";
        private const int ORDERING_PARAMETERS_MAX_COUNT = 2;

        /// <summary>
        /// Unique identifier for the channel
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty(PropertyName = "id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
        public long? Id { get; set; }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty(PropertyName = "name")]
        [XmlElement(ElementName = "name")]
        public KalturaMultilingualString Name { get; set; }

        /// <summary>
        /// Channel name
        /// </summary>
        [DataMember(Name = "oldName")]
        [JsonProperty(PropertyName = "oldName")]
        [XmlElement(ElementName = "oldName")]
        [OldStandardProperty("name", OPC_MERGE_VERSION)]
        public string OldName { get; set; }

        /// <summary>
        /// Channel system name
        /// </summary>
        [DataMember(Name = "systemName")]
        [JsonProperty("systemName")]
        [XmlElement(ElementName = "systemName", IsNullable = true)]
        public string SystemName { get; set; }

        /// <summary>
        /// Cannel description
        /// </summary>
        [DataMember(Name = "description")]
        [JsonProperty(PropertyName = "description")]
        [XmlElement(ElementName = "description")]
        public KalturaMultilingualString Description { get; set; }

        /// <summary>
        /// Cannel description
        /// </summary>
        [DataMember(Name = "oldDescription")]
        [JsonProperty(PropertyName = "oldDescription")]
        [XmlElement(ElementName = "oldDescription")]
        [OldStandardProperty("description", OPC_MERGE_VERSION)]
        public string OldDescription { get; set; }

        /// <summary>
        /// Channel images 
        /// </summary>
        [DataMember(Name = "images")]
        [JsonProperty(PropertyName = "images")]
        [XmlArray(ElementName = "images", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        [XmlArrayItem("item")]
        [Deprecated(OPC_MERGE_VERSION)]
        public List<KalturaMediaImage> Images { get; set; }

        /// <summary>
        /// Asset types in the channel.
        /// -26 is EPG
        /// </summary>
        [DataMember(Name = "assetTypes")]
        [JsonProperty(PropertyName = "assetTypes")]
        [XmlArray(ElementName = "assetTypes", IsNullable = true)]
        [XmlArrayItem("item")]
        [OldStandardProperty("asset_types")]
        [Deprecated(OPC_MERGE_VERSION)]
        [Obsolete]
        public List<KalturaIntegerValue> AssetTypes { get; set; }

        /// <summary>
        /// Media types in the channel 
        /// -26 is EPG
        /// </summary>
        [DataMember(Name = "media_types")]
        [JsonIgnore]
        [Obsolete]
        [Deprecated(OPC_MERGE_VERSION)]
        public List<KalturaIntegerValue> MediaTypes { get; set; }

        /// <summary>
        /// Filter expression
        /// </summary>
        [DataMember(Name = "filterExpression")]
        [JsonProperty("filterExpression")]
        [XmlElement(ElementName = "filterExpression")]
        [OldStandardProperty("filter_expression")]
        [Deprecated(OPC_MERGE_VERSION)]
        public string FilterExpression
        {
            get;
            set;
        }

        /// <summary>
        /// active status
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public bool? IsActive
        {
            get;
            set;
        }

        /// <summary>
        /// Channel order
        /// </summary>
        [DataMember(Name = "order")]
        [JsonProperty("order")]
        [XmlElement(ElementName = "order", IsNullable = true)]
        [Deprecated(OPC_MERGE_VERSION)]
        public KalturaAssetOrderBy? Order
        {
            get;
            set;
        }
        
        /// <summary>
        /// Channel group by
        /// </summary>
        [DataMember(Name = "groupBy")]
        [JsonProperty("groupBy")]
        [XmlElement(ElementName = "groupBy", IsNullable = true)]
        [Deprecated(OPC_MERGE_VERSION)]
        [Obsolete]
        public KalturaAssetGroupBy GroupBy
        {
            get;
            set;
        }

        /// <summary>
        /// Channel order by
        /// </summary>
        [DataMember(Name = "orderBy")]
        [JsonProperty("orderBy")]
        [XmlElement(ElementName = "orderBy", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public KalturaChannelOrder OrderBy { get; set; }

        /// <summary>
        /// Parameters for asset list sorting.
        /// </summary>
        [DataMember(Name = "orderingParametersEqual")]
        [JsonProperty(PropertyName = "orderingParametersEqual")]
        [XmlElement(ElementName = "orderingParametersEqual")]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public List<KalturaBaseChannelOrder> OrderingParameters { get; set; }

        /// <summary>
        /// Specifies when was the Channel was created. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "createDate")]
        [JsonProperty("createDate")]
        [XmlElement(ElementName = "createDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long CreateDate { get; set; }

        /// <summary>
        /// Specifies when was the Channel last updated. Date and time represented as epoch.
        /// </summary>
        [DataMember(Name = "updateDate")]
        [JsonProperty("updateDate")]
        [XmlElement(ElementName = "updateDate", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long UpdateDate { get; set; }

        /// <summary>
        /// Specifies whether the assets in this channel will be ordered based on their match to the user's segments (see BEO-5524)
        /// </summary>
        [DataMember(Name = "supportSegmentBasedOrdering")]
        [JsonProperty("supportSegmentBasedOrdering")]
        [XmlElement(ElementName = "supportSegmentBasedOrdering")]
        [SchemeProperty()]
        public bool SupportSegmentBasedOrdering { get; set; }

        /// <summary>
        /// Asset user rule identifier 
        /// </summary>
        [DataMember(Name = "assetUserRuleId")]
        [JsonProperty("assetUserRuleId")]
        [XmlElement(ElementName = "assetUserRuleId")]
        [SchemeProperty(RequiresPermission = (int)RequestType.WRITE, IsNullable = true)]
        public long? AssetUserRuleId { get; set; }

        /// <summary>
        /// key/value map field for extra data
        /// </summary>
        [DataMember(Name = "metaData")]
        [JsonProperty("metaData")]
        [XmlElement(ElementName = "metaData", IsNullable = true)]
        [SchemeProperty(IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> MetaData { get; set; }

        /// <summary>
        /// Virtual asset id
        /// </summary>
        [DataMember(Name = "virtualAssetId")]
        [JsonProperty("virtualAssetId")]
        [XmlElement(ElementName = "virtualAssetId", IsNullable = true)]
        [SchemeProperty(ReadOnly = true)]
        public long? VirtualAssetId { get; set; }

        internal void BuildOrderingsForInsert()
        {
            BuildOrderings(true);
        }

        internal virtual void ValidateForInsert()
        {
            if (string.IsNullOrEmpty(SystemName))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (Name?.Values == null || Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            Name.Validate("multilingualName");

            if (Description != null)
            {
                if (Description.Values == null || Description.Values.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
                }
                else
                {
                    Description.Validate("multilingualDescription");
                }
            }

            OrderBy?.Validate(GetType());

            if (!OrderingParameters.Any())
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "orderingParametersEqual");
            }

            if (OrderingParameters.Count > ORDERING_PARAMETERS_MAX_COUNT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "orderingParametersEqual", ORDERING_PARAMETERS_MAX_COUNT);
            }

            if (OrderingParameters.Count > 1 && GroupBy != null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "groupBy", "orderingParametersEqual");
            }
        }

        internal void BuildOrderingsForUpdate()
        {
            BuildOrderings(OrderBy != null);
        }

        internal virtual void ValidateForUpdate()
        {
            if (SystemName != null && SystemName == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (Name != null)
            {
                if (Name.Values == null || Name.Values.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }
                else
                {
                    Name.Validate("multilingualName");
                }
            }

            if (Description != null)
            {
                if (Description.Values == null || Description.Values.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
                }
                else
                {
                    Description.Validate("multilingualDescription", true, false);
                }
            }

            OrderBy?.Validate(GetType());

            if (OrderingParameters?.Count > ORDERING_PARAMETERS_MAX_COUNT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "orderingParametersEqual", ORDERING_PARAMETERS_MAX_COUNT);
            }

            if (OrderingParameters?.Count > 1 && GroupBy != null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "groupBy", "orderingParametersEqual");
            }
        }

        internal virtual void FillEmptyFieldsForUpdate()
        {
            if (this.NullableProperties != null && this.NullableProperties.Contains("metadata"))
            {
                this.MetaData = new SerializableDictionary<string, KalturaStringValue>();
            }
        }

        public int[] GetAssetTypes()
        {
            if (AssetTypes == null && MediaTypes != null)
                AssetTypes = MediaTypes;

            if (AssetTypes == null)
                return new int[0];

            int[] assetTypes = new int[AssetTypes.Count];
            for (int i = 0; i < AssetTypes.Count; i++)
            {
                assetTypes[i] = AssetTypes[i].value;
            }

            return assetTypes;
        }

        private void BuildOrderings(bool isOrderingParametersRequired)
        {
            if (OrderingParameters == null)
            {
                OrderingParameters = new List<KalturaBaseChannelOrder>();
            }

            if (OrderingParameters.Any() && OrderBy != null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "orderingParametersEqual", "orderBy");
            }

            if (!OrderingParameters.Any() && isOrderingParametersRequired)
            {
                var orderBy = OrderBy ?? new KalturaChannelOrder { orderBy = KalturaChannelOrderBy.CREATE_DATE_DESC };
                var assetOrder = CreateBaseChannelOrder(orderBy);
                OrderingParameters.Add(assetOrder);
            }
        }

        private KalturaBaseChannelOrder CreateBaseChannelOrder(KalturaChannelOrder order)
        {
            if (order.DynamicOrderBy != null)
            {
                var metaTagOrderBy = OrderBy.DynamicOrderBy.OrderBy ?? KalturaMetaTagOrderBy.META_ASC;
                return new KalturaChannelDynamicOrder { Name = OrderBy.DynamicOrderBy.Name, OrderBy = metaTagOrderBy };
            }

            var slidingWindowPeriod = order.SlidingWindowPeriod ?? 0;
            switch (order.orderBy)
            {
                case KalturaChannelOrderBy.NAME_ASC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.NAME_ASC };
                case KalturaChannelOrderBy.NAME_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.NAME_DESC };
                case KalturaChannelOrderBy.START_DATE_ASC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.START_DATE_ASC };
                case KalturaChannelOrderBy.START_DATE_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.START_DATE_DESC };
                case KalturaChannelOrderBy.CREATE_DATE_ASC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.CREATE_DATE_ASC };
                case KalturaChannelOrderBy.CREATE_DATE_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.CREATE_DATE_DESC };
                case KalturaChannelOrderBy.RELEVANCY_DESC:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.RELEVANCY_DESC };
                case KalturaChannelOrderBy.ORDER_NUM:
                    return new KalturaChannelFieldOrder { OrderBy = KalturaChannelFieldOrderByType.ORDER_NUM };
                case KalturaChannelOrderBy.LIKES_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.LIKES_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                case KalturaChannelOrderBy.VOTES_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.VOTES_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                case KalturaChannelOrderBy.RATINGS_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.RATINGS_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                case KalturaChannelOrderBy.VIEWS_DESC:
                    return new KalturaChannelSlidingWindowOrder { OrderBy = KalturaChannelSlidingWindowOrderByType.VIEWS_DESC, SlidingWindowPeriod = slidingWindowPeriod };
                default:
                    throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, order.orderBy, "orderBy.orderBy");
            }
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public enum KalturaAssetHistoryOrderBy
    {
        NONE
    }

    [OldStandard("filterTypes", "filter_types")]
    [OldStandard("statusEqual", "filter_status")]
    [OldStandard("daysLessThanOrEqual", "days")]
    public class KalturaAssetHistoryFilter : KalturaFilter<KalturaAssetHistoryOrderBy>
    {
        public override KalturaAssetHistoryOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetHistoryOrderBy.NONE;
        }

        /// <summary>
        /// List of asset types to search within. The list is a string separated be comma.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "filterTypes")]
        [JsonProperty(PropertyName = "filterTypes")]
        [XmlArray(ElementName = "filterTypes", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [Obsolete]
        public List<KalturaIntegerValue> filterTypes { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within.
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty(PropertyName = "typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        /// <summary>
        /// Comma separated list of asset identifiers.
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlElement(ElementName = "assetIdIn", IsNullable = true)]
        public string AssetIdIn { get; set; }

        /// <summary>
        /// Which type of recently watched media to include in the result – those that finished watching, those that are in progress or both.
        /// If omitted or specified filter = all – return all types.
        /// Allowed values: progress – return medias that are in-progress, done – return medias that finished watching.
        /// </summary>
        [DataMember(Name = "statusEqual")]
        [JsonProperty(PropertyName = "statusEqual")]
        [XmlElement(ElementName = "statusEqual", IsNullable = true)]
        public KalturaWatchStatus? StatusEqual { get; set; }

        /// <summary>
        /// How many days back to return the watched media. If omitted, default to 7 days
        /// </summary>
        [DataMember(Name = "daysLessThanOrEqual")]
        [JsonProperty(PropertyName = "daysLessThanOrEqual")]
        [XmlElement(ElementName = "daysLessThanOrEqual")]
        [Obsolete]
        public int? DaysLessThanOrEqual { get; set; }

        /// <summary>
        /// Additional data to return per asset, formatted as a comma-separated array. 
        /// Possible values: stats – add the AssetStats model to each asset. files – add the AssetFile model to each asset. images - add the Image model to each asset.
        /// </summary>
        [DataMember(Name = "with")]
        [JsonProperty(PropertyName = "with")]
        [XmlArray(ElementName = "with", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [Obsolete]
        public List<KalturaCatalogWithHolder> with { get; set; }

        internal int getDaysLessThanOrEqual()
        {
            return DaysLessThanOrEqual.HasValue ? DaysLessThanOrEqual.Value : 0;
        }

        internal List<int> getTypeIn()
        {
            if (filterTypes != null)
                return filterTypes.Select(x => x.value).ToList();

            if (string.IsNullOrEmpty(TypeIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.TypeIn contains invalid id {0}", value));
                }
            }

            return values;
        }

        internal List<string> getAssetIdIn()
        {
            if (AssetIdIn == null)
                return null;

            return AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
        }
    }
}
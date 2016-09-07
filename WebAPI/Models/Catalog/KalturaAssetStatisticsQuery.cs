using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaAssetStatisticsQuery : KalturaOTTObject
    {
        /// <summary>
        /// Comma separated list of asset identifiers.
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlElement(ElementName = "assetIdIn", IsNullable = true)]
        public string AssetIdIn { get; set; }

        /// <summary>
        /// Asset type
        /// </summary>
        [DataMember(Name = "assetTypeEqual")]
        [JsonProperty(PropertyName = "assetTypeEqual")]
        [XmlElement(ElementName = "assetTypeEqual", IsNullable = true)]
        public KalturaAssetType AssetTypeEqual { get; set; }

        /// <summary>
        /// The beginning of the time window to get the statistics for (in epoch). 
        /// </summary>
        [DataMember(Name = "startDateGreaterThanOrEqual")]
        [JsonProperty(PropertyName = "startDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "startDateGreaterThanOrEqual", IsNullable = true)]
        public long StartDateGreaterThanOrEqual { get; set; }

        /// <summary>
        /// /// The end of the time window to get the statistics for (in epoch).
        /// </summary>
        [DataMember(Name = "endDateGreaterThanOrEqual")]
        [JsonProperty(PropertyName = "endDateGreaterThanOrEqual")]
        [XmlElement(ElementName = "endDateGreaterThanOrEqual", IsNullable = true)]
        public long EndDateGreaterThanOrEqual { get; set; }

        internal void Validate()
        {
            if (string.IsNullOrEmpty(AssetIdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaAssetStatisticsQuery.assetIdIn");
            }

            if (AssetTypeEqual == KalturaAssetType.recording)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_ENUM_VALUE_NOT_SUPPORTED, "KalturaAssetStatisticsQuery.assetTypeEqual", "KalturaAssetType.recording");
            }
        }

        internal List<int> getAssetIdIn()
        {
            if (string.IsNullOrEmpty(AssetIdIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetStatisticsQuery.assetIdIn");
                }
            }

            return values;
        }

        
    }
}
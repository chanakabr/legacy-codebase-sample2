using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public enum KalturaRecordingContextOrderBy
    {
        NONE
    }

    /// <summary>
    /// Filtering assets
    /// </summary>
    [Serializable]
    public class KalturaRecordingContextFilter : KalturaFilter<KalturaRecordingContextOrderBy>
    {

        /// <summary>
        /// Comma separated asset ids
        /// </summary>
        [DataMember(Name = "assetIdIn")]
        [JsonProperty(PropertyName = "assetIdIn")]
        [XmlArray(ElementName = "assetIdIn", IsNullable = true)]
        [XmlArrayItem(ElementName = "assetIdIn")]
        public string AssetIdIn { get; set; }

        public override KalturaRecordingContextOrderBy GetDefaultOrderByValue()
        {
            return KalturaRecordingContextOrderBy.NONE;
        }

        internal long[] getAssetIdIn()
        {
            if (string.IsNullOrEmpty(AssetIdIn))
                return null;

            List<long> values = new List<long>();
            string[] stringValues = AssetIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                long value;
                if (long.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaRecordingContextFilter.assetIdIn");
                }
            }

            return values.ToArray();
        }
    }
}
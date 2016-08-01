using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public class KalturaAssetFilter : KalturaFilter<KalturaAssetOrderBy>
    {
        /// <summary>
        /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restriced to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each.
        /// (maximum length of entire filter is 1024 characters)]]>
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        /// <summary>
        /// Comma separated list of EPG channel ids to search within. 
        /// </summary>
        [DataMember(Name = "epgChannelIdIn")]
        [JsonProperty("epgChannelIdIn")]
        [XmlElement(ElementName = "epgChannelIdIn", IsNullable = true)]
        public string EpgChannelIdIn { get; set; }

        public override KalturaAssetOrderBy GetDefaultOrderByValue()
        {
            return KalturaAssetOrderBy.RELEVANCY_DESC;
        }

        /// <summary>
        /// For related media - the ID of the asset for which to return related assets
        /// </summary>
        [DataMember(Name = "relatedMediaIdEqual")]
        [JsonProperty("relatedMediaIdEqual")]
        [XmlElement(ElementName = "relatedMediaIdEqual", IsNullable = true)]
        public string RelatedMediaIdEqual { get; set; }

        internal List<int> getTypeIn()
        {
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


        internal List<int> getEpgChannelIdIn()
        {
            if (string.IsNullOrEmpty(EpgChannelIdIn))
                return null;

            List<int> values = new List<int>();
            string[] stringValues = EpgChannelIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.EpgChannelIdIn contains invalid id {0}", value));
                }
            }

            return values;
        }

        internal void Validate()
        {
            if (!string.IsNullOrEmpty(EpgChannelIdIn) && !string.IsNullOrEmpty(RelatedMediaIdEqual))
                throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.EpgChannelIdIn cannot be used together with filter.RelatedMediaIdEqual"));
        }
    }
}




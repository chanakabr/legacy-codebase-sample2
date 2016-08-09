using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;

namespace WebAPI.Models.Catalog
{
    public class KalturaRelatedFilter : KalturaAssetFilter
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
        /// the ID of the asset for which to return related assets
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual", IsNullable = true)]
        public string IdEqual { get; set; }

        /// <summary>
        /// Comma separated list of asset types to search within. 
        /// Possible values: 0 – EPG linear programs entries, any media type ID (according to media type IDs defined dynamically in the system).
        /// If omitted – all types should be included.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty("typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        public KalturaRelatedFilter()
        {
        }

        private int mediaId { get; set; }

        public int getMediaId()
        {
            if (mediaId == 0)
            {
                int parsed = 0;
                if (!int.TryParse(IdEqual, out parsed))
                {
                    throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "assetId must be a number");
                }
                mediaId = parsed;
            }
            return mediaId;
        }
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
        
        internal override void Validate()
        {
            if (!string.IsNullOrEmpty(IdEqual))
            {
                throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.RelatedMediaIdEqual cannot be used together with filter.RelatedMediaIdEqual"));
            }
            int parsed = 0;
            if (!int.TryParse(IdEqual, out parsed))
            {
                throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "related media id must be numeric");
            }
            mediaId = parsed;
            if (!string.IsNullOrEmpty(this.KSql) && this.KSql.Length > 1024)
            {
                throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, "too long filter");
            }

        }
    }
}
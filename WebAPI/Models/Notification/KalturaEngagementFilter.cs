using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    public enum KalturaEngagementOrderBy
    {
        NONE
    }

    public class KalturaEngagementFilter : KalturaFilter<KalturaEngagementOrderBy>
    {
        public override KalturaEngagementOrderBy GetDefaultOrderByValue()
        {
            return KalturaEngagementOrderBy.NONE;
        }

        /// <summary>
        /// List of inbox message types to search within.
        /// </summary>
        [DataMember(Name = "typeIn")]
        [JsonProperty(PropertyName = "typeIn")]
        [XmlElement(ElementName = "typeIn", IsNullable = true)]
        public string TypeIn { get; set; }

        /// <summary>
        /// createdAtGreaterThanOrEqual
        /// </summary>
        [DataMember(Name = "createdAtGreaterThanOrEqual")]
        [JsonProperty(PropertyName = "createdAtGreaterThanOrEqual")]
        [XmlElement(ElementName = "createdAtGreaterThanOrEqual", IsNullable = true)]
        public long? CreatedAtGreaterThanOrEqual { get; set; }

        /// <summary>
        /// createdAtLessThanOrEqual
        /// </summary>
        [DataMember(Name = "createdAtLessThanOrEqual")]
        [JsonProperty(PropertyName = "createdAtLessThanOrEqual")]
        [XmlElement(ElementName = "createdAtLessThanOrEqual", IsNullable = true)]
        public long? CreatedAtLessThanOrEqual { get; set; }


        internal List<KalturaEngagementType> getTypeIn()
        {
            List<KalturaEngagementType> values = new List<KalturaEngagementType>();

            if (string.IsNullOrEmpty(TypeIn))
                return values;

            string[] stringValues = TypeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            Type enumType = typeof(KalturaEngagementType);
            foreach (string value in stringValues)
            {
                KalturaEngagementType type = (KalturaEngagementType) Enum.Parse(enumType, value, true);
                values.Add(type);
            }

            return values;
        }
    }
}
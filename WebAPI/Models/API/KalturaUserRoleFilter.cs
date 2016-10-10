using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    public enum KalturaUserRoleOrderBy
    {
        NONE
    }

    /// <summary>
    /// User roles filter
    /// </summary>
    public class KalturaUserRoleFilter : KalturaFilter<KalturaUserRoleOrderBy>
    {

        /// <summary>
        /// The roles identifiers
        /// </summary>
        [DataMember(Name = "ids")]
        [JsonProperty("ids")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        [Obsolete]
        public List<KalturaLongValue> Ids { get; set; }

        /// <summary>
        /// Comma separated roles identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }

        public override KalturaUserRoleOrderBy GetDefaultOrderByValue()
        {
            return KalturaUserRoleOrderBy.NONE;
        }

        internal List<long> getIds()
        {
            List<long> values = new List<long>();
            if (Ids != null)
            {
                values.AddRange(Ids.Select(id => id.value));
                return values;
            }

            if (string.IsNullOrEmpty(IdIn))
                return null;

            string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                long value;
                if (long.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaUserRoleFilter.IdIn");
                }
            }

            return values;
        }
    }
}
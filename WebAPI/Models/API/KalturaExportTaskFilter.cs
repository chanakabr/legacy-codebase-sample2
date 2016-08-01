using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Bulk export tasks filter
    /// </summary>
    public class KalturaExportTaskFilter : KalturaFilter<KalturaExportTaskOrderBy>
    {

        /// <summary>
        /// Comma separated tasks identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public string IdIn { get; set; }

        public override KalturaExportTaskOrderBy GetDefaultOrderByValue()
        {
            return KalturaExportTaskOrderBy.CREATE_DATE_ASC;
        }

        internal long[] getIdIn()
        {
            if (string.IsNullOrEmpty(IdIn))
                return null;

            List<long> values = new List<long>();
            string[] stringValues = IdIn.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                long value;
                if (long.TryParse(stringValue, out value))
                {
                    values.Add(value);
                }
                else
                {
                    throw new WebAPI.Exceptions.BadRequestException((int)WebAPI.Managers.Models.StatusCode.BadRequest, string.Format("Filter.IdIn contains invalid id {0}", value));
                }
            }

            return values.ToArray();
        }
    }
}
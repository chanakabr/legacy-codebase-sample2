using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.Pricing
{
    public enum KalturaPpvOrderBy
    {
        NAME_ASC,
        NAME_DESC
    }

    /// <summary>
    /// Filtering Asset Struct Metas
    /// </summary>
    [Serializable]
    public partial class KalturaPpvFilter : KalturaFilter<KalturaPpvOrderBy>
    {
        /// <summary>
        /// Comma separated identifiers
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = true)]
        public string IdIn { get; set; }

        public override KalturaPpvOrderBy GetDefaultOrderByValue()
        {
            return KalturaPpvOrderBy.NAME_ASC;
        }
      
        public List<long> GetIdIn()
        {
            HashSet<long> list = new HashSet<long>();
            if (!string.IsNullOrEmpty(IdIn))
            {
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value) && !list.Contains(value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPpvFilter.idIn");
                    }
                }
            }

            return new List<long>(list);
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Currency filter
    /// </summary>
    public class KalturaCurrencyFilter : KalturaFilter<KalturaCurrencyOrderBy>
    {

        /// <summary>
        /// Currency codes
        /// </summary>
        [DataMember(Name = "codeIn")]
        [JsonProperty("codeIn")]
        [XmlElement(ElementName = "codeIn", IsNullable = true)]
        public string CodeIn { get; set; }

        public override KalturaCurrencyOrderBy GetDefaultOrderByValue()
        {
            return KalturaCurrencyOrderBy.NAME_ASC;
        }

        public List<string> GetCodeIn()
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(CodeIn))
            {
                string[] stringValues = CodeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string currencyCode in stringValues)
                {
                    if (!string.IsNullOrEmpty(currencyCode))
                    {
                        list.Add(currencyCode);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaCurrencyFilter.CodeIn");
                    }
                }
            }

            return list;
        }

    }
}
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
    /// Language filter
    /// </summary>
    public class KalturaLanguageFilter : KalturaFilter<KalturaLanguageOrderBy>
    {

        /// <summary>
        /// Language codes
        /// </summary>
        [DataMember(Name = "codeIn")]
        [JsonProperty("codeIn")]
        [XmlElement(ElementName = "codeIn", IsNullable = true)]
        public string CodeIn { get; set; }

        public override KalturaLanguageOrderBy GetDefaultOrderByValue()
        {
            return KalturaLanguageOrderBy.SYSTEM_NAME_ASC;
        }

        public List<string> GetCodeIn()
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(CodeIn))
            {
                string[] stringValues = CodeIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string languageCode in stringValues)
                {
                    if (!string.IsNullOrEmpty(languageCode))
                    {
                        list.Add(languageCode);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaLanguageFilter.CodeIn");
                    }
                }
            }

            return list;
        }

    }
}
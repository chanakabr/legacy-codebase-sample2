using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// External channel profile filter
    /// </summary>
    public partial class KalturaExternalChannelProfileFilter : KalturaFilter<KalturaExternalChannelProfileOrderBy>
    {
        public override KalturaExternalChannelProfileOrderBy GetDefaultOrderByValue()
        {
            return KalturaExternalChannelProfileOrderBy.NONE;
        }

        internal virtual void Validate() { }
    }

    public partial class KalturaExternalChannelProfileByIdInFilter : KalturaExternalChannelProfileFilter
    {
        /// <summary>
        /// Comma separated external channel profile ids 
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
        public string IdIn { get; set; }

        internal override void Validate()
        {
            if (string.IsNullOrEmpty(IdIn))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "idIn");
            }

            var orderBy = GetDefaultOrderByValue();
            if (OrderBy != orderBy)
            {
                throw new BadRequestException(BadRequestException.INVALID_AGRUMENT_VALUE, "orderBy", orderBy);
            }
        }

        internal List<long> GetIdIn()
        {
            List<long> list = null;

            if (!string.IsNullOrEmpty(IdIn))
            {
                list = new List<long>();
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    if (long.TryParse(stringValue, out long longValue))
                    {
                        list.Add(longValue);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaExternalChannelProfileByIdInFilter.idIn");
                    }
                }
            }

            return list;
        }
    }

    public enum KalturaExternalChannelProfileOrderBy
    {
        NONE
    }
}
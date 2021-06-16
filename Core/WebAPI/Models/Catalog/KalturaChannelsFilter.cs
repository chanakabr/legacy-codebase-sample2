using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Catalog
{
    public partial class KalturaChannelsFilter : KalturaFilter<KalturaChannelsOrderBy>
    {
        /// <summary>
        /// channel identifier to filter by
        /// </summary>
        [DataMember(Name = "idEqual")]
        [JsonProperty("idEqual")]
        [XmlElement(ElementName = "idEqual")]
        [SchemeProperty(MinInteger = 1)]
        public int IdEqual { get; set; }

        /// <summary>
        /// media identifier to filter by
        /// </summary>
        [DataMember(Name = "mediaIdEqual")]
        [JsonProperty("mediaIdEqual")]
        [XmlElement(ElementName = "mediaIdEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ, MinInteger = 1)]        
        public long MediaIdEqual { get; set; }

        /// <summary>
        /// Exact channel name to filter by
        /// </summary>
        [DataMember(Name = "nameEqual")]
        [JsonProperty("nameEqual")]
        [XmlElement(ElementName = "nameEqual")]
        public string NameEqual { get; set; }

        /// <summary>
        /// Channel name starts with (auto-complete)
        /// </summary>
        [DataMember(Name = "nameStartsWith")]
        [JsonProperty("nameStartsWith")]
        [XmlElement(ElementName = "nameStartsWith")]
        public string NameStartsWith { get; set; }

        /// <summary>
        /// Comma separated channel ids 
        /// </summary>
        [DataMember(Name = "idIn")]
        [JsonProperty("idIn")]
        [XmlElement(ElementName = "idIn", IsNullable = false)]
        public string IdIn { get; set; }

        internal void Validate()
        {
            List<string> message = new List<string>();
            int inputCount = 0;

            if (MediaIdEqual > 0)
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.mediaIdEqual");
            }

            if (IdEqual > 0)
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.idEqual");
                ValidateCheck(message, inputCount);
            }

            if (!string.IsNullOrEmpty(NameEqual))
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.nameEqual");
                ValidateCheck(message, inputCount);
            }

            if (!string.IsNullOrEmpty(NameStartsWith))
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.nameStartsWith");
                ValidateCheck(message, inputCount);
            }

            if (!string.IsNullOrEmpty(IdIn))
            {
                inputCount++;
                message.Add("KalturaChannelsFilter.idIn");
                ValidateCheck(message, inputCount);
            }            
        }

        private static void ValidateCheck(List<string> message, int inputCount)
        {
            if (inputCount > 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, message[0], message[1]);
            }
        }

        public override KalturaChannelsOrderBy GetDefaultOrderByValue()
        {
            return KalturaChannelsOrderBy.NONE;
        }

        internal List<int> GetIdIn()
        {
            List<int> list = null;

            if (!string.IsNullOrEmpty(IdIn))
            {
                list = new List<int>();
                string[] stringValues = IdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    if (int.TryParse(stringValue, out int value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaChannelsFilter.idIn");
                    }
                }
            }

            return list;
        }

    }

    public enum KalturaChannelsOrderBy
    {
        NONE,
        NAME_ASC,
        NAME_DESC,
        CREATE_DATE_ASC,
        CREATE_DATE_DESC,
        UPDATE_DATE_ASC,
        UPDATE_DATE_DESC
    }
}
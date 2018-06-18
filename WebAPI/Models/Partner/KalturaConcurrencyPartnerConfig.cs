using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Partner;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner concurrency configuration
    /// </summary>
    public class KalturaConcurrencyPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Comma separated list of device Family Ids order by their priority.
        /// </summary>
        [DataMember(Name = "deviceFamilyIds")]
        [JsonProperty("deviceFamilyIds")]
        [XmlElement(ElementName = "deviceFamilyIds")]
        public string DeviceFamilyIds { get; set; }
        
        /// <summary>
        /// Priority By FIFO
        /// </summary>
        [DataMember(Name = "priorityByFIFO")]
        [JsonProperty("priorityByFIFO")]
        [XmlElement(ElementName = "priorityByFIFO")]
        public bool PriorityByFIFO { get; set; }
        
        internal HashSet<int> GetDeviceFamilyIds()
        {
            if (string.IsNullOrEmpty(DeviceFamilyIds))
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "deviceFamilyIds");

            HashSet<int> values = new HashSet<int>();
            string[] stringValues = DeviceFamilyIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                int value;
                if (int.TryParse(stringValue, out value) && value != 0)
                {
                    if (!values.Add(value))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "deviceFamilyIds");
                    }
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "deviceFamilyIds");
                }
            }

            return values;
        }
    }
}
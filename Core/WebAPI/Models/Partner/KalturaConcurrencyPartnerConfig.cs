using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner concurrency configuration
    /// </summary>
    public partial class KalturaConcurrencyPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Comma separated list of device Family Ids order by their priority.
        /// </summary>
        [DataMember(Name = "deviceFamilyIds")]
        [JsonProperty("deviceFamilyIds")]
        [XmlElement(ElementName = "deviceFamilyIds")]
        public string DeviceFamilyIds { get; set; }

        /// <summary>
        /// Policy of eviction devices
        /// </summary>
        [DataMember(Name = "evictionPolicy")]
        [JsonProperty("evictionPolicy")]
        [XmlElement(ElementName = "evictionPolicy")]
        public KalturaEvictionPolicyType EvictionPolicy { get; set; }
        
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

        internal override bool Update(int groupId)
        {
            return ClientsManager.ApiClient().UpdateConcurrencyPartner(groupId, this);
        }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.Concurrency; } }
    }
    
    public enum KalturaEvictionPolicyType
    {
        FIFO,
        LIFO
    }
}
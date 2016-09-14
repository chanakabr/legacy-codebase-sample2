using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Filters;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Domains
{
    public class KalturaHouseholdDeviceFilter : KalturaFilter<KalturaHouseholdDeviceOrderBy>
    {

        /// <summary>
        /// The identifier of the household
        /// </summary>
        [DataMember(Name = "householdIdEqual")]
        [JsonProperty("householdIdEqual")]
        [XmlElement(ElementName = "householdIdEqual")]
        [SchemeProperty(RequiresPermission = (int)RequestType.READ)]
        public int? HouseholdIdEqual { get; set; }

        /// <summary>
        /// Device family Ids
        /// </summary>
        [DataMember(Name = "deviceFamilyIdIn")]
        [JsonProperty(PropertyName = "deviceFamilyIdIn")]
        [XmlArray(ElementName = "deviceFamilyIdIn", IsNullable = true)]        
        public string DeviceFamilyIdIn { get; set; }

        public override KalturaHouseholdDeviceOrderBy GetDefaultOrderByValue()
        {
            return KalturaHouseholdDeviceOrderBy.NONE;
        }

        public List<long> ConvertDeviceFamilyIdIn()
        {
            List<long> deviceFamilyIds = null;
            if (!string.IsNullOrEmpty(DeviceFamilyIdIn))
            {
                string[] unparsedDeviceFamilyIds = DeviceFamilyIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                deviceFamilyIds = new List<long>();
                foreach (string deviceFamilyIdToParse in unparsedDeviceFamilyIds)
                {
                    long id;
                    if (long.TryParse(deviceFamilyIdToParse, out id))
                    {
                        deviceFamilyIds.Add(id);
                    }
                }
            }

            return deviceFamilyIds;

        }

    }

    public enum KalturaHouseholdDeviceOrderBy
    {
        NONE                
    }
}

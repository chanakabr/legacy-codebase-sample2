using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Partner
{
    public partial class KalturaRollingDeviceRemovalData : KalturaOTTObject
    {
        /// <summary>
        /// Rolling Device Policy
        /// </summary>
        [DataMember(Name = "rollingDeviceRemovalPolicy")]
        [JsonProperty("rollingDeviceRemovalPolicy")]
        [XmlElement(ElementName = "rollingDeviceRemovalPolicy")]
        [SchemeProperty(IsNullable = true)]
        public KalturaRollingDevicePolicy? RollingDeviceRemovalPolicy { get; set; }

        /// <summary>
        /// Rolling Device Policy in a CSV style
        /// </summary>
        [DataMember(Name = "rollingDeviceRemovalFamilyIds")]
        [JsonProperty("rollingDeviceRemovalFamilyIds")]
        [XmlElement(ElementName = "rollingDeviceRemovalFamilyIds")]
        public string RollingDeviceRemovalFamilyIds { get; set; }
        
        internal List<int> GetRollingDeviceRemovalFamilyIds()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(RollingDeviceRemovalFamilyIds, "KalturaRollingDeviceRemovalData.RollingDeviceRemovalFamilyIds", false, false);
        }
    }
}
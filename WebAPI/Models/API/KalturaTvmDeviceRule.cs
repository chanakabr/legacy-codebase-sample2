using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// TVM geo rule
    /// </summary>
    [Serializable]
    public partial class KalturaTvmDeviceRule : KalturaTvmRule
    {
        /// <summary>
        /// Comma separated list of country Ids.
        /// </summary>
        [DataMember(Name = "deviceBrandIds")]
        [JsonProperty("deviceBrandIds")]
        [XmlElement(ElementName = "deviceBrandIds")]
        public string DeviceBrandIds { get; set; }
        
        protected override void Init()
        {
            base.Init();
            this.RuleType = KalturaRuleType.device;
        }

        public HashSet<int> GetDeviceBrandIds()
        {
            return this.GetItemsIn<HashSet<int>, int>(DeviceBrandIds, "KalturaTvmDeviceRule.deviceBrandIds");
        }
    }
}
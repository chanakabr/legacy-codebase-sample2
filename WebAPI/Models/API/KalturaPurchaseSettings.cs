using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Xml.Serialization;
using WebAPI.Managers.Schema;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Purchase settings and PIN
    /// </summary>
    [Serializable]
    public class KalturaPurchaseSettings : KalturaPin
    {
        /// <summary>
        /// Purchase permission - block, ask or allow
        /// </summary>
        [DataMember(Name = "permission")]
        [JsonProperty(PropertyName = "permission", NullValueHandling = NullValueHandling.Ignore)]
        [XmlElement(ElementName = "permission", IsNullable = true)]
        public KalturaPurchaseSettingsType? Permission
        {
            get;
            set;
        }
    }
}

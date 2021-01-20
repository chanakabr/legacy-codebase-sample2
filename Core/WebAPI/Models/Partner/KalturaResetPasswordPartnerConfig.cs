using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Models.General;


namespace WebAPI.Models.Partner
{
    public partial class KalturaResetPasswordPartnerConfig : KalturaOTTObject
    {
        /// <summary>
        /// template List Label
        /// </summary>
        [DataMember(Name = "templateListLabel")]
        [JsonProperty("templateListLabel")]
        [XmlElement(ElementName = "templateListLabel")]
        public string TemplateListLabel { get; set; }

        /// <summary>
        /// templates
        /// </summary>
        [DataMember(Name = "templates")]
        [JsonProperty("templates")]
        [XmlElement(ElementName = "templates")]
        public List<KalturaResetPasswordPartnerConfigTemplate> Templates { get; set; }
    }
}
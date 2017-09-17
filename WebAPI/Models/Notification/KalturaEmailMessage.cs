using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Notification
{
    [DataContract(Name = "KalturaEmailMessage", Namespace = "")]
    [XmlRoot("KalturaEmailMessage")]
    public class KalturaEmailMessage : KalturaOTTObject
    {
        /// <summary>
        /// email template name 
        /// </summary>
        [DataMember(Name = "templateName")]
        [JsonProperty("templateName")]
        [XmlElement(ElementName = "templateName")]
        public string TemplateName { get; set; }

        /// <summary>
        /// email subject
        /// </summary>
        [DataMember(Name = "subject")]
        [JsonProperty("subject")]
        [XmlElement(ElementName = "subject")]
        public string Subject { get; set; }
        
        /// <summary>
        ///  first name
        /// </summary>
        [DataMember(Name = "firstName")]
        [JsonProperty("firstName")]
        [XmlElement(ElementName = "firstName")]
        public string FirstName { get; set; }

        /// <summary>
        ///last name
        /// </summary>
        [DataMember(Name = "lastName")]
        [JsonProperty("lastName")]
        [XmlElement(ElementName = "lastName")]
        public string LastName { get; set; }

        /// <summary>
        ///sender name
        /// </summary>
        [DataMember(Name = "senderName")]
        [JsonProperty("senderName")]
        [XmlElement(ElementName = "senderName")]
        public string SenderName { get; set; }

        /// <summary>
        ///sender from
        /// </summary>
        [DataMember(Name = "senderFrom")]
        [JsonProperty("senderFrom")]
        [XmlElement(ElementName = "senderFrom")]
        public string SenderFrom { get; set; }

        /// <summary>
        ///sender to
        /// </summary>
        [DataMember(Name = "senderTo")]
        [JsonProperty("senderTo")]
        [XmlElement(ElementName = "senderTo")]
        public string SenderTo { get; set; }

        /// <summary>
        ///bcc address - seperated by comma
        /// </summary>
        [DataMember(Name = "bccAddress")]
        [JsonProperty("bccAddress")]
        [XmlElement(ElementName = "bccAddress")]
        public string BccAddress { get; set; }

        /// <summary>
        ///extra parameters
        /// </summary>
        [DataMember(Name = "extraParameters")]
        [JsonProperty("extraParameters")]
        [XmlElement(ElementName = "extraParameters")]       
        public List<KalturaKeyValue> ExtraParameters { get; set; }


    }
}
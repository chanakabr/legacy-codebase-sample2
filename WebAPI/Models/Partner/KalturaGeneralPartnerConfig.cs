using Newtonsoft.Json;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace WebAPI.Models.Partner
{
    /// <summary>
    /// Partner General configuration
    /// </summary>
    public partial class KalturaGeneralPartnerConfig : KalturaPartnerConfiguration
    {
        /// <summary>
        /// Partner name
        /// </summary>
        [DataMember(Name = "partnerName")]
        [JsonProperty("partnerName")]
        [XmlElement(ElementName = "partnerName")]
        public string PartnerName { get; set; }

        /// <summary>
        /// Main metadata language
        /// </summary>
        [DataMember(Name = "mainLanguage")]
        [JsonProperty("mainLanguage")]
        [XmlElement(ElementName = "mainLanguage")]
        public int? MainLanguage { get; set; }

        /// <summary>
        /// A list of comma separated languages ids.        
        /// </summary>
        [DataMember(Name = "secondaryLanguages")]
        [JsonProperty("secondaryLanguages")]
        [XmlElement(ElementName = "secondaryLanguages")]        
        public string SecondaryLanguages { get; set; }

        /// <summary>
        /// Delete media policy
        /// </summary>
        [DataMember(Name = "deleteMediaPolicy")]
        [JsonProperty("deleteMediaPolicy")]
        [XmlElement(ElementName = "deleteMediaPolicy")]
        public KalturaDeleteMediaPolicy? DeleteMediaPolicy { get; set; }

        /// <summary>
        /// Main currency
        /// </summary>
        [DataMember(Name = "mainCurrency")]
        [JsonProperty("mainCurrency")]
        [XmlElement(ElementName = "mainCurrency")]
        public int? MainCurrency { get; set; }

        /// <summary>
        /// A list of comma separated currency ids.
        /// </summary>
        [DataMember(Name = "secondaryCurrencies")]
        [JsonProperty("secondaryCurrencies")]
        [XmlElement(ElementName = "secondaryCurrencies")]
        [XmlArrayItem("item")]
        public string SecondaryCurrencies { get; set; }

        /// <summary>
        /// Downgrade policy
        /// </summary>
        [DataMember(Name = "downgradePolicy")]
        [JsonProperty("downgradePolicy")]
        [XmlElement(ElementName = "downgradePolicy")]
        public KalturaDowngradePolicy? DowngradePolicy { get; set; }

        /// <summary>
        /// Mail settings
        /// </summary>
        [DataMember(Name = "mailSettings")]
        [JsonProperty("mailSettings")]
        [XmlElement(ElementName = "mailSettings")]
        public string MailSettings { get; set; }

        /// <summary>
        /// Default Date Format for Email notifications (default should be: DD Month YYYY)
        /// </summary>
        [DataMember(Name = "dateFormat")]
        [JsonProperty("dateFormat")]
        [XmlElement(ElementName = "dateFormat")]
        public string DateFormat { get; set; }

        /// <summary>
        /// Household limitation module
        /// </summary>
        [DataMember(Name = "householdLimitationModule")]
        [JsonProperty("householdLimitationModule")]
        [XmlElement(ElementName = "householdLimitationModule")]
        public int? HouseholdLimitationModule { get; set; } 
    }

    public enum KalturaDeleteMediaPolicy { Disable = 0, Delete = 1 }

    public enum KalturaDowngradePolicy { LIFO = 0, FIFO = 1 }
}
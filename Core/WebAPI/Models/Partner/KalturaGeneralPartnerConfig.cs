using Newtonsoft.Json;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.ClientManagers.Client;
using WebAPI.Managers.Scheme;

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
        [SchemeProperty(IsNullable = true)]
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
        [SchemeProperty(IsNullable = true)]
        public KalturaDeleteMediaPolicy? DeleteMediaPolicy { get; set; }

        /// <summary>
        /// Main currency
        /// </summary>
        [DataMember(Name = "mainCurrency")]
        [JsonProperty("mainCurrency")]
        [XmlElement(ElementName = "mainCurrency")]
        [SchemeProperty(IsNullable = true)]
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
        [SchemeProperty(IsNullable = true)]
        public KalturaDowngradePolicy? DowngradePolicy { get; set; }

        /// <summary>
        /// Priority Family Ids to remove devices on downgrade (first in the list first to remove)
        /// </summary>
        [DataMember(Name = "downgradePriorityFamilyIds")]
        [JsonProperty("downgradePriorityFamilyIds")]
        [XmlElement(ElementName = "downgradePriorityFamilyIds")]
        public string DowngradePriorityFamilyIds { get; set; }
        
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
        [SchemeProperty(IsNullable = true)]
        public int? HouseholdLimitationModule { get; set; }

        /// <summary>
        /// Enable Region Filtering
        /// </summary>
        [DataMember(Name = "enableRegionFiltering")]
        [JsonProperty("enableRegionFiltering")]
        [XmlElement(ElementName = "enableRegionFiltering")]
        [SchemeProperty(IsNullable = true)]
        public bool? EnableRegionFiltering { get; set; }

        /// <summary>
        /// Default Region
        /// </summary>
        [DataMember(Name = "defaultRegion")]
        [JsonProperty("defaultRegion")]
        [XmlElement(ElementName = "defaultRegion")]
        [SchemeProperty(IsNullable = true)]
        public int? DefaultRegion { get; set; }

        /// <summary>
        /// Rolling Device Policy
        /// </summary>
        [DataMember(Name = "rollingDeviceData")]
        [JsonProperty("rollingDeviceData")]
        [XmlElement(ElementName = "rollingDeviceData")]
        [SchemeProperty(IsNullable = true)]
        public KalturaRollingDeviceRemovalData RollingDeviceRemovalData { get; set; }

        /// <summary>
        /// minimum bookmark position of a linear channel to be included in a watch history 
        /// </summary>
        [DataMember(Name = "linearWatchHistoryThreshold")]
        [JsonProperty("linearWatchHistoryThreshold")]
        [XmlElement(ElementName = "linearWatchHistoryThreshold")]
        public int? LinearWatchHistoryThreshold { get; set; }

        /// <summary>
        /// Finished PercentThreshold
        /// </summary>
        [DataMember(Name = "finishedPercentThreshold")]
        [JsonProperty("finishedPercentThreshold")]
        [XmlElement(ElementName = "finishedPercentThreshold")]
        [SchemeProperty(MinInteger = 90, MaxInteger = 99, IsNullable = true)]
        public int? FinishedPercentThreshold { get; set; }

        /// <summary>
        /// Suspension Profile Inheritance
        /// </summary>
        [DataMember(Name = "suspensionProfileInheritanceType")]
        [JsonProperty("suspensionProfileInheritanceType")]
        [XmlElement(ElementName = "suspensionProfileInheritanceType")]
        [SchemeProperty(IsNullable = true)]
        public KalturaSuspensionProfileInheritanceType? SuspensionProfileInheritanceType { get; set; }

        /// <summary>
        /// Allow Device Mobility
        /// </summary>
        [DataMember(Name = "allowDeviceMobility")]
        [JsonProperty("allowDeviceMobility")]
        [XmlElement(ElementName = "allowDeviceMobility")]
        [SchemeProperty(IsNullable = true)]
        public bool? AllowDeviceMobility { get; set; }
        
        /// <summary>
        /// Enable multi LCNs per linear channel
        /// </summary>
        [DataMember(Name = "enableMultiLcns")]
        [JsonProperty("enableMultiLcns")]
        [XmlElement(ElementName = "enableMultiLcns")]
        [SchemeProperty(IsNullable = true)]
        public bool? EnableMultiLcns { get; set; }

        internal List<int> GetSecondaryLanguagesIds()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(SecondaryLanguages, "KalturaGeneralPartnerConfig.secondaryLanguages", false, false);
        }

        internal List<int> GetSecondaryCurrenciesIds()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(SecondaryCurrencies, "KalturaGeneralPartnerConfig.secondaryCurrencies", false, false);
        }
        internal List<int> GetDowngradePriorityFamilyIds()
        {
            return Utils.Utils.ParseCommaSeparatedValues<List<int>, int>(DowngradePriorityFamilyIds, "KalturaRollingDeviceRemovalData.DowngradePriorityFamilyIds", false, false);
        }

        internal override bool Update(int groupId)
        {
            return ClientsManager.ApiClient().UpdateGeneralPartnerConfiguration(groupId, this);
        }

        protected override KalturaPartnerConfigurationType ConfigurationType { get { return KalturaPartnerConfigurationType.General; } }
    }

    public enum KalturaDeleteMediaPolicy { Disable = 0, Delete = 1 }

    public enum KalturaDowngradePolicy { LIFO = 0, FIFO = 1, ACTIVE_DATE = 2 }

    public enum KalturaRollingDevicePolicy
    {
        NONE = 0,
        LIFO = 1,
        FIFO = 2,
        ACTIVE_DEVICE_ASCENDING = 3
    }

    public enum KalturaSuspensionProfileInheritanceType
    {
        ALWAYS = 1,
        NEVER = 2,
        DEFAULT = 3
    }
}

using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User
    /// </summary>
    [DataContract(Name = "user")]
    [OldStandard("householdId", "household_id")]
    [OldStandard("affiliateCode", "affiliate_code")]
    [OldStandard("externalId", "external_id")]
    [OldStandard("userType", "user_type")]
    [OldStandard("dynamicData", "dynamic_data")]
    [OldStandard("isHouseholdMaster", "is_household_master")]
    [OldStandard("suspentionState", "suspention_state")]
    [OldStandard("userState", "user_state")]
    [OldStandard("facebookId", "facebook_id")]
    [OldStandard("facebookImage", "facebook_image")]
    [OldStandard("facebookToken", "facebook_token")]
    public class KalturaOTTUser : KalturaBaseOTTUser
    {
        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "householdId")]
        [JsonProperty("householdId")]
        [XmlElement(ElementName = "householdId")]
        [SchemeProperty(ReadOnly = true)]
        public int? HouseholdID { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [DataMember(Name = "email")]
        [JsonProperty("email")]
        [XmlElement(ElementName = "email")]
        public string Email { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [DataMember(Name = "address")]
        [JsonProperty("address")]
        [XmlElement(ElementName = "address")]
        public string Address { get; set; }

        /// <summary>
        /// City
        /// </summary>
        [DataMember(Name = "city")]
        [JsonProperty("city")]
        [XmlElement(ElementName = "city")]
        public string City { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [DataMember(Name = "country")]
        [JsonProperty("country")]
        [XmlElement(ElementName = "country", IsNullable = true)]
        [Obsolete]
        public KalturaCountry Country { get; set; }

        /// <summary>
        /// Country identifier
        /// </summary>
        [DataMember(Name = "countryId")]
        [JsonProperty("countryId")]
        [XmlElement(ElementName = "countryId", IsNullable = true)]
        public int? CountryId { get; set; }

        /// <summary>
        /// Zip code
        /// </summary>
        [DataMember(Name = "zip")]
        [JsonProperty("zip")]
        [XmlElement(ElementName = "zip")]
        public string Zip { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        [DataMember(Name = "phone")]
        [JsonProperty("phone")]
        [XmlElement(ElementName = "phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Facebook identifier
        /// </summary>
        [DataMember(Name = "facebookId")]
        [JsonProperty("facebookId")]
        [XmlElement(ElementName = "facebookId")]
        [Obsolete]
        public string FacebookId { get; set; }

        /// <summary>
        /// Facebook image
        /// </summary>
        [DataMember(Name = "facebookImage")]
        [JsonProperty("facebookImage")]
        [XmlElement(ElementName = "facebookImage")]
        [Obsolete]
        public string FacebookImage { get; set; }

        /// <summary>
        /// Affiliate code
        /// </summary>
        [DataMember(Name = "affiliateCode")]
        [JsonProperty("affiliateCode")]
        [XmlElement(ElementName = "affiliateCode")]
        [SchemeProperty(InsertOnly = true)]
        public string AffiliateCode { get; set; }

        /// <summary>
        /// Facebook token
        /// </summary>
        [DataMember(Name = "facebookToken")]
        [JsonProperty("facebookToken")]
        [XmlElement(ElementName = "facebookToken")]
        [Obsolete]
        public string FacebookToken { get; set; }

        /// <summary>
        /// External user identifier
        /// </summary>
        [DataMember(Name = "externalId")]
        [JsonProperty("externalId")]
        [XmlElement(ElementName = "externalId")]
        [SchemeProperty(InsertOnly = true)]
        public string ExternalId { get; set; }

        /// <summary>
        /// User type
        /// </summary>
        [DataMember(Name = "userType")]
        [JsonProperty("userType")]
        [XmlElement(ElementName = "userType", IsNullable = true)]
        public KalturaOTTUserType UserType { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamicData")]
        [JsonProperty("dynamicData")]
        [XmlElement(ElementName = "dynamicData", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }

        /// <summary>
        /// Is the user the household master
        /// </summary>
        [DataMember(Name = "isHouseholdMaster")]
        [JsonProperty("isHouseholdMaster")]
        [XmlElement(ElementName = "isHouseholdMaster")]
        [SchemeProperty(ReadOnly = true)]
        public bool? IsHouseholdMaster { get; set; }

        /// <summary>
        /// Suspension state
        /// </summary>
        [DataMember(Name = "suspentionState")]
        [JsonProperty("suspentionState")]
        [XmlElement(ElementName = "suspentionState")]
        [SchemeProperty(ReadOnly = true)]
        [Obsolete]
        public KalturaHouseholdSuspentionState SuspentionState { get; set; }


        /// <summary>
        /// Suspension state
        /// </summary>
        [DataMember(Name = "suspensionState")]
        [JsonProperty("suspensionState")]
        [XmlElement(ElementName = "suspensionState")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaHouseholdSuspensionState SuspensionState { get; set; }

        /// <summary>
        /// User state
        /// </summary>
        [DataMember(Name = "userState")]
        [JsonProperty("userState")]
        [XmlElement(ElementName = "userState")]
        [SchemeProperty(ReadOnly = true)]
        public KalturaUserState UserState { get; set; }

        internal int getHouseholdID()
        {
            return HouseholdID.HasValue ? (int) HouseholdID : 0;
        }
    }
}
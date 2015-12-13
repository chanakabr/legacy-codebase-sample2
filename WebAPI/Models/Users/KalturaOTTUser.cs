using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User
    /// </summary>
    [DataContract(Name = "user")]
    public class KalturaOTTUser : KalturaBaseOTTUser
    {
        /// <summary>
        /// Household identifier
        /// </summary>
        [DataMember(Name = "household_id")]
        [JsonProperty("household_id")]
        [XmlElement(ElementName = "household_id")]
        public int HouseholdID { get; set; }

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
        public KalturaCountry Country { get; set; }

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
        [DataMember(Name = "facebook_id")]
        [JsonProperty("facebook_id")]
        [XmlElement(ElementName = "facebook_id")]
        public string FacebookId { get; set; }

        /// <summary>
        /// Facebook image
        /// </summary>
        [DataMember(Name = "facebook_image")]
        [JsonProperty("facebook_image")]
        [XmlElement(ElementName = "facebook_image")]
        public string FacebookImage { get; set; }

        /// <summary>
        /// Affiliate code
        /// </summary>
        [DataMember(Name = "affiliate_code")]
        [JsonProperty("affiliate_code")]
        [XmlElement(ElementName = "affiliate_code")]
        public string AffiliateCode { get; set; }

        /// <summary>
        /// Facebook token
        /// </summary>
        [DataMember(Name = "facebook_token")]
        [JsonProperty("facebook_token")]
        [XmlElement(ElementName = "facebook_token")]
        public string FacebookToken { get; set; }

        /// <summary>
        /// External user identifier
        /// </summary>
        [DataMember(Name = "external_id")]
        [JsonProperty("external_id")]
        [XmlElement(ElementName = "external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// User type
        /// </summary>
        [DataMember(Name = "user_type")]
        [JsonProperty("user_type")]
        [XmlElement(ElementName = "user_type", IsNullable = true)]
        public KalturaOTTUserType UserType { get; set; }

        /// <summary>
        /// Dynamic data
        /// </summary>
        [DataMember(Name = "dynamic_data")]
        [JsonProperty("dynamic_data")]
        [XmlElement(ElementName = "dynamic_data", IsNullable = true)]
        public SerializableDictionary<string, KalturaStringValue> DynamicData { get; set; }

        /// <summary>
        /// Is the user the household master
        /// </summary>
        [DataMember(Name = "is_household_master")]
        [JsonProperty("is_household_master")]
        [XmlElement(ElementName = "is_household_master")]
        public bool IsHouseholdMaster { get; set; }

        /// <summary>
        /// Suspention state
        /// </summary>
        [DataMember(Name = "suspention_state")]
        [JsonProperty("suspention_state")]
        [XmlElement(ElementName = "suspention_state")]
        public KalturaHouseholdSuspentionState SuspentionState { get; set; }

        /// <summary>
        /// User state
        /// </summary>
        [DataMember(Name = "user_state")]
        [JsonProperty("user_state")]
        [XmlElement(ElementName = "user_state")]
        public KalturaUserState UserState { get; set; }

        /// <summary>
        /// User role identifier
        /// </summary>
        [DataMember(Name = "user_role_id")]
        [JsonProperty("user_role_id")]
        [XmlElement(ElementName = "user_role_id")]
        public long UserRoleId { get; set; }
    }
}
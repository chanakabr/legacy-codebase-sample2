using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.Users
{
    /// <summary>
    /// User basic data
    /// </summary>
    public class KalturaUserBasicData
    {
        /// <summary>
        /// Username
        /// </summary>
        [DataMember(Name = "username")]
        [JsonProperty("username")]
        public string Username { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [DataMember(Name = "first_name")]
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        /// <summary>
        /// Last name
        /// </summary>
        [DataMember(Name = "last_name")]
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        /// <summary>
        /// Email
        /// </summary>
        [DataMember(Name = "email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        /// <summary>
        /// Address
        /// </summary>
        [DataMember(Name = "address")]
        [JsonProperty("address")]
        public string Address { get; set; }

        /// <summary>
        /// City
        /// </summary>
        [DataMember(Name = "city")]
        [JsonProperty("city")]
        public string City { get; set; }

        /// <summary>
        /// Country
        /// </summary>
        [DataMember(Name = "country")]
        [JsonProperty("country")]
        public KalturaCountry Country { get; set; }

        /// <summary>
        /// Zip code
        /// </summary>
        [DataMember(Name = "zip")]
        [JsonProperty("zip")]
        public string Zip { get; set; }

        /// <summary>
        /// Phone
        /// </summary>
        [DataMember(Name = "phone")]
        [JsonProperty("phone")]
        public string Phone { get; set; }

        /// <summary>
        /// Facebook identifier
        /// </summary>
        [DataMember(Name = "facebook_id")]
        [JsonProperty("facebook_id")]
        public string FacebookId { get; set; }

        /// <summary>
        /// Facebook image
        /// </summary>
        [DataMember(Name = "facebook_image")]
        [JsonProperty("facebook_image")]
        public string FacebookImage { get; set; }

        /// <summary>
        /// Affiliate code
        /// </summary>
        [DataMember(Name = "affiliate_code")]
        [JsonProperty("affiliate_code")]
        public string AffiliateCode { get; set; }

        /// <summary>
        /// Facebook token
        /// </summary>
        [DataMember(Name = "facebook_token")]
        [JsonProperty("facebook_token")]
        public string FacebookToken { get; set; }

        /// <summary>
        /// External user identifier
        /// </summary>
        [DataMember(Name = "external_id")]
        [JsonProperty("external_id")]
        public string ExternalId { get; set; }

        /// <summary>
        /// User type
        /// </summary>
        [DataMember(Name = "user_type")]
        [JsonProperty("user_type")]
        public KalturaUserType UserType { get; set; }

        //private Country m_CountryField;
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using AdapaterCommon.Models;

namespace SSOAdapter.Models
{
    [DataContract]
    public class User
    {
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public string FirstName { get; set; }
        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public int? HouseholdID { get; set; }
        [DataMember]
        public string Email { get; set; }
        [DataMember]
        public string Address { get; set; }
        [DataMember]
        public string City { get; set; }
        [DataMember]
        public int? CountryId { get; set; }
        [DataMember]
        public string Zip { get; set; }
        [DataMember]
        public string Phone { get; set; }
        [DataMember]
        public string ExternalId { get; set; }
        [DataMember]
        public UserType UserType { get; set; }
        [DataMember]
        public IDictionary<string, string> DynamicData { get; set; }
        [DataMember]
        public bool? IsHouseholdMaster { get; set; }
        [DataMember]
        public eHouseholdSuspensionState SuspensionState { get; set; }
        [DataMember]
        public eUserState UserState { get; set; }
    }
}
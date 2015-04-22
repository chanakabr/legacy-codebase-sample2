using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models
{
    /// <summary>
    /// User
    /// </summary>
    public class User
    {
        /// <summary>
        /// ID
        /// </summary>
        [DataMember(Name="id")]
        [Required]
        public string ID { get; set; }

        /// <summary>
        /// Domain ID
        /// </summary>
        [DataMember(Name = "domain_id")]
        public string DomainID { get; set; }

        /// <summary>
        /// First name
        /// </summary>
        [DataMember(Name = "first_name")]
        public string FirstName { get; set; }
    }
}
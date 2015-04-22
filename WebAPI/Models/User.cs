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
        /// Age
        /// </summary>
        [DataMember(Name = "age")]
        [Range(0, 100)]
        public int Age { get; set; }

        /// <summary>
        /// Date
        /// </summary>
        [DataMember(Name = "date")]
        public DateTime Date { get; set; }
    }
}
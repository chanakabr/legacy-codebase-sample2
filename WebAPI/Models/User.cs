using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
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
        [JsonProperty(PropertyName = "id")]
        [Required]
        public string ID { get; set; }

        /// <summary>
        /// Age
        /// </summary>
        [JsonProperty(PropertyName = "age")]        
        [Range(0, 100)]
        public int Age { get; set; }
    }
}
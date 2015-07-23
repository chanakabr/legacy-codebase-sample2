using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;

namespace WebAPI.Models.ConditionalAccess
{
    /// <summary>
    /// Charge user for PPV request 
    /// </summary>
    public class KalturaChargePPV : KalturaCharge
    {
        /// <summary>
        /// Media file identifier
        /// </summary>
        [DataMember(Name = "file_id")]
        [JsonProperty("file_id")]
        public int FileId { get; set; }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaCollectionEntitlement : KalturaEntitlement
    {
        /// <summary>
        /// View Life Cycle
        /// </summary>
        [DataMember(Name = "viewLifeCycle")]
        [JsonProperty("viewLifeCycle")]
        [XmlElement(ElementName = "viewLifeCycle")]
        public int ViewLifeCycle { get; set; }

        /// <summary>
        /// Collection Start Date
        /// </summary>
        [DataMember(Name = "collectionStartDate")]
        [JsonProperty("collectionStartDate")]
        [XmlElement(ElementName = "collectionStartDate")]
        public long CollectionStartDate { get; set; }

        /// <summary>
        /// Collection End Date
        /// </summary>
        [DataMember(Name = "collectionEndDate")]
        [JsonProperty("collectionEndDate")]
        [XmlElement(ElementName = "collectionEndDate")]
        public long CollectionEndDate { get; set; }

        /// <summary>
        /// Create And Updat eDate
        /// </summary>
        [DataMember(Name = "createAndUpdateDate")]
        [JsonProperty("createAndUpdateDate")]
        [XmlElement(ElementName = "createAndUpdateDate")]
        public long CreateAndUpdateDate { get; set; }
    }
}
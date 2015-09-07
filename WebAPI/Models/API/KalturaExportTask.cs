using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Bulk export task
    /// </summary>
    public class KalturaExportTask : KalturaOTTObject
    {
        /// <summary>
        /// Task identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        public long Id { get; set; }

        /// <summary>
        /// External key for the task used to solicit an export using API
        /// </summary>
        [DataMember(Name = "external_key")]
        [JsonProperty("external_key")]
        [XmlElement(ElementName = "external_key")]
        public string ExternalKey { get; set; }

        /// <summary>
        /// Task display name
        /// </summary>
        [DataMember(Name = "name")]
        [JsonProperty("name")]
        [XmlElement(ElementName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// The data type exported in this task
        /// </summary>
        [DataMember(Name = "data_type")]
        [JsonProperty("data_type")]
        [XmlElement(ElementName = "data_type")]
        public KalturaExportDataType DataType { get; set; }

        /// <summary>
        /// Filter to apply on the export, utilize KSQL.
        ///Note: KSQL currently applies to media assets only. It cannot be used for USERS filtering
        /// </summary>
        [DataMember(Name = "filter")]
        [JsonProperty("filter")]
        [XmlElement(ElementName = "filter")]
        public string Filter { get; set; }

        
        /// <summary>
        /// Type of export batch – full or incremental
        /// </summary>
        [DataMember(Name = "export_type")]
        [JsonProperty("export_type")]
        [XmlElement(ElementName = "export_type")]
        public KalturaExportType ExportType { get; set; }

        /// <summary>
        /// How often the export should occur, reasonable minimum threshold should apply, configurable in minutes
        /// </summary>
        [DataMember(Name = "frequency")]
        [JsonProperty("frequency")]
        [XmlElement(ElementName = "frequency")]
        public long Frequency { get; set; }
    }
}
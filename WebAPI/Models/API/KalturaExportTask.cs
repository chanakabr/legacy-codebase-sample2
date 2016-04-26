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
        public long? Id { get; set; }

        /// <summary>
        /// Alias for the task used to solicit an export using API
        /// </summary>
        [DataMember(Name = "alias")]
        [JsonProperty("alias")]
        [XmlElement(ElementName = "alias")]
        public string Alias { get; set; }

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
        public long? Frequency { get; set; }

        /// <summary>
        /// The URL for sending a notification when the task's export process is done
        /// </summary>
        [DataMember(Name = "notification_url")]
        [JsonProperty("notification_url")]
        [XmlElement(ElementName = "notification_url")]
        public string NotificationUrl { get; set; }

        /// <summary>
        /// List of media type identifiers (as configured in TVM) to export. used only in case data_type = vod
        /// </summary>
        [DataMember(Name = "vod_types")]
        [JsonProperty("vod_types")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaIntegerValue> VodTypes { get; set; }

        /// <summary>
        /// Indicates if the task is active or not
        /// </summary>
        [DataMember(Name = "is_active")]
        [JsonProperty("is_active")]
        [XmlElement(ElementName = "is_active", IsNullable = true)]
        public bool? IsActive { get; set; }

        internal long getFrequency()
        {
            return Frequency.HasValue ? (long)Frequency : 0;
        }

        internal long getId()
        {
            return Id.HasValue ? (long)Id : 0;
        }
    }
}
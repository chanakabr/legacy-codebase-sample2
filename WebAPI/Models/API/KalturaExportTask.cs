using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.API
{
    /// <summary>
    /// Bulk export task
    /// </summary>
    [OldStandard("dataType", "data_type")]
    [OldStandard("exportType", "export_type")]
    [OldStandard("notificationUrl", "notification_url")]
    [OldStandard("vodTypes", "vod_types")]
    [OldStandard("isActive", "is_active")]
    public class KalturaExportTask : KalturaOTTObject
    {
        /// <summary>
        /// Task identifier
        /// </summary>
        [DataMember(Name = "id")]
        [JsonProperty("id")]
        [XmlElement(ElementName = "id")]
        [SchemeProperty(ReadOnly = true)]
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
        [DataMember(Name = "dataType")]
        [JsonProperty("dataType")]
        [XmlElement(ElementName = "dataType")]
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
        [DataMember(Name = "exportType")]
        [JsonProperty("exportType")]
        [XmlElement(ElementName = "exportType")]
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
        [DataMember(Name = "notificationUrl")]
        [JsonProperty("notificationUrl")]
        [XmlElement(ElementName = "notificationUrl")]
        public string NotificationUrl { get; set; }

        /// <summary>
        /// List of media type identifiers (as configured in TVM) to export. used only in case data_type = vod
        /// </summary>
        [DataMember(Name = "vodTypes")]
        [JsonProperty("vodTypes")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem(ElementName = "item")]
        public List<KalturaIntegerValue> VodTypes { get; set; }

        /// <summary>
        /// Indicates if the task is active or not
        /// </summary>
        [DataMember(Name = "isActive")]
        [JsonProperty("isActive")]
        [XmlElement(ElementName = "isActive", IsNullable = true)]
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

    /// <summary>
    /// Export task list wrapper
    /// </summary>
    [Serializable]
    public class KalturaExportTaskListResponse : KalturaListResponse
    {
        /// <summary>
        /// Export task items
        /// </summary>
        [DataMember(Name = "objects")]
        [JsonProperty(PropertyName = "objects")]
        [XmlArray(ElementName = "objects", IsNullable = true)]
        [XmlArrayItem("item")]
        public List<KalturaExportTask> Objects { get; set; }

    }
}
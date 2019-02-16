using ApiObjects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using TVinciShared;
using WebAPI.App_Start;

namespace WebAPI.Models.General
{
    /// <summary>
    /// A representation to return an array of values
    /// </summary>
    [XmlInclude(typeof(KalturaStringValue))]
    [XmlInclude(typeof(KalturaIntegerValue))]
    [XmlInclude(typeof(KalturaBooleanValue))]
    [XmlInclude(typeof(KalturaDoubleValue))]
    [XmlInclude(typeof(KalturaMultilingualStringValue))]
    [XmlInclude(typeof(KalturaLongValue))]    
    public abstract partial class KalturaValue : KalturaOTTObject
    {
        internal const string TOPIC_TYPE = "TOPIC_TYPE";
        internal const string TOPIC_SYSTEM_NAME = "TOPIC_SYSTEM_NAME";

        /// <summary>
        /// Description
        /// </summary>
        [DataMember(Name = "description")]
        [XmlElement("description", IsNullable = true)]
        [JsonProperty("description")]
        public string description { get; set; } 

        internal static ExcelColumnType? GetExcelMetaColumnType(MetaType metaType)
        {
            switch (metaType)
            {
                case MetaType.String:
                    return ExcelColumnType.MetaText;
                    break;
                case MetaType.Number:
                    return ExcelColumnType.MetaNumber;
                    break;
                case MetaType.Bool:
                    return ExcelColumnType.MetaBool;
                    break;
                case MetaType.Tag:
                    return ExcelColumnType.Tag;
                    break;
                case MetaType.DateTime:
                    return ExcelColumnType.MetaDate;
                    break;
                case MetaType.MultilingualString:
                    return ExcelColumnType.MetaMultilingual;
                    break;
                case MetaType.All:
                default:
                    return null;
            }
        }
    }
}
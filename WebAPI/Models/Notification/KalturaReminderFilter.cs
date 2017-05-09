using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;


namespace WebAPI.Models.Notification
{
    public enum KalturaReminderOrderBy
    {
        NONE
    }

    public class KalturaReminderFilter : KalturaAssetFilter
    {
        /// <summary>
        /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restriced to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each.
        /// (maximum length of entire filter is 2048 characters)]]>
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        [Obsolete]
        public string KSql { get; set; }       
    }

    public class KalturaSingleReminderFilter : KalturaReminderFilter
    {
        /// <summary>
        /// <![CDATA[
        /// Search assets using dynamic criteria. Provided collection of nested expressions with key, comparison operators, value, and logical conjunction.
        /// Possible keys: any Tag or Meta defined in the system and the following reserved keys: start_date, end_date. 
        /// epg_id, media_id - for specific asset IDs.
        /// geo_block - only valid value is "true": When enabled, only assets that are not restriced to the user by geo-block rules will return.
        /// parental_rules - only valid value is "true": When enabled, only assets that the user doesn't need to provide PIN code will return.
        /// epg_channel_id – the channel identifier of the EPG program.
        /// entitled_assets - valid values: "free", "entitled", "both". free - gets only free to watch assets. entitled - only those that the user is implicitly entitled to watch.
        /// Comparison operators: for numerical fields =, >, >=, <, <=, : (in). For alpha-numerical fields =, != (not), ~ (like), !~, ^ (starts with). Logical conjunction: and, or. 
        /// Search values are limited to 20 characters each. 
        /// (maximum length of entire filter is 2048 characters)]]>
        /// </summary>
        [DataMember(Name = "kSql")]
        [JsonProperty("kSql")]
        [XmlElement(ElementName = "kSql", IsNullable = true)]
        [ValidationException(SchemeValidationType.FILTER_SUFFIX)]
        public string KSql { get; set; }
    }

    public class KalturaSeriesReminderFilter : KalturaReminderFilter
    {
        /// <summary>
        /// Comma separated series IDs
        /// </summary>
        [DataMember(Name = "seriesIdIn")]
        [JsonProperty("seriesIdIn")]
        [XmlElement(ElementName = "seriesIdIn", IsNullable = true)]
        public string SeriesIdIn { get; set; }

        /// <summary>
        /// EPG channel ID
        /// </summary>
        [DataMember(Name = "epgChannelIdEqual")]
        [JsonProperty("epgChannelIdEqual")]
        [XmlElement(ElementName = "epgChannelIdEqual", IsNullable = true)]
        public long? EpgChannelIdEqual { get; set; }

        public List<string> GetSeriesIdIn()
        {
            List<string> list = new List<string>();
            if (!string.IsNullOrEmpty(SeriesIdIn))
            {
                string[] stringValues = SeriesIdIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    list.Add(stringValue);
                }
            }

            return list;
        }
    }

    public class KalturaSeasonsReminderFilter : KalturaReminderFilter
    {
        /// <summary>
        /// Series ID
        /// </summary>
        [DataMember(Name = "seriesIdEqual")]
        [JsonProperty("seriesIdEqual")]
        [XmlElement(ElementName = "seriesIdEqual", IsNullable = true)]
        public string SeriesIdEqual { get; set; }

        /// <summary>
        /// Comma separated season numbers
        /// </summary>
        [DataMember(Name = "seasonNumberIn")]
        [JsonProperty("seasonNumberIn")]
        [XmlElement(ElementName = "seasonNumberIn", IsNullable = true)]
        public string SeasonNumberIn { get; set; }

        /// <summary>
        /// EPG channel ID
        /// </summary>
        [DataMember(Name = "epgChannelIdEqual")]
        [JsonProperty("epgChannelIdEqual")]
        [XmlElement(ElementName = "epgChannelIdEqual", IsNullable = true)]
        public long? EpgChannelIdEqual { get; set; }

        public List<long> GetSeasonNumberIn()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(SeasonNumberIn))
            {
                string[] stringValues = SeasonNumberIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    int value;
                    if (int.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaSeasonsReminderFilter.seasonNumberIn");
                    }
                }
            }

            return list;
        }
    }
}
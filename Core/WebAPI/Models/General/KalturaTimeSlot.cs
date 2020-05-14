using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.General
{
    [Serializable]
    public partial class KalturaTimeSlot : KalturaOTTObject
    { 
        /// <summary>
        /// Start date in seconds
        /// </summary>
        [DataMember(Name = "startDateInSeconds")]
        [JsonProperty("startDateInSeconds")]
        [XmlElement(ElementName = "startDateInSeconds")]
        public long? StartDateInSeconds { get; set; }

        /// <summary>
        /// End date in seconds
        /// </summary>
        [DataMember(Name = "startDendDateInSecondsateInSeconds")]
        [JsonProperty("endDateInSeconds")]
        [XmlElement(ElementName = "endDateInSeconds")]
        public long? EndDateInSeconds { get; set; }

        /// <summary>
        /// Start time in minutes
        /// </summary>
        [DataMember(Name = "startTimeInMinutes")]
        [JsonProperty("startTimeInMinutes")]
        [XmlElement(ElementName = "startTimeInMinutes")]
        public long? StartTimeInMinutes { get; set; }

        /// <summary>
        /// End time in minutes
        /// </summary>
        [DataMember(Name = "endTimeInMinutes")]
        [JsonProperty("endTimeInMinutes")]
        [XmlElement(ElementName = "endTimeInMinutes")]
        public long? EndTimeInMinutes { get; set; }

        /// <summary>
        /// Days of the week - separated with comma
        /// </summary>
        [DataMember(Name = "daysOfTheWeek")]
        [JsonProperty("daysOfTheWeek")]
        [XmlElement(ElementName = "daysOfTheWeek", IsNullable = true)]
        [SchemeProperty(DynamicType = typeof(KalturaDayOfTheWeek))]
        public string DaysOfTheWeek { get; set; }

        public List<KalturaDayOfTheWeek> DayOfTheWeekList()
        {
            List<KalturaDayOfTheWeek> dayOfTheWeekList = new List<KalturaDayOfTheWeek>();
            if (!string.IsNullOrEmpty(this.DaysOfTheWeek))
            {
                string[] daysOfTheWeek = this.DaysOfTheWeek.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string dayOfTheWeek in daysOfTheWeek)
                {
                    KalturaDayOfTheWeek kDayOfTheWeek;
                    if (Enum.TryParse<KalturaDayOfTheWeek>(dayOfTheWeek.ToUpper(), out kDayOfTheWeek))
                    {
                        dayOfTheWeekList.Add(kDayOfTheWeek);
                    }
                }
            }           

            return dayOfTheWeekList;
        }
    }

    [Serializable]
    public enum KalturaDayOfTheWeek
    {
        SUNDAY = 1,
        MONDAY = 2,
        TUESDAY = 3,
        WEDNESDAY = 4,
        THURSDAY = 5,
        FRIDAY = 6,
        SATURDAY = 7
    }
}
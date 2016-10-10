using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.Models.Catalog
{
    public class KalturaScheduledRecordingAssetFilter : KalturaAssetFilter
    {

        /// <summary>
        ///The type of recordings to return
        /// </summary>
        [DataMember(Name = "recordingTypeEqual")]
        [JsonProperty("recordingTypeEqual")]
        [XmlElement(ElementName = "recordingTypeEqual")]
        public KalturaScheduledRecordingAssetType RecordingTypeEqual { get; set; }

        /// <summary>
        /// Channels to filter by
        /// </summary>
        [DataMember(Name = "channelsIn")]
        [JsonProperty(PropertyName = "channelsIn")]
        [XmlArray(ElementName = "channelsIn", IsNullable = true)]
        public string ChannelsIn { get; set; }

        public List<long> ConvertChannelsIn()
        {
            List<long> channelsIds = new List<long>();
            if (!string.IsNullOrEmpty(ChannelsIn))
            {
                string[] splitChannels = ChannelsIn.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);                
                foreach (string channelId in splitChannels)
                {
                    long parsedChannelId;
                    if (long.TryParse(channelId, out parsedChannelId) && parsedChannelId > 0)
                    {
                        channelsIds.Add(parsedChannelId);
                    }
                }
            }

            return channelsIds;

        }
    }
}
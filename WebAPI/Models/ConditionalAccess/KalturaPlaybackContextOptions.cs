using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaPlaybackContextOptions : KalturaOTTObject
    {
        /// <summary>
        /// Protocol of the specific media object.
        /// </summary>
        [DataMember(Name = "mediaProtocol")]
        [JsonProperty("mediaProtocol")]
        [XmlElement(ElementName = "mediaProtocol")]
        public string MediaProtocol { get; set; }

        /// <summary>
        /// Playback streamer type: RTMP, HTTP, appleHttps, rtsp, sl.
        /// </summary>
        [DataMember(Name = "streamerType")]
        [JsonProperty("streamerType")]
        [XmlElement(ElementName = "streamerType")]
        public string StreamerType { get; set; }

        /// <summary>
        /// List of comma separated media file IDs
        /// </summary>
        [DataMember(Name = "mediaFileIds")]
        [JsonProperty("mediaFileIds")]
        [XmlElement(ElementName = "mediaFileIds")]
        public string MediaFileIds { get; set; }

        /// <summary>
        /// List of comma separated context types
        /// </summary>
        [DataMember(Name = "contexts")]
        [JsonProperty("contexts")]
        [XmlElement(ElementName = "contexts")]
        public string Contexts { get; set; }


        public List<long> GetMediaFileIds()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(MediaFileIds))
            {
                string[] stringValues = MediaFileIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (long.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPlaybackContextOptions.mediaFileIds");
                    }
                }
            }

            return list;
        }

        public List<KalturaContextType> GetContexts()
        {
            List<KalturaContextType> list = new List<KalturaContextType>();
            if (!string.IsNullOrEmpty(MediaFileIds))
            {
                string[] stringValues = MediaFileIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    KalturaContextType value;
                    if (Enum.TryParse(stringValue, out value))
                    {
                        list.Add(value);
                    }
                    else
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaPlaybackContextOptions.contexts");
                    }
                }
            }

            return list;
        }
    }
}
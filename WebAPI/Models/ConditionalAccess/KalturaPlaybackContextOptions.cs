using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Web;
using System.Xml.Serialization;
using WebAPI.Exceptions;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;

namespace WebAPI.Models.ConditionalAccess
{
    public class KalturaPlaybackContextOptions : KalturaOTTObject
    {
        /// <summary>
        /// Protocol of the specific media object (http / https).
        /// </summary>
        [DataMember(Name = "mediaProtocol")]
        [JsonProperty("mediaProtocol")]
        [XmlElement(ElementName = "mediaProtocol")]
        public string MediaProtocol { get; set; }

        /// <summary>
        /// Playback streamer type: applehttp, mpegdash, url.
        /// </summary>
        [DataMember(Name = "streamerType")]
        [JsonProperty("streamerType")]
        [XmlElement(ElementName = "streamerType")]
        public string StreamerType { get; set; }

        /// <summary>
        /// List of comma separated media file IDs
        /// </summary>
        [DataMember(Name = "assetFileIds")]
        [JsonProperty("assetFileIds")]
        [XmlElement(ElementName = "assetFileIds")]
        [SchemeProperty(DynamicMinInt = 1)]
        public string AssetFileIds { get; set; }

        /// <summary>
        /// List of comma separated context types, possible values: TRAILER, CATCHUP, START_OVER, PLAYBACK
        /// </summary>
        [DataMember(Name = "contexts")]
        [JsonProperty("contexts")]
        [XmlElement(ElementName = "contexts")]
        [SchemeProperty(DynamicType = typeof(KalturaPlaybackContextType))]
        public string Contexts { get; set; }


        public List<long> GetMediaFileIds()
        {
            List<long> list = new List<long>();
            if (!string.IsNullOrEmpty(AssetFileIds))
            {
                string[] stringValues = AssetFileIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value = long.Parse(stringValue);
                    list.Add(value);
                }
            }

            return list;
        }

        public List<KalturaPlaybackContextType> GetContexts()
        {
            List<KalturaPlaybackContextType> list = new List<KalturaPlaybackContextType>();
            if (!string.IsNullOrEmpty(AssetFileIds))
            {
                string[] stringValues = AssetFileIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    KalturaPlaybackContextType value = (KalturaPlaybackContextType)Enum.Parse(typeof(KalturaPlaybackContextType), stringValue);
                    list.Add(value);
                }
            }

            return list;
        }
    }
}
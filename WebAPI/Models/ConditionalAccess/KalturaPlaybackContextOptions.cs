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
        /// Playback context type
        /// </summary>
        [DataMember(Name = "context")]
        [JsonProperty("context")]
        [XmlElement(ElementName = "context")]
        public KalturaPlaybackContextType? Context { get; set; }

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

        internal void Validate()
        {
            if (!Context.HasValue)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaPlaybackContextOptions.context");
            }
        }

        internal void Validate(Catalog.KalturaAssetType assetType)
        {
            Validate();

            if (Context.HasValue)
            {
                if (((Context.Value == KalturaPlaybackContextType.CATCHUP || Context.Value == KalturaPlaybackContextType.START_OVER) && assetType != Catalog.KalturaAssetType.epg) ||
                    (Context.Value == KalturaPlaybackContextType.TRAILER && assetType != Catalog.KalturaAssetType.media) ||
                    (Context.Value == KalturaPlaybackContextType.PLAYBACK && assetType == Catalog.KalturaAssetType.epg))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaPlaybackContextOptions.context", "assetType");
                }
            }
        }
    }
}
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApiModule.Helper;

namespace TVPApiModule.Objects.Responses
{
    public class AssetInfo
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public AssetType Type { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "pictures")]
        public List<Picture> Pictures { get; set; }

        [JsonProperty(PropertyName = "files")]
        public List<File> Files { get; set; }

        [JsonProperty(PropertyName = "metas")]
        public List<KeyValuePair<string, string>> Metas { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public List<KeyValuePair<string, string>> Tags { get; set; }

        [JsonProperty(PropertyName = "start_time")]
        public double StartTime { get; set; }

        [JsonProperty(PropertyName = "end_time")]
        public double EndTime { get; set; }

        [JsonProperty(PropertyName = "rating")]
        public double Rating { get; set; }

        [JsonProperty(PropertyName = "extra_params")]
        public List<KeyValuePair<string, string>> ExtraParams { get; set; }


        public AssetInfo(Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaObj media)
        {
            Id = media.m_nID;
            Type = AssetType.Media;
            Name = media.m_sName;
            Description = media.m_sDescription;
            StartTime = TimeHelper.ConvertToUnixTimestamp(media.m_dStartDate);
            EndTime = TimeHelper.ConvertToUnixTimestamp(media.m_dEndDate);
            //Rating = media.m_oRatingMedia;

            if (media.m_lPicture != null)
            {
                Pictures = new List<Picture>();
                foreach (var mediaPicture in media.m_lPicture)
                {
		            Picture picture = new Picture(mediaPicture);
                    Pictures.Add(picture);
                }
            }

            if (media.m_lFiles != null)
            {
                Files = new List<File>();
                foreach (var mediaFile in media.m_lFiles)
	            {
                    File file = new File(mediaFile);
                    Files.Add(file);
	            }
            }

            if (media.m_lMetas != null)
            {
                Metas = new List<KeyValuePair<string,string>>();
                foreach (var mediaMeta in media.m_lMetas)
	            {
                    KeyValuePair<string, string> meta = new KeyValuePair<string,string>(mediaMeta.m_oTagMeta.m_sName, mediaMeta.m_sValue);
                    Metas.Add(meta);
	            }
            }

            if (media.m_lTags != null)
            {
                Tags = new List<KeyValuePair<string,string>>();
                foreach (var mediaTag in media.m_lTags)
	            {
                    KeyValuePair<string, string> tag = new KeyValuePair<string,string>(mediaTag.m_oTagMeta.m_sName, string.Join("|", mediaTag.m_lValues.ToArray()));
                    Tags.Add(tag);
	            }
            }

            ExtraParams = new List<KeyValuePair<string, string>>();
        }

        public AssetInfo(Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject epg)
        {
            Id = epg.EPG_ID;
            Type = AssetType.EPG;
            Name = epg.NAME;
            Description = epg.DESCRIPTION;
            StartTime = TimeHelper.ConvertToUnixTimestamp(DateTime.Parse(epg.START_DATE));
            EndTime = TimeHelper.ConvertToUnixTimestamp(DateTime.Parse(epg.END_DATE));
            //Rating =
            //Pictures = 
            //Files = 
            if (epg.EPG_Meta != null)
            {
                Metas = new List<KeyValuePair<string,string>>();
                foreach (var epgMeta in epg.EPG_Meta)
	            {
                    KeyValuePair<string, string> meta = new KeyValuePair<string,string>(epgMeta.Key, epgMeta.Value);
                    Metas.Add(meta);
	            }
            }

            if (epg.EPG_TAGS != null)
            {
                Tags = new List<KeyValuePair<string,string>>();
                foreach (var epgTag in epg.EPG_TAGS)
	            {
                    KeyValuePair<string, string> tag = new KeyValuePair<string,string>(epgTag.Key, epgTag.Value);
                    Tags.Add(tag);
	            }
            }
            
            ExtraParams = new List<KeyValuePair<string,string>>();
        }
    }
}

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

        [JsonProperty(PropertyName = "start_date")]
        public double StartDate { get; set; }

        [JsonProperty(PropertyName = "end_date")]
        public double EndDate { get; set; }

        [JsonProperty(PropertyName = "statistics")]
        public Statistics Statistics { get; set; }

        [JsonProperty(PropertyName = "extra_params")]
        public List<KeyValuePair<string, string>> ExtraParams { get; set; }

        public AssetInfo(Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaObj media)
        {
            if (media != null)
            {
                Id = media.m_nID;
                Type = AssetType.Media;
                Name = media.m_sName;
                Description = media.m_sDescription;
                StartDate = TimeHelper.ConvertToUnixTimestamp(media.m_dCatalogStartDate);
                EndDate = TimeHelper.ConvertToUnixTimestamp(media.m_dEndDate);

                Statistics = new Statistics()
                {
                    Likes = media.m_nLikeCounter
                };
                if (media.m_oRatingMedia != null)
                {
                    Statistics.RatingAvg = media.m_oRatingMedia.m_nRatingAvg;
                    Statistics.RatingCount = media.m_oRatingMedia.m_nRatingCount;
                    Statistics.RatingSum = media.m_oRatingMedia.m_nRatingSum;
                    Statistics.Views = media.m_oRatingMedia.m_nViwes;
                }

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
                    Metas = new List<KeyValuePair<string, string>>();
                    foreach (var mediaMeta in media.m_lMetas)
                    {
                        KeyValuePair<string, string> meta = new KeyValuePair<string, string>(mediaMeta.m_oTagMeta.m_sName, mediaMeta.m_sValue);
                        Metas.Add(meta);
                    }
                }

                if (media.m_lTags != null)
                {
                    Tags = new List<KeyValuePair<string, string>>();
                    foreach (var mediaTag in media.m_lTags)
                    {
                        KeyValuePair<string, string> tag = new KeyValuePair<string, string>(mediaTag.m_oTagMeta.m_sName, string.Join("|", mediaTag.m_lValues.ToArray()));
                        Tags.Add(tag);
                    }
                }

                ExtraParams = new List<KeyValuePair<string, string>>();

                ExtraParams.Add(new KeyValuePair<string, string>("StartDate", media.m_dStartDate.ToString()));
                ExtraParams.Add(new KeyValuePair<string, string>("FinalDate", media.m_dFinalDate.ToString()));
                ExtraParams.Add(new KeyValuePair<string, string>("ExternalIds", media.m_ExternalIDs));
                ExtraParams.Add(new KeyValuePair<string, string>("LastWatchedDevice", media.m_sLastWatchedDevice));

                if (media.m_oMediaType != null)
                {
                    ExtraParams.Add(new KeyValuePair<string, string>("MediaTypeId", media.m_oMediaType.m_nTypeID.ToString()));
                    ExtraParams.Add(new KeyValuePair<string, string>("MediaTypeName", media.m_oMediaType.m_sTypeName.ToString()));
                }

                if (media.m_dLastWatchedDate != null)
                {
                    ExtraParams.Add(new KeyValuePair<string, string>("LastWatchedDate", media.m_dLastWatchedDate.ToString()));
                }
            }
        }

        public AssetInfo(Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject epg)
        {
            Id = epg.EPG_ID;
            Type = AssetType.EPG;
            Name = epg.NAME;
            Description = epg.DESCRIPTION;
            StartDate = TimeHelper.ConvertToUnixTimestamp(DateTime.Parse(epg.START_DATE));
            EndDate = TimeHelper.ConvertToUnixTimestamp(DateTime.Parse(epg.END_DATE));

            Statistics = new Statistics()
            {
                Likes = epg.LIKE_COUNTER
            };

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

            ExtraParams.Add(new KeyValuePair<string, string>("EpgChannelId", epg.EPG_CHANNEL_ID));
            ExtraParams.Add(new KeyValuePair<string, string>("EpgIdentifier", epg.EPG_IDENTIFIER));
            ExtraParams.Add(new KeyValuePair<string, string>("MediaId", epg.media_id));


        }
    }
}

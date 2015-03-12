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
        public string Type { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "pictures")]
        public List<Picture> Pictures { get; set; }

        //[JsonProperty(PropertyName = "files")]
        //public List<File> Files { get; set; }

        [JsonProperty(PropertyName = "metas")]
        public Dictionary<string, string> Metas { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string, object> Tags { get; set; }

        [JsonProperty(PropertyName = "start_date")]
        public double StartDate { get; set; }

        [JsonProperty(PropertyName = "end_date")]
        public double EndDate { get; set; }

        [JsonProperty(PropertyName = "statistics")]
        public Statistics Statistics { get; set; }

        [JsonProperty(PropertyName = "extra_params")]
        public Dictionary<string, string> ExtraParams { get; set; }

        public AssetInfo(Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaObj media)
        {
            if (media != null)
            {
                Id = media.m_nID;
                Name = media.m_sName;
                Description = media.m_sDescription;
                StartDate = TimeHelper.ConvertToUnixTimestamp(media.m_dCatalogStartDate);
                EndDate = TimeHelper.ConvertToUnixTimestamp(media.m_dEndDate);

                if (media.m_oMediaType != null)
                {
                    Type = media.m_oMediaType.m_sTypeName;
                }

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

                //if (media.m_lFiles != null)
                //{
                //    Files = new List<File>();
                //    foreach (var mediaFile in media.m_lFiles)
                //    {
                //        File file = new File(mediaFile);
                //        Files.Add(file);
                //    }
                //}

                if (media.m_lMetas != null)
                {
                    Metas = new Dictionary<string, string>();
                    foreach (var mediaMeta in media.m_lMetas)
                    {
                        Metas.Add(mediaMeta.m_oTagMeta.m_sName, mediaMeta.m_sValue);
                    }
                }

                if (media.m_lTags != null)
                {
                    Tags = new Dictionary<string, object>();
                    foreach (var mediaTag in media.m_lTags)
                    {
                        Tags.Add(mediaTag.m_oTagMeta.m_sName, mediaTag.m_lValues);
                    }
                }

                ExtraParams = new Dictionary<string, string>();

                ExtraParams.Add("StartDate", media.m_dStartDate.ToString());
                ExtraParams.Add("FinalDate", media.m_dFinalDate.ToString());
                ExtraParams.Add("ExternalIds", media.m_ExternalIDs);
                ExtraParams.Add("LastWatchedDevice", media.m_sLastWatchedDevice);

                if (media.m_oMediaType != null)
                {
                    ExtraParams.Add("MediaTypeId", media.m_oMediaType.m_nTypeID.ToString());
                    ExtraParams.Add("MediaTypeName", media.m_oMediaType.m_sTypeName.ToString());
                }

                if (media.m_dLastWatchedDate != null)
                {
                    ExtraParams.Add("LastWatchedDate", media.m_dLastWatchedDate.ToString());
                }
            }
        }

        public AssetInfo(Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject epg)
        {
            Id = epg.EPG_ID;
            Type = "EPG";
            Name = epg.NAME;
            Description = epg.DESCRIPTION;
            //StartDate = TimeHelper.ConvertToUnixTimestamp(DateTime.Parse(epg.START_DATE));
            //EndDate = TimeHelper.ConvertToUnixTimestamp(DateTime.Parse(epg.END_DATE));

            Statistics = new Statistics()
            {
                Likes = epg.LIKE_COUNTER
            };

            if (epg.EPG_Meta != null)
            {
                Metas = new Dictionary<string, string>();
                foreach (var epgMeta in epg.EPG_Meta)
	            {
                    Metas.Add(epgMeta.Key, epgMeta.Value);
	            }
            }

            if (epg.EPG_TAGS != null)
            {
                Tags = new Dictionary<string, object>();
                List<string> tags;
                foreach (var epgTag in epg.EPG_TAGS)
	            {
                    if (Tags.ContainsKey(epgTag.Key))
                    {
                        ((List<string>)Tags[epgTag.Key]).Add(epgTag.Value);
                    }
                    else
                    {
                        tags = new List<string>();
                        tags.Add(epgTag.Value);
                        Tags.Add(epgTag.Key, tags);
                    }
	            }
            }

            ExtraParams = new Dictionary<string, string>();

            ExtraParams.Add("EpgChannelId", epg.EPG_CHANNEL_ID);
            ExtraParams.Add("EpgIdentifier", epg.EPG_IDENTIFIER);
            ExtraParams.Add("MediaId", epg.media_id);


        }
    }
}

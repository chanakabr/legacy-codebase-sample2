using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.Helper;

namespace TVPApiModule.Objects.Responses
{
    public class AssetInfo
    {
        [JsonProperty(PropertyName = "id")]
        public string Id
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "type")]
        public int Type
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "name")]
        public string Name
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "images")]
        public List<Image> Images
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "files", NullValueHandling = NullValueHandling.Ignore)]
        public List<File> Files
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "metas")]
        public Dictionary<string, string> Metas
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string, List<string>> Tags
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "start_date")]
        public long StartDate
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "end_date")]
        public long EndDate
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "stats", NullValueHandling = NullValueHandling.Ignore)]
        public Statistics Statistics
        {
            get;
            set;
        }

        [JsonProperty(PropertyName = "extra_params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> ExtraParams
        {
            get;
            set;
        }

        public AssetInfo(MediaObj media, bool shouldAddFiles)
        {
            if (media != null)
            {
                Id = media.AssetId;
                Name = media.m_sName;
                Description = media.m_sDescription;
                StartDate = (long)TimeHelper.ConvertToUnixTimestamp(media.m_dCatalogStartDate);
                EndDate = (long)TimeHelper.ConvertToUnixTimestamp(media.m_dEndDate);

                if (media.m_oMediaType != null)
                {
                    Type = media.m_oMediaType.m_nTypeID;
                }

                if (media.m_lPicture != null)
                {
                    Images = new List<Image>();
                    foreach (var mediaPicture in media.m_lPicture)
                    {
                        Image picture = new Image(mediaPicture);
                        Images.Add(picture);
                    }
                }

                if (shouldAddFiles && media.m_lFiles != null)
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
                    Metas = new Dictionary<string, string>();
                    foreach (var mediaMeta in media.m_lMetas)
                    {
                        Metas.Add(mediaMeta.m_oTagMeta.m_sName, mediaMeta.m_sValue);
                    }
                }

                if (media.m_lTags != null)
                {
                    Tags = new Dictionary<string, List<string>>();
                    foreach (var mediaTag in media.m_lTags)
                    {
                        Tags.Add(mediaTag.m_oTagMeta.m_sName, mediaTag.m_lValues);
                    }
                }

                ExtraParams = new Dictionary<string, string>();

                ExtraParams.Add("sys_start_date", TimeHelper.ConvertToUnixTimestamp(media.m_dStartDate).ToString());
                ExtraParams.Add("sys_final_date", TimeHelper.ConvertToUnixTimestamp(media.m_dFinalDate).ToString());
                ExtraParams.Add("external_ids", media.m_ExternalIDs);
                ExtraParams.Add("entry_id", media.EntryId);
            }
        }

        public AssetInfo(MediaObj media, AssetStatsResult stats, bool shouldAddFiles) :
            this(media, shouldAddFiles)
        {
            if (stats != null)
            {
                Statistics = new Statistics()
                {
                    Likes = stats.m_nLikes,
                    Rating = stats.m_dRate,
                    Id = stats.m_nAssetID,
                    Views = stats.m_nViews,
                    RatingCount = stats.m_nVotes
                };
            }
        }

        public AssetInfo(EPGChannelProgrammeObject epg)
        {
            Id = epg.EPG_ID.ToString();
            Type = 0;
            Name = epg.NAME;
            Description = epg.DESCRIPTION;
            StartDate = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.ParseExact(epg.START_DATE, "dd/MM/yyyy HH:mm:ss", null));
            EndDate = (long)TimeHelper.ConvertToUnixTimestamp(DateTime.ParseExact(epg.END_DATE, "dd/MM/yyyy HH:mm:ss", null));

            if (epg.EPG_PICTURES != null)
            {
                Images = new List<Image>();
                foreach (var epgPicture in epg.EPG_PICTURES)
                {
                    Image picture = new Image(epgPicture);
                    Images.Add(picture);
                }
            }

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
                Tags = new Dictionary<string, List<string>>();
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

            ExtraParams.Add("epg_channel_id", epg.EPG_CHANNEL_ID);
            ExtraParams.Add("epg_id", epg.EPG_IDENTIFIER);
            ExtraParams.Add("related_media_id", epg.media_id);
        }

        public AssetInfo(EPGChannelProgrammeObject epg, AssetStatsResult stats)
            : this(epg)
        {
            if (stats != null)
            {
                Statistics = new Statistics()
                {
                    Likes = stats.m_nLikes,
                    Rating = stats.m_dRate,
                    Id = stats.m_nAssetID,
                    Views = stats.m_nViews,
                    RatingCount = stats.m_nVotes
                };
            }
        }

        public AssetInfo(NpvrRecordAsset npvr)
        {
            Id = npvr.Id;
            Type = npvr.Type;
        }
    }
}

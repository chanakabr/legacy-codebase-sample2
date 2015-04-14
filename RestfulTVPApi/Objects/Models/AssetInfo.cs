using Newtonsoft.Json;
using RestfulTVPApi.Catalog;
using RestfulTVPApi.ServiceInterface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RestfulTVPApi.Objects.Models
{
    public class AssetInfo 
    {
        [JsonProperty(PropertyName = "id")]
        public long Id { get; set; }

        [JsonProperty(PropertyName = "type")]
        public int Type { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "images")]
        public List<Image> Images { get; set; }

        [JsonProperty(PropertyName = "files", NullValueHandling = NullValueHandling.Ignore)]
        public List<File> Files { get; set; }

        [JsonProperty(PropertyName = "metas")]
        public Dictionary<string, string> Metas { get; set; }

        [JsonProperty(PropertyName = "tags")]
        public Dictionary<string, List<string>> Tags { get; set; }

        [JsonProperty(PropertyName = "start_date")]
        public long StartDate { get; set; }

        [JsonProperty(PropertyName = "end_date")]
        public long EndDate { get; set; }

        [JsonProperty(PropertyName = "stats", NullValueHandling = NullValueHandling.Ignore)]
        public AssetStats Statistics { get; set; }

        [JsonProperty(PropertyName = "extra_params", NullValueHandling = NullValueHandling.Ignore)]
        public Dictionary<string, string> ExtraParams { get; set; }

        public static AssetInfo CreateFromObject(MediaObj media, bool shouldAddFiles, AssetStats stats = null)
        {
            if (media == null)
            {
                return null;
            }

            AssetInfo assetInfo = new AssetInfo()
            {
                Id = media.m_nID,
                Name = media.m_sName,
                Description = media.m_sDescription,
                StartDate = (long)Utils.ConvertToUnixTimestamp(media.m_dCatalogStartDate),
                EndDate = (long)Utils.ConvertToUnixTimestamp(media.m_dEndDate),
            };

            if (media.m_oMediaType != null)
            {
                assetInfo.Type = media.m_oMediaType.m_nTypeID;
            }

            if (media.m_lPicture != null)
            {
                assetInfo.Images = new List<Image>();
                foreach (var mediaPicture in media.m_lPicture)
                {
                    Image image = Image.CreateFromObject(mediaPicture);
                    assetInfo.Images.Add(image);
                }
            }

            if (shouldAddFiles && media.m_lFiles != null)
            {
                assetInfo.Files = new List<File>();
                foreach (var mediaFile in media.m_lFiles)
                {
                    File file = File.CreateFromObject(mediaFile);
                    assetInfo.Files.Add(file);
                }
            }

            if (media.m_lMetas != null)
            {
                assetInfo.Metas = new Dictionary<string, string>();
                foreach (var mediaMeta in media.m_lMetas)
                {
                    assetInfo.Metas.Add(mediaMeta.m_oTagMeta.m_sName, mediaMeta.m_sValue);
                }
            }

            if (media.m_lTags != null)
            {
                assetInfo.Tags = new Dictionary<string, List<string>>();
                foreach (var mediaTag in media.m_lTags)
                {
                    assetInfo.Tags.Add(mediaTag.m_oTagMeta.m_sName, mediaTag.m_lValues);
                }
            }

            assetInfo.ExtraParams = new Dictionary<string, string>();

            assetInfo.ExtraParams.Add("start_date", Utils.ConvertToUnixTimestamp(media.m_dStartDate).ToString());
            assetInfo.ExtraParams.Add("final_date", Utils.ConvertToUnixTimestamp(media.m_dFinalDate).ToString());
            assetInfo.ExtraParams.Add("external_ids", media.m_ExternalIDs);

            if (stats != null)
            {
                assetInfo.Statistics = stats;
            }

            return assetInfo;
        }

        public static AssetInfo CreateFromObject(Catalog.EPGChannelProgrammeObject epg, AssetStats stats = null)
        {
            if (epg == null)
            {
                return null;
            }

            AssetInfo assetInfo = new AssetInfo()
            {
                Id = epg.EPG_ID,
                Type = 0,
                Name = epg.NAME,
                Description = epg.DESCRIPTION,
                StartDate = (long)Utils.ConvertToUnixTimestamp(DateTime.ParseExact(epg.START_DATE, "dd/MM/yyyy HH:mm:ss", null)),
                EndDate = (long)Utils.ConvertToUnixTimestamp(DateTime.ParseExact(epg.END_DATE, "dd/MM/yyyy HH:mm:ss", null)),
            };

            if (epg.EPG_PICTURES != null)
            {
                assetInfo.Images = new List<Image>();
                foreach (var epgPicture in epg.EPG_PICTURES)
                {
                    Image image = Image.CreateFromObject(epgPicture);
                    assetInfo.Images.Add(image);
                }
            }

            if (epg.EPG_Meta != null)
            {
                assetInfo.Metas = new Dictionary<string, string>();
                foreach (var epgMeta in epg.EPG_Meta)
                {
                    assetInfo.Metas.Add(epgMeta.Key, epgMeta.Value);
                }
            }

            if (epg.EPG_TAGS != null)
            {
                assetInfo.Tags = new Dictionary<string, List<string>>();
                List<string> tags;
                foreach (var epgTag in epg.EPG_TAGS)
                {
                    if (assetInfo.Tags.ContainsKey(epgTag.Key))
                    {
                        ((List<string>)assetInfo.Tags[epgTag.Key]).Add(epgTag.Value);
                    }
                    else
                    {
                        tags = new List<string>();
                        tags.Add(epgTag.Value);
                        assetInfo.Tags.Add(epgTag.Key, tags);
                    }
                }
            }

            assetInfo.ExtraParams = new Dictionary<string, string>();

            assetInfo.ExtraParams.Add("epg_channel_id ", epg.EPG_CHANNEL_ID);
            assetInfo.ExtraParams.Add("epg_id", epg.EPG_IDENTIFIER);
            assetInfo.ExtraParams.Add("related_media_id", epg.media_id);

            if (stats != null)
            {
                assetInfo.Statistics = stats;
            }

            return assetInfo;
        }
    }
}
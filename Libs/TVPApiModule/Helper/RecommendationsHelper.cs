using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using KLogMonitor;
using Core.Catalog.Request;
using Core.Catalog.Response;
using Core.Catalog;
using ApiObjects;
using ApiObjects.Response;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Manager;
using TVPApiModule.Objects.ORCARecommendations;
using TVPApiModule.Services;
using TVPApiModule.yes.tvinci.ITProxy;
using TVPPro.Configuration.OrcaRecommendations;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using ApiObjects.SearchObjects;
using Media = TVPApi.Media;

namespace TVPApiModule.Helper
{
    public class OrcaResponse
    {
        public eContentType ContentType { get; set; }
        public Object Content { get; set; }
    }

    public class RecommendationsHelper
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        internal static object ParseOrcaResponseToVideoRecommendationList(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                logger.ErrorFormat("RecommendationsHelper::ParseOrcaResponseToVideoRecommendationList -> No response from Orca");
                return null;
            }


            // check if the userToken is still valid
            if (xml.Contains("INVALID_CREDENTIALS"))
            {
                logger.ErrorFormat("RecommendationsHelper::ParseOrcaResponseToVideoRecommendationList -> Resonse from Orca contains INVALID_CREDENTIALS status");
                return "INVALID_CREDENTIALS";
            }

            List<VideoRecommendation> retVal = new List<VideoRecommendation>();

            VideoRecommendation recommendation;

            foreach (Match m in Regex.Matches(xml, @"(?<=VideoRecommendation)[\S\s]*?(?=\<\/VideoRecommendation>)"))
            {
                recommendation = new VideoRecommendation();
                Match externalContentId = Regex.Match(m.Value, "(?<externalContentId>(?<=\\bexternalContentId=\")[^\"]*)");
                if (externalContentId.Success)
                    recommendation.ContentID = externalContentId.Value;
                Match contentType = Regex.Match(m.Value, "(?<contentType>(?<=\\bcontentType=\")[^\"]*)");
                if (contentType.Success)
                    recommendation.ContentType = contentType.Value;
                if (recommendation.ContentType == "Season")
                {
                    Match externalSeriesId = Regex.Match(m.Value, "(?<externalSeriesId>(?<=\\bexternalSeriesId=\")[^\"]*)");
                    if (externalContentId.Success)
                        recommendation.SeriesID = externalSeriesId.Value;
                }
                retVal.Add(recommendation);
            }

            return retVal;
        }

        internal static object ParseOrcaResponseToLiveRecommendationList(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                logger.ErrorFormat("RecommendationsHelper::ParseOrcaResponseToLiveRecommendationList -> No response from Orca");
                return null;
            }

            // check if the userToken is still valid
            if (xml.Contains("INVALID_CREDENTIALS"))
            {
                logger.ErrorFormat("RecommendationsHelper::ParseOrcaResponseToVideoRecommendationList -> Resonse from Orca contains INVALID_CREDENTIALS status");
                return "INVALID_CREDENTIALS";
            }

            List<LiveRecommendation> retVal = new List<LiveRecommendation>();

            LiveRecommendation recommendation;

            foreach (Match m in Regex.Matches(xml, @"(?<=LiveRecommendation)[\S\s]*?(?=\<\/LiveRecommendation)"))
            {

                foreach (Match cp in Regex.Matches(m.Value, @"(?<=CustomProperty)[\S\s]*?(?=\<\/CustomProperty)"))
                {
                    Match name = Regex.Match(cp.Value, "(?<name>(?<=\\bname=\")[^\"]*)");
                    if (name.Success && name.Value == "CISCO_SCHEDULE_ID")
                    {
                        Match program = Regex.Match(cp.Value, "(?<value>(?<=\\bvalue=\")[^\"]*)");
                        if (program.Success)
                        {
                            recommendation = new LiveRecommendation();
                            recommendation.ProgramID = program.Value;
                            retVal.Add(recommendation);

                        }
                    }

                }
            }
            return retVal;
        }

        internal static long ConvertDateTimeToEpoch(DateTime time)
        {
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (long)(time - epoch).TotalMilliseconds;
        }

        internal static dsItemInfo GetVodRecommendedMediasFromCatalog(int groupID, PlatformType platform, string picSize, string culture, List<VideoRecommendation> videoRecommendations)
        {
            dsItemInfo medias = null;

            List<KeyValue> metas = new List<KeyValue>();
            List<KeyValue> tags = new List<KeyValue>();

            List<KeyValue> orList = new List<KeyValue>();


            foreach (var recommendation in videoRecommendations)
            {
                if (recommendation.ContentType == "Movie" || recommendation.ContentType == "Program")
                    orList.Add(new KeyValue() { m_sKey = "IBMSTitleID", m_sValue = recommendation.ContentID });


                else if (recommendation.ContentType == "Season")
                    orList.Add(new KeyValue() { m_sKey = "IBMSseriesID", m_sValue = string.IsNullOrEmpty(recommendation.SeriesID) ? string.Empty : recommendation.SeriesID });
                else
                    orList.Add(new KeyValue() { m_sKey = "OfferID", m_sValue = recommendation.ContentID });
            }

            APISearchMediaLoader loader = new APISearchMediaLoader(groupID, groupID, platform.ToString(), SiteHelper.GetClientIP(), 0, 0, picSize, string.Empty)
            {
                OrList = orList,
                And = false,
                Exact = false,
                Culture = culture
            };

            loader.Name = string.Empty;
            loader.Description = string.Empty;

            medias = loader.Execute() as dsItemInfo;

            return medias;
        }

        internal static List<EPGChannelProgrammeObject> GetLiveRecommendedMedias(string siteGuid, int groupID, PlatformType platform, string calture, string picSize, List<LiveRecommendation> liveRecommendations)
        {
            List<EPGChannelProgrammeObject> epgChannelProgrammes = null;

            List<string> pids = liveRecommendations.Select(r => r.ProgramID).ToList();

            try
            {
                //var lng = TextLocalizationManager.Instance.GetTextLocalization(groupID, platform).GetLanguages().Where(l => l.Culture == calture).FirstOrDefault();
                //Language language = Language.Hebrew;
                //if (lng != null)
                //    language = (Language)Enum.Parse(typeof(Language), lng.Name);

                //var duration = (int)(RecommendationsHelper.GetEndTimeForLiveRequest(ConfigManager.GetInstance().GetConfig(groupID, platform).OrcaRecommendationsConfiguration) - DateTime.UtcNow).TotalMinutes;
                //epgChannelProgrammes = new EPGProgramsByProgramsIdentefierLoader(groupID, SiteHelper.GetClientIP(), pids.Count(), 0, pids, duration, language)
                //{
                //    SiteGuid = siteGuid
                //}.Execute() as List<EPGChannelProgrammeObject>;

                var orcaConfiguration = ConfigManager.GetInstance().GetConfig(groupID, platform).OrcaRecommendationsConfiguration;
                var endTime = RecommendationsHelper.GetEndTimeForLiveRequest(orcaConfiguration);
                var startTime = DateTime.UtcNow.AddHours(orcaConfiguration.Data.GMTOffset);
                var orList = new List<KeyValue>();
                pids.ForEach(pid => orList.Add(new KeyValue() { m_sKey = "epg_identifier", m_sValue = pid }));

                var programs = new APIEPGSearchLoader(groupID, platform.ToString(), SiteHelper.GetClientIP(), 0, 0, new List<KeyValue>(), orList, true, startTime, endTime)
                    {
                        Culture = calture
                    }.Execute() as List<BaseObject>;

                epgChannelProgrammes = programs != null ? programs.Select(p => (p as ProgramObj).m_oProgram).ToList() : null;

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RecommendationsHelper::GetLiveRecommendedMedias -> {0}", ex);
            }

            return epgChannelProgrammes;
        }

        internal static TVPApiModule.yes.tvinci.ITProxy.KeyValuePair[] GetExtraParamsFromConfig(ParamsParamCollection paramCollection, int mediaID, int groupID, PlatformType platform, string coGuid)
        {
            List<TVPApiModule.yes.tvinci.ITProxy.KeyValuePair> retVal = null;

            if (paramCollection != null && paramCollection.Count > 0)
            {
                retVal = new List<TVPApiModule.yes.tvinci.ITProxy.KeyValuePair>();
                TVPApiModule.yes.tvinci.ITProxy.KeyValuePair pair;

                foreach (Param param in paramCollection)
                {
                    pair = new TVPApiModule.yes.tvinci.ITProxy.KeyValuePair();
                    if (param.name == "external_content_id")
                    {
                        // get media's 
                        string externalContentID = GetExternalContentIdForMedia(mediaID, groupID, platform);
                        if (!string.IsNullOrEmpty(externalContentID))
                            pair = new TVPApiModule.yes.tvinci.ITProxy.KeyValuePair() { Key = param.key, Value = externalContentID };
                    }
                    else if (param.name == "co_guid")
                    {
                        pair = new TVPApiModule.yes.tvinci.ITProxy.KeyValuePair() { Key = param.key, Value = coGuid };
                    }
                    else
                        pair = new TVPApiModule.yes.tvinci.ITProxy.KeyValuePair() { Key = param.key, Value = param.value };

                    retVal.Add(pair);
                }
            }
            return retVal != null ? retVal.ToArray() : null;
        }

        private static string GetExternalContentIdForMedia(int mediaID, int groupID, PlatformType platform)
        {
            string retVal = null;
            dsItemInfo media = new TVPApiModule.CatalogLoaders.APIMediaLoader(mediaID, groupID, groupID, platform.ToString(), SiteHelper.GetClientIP(), string.Empty).Execute() as dsItemInfo;
            if (media != null && media.Item.Count > 0)
            {
                if (media.Item[0].MediaType.ToLower() == "program" || media.Item[0].MediaType.ToLower() == "movie" || media.Item[0].MediaType.ToLower() == "episode")
                {
                    string sIbmsTitleID = media.Item[0].GetMetasRows()[0]["IBMSTitleID"].ToString();
                    string[] ibmsTitleID = !string.IsNullOrEmpty(sIbmsTitleID) ? sIbmsTitleID.Split('-') : null;
                    if (ibmsTitleID != null && ibmsTitleID.Length > 0)
                        retVal = ibmsTitleID[0].Substring(sIbmsTitleID.LastIndexOf('/') + 1);
                }
                else
                {
                    string offerID = media.Item[0].GetTagsRows()[0]["OfferID"].ToString();
                    retVal = !string.IsNullOrEmpty(offerID) ? offerID.Replace(";", string.Empty) : null;
                }
            }
            return retVal;
        }

        private static List<Media> GetFailOverChannel(TVPApi.InitializationObject initObj, int groupID, int channelID, string picSize, int maxResults)
        {
            List<Media> retVal = null;

            // get channel medias 
            dsItemInfo medias = new APIChannelMediaLoader(channelID, groupID, groupID, initObj.Platform.ToString(), SiteHelper.GetClientIP(), maxResults, 0, null, picSize, null, CutWith.OR) { Culture = initObj.Locale.LocaleLanguage }.Execute() as dsItemInfo;
            if (medias != null && medias.Item != null && medias.Item.Rows != null && medias.Item.Rows.Count > 0)
            {
                retVal = new List<Media>();
                foreach (dsItemInfo.ItemRow row in medias.Item.Rows)
                    (retVal as List<Media>).Add(new Media(row, initObj, groupID, false, medias.Item.Count));
            }
            return retVal;
        }

        internal static Gallery GetGalleryConfigurationByType(TVPApi.Configuration.OrcaConfiguration.ApiOrcaRecommendationsConfiguration orcaConfiguration, eGalleryType galleryType)
        {
            foreach (Gallery gallery in orcaConfiguration.Data.Galleries.GalleryCollection)
            {
                if (gallery.eGalleryType == galleryType)
                    return gallery;
            }
            return null;
        }

        internal static DateTime GetEndTimeForLiveRequest(TVPApi.Configuration.OrcaConfiguration.ApiOrcaRecommendationsConfiguration orcaConfiguration)
        {
            DateTime dtEndTime = DateTime.UtcNow.AddHours(orcaConfiguration.Data.EndTimeOffset);
            int gmtOffset = orcaConfiguration.Data.GMTOffset;
            if (dtEndTime.Date != DateTime.UtcNow.Date && orcaConfiguration.Data.EndTimeLimit)
                return new DateTime(DateTime.UtcNow.AddHours(gmtOffset).Year, DateTime.UtcNow.AddHours(gmtOffset).Month, DateTime.UtcNow.AddHours(gmtOffset).Day, 23, 59, 59);
            else
                return dtEndTime;
        }

        internal static OrcaResponse GetFailoverChannel(object orcaResponse, int groupID, TVPApi.InitializationObject initObj, string picSize)
        {
            OrcaResponse retVal = new OrcaResponse();
            var orcaConfiguration = ConfigManager.GetInstance().GetConfig(groupID, initObj.Platform).OrcaRecommendationsConfiguration;
            int maxResults = orcaConfiguration.Data.MaxResults;
            int channelID;

            if (orcaResponse is List<LiveRecommendation>)
                channelID = orcaConfiguration.Data.LiveFailOverChannelID;
            else
                channelID = orcaConfiguration.Data.VODFailOverChannelID;

            retVal.ContentType = eContentType.VOD;
            retVal.Content = GetFailOverChannel(initObj, groupID, channelID, picSize, maxResults);

            return retVal;
        }
    }
}


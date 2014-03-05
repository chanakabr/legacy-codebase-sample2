using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using log4net;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.CatalogLoaders;
using TVPApiModule.Manager;
using TVPApiModule.Objects.ORCARecommendations;
using TVPApiModule.Services;
using TVPApiModule.yes.tvinci.ITProxy;
using TVPPro.Configuration.OrcaRecommendations;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.CatalogLoaders;


namespace TVPApiModule.Helper
{
    public class OrcaResponse
    {
        public eContentType ContentType { get; set; }
        public Object Content { get; set; }
    }
    
    public class RecommendationsHelper
    {
        private static readonly ILog logger = LogManager.GetLogger(typeof(RecommendationsHelper));

        internal static List<VideoRecommendation> ParseOrcaResponseToVideoRecommendationList(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                logger.ErrorFormat("RecommendationsHelper::ParseOrcaResponseToVideoRecommendationList -> No response from Orca");
                return null;
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

        internal static List<LiveRecommendation> ParseOrcaResponseToLiveRecommendationList(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                logger.ErrorFormat("RecommendationsHelper::ParseOrcaResponseToLiveRecommendationList -> No response from Orca");
                return null;
            }

            List<LiveRecommendation> retVal = new List<LiveRecommendation>();

            LiveRecommendation recommendation;

            foreach (Match m in Regex.Matches(xml, @"(?<=LiveRecommendation)[\S\s]*?(?=\<\/LiveRecommendation)"))
            {
                //    recommendation = new LiveRecommendation();
                //    Match externalContentId = Regex.Match(m.Value, "(?<programId>(?<=\\bprogramId=\")[^\"]*)");
                //    if (externalContentId.Success)
                //    {
                //        recommendation.ProgramID = externalContentId.Value;
                //    }
                //    retVal.Add(recommendation);
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
                            //Match id = Regex.Match(program.Value, "(?<=-)(.*?)(?=-)");
                            //if (id.Success)
                            //{
                            //    recommendation.ProgramID = id.Value;
                            //    retVal.Add(recommendation);
                            //    break;
                            //}
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

        internal static dsItemInfo GetVodRecommendedMediasFromCatalog(int groupID, PlatformType platform, string picSize, List<VideoRecommendation> videoRecommendations)
        {
            dsItemInfo medias = null;
            //List<string> testMetas = new List<string>();
            //List<string> testTags = new List<string>();

            List<KeyValue> metas = new List<KeyValue>();
            List<KeyValue> tags = new List<KeyValue>();

            List<KeyValue> orList = new List<KeyValue>();


            foreach (var recommendation in videoRecommendations)
            {
                if (recommendation.ContentType == "Movie" || recommendation.ContentType == "Program")
                    //testMetas.Add(recommendation.ContentID.ToString());
                    //metas.Add(new KeyValue() { m_sKey = "IBMSTitleID", m_sValue = recommendation.ContentID });
                    orList.Add(new KeyValue() { m_sKey = "IBMSTitleID", m_sValue = recommendation.ContentID });


                else if (recommendation.ContentType == "Season")
                    //metas.Add(new KeyValue() { m_sKey = "IBMSseriesID", m_sValue = string.IsNullOrEmpty(recommendation.SeriesID) ? string.Empty : recommendation.SeriesID });
                    orList.Add(new KeyValue() { m_sKey = "IBMSseriesID", m_sValue = string.IsNullOrEmpty(recommendation.SeriesID) ? string.Empty : recommendation.SeriesID });
                else
                    //testTags.Add(recommendation.ContentID.ToString());
                    //tags.Add(new KeyValue() { m_sKey = "OfferID", m_sValue = recommendation.ContentID});                        
                    orList.Add(new KeyValue() { m_sKey = "OfferID", m_sValue = recommendation.ContentID });                        
            }

            APISearchMediaLoader loader = new APISearchMediaLoader(groupID, groupID, platform.ToString(), SiteHelper.GetClientIP(), 0, 0, picSize, string.Empty)
            {
                //Tags = tags,
                //Tags = new List<KeyValue>()
                //        {
                //            new KeyValue() {m_sKey = "OfferID", m_sValue = string.Join(";", testTags.ToArray()),}
                //        },
                //Metas = metas,
                //Metas = new List<KeyValue>()
                //        {
                //            new KeyValue() {m_sKey = "IBMSTitleID", m_sValue = string.Join(";", testMetas.ToArray()),}
                //        },
                OrList = orList, 
                And = false,
                Exact = false, 
                
            };

            loader.Name = string.Empty;
            loader.Description = string.Empty;
            
            medias = loader.Execute() as dsItemInfo;
            
            return medias;
        }

        internal static List<Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject> GetLiveRecommendedMedias(string siteGuid, int groupID, PlatformType platform, string calture, string picSize, List<LiveRecommendation> liveRecommendations)
        {
            List<Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject> epgChannelProgrammes = null;

            List<string> pids = liveRecommendations.Select(r => r.ProgramID).ToList();

            try
            {
                //ApiApiService apiService = new ApiApiService(groupID, platform);
                var lng = TextLocalizationManager.Instance.GetTextLocalization(groupID, platform).GetLanguages().Where(l => l.Culture == calture).FirstOrDefault();
                Tvinci.Data.Loaders.TvinciPlatform.Catalog.Language language = Tvinci.Data.Loaders.TvinciPlatform.Catalog.Language.Hebrew;
                if (lng != null)
                    language = (Tvinci.Data.Loaders.TvinciPlatform.Catalog.Language)Enum.Parse(typeof(Tvinci.Data.Loaders.TvinciPlatform.Catalog.Language), lng.Name);

                var duration = (int)(RecommendationsHelper.GetEndTimeForLiveRequest(ConfigManager.GetInstance().GetConfig(groupID, platform).OrcaRecommendationsConfiguration) - DateTime.UtcNow).TotalMinutes;
                epgChannelProgrammes = new EPGProgramsByProgramsIdentefierLoader(groupID, SiteHelper.GetClientIP(), pids.Count(), 0, pids, duration, language)
                    {
                        SiteGuid = siteGuid
                    }.Execute() as List<Tvinci.Data.Loaders.TvinciPlatform.Catalog.EPGChannelProgrammeObject>;

                    //apiService.GetEPGProgramsByProgramsIdentefier(siteGuid, pids, language, duration);
                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("RecommendationsHelper::GetLiveRecommendedMedias -> {0}", ex);
            }

            return epgChannelProgrammes;
        }

        internal static KeyValuePair[] GetExtraParamsFromConfig(ParamsParamCollection paramCollection, int mediaID, int groupID, PlatformType platform)
        {
            List<KeyValuePair> retVal = null;

            if (paramCollection != null && paramCollection.Count > 0)
            {
                retVal = new List<KeyValuePair>();
                KeyValuePair pair;

                foreach (Param param in paramCollection)
                {
                    pair = new KeyValuePair();
                    if (param.name == "external_content_id")
                    {
                        // get media's 
                        string externalContentID = GetExternalContentIdForMedia(mediaID, groupID, platform);
                        if (!string.IsNullOrEmpty(externalContentID))
                            pair = new KeyValuePair() { Key = param.key, Value = externalContentID };
                    }
                    else
                        pair = new KeyValuePair() { Key = param.key, Value = param.value };

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
                if (media.Item[0].MediaType.ToLower() == "program" || media.Item[0].MediaType.ToLower() == "movie")
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

        internal static List<Media> GetFailOverChannel(TVPApi.InitializationObject initObj, int groupID, int channelID, string picSize, int maxResults)
        {
            List<Media> retVal = null;

            // get channel medias 
            dsItemInfo medias = new APIChannelMediaLoader(channelID, groupID, groupID, initObj.Platform.ToString(), SiteHelper.GetClientIP(), maxResults, 0, null, picSize, null, CutWith.OR)
                {
                    SiteGuid = initObj.SiteGuid
                }.Execute() as dsItemInfo;
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

        public static TVPPro.SiteManager.TvinciPlatform.api.EPGChannelProgrammeObject[] EPGProgramsByProgramsIdentefierLoader { get; set; }
    }
}


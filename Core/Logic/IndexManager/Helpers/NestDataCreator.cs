using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiLogic.IndexManager.NestData;
using ApiObjects;
using ApiObjects.Nest;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using GroupsCacheManager;
using RestSharp.Serializers;
using Media = ApiLogic.IndexManager.NestData.Media;

namespace ApiLogic.IndexManager.Helpers
{
    public static class NestDataCreator
    {
        public static Epg GetEpg(EpgCB epgCb, int languageId, bool withRouting = true, bool isOpc = false,
            long? expiryUnixTimeStamp=null, long? recordingId = null)
        {
            return new Epg(epgCb,
                languageId,
                isOpc,
                withRouting,
                ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT,
                recordingId,
                expiryUnixTimeStamp);
        }

        public static Media GetMedia(ApiObjects.SearchObjects.Media media, LanguageObj language)
        {
            return new Media(media, language.Code,language.ID);
        }


        public static NestData.SocialActionStatistics GetSocialActionStatistics(ApiObjects.Statistics.SocialActionStatistics statistics)
        {
            return new NestData.SocialActionStatistics()
            {
                Action = statistics.Action,
                Count = statistics.Count,
                Date = statistics.Date,
                MediaType = statistics.MediaType,
                RateValue = statistics.RateValue,
                GroupID = statistics.GroupID,
                MediaID = statistics.MediaID
            };
            
        }

        public static ChannelMetadata GetChannelMetadata(Channel channel)
        {
            return new ChannelMetadata(channel);
        }
    }
}
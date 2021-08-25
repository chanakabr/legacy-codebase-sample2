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
using NestMedia = ApiLogic.IndexManager.NestData.NestMedia;
using NestSocialActionStatistics = ApiObjects.Nest.NestSocialActionStatistics;

namespace ApiLogic.IndexManager.Helpers
{
    public static class NestDataCreator
    {
        public static NestEpg GetEpg(EpgCB epgCb, int languageId, bool withRouting = true, bool isOpc = false,
            long? expiryUnixTimeStamp=null, long? recordingId = null)
        {
            return new NestEpg(epgCb,
                languageId,
                isOpc,
                withRouting,
                ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT,
                recordingId,
                expiryUnixTimeStamp);
        }

        public static NestMedia GetMedia(ApiObjects.SearchObjects.Media media, LanguageObj language)
        {
            return new NestMedia(media, language.Code,language.ID);
        }


        public static NestSocialActionStatistics GetSocialActionStatistics(ApiObjects.Statistics.SocialActionStatistics statistics)
        {
            return new NestSocialActionStatistics()
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

        public static NestChannelMetadata GetChannelMetadata(Channel channel)
        {
            return new NestChannelMetadata(channel);
        }
    }
}
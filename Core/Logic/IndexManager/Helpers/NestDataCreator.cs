using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using ApiObjects.Nest;
using ApiObjects.Statistics;
using RestSharp.Serializers;

namespace ApiLogic.IndexManager.Helpers
{
    public static class NestDataCreator
    {
        public static NestEpg GetEpg(EpgCB epgCb,int languageId, bool withRouting = true, bool isOpc = false)
        {
            return new NestEpg(epgCb,languageId, isOpc, withRouting, ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT);
        }


        public static NestSocialActionStatistics GetSocialActionStatistics(SocialActionStatistics statistics)
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
    }
}
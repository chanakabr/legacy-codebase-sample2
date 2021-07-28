using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects;
using ApiObjects.Nest;
using RestSharp.Serializers;

namespace ApiLogic.IndexManager.Helpers
{
    public class ElasticSearchNestDataBuilder
    {
        public NestEpg BuildEpg(EpgCB epgCb, string suffix = null, bool withRouting = true, bool isOpc = false)
        {
            var epg = new NestEpg(epgCb, isOpc,withRouting,ElasticSearch.Common.Utils.ES_DATEONLY_FORMAT,suffix);
            return epg;
        }

        
    }
}
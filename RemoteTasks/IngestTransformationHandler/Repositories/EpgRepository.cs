using System;
using System.Collections.Generic;
using System.Reflection;
using ApiObjects.BulkUpload;
using ApiObjects.SearchObjects;
using Core.Catalog.CatalogManagement;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using KLogMonitor;
using Newtonsoft.Json.Linq;
using ESUtils = ElasticSearch.Common.Utils;

namespace IngestTransformationHandler.Repositories
{
    public interface IEpgRepository
    {
        IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int groupId, int channelId, DateTime fromDate, DateTime toDate);
    }

    public class EpgRepository : IEpgRepository
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly ElasticSearchApi _esClient;

        public EpgRepository()
        {
            _esClient = new ElasticSearchApi();
        }
        
        public IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int groupId, int channelId, DateTime fromDate, DateTime toDate)
        {
            _logger.Debug($"GetCurrentProgramsByDate > fromDate:[{fromDate}], toDate:[{toDate}]");
            var result = new List<EpgProgramBulkUploadObject>();
            var index = IndexManager.GetEpgIndexAlias(groupId);

            // if index does not exist - then we have a fresh start, we have 0 programs currently
            if (!_esClient.IndexExists(index))
            {
                _logger.Debug($"GetCurrentProgramsByDate > index alias:[{index}] does not exits, assuming no current programs");
                return result;
            }

            _logger.Debug($"GetCurrentProgramsByDate > index alias:[{index}] found, searching current programs, minStartDate:[{fromDate}], maxEndDate:[{toDate}]");
            var query = new FilteredQuery();

            // Program end date > minimum start date
            // program start date < maximum end date
            var minimumRange = new ESRange(false, "end_date", eRangeComp.GTE, fromDate.ToString(ESUtils.ES_DATE_FORMAT));
            var maximumRange = new ESRange(false, "start_date", eRangeComp.LTE, toDate.ToString(ESUtils.ES_DATE_FORMAT));
            var channelFilter = ESTerms.GetSimpleNumericTerm("epg_channel_id", new[] {channelId});


            var filterCompositeType = new FilterCompositeType(CutWith.AND);
            filterCompositeType.AddChild(minimumRange);
            filterCompositeType.AddChild(maximumRange);
            filterCompositeType.AddChild(channelFilter);


            query.Filter = new QueryFilter()
            {
                FilterSettings = filterCompositeType
            };

            query.ReturnFields.Clear();
            query.AddReturnField("_index");
            query.AddReturnField("epg_id");
            query.AddReturnField("start_date");
            query.AddReturnField("end_date");
            query.AddReturnField("epg_identifier");
            query.AddReturnField("is_auto_fill");
            query.AddReturnField("linear_media_id");
            query.AddReturnField("group_id");

            // get the epg document ids from elasticsearch
            var searchQuery = query.ToString();
            var searchResult = _esClient.Search(index, IndexManager.EPG_INDEX_TYPE, ref searchQuery);
            
            // get the programs - epg ids from elasticsearch, information from EPG DAL
            if (!string.IsNullOrEmpty(searchResult))
            {
                var json = JObject.Parse(searchResult);

                var hits = (json["hits"]["hits"] as JArray);

                foreach (var hit in hits)
                {
                    var hitFields = hit["fields"];
                    var epgItem = new EpgProgramBulkUploadObject();
                    epgItem.EpgExternalId = ESUtils.ExtractValueFromToken<string>(hitFields, "epg_identifier");
                    epgItem.StartDate = ESUtils.ExtractDateFromToken(hit["fields"], "start_date");
                    epgItem.EndDate = ESUtils.ExtractDateFromToken(hit["fields"], "end_date");
                    epgItem.EpgId = ESUtils.ExtractValueFromToken<ulong>(hitFields, "epg_id");
                    epgItem.IsAutoFill = ESUtils.ExtractValueFromToken<bool>(hitFields, "is_auto_fill");
                    epgItem.ChannelId = channelId;
                    epgItem.LinearMediaId = ESUtils.ExtractValueFromToken<int>(hitFields, "linear_media_id");
                    epgItem.ParentGroupId = ESUtils.ExtractValueFromToken<int>(hitFields, "group_id");;
                    epgItem.GroupId = groupId;

                    result.Add(epgItem);
                }
            }

            return result;
        }

    }
}
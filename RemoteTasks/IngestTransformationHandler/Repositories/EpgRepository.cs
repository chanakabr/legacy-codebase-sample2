using System;
using System.Collections.Generic;
using System.Reflection;
using ApiObjects.BulkUpload;
using ApiObjects.SearchObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Social.Requests;
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
        private readonly IIndexManagerFactory _indexManagerFactory;

        public EpgRepository()
        {
            _indexManagerFactory = IndexManagerFactory.Instance;
        }

        public IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int groupId, int channelId, DateTime fromDate, DateTime toDate)
        {
            return _indexManagerFactory.GetIndexManager(groupId).GetCurrentProgramsByDate(channelId, fromDate, toDate);
        }
    }
}
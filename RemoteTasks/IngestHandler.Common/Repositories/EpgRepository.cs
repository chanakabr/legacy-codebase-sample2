using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.BulkUpload;
using Core.Catalog;
using CouchbaseManager;
using Phx.Lib.Log;
using ESUtils = ElasticSearch.Common.Utils;

namespace IngestHandler.Common.Repositories
{
    public interface IEpgRepository
    {
        IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int groupId, int channelId, DateTime fromDate, DateTime toDate);

        EpgProgramInfo[] GetCurrentProgramInfosByDate(int partnerId, int channelId, DateTime fromDate, DateTime toDate);
    }

    public class EpgRepository : IEpgRepository
    {
        private static readonly KLogger _logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private readonly IIndexManagerFactory _indexManagerFactory;

        public EpgRepository( )
        {
            _indexManagerFactory = IndexManagerFactory.Instance;
        }

        public IList<EpgProgramBulkUploadObject> GetCurrentProgramsByDate(int groupId, int channelId, DateTime fromDate, DateTime toDate)
        {
            return _indexManagerFactory.GetIndexManager(groupId).GetCurrentProgramsByDate(channelId, fromDate, toDate);
        }

        public EpgProgramInfo[] GetCurrentProgramInfosByDate(int partnerId, int channelId, DateTime fromDate, DateTime toDate)
        {
            return _indexManagerFactory.GetIndexManager(partnerId).GetCurrentProgramInfosByDate(channelId, fromDate, toDate).ToArray();
        }
    }
}
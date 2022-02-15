using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupsCacheManager;
using Phx.Lib.Log;
using System.Reflection;
using Core.Catalog;
using Core.Catalog.CatalogManagement;

namespace ElasticSearchHandler.Updaters
{
    public class ChannelUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string PERCOLATOR = ".percolator";

        private int m_nGroupID;
        private IIndexManager _indexManager;

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public ChannelUpdaterV2(int nGroupID)
        {
            m_nGroupID = nGroupID;
            _indexManager = IndexManagerFactory.Instance.GetIndexManager(nGroupID);
        }

        public bool Start()
        {
            bool result = false;
            log.Debug("Info - Start Channel update");
            if (IDs == null || IDs.Count == 0)
            {
                log.Debug("Info - Channel Id list empty");
                result = true;

                return result;
            }

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                    result = DeleteChannel(IDs);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    result = UpdateChannel(IDs);
                    break;
                default:
                    result = true;
                    break;
            }

            return result;
        }

        private bool DeleteChannel(List<int> channelIds)
        {
            return _indexManager.DeleteChannelPercolator(channelIds);
        }

        private bool UpdateChannel(List<int> channelIds)
        {
            return _indexManager.UpdateChannelPercolator(channelIds);
        }
    }
}
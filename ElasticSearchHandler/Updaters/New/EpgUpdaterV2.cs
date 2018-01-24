using ApiObjects;
using ElasticSearch.Common;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using ElasticSearch.Searcher;
using KlogMonitorHelper;

namespace ElasticSearchHandler.Updaters
{
    public class EpgUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string EPG = "epg";
        public static readonly int DAYS = 30;

        #region Data Members

        protected int groupId;
        protected ElasticSearch.Common.ESSerializerV2 esSerializer;
        protected ElasticSearch.Common.ElasticSearchApi esApi;
        protected EpgBL.BaseEpgBL epgBL;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public string ElasticSearchUrl
        {
            get
            {
                if (esApi != null)
                {
                    return esApi.baseUrl;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (esApi != null)
                {
                    esApi.baseUrl = value;
                }
            }
        }

        #endregion

        #region Ctors

        public EpgUpdaterV2(int groupId)
        {
            this.groupId = groupId;
            esSerializer = new ElasticSearch.Common.ESSerializerV2();
            esApi = new ElasticSearch.Common.ElasticSearchApi();

            epgBL = EpgBL.Utils.GetInstance(this.groupId);
        }

        #endregion

        #region Interface methods

        public virtual bool Start()
        {
            bool result = false;
            log.Debug("Info - Start EPG update");
            if (IDs == null || IDs.Count == 0)
            {
                log.Debug("Info - EPG Id list empty");
                result = true;

                return result;
            }

            if (!esApi.IndexExists(GetAlias()))
            {
                log.Error("Error - " + string.Format("Index of type EPG for group {0} does not exist", groupId));
                return result;
            }

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                    result = DeleteEpg(IDs);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    {
                        // First we delete so we don't get this weird duplicate ID bug.
                        result = DeleteEpg(IDs);

                        // Only then we update normally
                        result &= UpdateEpg(IDs);
                        break;
                    }
                default:
                    result = true;
                    break;
            }

            return result;
        }

        #endregion

        protected bool UpdateEpg(List<int> epgIds)
        {
            bool result = true;

            foreach (int id in epgIds)
            {
                result &= Core.Catalog.CatalogManagement.IndexManager.UpsertEpg(groupId, id);
            }

            return result;
        }

        protected virtual ulong GetDocumentId(EpgCB epg)
        {
            return epg.EpgID;
        }

        protected virtual string GetDocumentType()
        {
            return EPG;
        }

        protected virtual ulong GetDocumentId(int epgId)
        {
            return (ulong)epgId;
        }

        protected virtual string SerializeEPG(EpgCB epg, string suffix = null)
        {
            return esSerializer.SerializeEpgObject(epg, suffix);
        }

        protected bool DeleteEpg(List<int> epgIDs)
        {
            bool result = true;

            if (epgIDs != null & epgIDs.Count > 0)
            {
                foreach (int id in epgIDs)
                {
                    result &= Core.Catalog.CatalogManagement.IndexManager.DeleteEpg(groupId, id);
                }
            }

            return result;
        }

        protected virtual string GetAlias()
        {
            return ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(groupId);
        }
    }
}

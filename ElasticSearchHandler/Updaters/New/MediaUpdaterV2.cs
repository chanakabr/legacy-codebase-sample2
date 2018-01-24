using ApiObjects.SearchObjects;
using ElasticSearch.Common;
using ElasticSearch.Common.DeleteResults;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;
using APILogic.Api.Managers;

namespace ElasticSearchHandler.Updaters
{
    public class MediaUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string MEDIA = "media";

        #region Data Members

        private int groupID;
        private ElasticSearch.Common.ElasticSearchApi esApi;

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

        public MediaUpdaterV2(int groupID)
        {
            this.groupID = groupID;
            esApi = new ElasticSearch.Common.ElasticSearchApi();
        }

        #endregion

        #region Interface Methods

        public bool Start()
        {
            bool result = false;

            log.Debug("Info - Start Media update");

            if (this.IDs == null || this.IDs.Count == 0)
            {
                log.Debug("Info - Media id list empty");
                result = true;

                return result;
            }

            if (!esApi.IndexExists(ElasticsearchTasksCommon.Utils.GetMediaGroupAliasStr(groupID)))
            {
                log.Error("Error - " + string.Format("Index of type media for group {0} does not exist", groupID));

                return result;
            }

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    result = UpdateMedias(IDs);
                    break;
                case ApiObjects.eAction.Delete:
                    result = Delete(IDs);
                    break;
                default:
                    result = true;
                    break;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private bool UpdateMedias(List<int> mediaIds)
        {
            bool result = true;

            foreach (int mediaId in mediaIds)
            {
                result &= Core.Catalog.CatalogManagement.IndexManager.UpsertMedia(groupID, mediaId);
            }
            
            return result;
        }

        private bool Delete(List<int> mediaIDs)
        {
            bool result = true;

            foreach (int id in mediaIDs)
            {
                result &= Core.Catalog.CatalogManagement.IndexManager.DeleteMedia(groupID, id);
            }

            return result;
        }

        #endregion
    }
}

using ApiObjects.SearchObjects;
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
using Core.Catalog;

namespace ElasticSearchHandler.Updaters
{
    public class MediaUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string MEDIA = "media";

        #region Data Members

        private int groupID;
        private IIndexManager _indexManager;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public MediaUpdaterV2(int groupID)
        {
            this.groupID = groupID;
            _indexManager = IndexManagerFactory.Instance.GetIndexManager(groupID);
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
                bool res = _indexManager.UpsertMedia(mediaId);

                if (res)
                {
                    Core.Api.Managers.AssetRuleManager.UpdateMedia(groupID, mediaId, true);
                }

                result &= res;
            }

            return result;
        }

        private bool Delete(List<int> mediaIDs)
        {
            bool result = true;

            foreach (int id in mediaIDs)
            {
                result &= _indexManager.DeleteMedia(id);
            }

            return result;
        }

        #endregion
    }
}
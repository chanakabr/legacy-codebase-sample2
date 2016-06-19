using ApiObjects.SearchObjects;
using Catalog;
using ElasticSearch.Common;
using ElasticSearch.Common.DeleteResults;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Catalog.Cache;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearchHandler.Updaters
{
    public class MediaUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string MEDIA = "media";

        #region Data Members

        private int groupID;
        private ElasticSearch.Common.ESSerializerV1 esSerializer;
        private ElasticSearch.Common.ElasticSearchApi esApi;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public MediaUpdaterV2(int groupID)
        {
            this.groupID = groupID;
            esSerializer = new ElasticSearch.Common.ESSerializerV1();
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
            GroupManager groupManager = new GroupManager();
            Group group = groupManager.GetGroup(this.groupID);

            if (group == null)
            {
                log.ErrorFormat("Couldn't get group {0}", this.groupID);
                return false;
            }

            bool tempResult;

            foreach (int mediaId in mediaIds)
            {
                try
                {
                    //Create Media Object
                    Dictionary<int, Dictionary<int, Media>> mediaDictionary = ElasticsearchTasksCommon.Utils.GetGroupMedias(groupID, mediaId);

                    if (mediaDictionary != null)
                    {
                        // Just to be sure
                        if (mediaDictionary.ContainsKey(mediaId))
                        {
                            foreach (int languageId in mediaDictionary[mediaId].Keys)
                            {
                                Media media = mediaDictionary[mediaId][languageId];

                                if (media != null)
                                {
                                    string serializedMedia;

                                    serializedMedia = esSerializer.SerializeMediaObject(media);

                                    string type = ElasticsearchTasksCommon.Utils.GetTanslationType(MEDIA, group.GetLanguage(languageId));

                                    if (!string.IsNullOrEmpty(serializedMedia))
                                    {
                                        tempResult = esApi.InsertRecord(groupID.ToString(), type, media.m_nMediaID.ToString(), serializedMedia);
                                        result &= tempResult;

                                        if (!tempResult)
                                        {
                                            log.Error("Error - " + string.Format(
                                                "Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3};",
                                                groupID, type, media.m_nMediaID, serializedMedia));
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Error - " + string.Format("Update medias threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
                    throw ex;
                }
            }

            return result;
        }

        private bool Delete(List<int> mediaIDs)
        {
            bool result = true;
            string index = groupID.ToString();

            ESDeleteResult deleteResult;

            foreach (int id in mediaIDs)
            {
                deleteResult = esApi.DeleteDoc(index, MEDIA, id.ToString());

                if (!deleteResult.Ok)
                {
                    log.Error("Error - " + String.Concat("Could not delete media from ES. Media id=", id));
                }

                result &= deleteResult.Ok;
            }

            return result;
        }

        #endregion
    }
}

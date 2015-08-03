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

namespace ESIndexUpdateHandler.Updaters
{
    public class MediaUpdater : IUpdateable
    {
        #region Consts
        
        public static readonly string MEDIA = "media";

        #endregion

        #region Data Members

        private int groupID;
        private ElasticSearch.Common.ESSerializer esSerializer;
        private ElasticSearch.Common.ElasticSearchApi esApi;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public MediaUpdater(int groupID)
        {
            this.groupID = groupID;
            esSerializer = new ElasticSearch.Common.ESSerializer();
            esApi = new ElasticSearch.Common.ElasticSearchApi();
        }

        #endregion

        #region Interface Methods

        public bool Start()
        {
            bool result = false;

            Logger.Logger.Log("Info", "Start Media update", "ESUpdateHandler");

            if (this.IDs == null || this.IDs.Count == 0)
            {
                Logger.Logger.Log("Info", "Media id list empty", "ESUpdateHandler");
                result = true;

                return result;
            }

            if (!esApi.IndexExists(ElasticsearchTasksCommon.Utils.GetMediaGroupAliasStr(groupID)))
            {
                Logger.Logger.Log("Error", string.Format("Index of type media for group {0} does not exist", groupID), "ESUpdateHandler");

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
                                            Logger.Logger.Log("Error", string.Format(
                                                "Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3};", 
                                                groupID, type, media.m_nMediaID, serializedMedia), "ESUpdateHandler");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Error", string.Format("Update medias threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), "ESUpdateHandler");
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
                    Logger.Logger.Log("Error", String.Concat("Could not delete media from ES. Media id=", id), "ESUpdateHandler");
                }

                result &= deleteResult.Ok;
            }

            return result;
        }

        #endregion
    }
}

using ApiObjects.SearchObjects;
using Catalog;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESIndexUpdateHandler.Updaters
{
    public class MediaUpdater : IUpdateable
    {
        public static readonly string MEDIA = "media";

        private int m_nGroupID;
        private ElasticSearch.Common.ESSerializer m_oESSerializer;
        private ElasticSearch.Common.ElasticSearchApi m_oESApi;

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public MediaUpdater(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESSerializer = new ElasticSearch.Common.ESSerializer();
            m_oESApi = new ElasticSearch.Common.ElasticSearchApi();
        }

        public bool Start()
        {
            bool result = false;
            Logger.Logger.Log("Info", "Start Media update", "ESUpdateHandler");
            if (IDs == null || IDs.Count == 0)
            {
                Logger.Logger.Log("Info", "Media id list empty", "ESUpdateHandler");
                result = true;

                return result;
            }

            if (!m_oESApi.IndexExists(ElasticsearchTasksCommon.Utils.GetMediaGroupAliasStr(m_nGroupID)))
            {
                Logger.Logger.Log("Error", string.Format("Index of type media for group {0} does not exist", m_nGroupID), "ESUpdateHandler");
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

        private bool UpdateMedias(List<int> lMediaIDs)
        {
            bool bRes = true;
            Group oGroup = GroupsCache.Instance.GetGroup(m_nGroupID);

            if (oGroup == null)
                return false;

            bool bTempRes;
            foreach (int nMediaID in lMediaIDs)
            {
                try
                {
                    //Create Media Object
                    Dictionary<int, Dictionary<int, Media>> dMedias = ElasticsearchTasksCommon.Utils.GetGroupMedias(m_nGroupID, nMediaID);

                    if (dMedias != null)
                    {
                        List<ESBulkRequestObj<int>> lBulkObj = new List<ESBulkRequestObj<int>>();

                        if (dMedias.ContainsKey(nMediaID))
                        {
                            foreach (int nLangID in dMedias[nMediaID].Keys)
                            {
                                Media oMedia = dMedias[nMediaID][nLangID];

                                if (oMedia != null)
                                {
                                    string sMediaObj;

                                    sMediaObj = m_oESSerializer.SerializeMediaObject(oMedia);

                                    string sType = ElasticsearchTasksCommon.Utils.GetTanslationType(MEDIA, oGroup.GetLanguage(nLangID));
                                    if (!string.IsNullOrEmpty(sMediaObj))
                                    {

                                        bTempRes = m_oESApi.InsertRecord(m_nGroupID.ToString(), sType, oMedia.m_nMediaID.ToString(), sMediaObj);
                                        bRes &= bTempRes;
                                        if (!bTempRes)
                                        {
                                            Logger.Logger.Log("Error", string.Format("Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3}", m_nGroupID, sType, oMedia.m_nMediaID, sMediaObj), "ESUpdateHandler");
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
                }
            }

            return bRes;
        }
        
        private bool Delete(List<int> lMediaIDs)
        {
            bool bRes = true;

            string sIndex = m_nGroupID.ToString();
            bool bTemp;
            foreach (int id in lMediaIDs)
            {
                bTemp = m_oESApi.DeleteDoc(sIndex, MEDIA, id.ToString());

                if (!bTemp)
                {
                    Logger.Logger.Log("Error", String.Concat("Could not delete media from ES. Media id=",id), "ESUpdateHandler");
                }

                bRes&= bTemp;
            }

            return bRes;
        }

        


    }
}

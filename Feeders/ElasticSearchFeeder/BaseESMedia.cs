using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ApiObjects.MediaIndexingObjects;
using Catalog;
using System.Data;
using ApiObjects.SearchObjects;
using QueueWrapper;
using ElasticSearch.Searcher;
using ElasticSearch.Common;
using ApiObjects;

namespace ElasticSearchFeeder
{
    public class BaseESMedia : ElasticSearchBaseImplementor
    {
        protected const string MEDIA = "media";
        protected const string EPG = "epg";

        protected ESSerializer m_oESSerializer;
        protected ElasticSearchApi m_oESApi;
        protected DateTime m_dStartDate, m_dEndDate;
        public bool bSwitchIndex { get; set; }


        public BaseESMedia(int nGroupID, string sQueueName, bool bRebuildIndex)
            : base(nGroupID, sQueueName, bRebuildIndex)
        {
            m_oESSerializer = new ESSerializer();
            m_oESApi = new ElasticSearchApi();
        }

        public BaseESMedia(int nGroupID, string sQueueName, bool bRebuildIndex, DateTime dStartDate, DateTime dEndDate)
            : base(nGroupID, sQueueName, bRebuildIndex)
        {
            m_oESSerializer = new ESSerializer();
            m_oESApi = new ElasticSearchApi();
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;
        }

        public override async void Update(eESFeederType eESFeeder)
        {
            try
            {
                //If requested, reload index
                if (m_bRebuildIndex || !checkIndexExists(eESFeeder))
                {
                    IndexBuilders.AbstractIndexBuilder oIndexBuilder = IndexBuilders.AbstractIndexBuilder.GetIndexBuilder(m_nGroupID, eESFeeder);
                    oIndexBuilder.dStartDate = m_dStartDate;
                    oIndexBuilder.dEndDate = m_dEndDate;
                    oIndexBuilder.bSwitchIndex = bSwitchIndex;

                    bool bReloadSuccess = await oIndexBuilder.BuildIndex();

                    if (!bReloadSuccess)
                    {
                        Logger.Logger.Log("Error", string.Format("Reloading index failed for group {0}", m_nGroupID), "ESFeeder");
                        return;
                    }
                }
                //Get message queue
                using (IQueueImpl oMessageQueue = QueueImplFactory.GetQueueImp(QueueWrapper.Enums.QueueType.RabbitQueue))
                {
                    if (oMessageQueue != null)
                    {
                        string sGroupID = m_nGroupID.ToString();
                        IndexingData oMessage = null;

                        //Do while queue is not empty
                        string sAckId;
                        bool bRetVal;
                        try
                        {
                            while ((oMessage = oMessageQueue.Dequeue<IndexingData>(m_sQueueName, out sAckId)) != null)
                            {

                                if (oMessage.Ids != null && oMessage.Ids.Count > 0)
                                {
                                    try
                                    {
                                        bRetVal = false;

                                        switch (oMessage.ObjectType)
                                        {
                                            case ApiObjects.eObjectType.Media:
                                                bRetVal = await MediaChanged(oMessage.Ids, oMessage.Action);
                                                break;
                                            case ApiObjects.eObjectType.Channel:
                                                bRetVal = ChannelChanged(oMessage.Ids, oMessage.Action);
                                                break;
                                            case ApiObjects.eObjectType.EPG:
                                                bRetVal = EpgChanged(oMessage.Ids, oMessage.Action);
                                                break;
                                            default:
                                                bRetVal = true;
                                                break;
                                        }
                                        if (bRetVal && !string.IsNullOrEmpty(sAckId))
                                        {
                                            oMessageQueue.Ack(m_sQueueName, sAckId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Logger.Log("Exception ", ex.Message, "ESFeeder");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("Exception on Dequeue", ex.Message, "ESFeeder");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", ex.Message, "ESFeeder");
            }
        }

        private bool EpgChanged(List<int> lEpgIDs, ApiObjects.eAction eAction)
        {
            bool bRes = false;

            try
            {

                Task<EpgCB>[] tPrograms = new Task<EpgCB>[lEpgIDs.Count];
                //open task factory and run GetEpgProgram on different threads
                //wait to finish
                //bulk insert
                for (int i = 0; i < lEpgIDs.Count; i++)
                {
                    tPrograms[i] = Task.Factory.StartNew<EpgCB>(
                        (index) =>
                        {
                            return Utils.GetEpgProgram(m_nGroupID, (int)index);
                        }, lEpgIDs[i]);
                }

                Task.WaitAll(tPrograms);

                List<EpgCB> lEpg = tPrograms.Select(t => t.Result).Where(t => t != null).ToList();

                if (lEpg != null & lEpg.Count > 0)
                {
                    List<KeyValuePair<ulong, string>> lKvp = new List<KeyValuePair<ulong, string>>();
                    string sSerializedEpg;
                    foreach (EpgCB epg in lEpg)
                    {
                        sSerializedEpg = m_oESSerializer.SerializeEpgObject(epg);
                        lKvp.Add(new KeyValuePair<ulong, string>(epg.EpgID, sSerializedEpg));
                    }
                    string sAlias = Utils.GetEpgGroupAliasStr(m_nGroupID);
                    m_oESApi.CreateBulkIndexRequest(sAlias, EPG, lKvp);

                    bRes = true;
                }
            }
            catch { }

            return bRes;
        }

        private bool checkIndexExists(eESFeederType eFeeder)
        {
            bool bRes = false;

            switch (eFeeder)
            {
                case eESFeederType.MEDIA:
                    bRes = m_oESApi.IndexExists(Utils.GetMediaGroupAliasStr(m_nGroupID));
                    break;
                case eESFeederType.EPG:
                    bRes = m_oESApi.IndexExists(Utils.GetEpgGroupAliasStr(m_nGroupID));
                    break;
                default:
                    break;
            }

            return bRes;
        }

        private bool ChannelChanged(List<int> lChannelIDs, ApiObjects.eAction eAction)
        {
            bool bRes = false;
            switch (eAction)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                    bRes = Delete(lChannelIDs, ApiObjects.eObjectType.Channel);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    bRes = UpdateChannel(lChannelIDs);
                    break;
                default:
                    bRes = true;
                    break;
            }

            return bRes;
        }

        private async Task<bool> MediaChanged(List<int> lMediaIDs, ApiObjects.eAction eAction)
        {
            bool bRes = false;
            switch (eAction)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    bRes = await UpdateMedias(lMediaIDs);
                    break;
                case ApiObjects.eAction.Delete:
                    bRes = Delete(lMediaIDs, ApiObjects.eObjectType.Media);
                    break;
                default:
                    bRes = true;
                    break;
            }

            return bRes;
        }

        private bool Delete(List<int> lIDs, ApiObjects.eObjectType eObjectType)
        {
            bool bRes = false;

            if (lIDs == null || lIDs.Count == 0)
                return bRes;

            string sIndex = m_nGroupID.ToString();

            if (eObjectType == ApiObjects.eObjectType.Media)
            {
                foreach (int id in lIDs)
                {
                    bRes = m_oESApi.DeleteDoc(sIndex, MEDIA, id.ToString());
                }
            }
            else if (eObjectType == ApiObjects.eObjectType.Channel)
            {
                List<string> aliases = m_oESApi.GetAliases(sIndex);
                if (aliases != null && aliases.Count > 0)
                {
                    foreach (int nChannelID in lIDs)
                    {
                        foreach (string index in aliases)
                        {
                            bRes = m_oESApi.DeleteDoc("_percolator", index, nChannelID.ToString());
                        }
                    }
                }
            }

            return bRes;
        }

        private bool UpdateChannel(List<int> lChannelIds)
        {
            bool bRes = false;
            Group oGroup = GroupsCache.Instance.GetGroup(m_nGroupID);
            if (oGroup == null || oGroup.m_oGroupChannels == null)
                return bRes;

            List<string> aliases = m_oESApi.GetAliases(m_nGroupID.ToString());

            Channel oChannel;
            MediaSearchObj oSearchObj;
            ESMediaQueryBuilder oQueryParser;
            string sQueryStr;

            if (aliases != null && aliases.Count > 0)
            {

                foreach (int nChannelID in lChannelIds)
                {
                    oChannel = ChannelRepository.GetChannel(nChannelID, oGroup);
                    if (oChannel != null && oChannel.m_nIsActive == 1)
                    {
                        oQueryParser = new ESMediaQueryBuilder() { QueryType = eQueryType.EXACT, m_nGroupID = oChannel.m_nGroupID };
                        oSearchObj = BuildBaseChannelSearchObject(oChannel);
                        oQueryParser.oSearchObject = oSearchObj;
                        sQueryStr = oQueryParser.BuildSearchQueryString(false);

                        foreach (string sIndex in aliases)
                        {
                            bRes = m_oESApi.AddQueryToPercolator(sIndex, oChannel.m_nChannelID.ToString(), ref sQueryStr);
                        }
                    }
                }
            }

            return bRes;
        }

        private async Task<bool> UpdateMedias(List<int> lMediaIDs)
        {
            bool bRes = true;
            foreach (int nMediaID in lMediaIDs)
            {
                try
                {
                    //Create Media Object
                    Dictionary<int, Media> dMedias = await GetGroupMedias(nMediaID);

                    if (dMedias.ContainsKey(nMediaID))
                    {
                        Media oMedia = dMedias[nMediaID];

                        string sMediaObject = m_oESSerializer.SerializeMediaObject(oMedia);

                        if (!string.IsNullOrEmpty(sMediaObject))
                        {
                            bRes &= m_oESApi.InsertRecord(m_nGroupID.ToString(), MEDIA, oMedia.m_nMediaID.ToString(), sMediaObject);

                        }
                    }
                    else
                    {
                        bRes = false;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Start update Media", "Exception " + ex.Message, "ESFeeder");
                }
            }

            return bRes;
        }

        protected async Task<Dictionary<int, Media>> GetGroupMedias(int nMediaID)
        {
            Dictionary<int, Media> medias = new Dictionary<int, Media>();
            try
            {
                Group oGroup = GroupsCache.Instance.GetGroup(m_nGroupID);
                if (oGroup == null)
                {
                    return medias;
                }
                ODBCWrapper.StoredProcedure GroupMedias = new ODBCWrapper.StoredProcedure("Get_GroupMedias");
                GroupMedias.SetConnectionKey("MAIN_CONNECTION_STRING");

                GroupMedias.AddParameter("@GroupID", m_nGroupID);
                GroupMedias.AddParameter("@MediaID", nMediaID);

                Task<DataSet> tDS = Task<DataSet>.Factory.StartNew(() => GroupMedias.ExecuteDataSet());
                tDS.Wait();
                DataSet ds = tDS.Result;

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows.Count > 0)
                    {
                        //for (int i = 0; i < ds.Tables[0].Rows.Count; i++)
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            Media media = new Media();
                            if (ds.Tables[0].Columns != null && ds.Tables[0].Rows != null)
                            {
                                #region media info
                                media.m_nMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "ID");
                                media.m_nWPTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "watch_permission_type_id");
                                media.m_nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_type_id");
                                media.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(row, "group_id");
                                media.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(row, "is_active");
                                media.m_nDeviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(row, "device_rule_id");
                                media.m_nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(row, "like_counter");
                                media.m_nViews = ODBCWrapper.Utils.GetIntSafeVal(row, "views");
                                media.m_sUserTypes = ODBCWrapper.Utils.GetSafeStr(row["user_types"]);

                                double dSum = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_sum");
                                double dCount = ODBCWrapper.Utils.GetDoubleSafeVal(row, "votes_count");

                                if (dCount > 0)
                                {
                                    media.m_nVotes = (int)dCount;
                                    media.m_dRating = dSum / dCount;
                                }

                                media.m_sName = ODBCWrapper.Utils.GetSafeStr(row, "name");
                                media.m_sDescription = ODBCWrapper.Utils.GetSafeStr(row, "description");

                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "create_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "create_date");
                                    media.m_sCreateDate = dt.ToString("yyyyMMddHHmmss");
                                }
                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "update_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "update_date");
                                    media.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
                                }
                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "start_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "start_date");
                                    media.m_sStartDate = dt.ToString("yyyyMMddHHmmss");
                                }

                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "end_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "end_date");
                                    media.m_sEndDate = dt.ToString("yyyyMMddHHmmss");

                                }

                                if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row, "final_end_date")))
                                {
                                    DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row, "final_end_date");
                                    media.m_sFinalEndDate = dt.ToString("yyyyMMddHHmmss");

                                }
                                #endregion

                                #region - get all metas by groupId
                                //Strings
                                Dictionary<string, string> dMetas;
                                //Get Meta - MetaNames (e.g. will contain key/value <META1_STR, show>)
                                if (oGroup.m_oMetasValuesByGroupId.TryGetValue(media.m_nGroupID, out dMetas))
                                {
                                    foreach (string sMeta in dMetas.Keys)
                                    {
                                        //Retreive meta name and check that it is not null or empty so that it will not form an invalid field later on
                                        string sMetaName;
                                        dMetas.TryGetValue(sMeta, out sMetaName);

                                        if (!string.IsNullOrEmpty(sMetaName))
                                        {
                                            string sMetaValue = ODBCWrapper.Utils.GetSafeStr(row[sMeta]);
                                            media.m_oMeatsValues.Add(sMetaName, sMetaValue);
                                        }
                                    }
                                }
                            }
                            medias.Add(media.m_nMediaID, media);
                                #endregion
                        }

                        #region - get all the media files types for each mediaId that have been selected.
                        if (ds.Tables[1].Columns != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                        {
                            //for (int i = 0; i < ds.Tables[1].Rows.Count; i++)
                            foreach (DataRow row in ds.Tables[1].Rows)
                            {
                                int mediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                                string sMFT = ODBCWrapper.Utils.GetSafeStr(row, "media_type_id");
                                medias[mediaID].m_sMFTypes += string.Format("{0};", sMFT);
                            }
                        }
                        #endregion

                        #region - get all media tags
                        if (ds.Tables[2].Columns != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            //for (int i = 0; i < ds.Tables[2].Rows.Count; i++)
                            foreach (DataRow row in ds.Tables[2].Rows)
                            {
                                int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(row, "media_id");
                                int mttn = ODBCWrapper.Utils.GetIntSafeVal(row, "tag_type_id");
                                string val = ODBCWrapper.Utils.GetSafeStr(row, "value");


                                if (oGroup.m_oGroupTags.ContainsKey(mttn))
                                {
                                    string sTagName = oGroup.m_oGroupTags[mttn];

                                    if (!string.IsNullOrEmpty(sTagName))
                                    {
                                        if (medias[nTagMediaID].m_oTagsValues.ContainsKey(sTagName))
                                        {
                                            medias[nTagMediaID].m_oTagsValues[sTagName] += string.Format(" ; {0}", val);
                                        }
                                        else
                                        {
                                            medias[nTagMediaID].m_oTagsValues.Add(sTagName, val);
                                        }
                                    }
                                }
                            }
                        }
                        #endregion
                    }

                }

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Media Exception", ex.Message, "ESFeeder");
            }
            return medias;
        }


        private static ApiObjects.SearchObjects.MediaSearchObj BuildBaseChannelSearchObject(Channel channel)
        {
            ApiObjects.SearchObjects.MediaSearchObj searchObject = new ApiObjects.SearchObjects.MediaSearchObj();
            searchObject.m_nGroupId = channel.m_nGroupID;
            searchObject.m_bExact = true;
            searchObject.m_eCutWith = channel.m_eCutWith;
            searchObject.m_sMediaTypes = channel.m_nMediaType.ToString();
            searchObject.m_sPermittedWatchRules = Utils.GetPermittedWatchRules(channel.m_nGroupID);
            searchObject.m_oOrder = new ApiObjects.SearchObjects.OrderObj();

            searchObject.m_bUseStartDate = false;
            searchObject.m_bUseFinalEndDate = false;

            CopySearchValuesToSearchObjects(ref searchObject, channel.m_eCutWith, channel.m_lChannelTags);
            return searchObject;
        }

        private static void CopySearchValuesToSearchObjects(ref ApiObjects.SearchObjects.MediaSearchObj searchObject, ApiObjects.SearchObjects.CutWith cutWith, List<ApiObjects.SearchObjects.SearchValue> channelSearchValues)
        {
            List<ApiObjects.SearchObjects.SearchValue> m_dAnd = new List<ApiObjects.SearchObjects.SearchValue>();
            List<ApiObjects.SearchObjects.SearchValue> m_dOr = new List<ApiObjects.SearchObjects.SearchValue>();

            ApiObjects.SearchObjects.SearchValue search = new ApiObjects.SearchObjects.SearchValue();
            if (channelSearchValues != null && channelSearchValues.Count > 0)
            {
                foreach (ApiObjects.SearchObjects.SearchValue searchValue in channelSearchValues)
                {
                    if (!string.IsNullOrEmpty(searchValue.m_sKey))
                    {
                        search = new ApiObjects.SearchObjects.SearchValue();
                        search.m_sKey = searchValue.m_sKey;
                        search.m_lValue = searchValue.m_lValue;
                        search.m_sKeyPrefix = searchValue.m_sKeyPrefix;
                        search.m_eInnerCutWith = searchValue.m_eInnerCutWith;

                        switch (cutWith)
                        {
                            case ApiObjects.SearchObjects.CutWith.OR:
                                {
                                    m_dOr.Add(search);
                                    break;
                                }
                            case ApiObjects.SearchObjects.CutWith.AND:
                                {
                                    m_dAnd.Add(search);
                                    break;
                                }
                            default:
                                break;
                        }
                    }
                }
            }

            if (m_dOr.Count > 0)
            {
                searchObject.m_dOr = m_dOr;
            }

            if (m_dAnd.Count > 0)
            {
                searchObject.m_dAnd = m_dAnd;
            }
        }
    }
}

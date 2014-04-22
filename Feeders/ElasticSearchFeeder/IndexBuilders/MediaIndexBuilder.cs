using ApiObjects.SearchObjects;
using Catalog;
using ElasticSearch.Common;
using ElasticSearch.Searcher;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchFeeder.IndexBuilders
{
    public class MediaIndexBuilder : AbstractIndexBuilder
    {
        protected ElasticSearchApi m_oESApi;
        protected int m_nGroupID;
        protected ESSerializer m_oESSerializer;

        public MediaIndexBuilder(int nGroupID)
        {
            m_nGroupID = nGroupID;
            bSwitchIndex = false;
            bSwitchIndex = false;
            m_oESApi = new ElasticSearchApi();
            m_oESSerializer = new ESSerializer();
        }

        public override async Task<bool> BuildIndex()
        {
            string sNewIndex = Utils.GetNewMediaIndexStr(m_nGroupID);

            #region Build new index and specify number of nodes/shards

            string sNumOfShards = Utils.GetWSURL("ES_NUM_OF_SHARDS");
            string sNumOfReplicas = Utils.GetWSURL("ES_NUM_OF_REPLICAS");

            int nNumOfShards, nNumOfReplicas;

            int.TryParse(sNumOfReplicas, out nNumOfReplicas);
            int.TryParse(sNumOfShards, out nNumOfShards);

            bool bRes = m_oESApi.BuildIndex(sNewIndex, nNumOfShards, nNumOfReplicas);

            if (!bRes)
                return false;
            #endregion

            GroupsCache.Instance.RemoveGroup(m_nGroupID);
            Group oGroup = GroupsCache.Instance.GetGroup(m_nGroupID);

            #region create mapping

            if (oGroup == null)
                return false;

            string sMapping = m_oESSerializer.CreateMediaMapping(oGroup.m_oMetasValuesByGroupId, oGroup.m_oGroupTags);

            bRes = m_oESApi.InsertMapping(sNewIndex, MEDIA, sMapping.ToString());

            if (!bRes)
                return false;
            #endregion

            #region insert medias
            Dictionary<int, Media> dGroupMedias = await GetGroupMedias(0);

            if (dGroupMedias != null)
            {
                List<KeyValuePair<int, string>> lMediaObject = new List<KeyValuePair<int, string>>();
                foreach (int nMediaID in dGroupMedias.Keys)
                {
                    Media oMedia = dGroupMedias[nMediaID];

                    if (oMedia != null)
                    {
                        string sMediaObj = m_oESSerializer.SerializeMediaObject(oMedia);
                        lMediaObject.Add(new KeyValuePair<int, string>(oMedia.m_nMediaID, sMediaObj));
                    }
                    if (lMediaObject.Count >= 50)
                    {
                        Task<List<KeyValuePair<int, string>>> t = Task<List<KeyValuePair<int, string>>>.Factory.StartNew(() => m_oESApi.CreateBulkIndexRequest(sNewIndex, MEDIA, lMediaObject));
                        t.Wait();

                        lMediaObject = new List<KeyValuePair<int, string>>();
                    }
                }

                if (lMediaObject.Count > 0)
                {
                    Task<List<KeyValuePair<int, string>>> t = Task<List<KeyValuePair<int, string>>>.Factory.StartNew(() => m_oESApi.CreateBulkIndexRequest(sNewIndex, MEDIA, lMediaObject));
                    t.Wait();
                }
            }
            #endregion

            #region insert channel queries

            if (oGroup.m_oGroupChannels != null)
            {
                MediaSearchObj oSearchObj;
                string sQueryStr;
                ESMediaQueryBuilder oQueryParser = new ESMediaQueryBuilder() { QueryType = eQueryType.EXACT };


                List<KeyValuePair<int, string>> lChannelRequests = new List<KeyValuePair<int, string>>();
                foreach (int channelId in oGroup.m_oGroupChannels.Keys)
                {
                    Channel oChannel = oGroup.m_oGroupChannels[channelId];

                    if (oChannel == null || oChannel.m_nIsActive != 1)
                        continue;

                    oQueryParser.m_nGroupID = oChannel.m_nGroupID;
                    oSearchObj = BuildBaseChannelSearchObject(oChannel);
                    oQueryParser.oSearchObject = oSearchObj;
                    sQueryStr = oQueryParser.BuildSearchQueryString(false);

                    lChannelRequests.Add(new KeyValuePair<int, string>(oChannel.m_nChannelID, sQueryStr));

                    if (lChannelRequests.Count > 50)
                    {
                        m_oESApi.CreateBulkIndexRequest("_percolator", sNewIndex, lChannelRequests);
                        lChannelRequests.Clear();
                    }
                }

                if (lChannelRequests.Count > 0)
                {
                    m_oESApi.CreateBulkIndexRequest("_percolator", sNewIndex, lChannelRequests);
                }
            }

            #endregion


            if (bSwitchIndex)
            {
                string sAlias = Utils.GetMediaGroupAliasStr(m_nGroupID);
                List<string> lOldIndices = m_oESApi.GetAliases(sAlias);

                Task<bool> tSwitchIndex = Task<bool>.Factory.StartNew(() => m_oESApi.SwitchIndex(sNewIndex, sAlias, lOldIndices));
                tSwitchIndex.Wait();

                if (tSwitchIndex.Result && lOldIndices.Count > 0)
                {
                    Task t = Task.Factory.StartNew(() => m_oESApi.DeleteIndices(lOldIndices));
                    t.Wait();
                }
            }

            return true;
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

                Task<DataSet> tDS = Task<DataSet>.Factory.StartNew(() => Tvinci.Core.DAL.CatalogDAL.Get_GroupMedias(m_nGroupID, nMediaID));
                tDS.Wait();
                DataSet ds = tDS.Result;

                if (ds != null && ds.Tables.Count > 0)
                {
                    if (ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0 && ds.Tables[0].Columns != null)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            Media media = new Media();
                            #region media info
                            media.m_nMediaID = ODBCWrapper.Utils.GetIntSafeVal(row["ID"]);
                            media.m_nWPTypeID = ODBCWrapper.Utils.GetIntSafeVal(row["watch_permission_type_id"]);
                            media.m_nMediaTypeID = ODBCWrapper.Utils.GetIntSafeVal(row["media_type_id"]);
                            media.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(row["group_id"]);
                            media.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(row["is_active"]);
                            media.m_nDeviceRuleId = ODBCWrapper.Utils.GetIntSafeVal(row["device_rule_id"]);
                            media.m_nLikeCounter = ODBCWrapper.Utils.GetIntSafeVal(row["like_counter"]);
                            media.m_nViews = ODBCWrapper.Utils.GetIntSafeVal(row["views"]);
                            media.m_sUserTypes = ODBCWrapper.Utils.GetSafeStr(row["user_types"]);

                            double dSum = ODBCWrapper.Utils.GetDoubleSafeVal(row["votes_sum"]);
                            double dCount = ODBCWrapper.Utils.GetDoubleSafeVal(row["votes_count"]);

                            if (dCount > 0)
                            {
                                media.m_nVotes = (int)dCount;
                                media.m_dRating = dSum / dCount;
                            }

                            media.m_sName = ODBCWrapper.Utils.GetSafeStr(row["name"]);
                            media.m_sDescription = ODBCWrapper.Utils.GetSafeStr(row["description"]);

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["create_date"])))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row["create_date"]);
                                media.m_sCreateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["update_date"])))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row["update_date"]);
                                media.m_sUpdateDate = dt.ToString("yyyyMMddHHmmss");
                            }
                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["start_date"])))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row["start_date"]);
                                media.m_sStartDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["end_date"])))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row["end_date"]);
                                media.m_sEndDate = dt.ToString("yyyyMMddHHmmss");
                            }

                            if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["final_end_date"])))
                            {
                                DateTime dt = ODBCWrapper.Utils.GetDateSafeVal(row["final_end_date"]);
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
                            medias.Add(media.m_nMediaID, media);
                            #endregion
                        }

                        #region - get all the media files types for each mediaId that have been selected.
                        if (ds.Tables[1].Columns != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                        {
                            foreach (DataRow item in ds.Tables[1].Rows)
                            {
                                int mediaID = ODBCWrapper.Utils.GetIntSafeVal(item["media_id"]);
                                string sMFT = ODBCWrapper.Utils.GetSafeStr(item["media_type_id"]);
                                medias[mediaID].m_sMFTypes += string.Format("{0};", sMFT);
                            }
                        }
                        #endregion

                        #region - get all media tags
                        if (ds.Tables[2].Columns != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            foreach (DataRow item in ds.Tables[2].Rows)
                            {
                                int nTagMediaID = ODBCWrapper.Utils.GetIntSafeVal(item["media_id"]);
                                int mttn = ODBCWrapper.Utils.GetIntSafeVal(item["tag_type_id"]);
                                string val = ODBCWrapper.Utils.GetSafeStr(item["value"]);

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
            searchObject.m_sPermittedWatchRules = GetPermittedWatchRules(channel.m_nGroupID);
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

        private static string GetPermittedWatchRules(int nGroupId)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }
    }
}

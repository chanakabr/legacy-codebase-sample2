using Logger;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;
using ApiObjects.SearchObjects;
using ApiObjects;
using System.Threading.Tasks;
using Catalog.Cache;

namespace Catalog
{
    public class ChannelRepository
    {
        private static readonly ILogger4Net _logger = Log4NetManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region CONSTS
        private static string META_END_SUFFIX = "_NAME";
        private static string META_USE_PREFIX = "USE_";
        private static string META_DOUBLE_SUFFIX = "_DOUBLE";
        private static string META_BOOL_SUFFIX = "_BOOL";
        private static string DEFAULT_GROUP_TAG_FREE = "free";
        private static readonly string TAGS = "tags";
        private static readonly string METAS = "metas";
        #endregion

        #region Public Methods

        public static Group BuildGroup(int nGroupID)
        {
            //Group newGroup = new Group(nGroupID);
            Group newGroup = new Group();
            newGroup.Init(nGroupID);
            newGroup.m_nSubGroup = Get_SubGroupsTree(nGroupID);
            
            GetGroupEpgTagsAndMetas(ref newGroup);
            SetGroupMetas(ref newGroup);
            if (newGroup != null)
            {
                GetGroupsTagsTypes(ref newGroup);
                GetAllGroupChannels(newGroup);
            }

            //get all PermittedWatchRules by groupID
            SetPermittedWatchRules(ref newGroup);
            return newGroup;
        }

        private static List<int> Get_SubGroupsTree(int nGroupID)
        {
            List<int> lGroups = new List<int>();

            DataTable dt = DAL.UtilsDal.GetGroupsTree(nGroupID);
            if (dt != null && dt.DefaultView.Count > 0)
            {
                int groupId;
                for (int i = 0; i < dt.DefaultView.Count; i++)
                {
                    groupId = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[i], "id");
                    if (groupId != 0)
                    {
                        lGroups.Add(groupId);
                    }
                }
            }
           
            return lGroups;
        }
        
        private static void GetGroupEpgTagsAndMetas(ref Group newGroup)
        {
            try
            {
                EpgGroupSettings egs = new EpgGroupSettings();
                DataSet ds = Tvinci.Core.DAL.EpgDal.Get_GroupsTagsAndMetas(newGroup.m_nParentGroupID, newGroup.m_nSubGroup);

                if (ds != null && ds.Tables != null && ds.Tables.Count >= 2)
                {
                    #region metas
                    if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[0].Rows)
                        {
                            string filed = ODBCWrapper.Utils.GetSafeStr(row["name"]);
                            if (!string.IsNullOrEmpty(filed))
                            {
                                egs.m_lMetasName.Add(filed);
                            }
                        }
                    }
                    #endregion
                    #region tags
                    if (ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
                    {
                        foreach (DataRow row in ds.Tables[1].Rows)
                        {
                            string filed = ODBCWrapper.Utils.GetSafeStr(row["name"]);
                            if (!string.IsNullOrEmpty(filed))
                            {
                                egs.m_lTagsName.Add(filed);
                            }
                        }
                    }
                    #endregion

                    newGroup.m_oEpgGroupSettings = egs;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Error", string.Format("Caugh exception when fetching EPG group tags and metas. Ex={0}", ex.Message), "ElasticSearch");
            }
        }
        
        private static void GetAllGroupChannels(Group oGroup)
        {
            oGroup.m_oGroupChannels = new System.Collections.Concurrent.ConcurrentDictionary<int, Channel>();

            DataTable dt = Tvinci.Core.DAL.CatalogDAL.Get_GroupChannels(oGroup.m_nParentGroupID, oGroup.m_nSubGroup);
            List<int> channelIDList = new List<int>();
            if (dt != null && dt.DefaultView.Count > 0)
            {
                int channelID;
                foreach (DataRow row in dt.Rows)
                {
                    channelID = ODBCWrapper.Utils.GetIntSafeVal(row, "id");

                    if (channelID != 0)
                        channelIDList.Add(channelID);
                }
                Task[] buildChannelTask = new Task[channelIDList.Count];

                for (int i = 0; i < channelIDList.Count; i++)
                {
                    buildChannelTask[i] = new Task(
                         (obj) =>
                         {
                             try
                             {
                                 Channel oChannel = GetChannel(channelIDList[(int)obj], oGroup);
                                 if (oChannel != null)
                                     oGroup.m_oGroupChannels.TryAdd(oChannel.m_nChannelID, oChannel);
                             }
                             catch (Exception ex)
                             {
                                 Logger.Logger.Log("Error", string.Format("Error running SearchSubsciptionMedias. Exception {0}", ex.Message), "ElasticSearch");
                             }
                         }, i);
                    buildChannelTask[i].Start();
                }
                Task.WaitAll(buildChannelTask);
            }

        }        

        private static void SetPermittedWatchRules(ref Group newGroup)
        {   
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(newGroup.m_nParentGroupID, newGroup.m_nSubGroup);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    newGroup.m_sPermittedWatchRules.Add(Utils.GetStrSafeVal(permittedWatchRuleRow,"RuleID"));
                }
            }
        }

        public static Channel GetChannel(int nChannelId, Group group)
        {
            #region - select channel by channelId, and the parent_group_id
            
            Channel oChannel = new Channel();

            DataTable channelData = Tvinci.Core.DAL.CatalogDAL.GetChannelByChannelId(nChannelId);

            if (channelData != null && channelData.Rows != null)
            {
                if (channelData.Rows.Count > 0)
                {
                    if (oChannel.m_lChannelTags == null)
                    {
                        oChannel.m_lChannelTags = new List<ApiObjects.SearchObjects.SearchValue>();
                    }

                    DataRow rowData = channelData.Rows[0];
                    oChannel.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(rowData["is_active"]);
                    oChannel.m_nStatus = ODBCWrapper.Utils.GetIntSafeVal(rowData["status"]);
                    oChannel.m_nChannelID = nChannelId;
                    oChannel.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(rowData["group_id"]);
                    oChannel.m_nChannelTypeID = ODBCWrapper.Utils.GetIntSafeVal(rowData["channel_type"]);
                    oChannel.m_nMediaType = ODBCWrapper.Utils.GetIntSafeVal(rowData["MEDIA_TYPE_ID"]);
                    oChannel.m_nParentGroupID = group.m_nParentGroupID;
                    oChannel.m_OrderObject = new ApiObjects.SearchObjects.OrderObj();

                    int nOrderBy = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_type"]);
                    UpdateOrderByObjec(nOrderBy, ref oChannel, group);// initiate orderBy object 

                    int nOrderDir = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_dir"]) - 1;
                    oChannel.m_OrderObject.m_eOrderDir = (ApiObjects.SearchObjects.OrderDir)ApiObjects.SearchObjects.OrderDir.ToObject(typeof(ApiObjects.SearchObjects.OrderDir), nOrderDir);
                    oChannel.m_OrderObject.m_bIsSlidingWindowField = ODBCWrapper.Utils.GetIntSafeVal(rowData["IsSlidingWindow"]) == 1;
                    oChannel.m_OrderObject.lu_min_period_id = ODBCWrapper.Utils.GetIntSafeVal(rowData["SlidingWindowPeriod"]);

                    int nIsAnd = ODBCWrapper.Utils.GetIntSafeVal(rowData["IS_AND"]);

                    if (oChannel.m_nIsActive == 1 && oChannel.m_nStatus == 1)
                    {
                        if (nIsAnd == 1)
                        {
                            oChannel.m_eCutWith = ApiObjects.SearchObjects.CutWith.AND;
                        }

                        // If automatic channel, grab tags values
                        if (oChannel.m_nChannelTypeID == 1)
                        {
                            // Matching meta values against meta mapping dictionary
                            if (group.m_oMetasValuesByGroupId.ContainsKey(oChannel.m_nGroupID))
                            {
                                Dictionary<string, string> mappedValuesForGroupId = group.m_oMetasValuesByGroupId[oChannel.m_nGroupID];
                                foreach (KeyValuePair<string, string> mapping in mappedValuesForGroupId)
                                {
                                    string sMetaParameter = mapping.Key;
                                    string sMappedMetaParameter = mapping.Value;

                                    bool bIsValidSearchValue = true;

                                    if (sMetaParameter.Contains(META_DOUBLE_SUFFIX) || sMetaParameter.Contains(META_BOOL_SUFFIX))
                                    {
                                        int nUse = ODBCWrapper.Utils.GetIntSafeVal(rowData[META_USE_PREFIX + sMetaParameter]);

                                        if (nUse == 0)
                                        {
                                            bIsValidSearchValue = false;
                                        }
                                    }

                                    if (bIsValidSearchValue)
                                    {
                                        string oMeta = Utils.GetStrSafeVal(rowData,sMetaParameter);
                                        if (!string.IsNullOrEmpty(oMeta))
                                        {
                                            bool bIsAlreadyExist = false;
                                            ApiObjects.SearchObjects.SearchValue searchedSearchValue = oChannel.m_lChannelTags.Find(o => o.m_sKey.Equals(sMappedMetaParameter));
                                            if (searchedSearchValue == null)
                                            {
                                                ApiObjects.SearchObjects.SearchValue oNewSearchValue = new ApiObjects.SearchObjects.SearchValue();
                                                CreateSearchValueObject(ref oNewSearchValue, sMappedMetaParameter, oMeta, bIsAlreadyExist, METAS);
                                                oChannel.m_lChannelTags.Add(oNewSearchValue);
                                            }
                                            else
                                            {
                                                bIsAlreadyExist = true;
                                                CreateSearchValueObject(ref searchedSearchValue, sMappedMetaParameter, oMeta, bIsAlreadyExist, METAS);
                                            }
                                        }
                                    }
                                }
                            }

                            // Collect all tags
                            GetChannelTags(oChannel, group);
                        }
                        else // Manual Channel
                        {
                            List<ManualMedia> lManualMedias;
                            oChannel.m_lChannelTags = GetMediasForManualChannel(oChannel.m_nChannelID, out lManualMedias);
                            if (lManualMedias != null)
                            {
                                oChannel.m_lManualMedias = lManualMedias.ToList();
                            }
                        }

                        UpdateOrderByObject(ref oChannel, group.m_oMetasValuesByGroupId[oChannel.m_nGroupID]);
                    }
                    else
                    {
                        oChannel = null;
                    }
                }
            }

            return oChannel; 
            
            #endregion
        }

        public static List<Channel> GetChannels(List<int> lChannelIds, Group group)
        {
            #region - select channel by channelId, and the parent_group_id

            List<Channel> channels = null;

            _logger.Info("Getting channels for subscription");


            DataTable channelsData = Tvinci.Core.DAL.CatalogDAL.GetChanneslByChannelIds(lChannelIds);

            if (channelsData != null && channelsData.Rows != null)
            {
                if (channelsData.Rows.Count > 0)
                {
                    channels = new List<Channel>();
                    
                    foreach (DataRow rowData in channelsData.Rows)
                    {
                        _logger.Info("new channel");

                        Channel oChannel = new Channel();
                        if (oChannel.m_lChannelTags == null)
                        {
                            oChannel.m_lChannelTags = new List<SearchValue>();
                        }

                        oChannel.m_nChannelID = ODBCWrapper.Utils.GetIntSafeVal(rowData["Id"]);
                        oChannel.m_nIsActive = ODBCWrapper.Utils.GetIntSafeVal(rowData["is_active"]);
                        oChannel.m_nStatus = ODBCWrapper.Utils.GetIntSafeVal(rowData["status"]);                        
                        oChannel.m_nGroupID = ODBCWrapper.Utils.GetIntSafeVal(rowData["group_id"]);
                        oChannel.m_nChannelTypeID = ODBCWrapper.Utils.GetIntSafeVal(rowData["channel_type"]);
                        oChannel.m_nMediaType = ODBCWrapper.Utils.GetIntSafeVal(rowData["MEDIA_TYPE_ID"]);
                        oChannel.m_nParentGroupID = group.m_nParentGroupID;
                        oChannel.m_OrderObject = new ApiObjects.SearchObjects.OrderObj();
                        int nOrderBy = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_type"]);
                        UpdateOrderByObjec(nOrderBy, ref oChannel, group);// initiate orderBy object 

                        int nOrderDir = ODBCWrapper.Utils.GetIntSafeVal(rowData["order_by_dir"]) - 1;
                        oChannel.m_OrderObject.m_eOrderDir = (ApiObjects.SearchObjects.OrderDir)ApiObjects.SearchObjects.OrderDir.ToObject(typeof(ApiObjects.SearchObjects.OrderDir), nOrderDir);
                        
                        int nIsAnd = ODBCWrapper.Utils.GetIntSafeVal(rowData["IS_AND"]);
                        _logger.Info("Channel " + oChannel.m_nChannelID + " active: " + oChannel.m_nIsActive + " and status: " + oChannel.m_nStatus);
                        if (oChannel.m_nIsActive == 1 && oChannel.m_nStatus == 1)
                        {
                            if (nIsAnd == 1)
                            {
                                oChannel.m_eCutWith = CutWith.AND;
                            }

                            // If automatic channel, grab tags values
                            if (oChannel.m_nChannelTypeID == 1)
                            {
                                // Matching meta values against meta mapping dictionary
                                if (group.m_oMetasValuesByGroupId.ContainsKey(oChannel.m_nGroupID))
                                {
                                    _logger.Info("Got mapped value for group " + oChannel.m_nGroupID + " in channel " + oChannel.m_nChannelID);
                                    Dictionary<string, string> mappedValuesForGroupId = group.m_oMetasValuesByGroupId[oChannel.m_nGroupID];

                                    if (mappedValuesForGroupId == null || mappedValuesForGroupId.Count == 0)
                                    {
                                        _logger.Info("llll" + oChannel.m_nGroupID + " in channel " + oChannel.m_nChannelID);
                                    }                                    
                                    
                                    foreach (KeyValuePair<string, string> mapping in mappedValuesForGroupId)
                                    {
                                        string sMetaParameter = mapping.Key;
                                        string sMappedMetaParameter = mapping.Value;

                                        bool bIsValidSearchValue = true;

                                        if (sMetaParameter.Contains(META_DOUBLE_SUFFIX) || sMetaParameter.Contains(META_BOOL_SUFFIX))
                                        {
                                            int nUse = ODBCWrapper.Utils.GetIntSafeVal(rowData[META_USE_PREFIX + sMetaParameter]);

                                            if (nUse == 0)
                                            {
                                                bIsValidSearchValue = false;
                                            }
                                        }

                                        if (bIsValidSearchValue)
                                        {
                                            string oMeta = Utils.GetStrSafeVal(rowData,sMetaParameter);
                                            if (!string.IsNullOrEmpty(oMeta))
                                            {
                                                bool bIsAlreadyExist = false;
                                                SearchValue searchedSearchValue = oChannel.m_lChannelTags.Find(o => o.m_sKey.Equals(sMappedMetaParameter));
                                                if (searchedSearchValue == null)
                                                {
                                                    SearchValue oNewSearchValue = new SearchValue();
                                                    CreateSearchValueObject(ref oNewSearchValue, sMappedMetaParameter, oMeta, bIsAlreadyExist, METAS);
                                                    oChannel.m_lChannelTags.Add(oNewSearchValue);
                                                }
                                                else
                                                {
                                                    bIsAlreadyExist = true;
                                                    CreateSearchValueObject(ref searchedSearchValue, sMappedMetaParameter, oMeta, bIsAlreadyExist, METAS);
                                                }
                                            }
                                        }
                                    }
                                }

                                // Collect all tags
                                _logger.Info("Collecting tags in channel " + oChannel.m_nChannelID);

                                GetChannelTags(oChannel, group);
                                _logger.Info("Finished Collecting tags in channel " + oChannel.m_nChannelID);
                            }
                            else // Manual Channel
                            {
                                List<ManualMedia> lManualMedias;
                                oChannel.m_lChannelTags = GetMediasForManualChannel(oChannel.m_nChannelID, out lManualMedias);
                                if (lManualMedias != null)
                                {
                                    oChannel.m_lManualMedias = lManualMedias.ToList();
                                }
                            }

                           // UpdateOrderByObject(ref oChannel, group);
                            channels.Add(oChannel);
                        }
                        else
                        {
                            oChannel = null;
                        }
                    }
                }
            }

            return channels;

            #endregion
        }

        #endregion

        #region Private Methods
        private static void UpdateOrderByObjec(int nOrderBy, ref Channel oChannel, Group group)
        {
            if (nOrderBy >= 1 && nOrderBy <= 30)// all META_STR/META_DOUBLE values
            {
                // get the specific value of the meta
                int nMetaEnum = (nOrderBy);
                string enumName = Enum.GetName(typeof(MetasEnum), nMetaEnum);
                if (group.m_oMetasValuesByGroupId[oChannel.m_nGroupID].ContainsKey(enumName))
                {
                    oChannel.m_OrderObject.m_sOrderValue = group.m_oMetasValuesByGroupId[oChannel.m_nGroupID][enumName];
                    oChannel.m_OrderObject.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
                }
            }
            else
            {
                oChannel.m_OrderObject.m_eOrderBy = (ApiObjects.SearchObjects.OrderBy)ApiObjects.SearchObjects.OrderBy.ToObject(typeof(ApiObjects.SearchObjects.OrderBy), nOrderBy);
            }
        }

        private static void UpdateOrderByObject(ref Channel oChannel, Dictionary<string, string> oMetasValues)
        {
            if (oChannel.m_OrderObject != null)
            {
                string sMetaValue = string.Empty;
                if (oChannel.m_OrderObject.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META)
                {
                    oMetasValues.TryGetValue(oChannel.m_OrderObject.m_eOrderBy.ToString(), out sMetaValue);
                    if (!string.IsNullOrEmpty(sMetaValue))
                    {
                        oChannel.m_OrderObject = UpdateOrderByValues(oChannel.m_OrderObject, sMetaValue);
                    }
                }
            }
        }

        private static void UpdateOrderByObject(ref Channel oChannel, Group group)
        {
            if (oChannel.m_OrderObject != null)
            {
                string sMetaValue = string.Empty;
                //if (oChannel.m_OrderObject.m_eOrderBy >= OrderBy.META1_STR && oChannel.m_OrderObject.m_eOrderBy <= OrderBy.META10_DOUBLE)
                if (oChannel.m_OrderObject.m_eOrderBy == ApiObjects.SearchObjects.OrderBy.META)
                {
                    if (group.m_oMetasValuesByGroupId != null && group.m_oMetasValuesByGroupId.ContainsKey(oChannel.m_nGroupID))
                    {
                        group.m_oMetasValuesByGroupId[oChannel.m_nGroupID].TryGetValue(oChannel.m_OrderObject.m_eOrderBy.ToString(), out sMetaValue);
                        if (!string.IsNullOrEmpty(sMetaValue))
                        {
                            oChannel.m_OrderObject = UpdateOrderByValues(oChannel.m_OrderObject, sMetaValue);
                        }
                    }
                }
            }
        }

        private static OrderObj UpdateOrderByValues(OrderObj orderObj, string sMetaValue)
        {
            OrderObj oNewOrderObj = new OrderObj();
            oNewOrderObj.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.META;
            oNewOrderObj.m_sOrderValue = sMetaValue;
            oNewOrderObj.m_eOrderDir = orderObj.m_eOrderDir;

            return oNewOrderObj;
        }

        private static void CreateSearchValueObject(ref SearchValue oNewSearchValue, string key, string value, bool isAlreadyExist, string sKeyPrefix)
        {
            if (!isAlreadyExist)
            {
                oNewSearchValue.m_sKey = key;
                List<string> tagValues = new List<string>();
                oNewSearchValue.m_lValue = tagValues;
            }
            oNewSearchValue.m_sKeyPrefix = sKeyPrefix;
            List<string> lCurrentTagsValues = oNewSearchValue.m_lValue.ToList();
            lCurrentTagsValues.Add(value);
            oNewSearchValue.m_lValue = lCurrentTagsValues;
        }

        private static List<SearchValue> GetMediasForManualChannel(int nChannelId, out List<ManualMedia> lManualMedias)
        {
            List<SearchValue> lMediaIds = null;
            DataTable mediaIdsTable = Tvinci.Core.DAL.CatalogDAL.GetMediaIdsByChannelId(nChannelId);
            lManualMedias = null;
            if (mediaIdsTable != null && mediaIdsTable.Rows.Count > 0)
            {
                lManualMedias = new List<ManualMedia>();
                lMediaIds = new List<SearchValue>();
                foreach (DataRow mediaIdRow in mediaIdsTable.Rows)
                {
                    string sMediaID = Utils.GetStrSafeVal(mediaIdRow,"media_id");
                    int nOrderNum = Utils.GetIntSafeVal(mediaIdRow,"order_num");
                    bool bIsAlreadyExist = false;
                    
                    SearchValue searchedSearchValue = lMediaIds.Find(o => o.m_sKey.Equals("media_id"));
                    if (searchedSearchValue == null)
                    {
                        SearchValue oNewSearchValue = new SearchValue();
                        CreateSearchValueObject(ref oNewSearchValue, "media_id", sMediaID, bIsAlreadyExist, string.Empty);
                        lMediaIds.Add(oNewSearchValue);

                    }
                    else
                    {
                        bIsAlreadyExist = true;
                        CreateSearchValueObject(ref searchedSearchValue, "media_id", sMediaID, bIsAlreadyExist, string.Empty);
                    }

                    lManualMedias.Add(new ManualMedia(sMediaID, nOrderNum));
                }
            }

            return lMediaIds;
        }  

        private static void GetChannelTags(Channel oChannel, Group group)
        {
            try
            {
                DataTable tagsValuesByTagTypeIds = Tvinci.Core.DAL.CatalogDAL.GetTagsValuesByTagTypeIds(oChannel.m_nChannelID);

                if (tagsValuesByTagTypeIds != null && tagsValuesByTagTypeIds.Rows.Count > 0)
                {
                    foreach (DataRow tableRow in tagsValuesByTagTypeIds.Rows)
                    {
                        int nTagTypeId = Utils.GetIntSafeVal(tableRow, "tag_type_id");
                        string sTagValue = Utils.GetStrSafeVal(tableRow ,"value");
                        string sTagName = GetTagName(nTagTypeId, group);

                        if (!string.IsNullOrEmpty(sTagValue) && !string.IsNullOrEmpty(sTagName))
                        {
                            SearchValue oSearchValue = new SearchValue();
                            bool bIsAlreadyExist = false;
                            SearchValue searchedSearchValue = oChannel.m_lChannelTags.Find(o => o.m_sKey.Equals(sTagName));
                            if (searchedSearchValue == null)
                            {
                                SearchValue oNewSearchValue = new SearchValue();
                                CreateSearchValueObject(ref oNewSearchValue, sTagName, sTagValue, bIsAlreadyExist, TAGS);
                                oChannel.m_lChannelTags.Add(oNewSearchValue);
                            }
                            else
                            {
                                bIsAlreadyExist = true;
                                CreateSearchValueObject(ref searchedSearchValue, sTagName, sTagValue, bIsAlreadyExist, TAGS);
                            }
                        }                        
                    }
                }
            }
            catch (Exception ex)
            {
                //_logger.Error(ex.Message, ex);
            }
        }

        private static void SetGroupMetas(ref Group group)
        {
            DataTable dtMappedMetaValues = Tvinci.Core.DAL.CatalogDAL.GetMappedMetasByGroupId(group.m_nParentGroupID, group.m_nSubGroup);
            if (dtMappedMetaValues != null && dtMappedMetaValues.Rows.Count > 0)
            {
                foreach (DataRow metaDataRow in dtMappedMetaValues.Rows)
                {
                    string sSubGroupNumber = Utils.GetStrSafeVal(metaDataRow ,"Id");
                    int nSubGroupNumberId = -1;
                    bool bIsIdParseSucceeded = int.TryParse(sSubGroupNumber, out nSubGroupNumberId);
                    if (bIsIdParseSucceeded)
                    {
                        if (!group.m_oMetasValuesByGroupId.ContainsKey(nSubGroupNumberId))
                        {
                            group.m_oMetasValuesByGroupId.Add(nSubGroupNumberId, new Dictionary<string, string>());
                        }

                        string sMetaColumn = RemoveMetasSuffixName(Utils.GetStrSafeVal(metaDataRow, "columnname"));
                        string sMappedMetaColumnName = Utils.GetStrSafeVal(metaDataRow, "value");
                        Dictionary<string, string> oMappedMettaValuesForCurrentGroupId = group.m_oMetasValuesByGroupId[nSubGroupNumberId];
                        if (!oMappedMettaValuesForCurrentGroupId.ContainsKey(sMetaColumn))
                        {
                            group.m_oMetasValuesByGroupId[nSubGroupNumberId].Add(sMetaColumn, sMappedMetaColumnName);
                        }
                    }
                }
            }
            else
            {
                group = null;
            }
        }

        private static void GetGroupsTagsTypes(ref Group group)
        {
            group.m_oGroupTags.Add(0, DEFAULT_GROUP_TAG_FREE);
            string sAllGroups = group.GetSubTreeGroupIds();
            
            if (!string.IsNullOrEmpty(sAllGroups))
            {
                DataTable mediaTagsType = Tvinci.Core.DAL.CatalogDAL.GetMediaTagsTypesByGroupIds(sAllGroups);
                if (mediaTagsType != null && mediaTagsType.Rows.Count > 0)
                {
                    foreach (DataRow mediaTagTypeRow in mediaTagsType.Rows)
                    {
                        int nMediaTagTypeId = Utils.GetIntSafeVal(mediaTagTypeRow, "id");
                        string sMediaTagTypeName = Utils.GetStrSafeVal(mediaTagTypeRow, "name");
                        
                        if (!group.m_oGroupTags.ContainsKey(nMediaTagTypeId))
                        {
                            group.m_oGroupTags.Add(nMediaTagTypeId, sMediaTagTypeName);
                        }
                    }
                }
            }        
        }        

        /// <summary>
        /// This function is used in order to match between channels META columns and the values recieved from procedure Get_MetasByGroup
        /// </summary>
        /// <param name="metaName">The meta name which holds "name" before removing it</param>
        /// <returns>Meta name without the suffix "_NAME"</returns>
        private static string RemoveMetasSuffixName(string metaName)
        {
            if (!metaName.Equals(string.Empty))
            {
                if (metaName.EndsWith(META_END_SUFFIX))
                {
                    metaName = metaName.Substring(0, metaName.LastIndexOf(META_END_SUFFIX));                    
                }
            }

            return metaName;
        }

        /// <summary>
        /// This function returns all group ids which are related to their parent group id
        /// </summary>
        /// <param name="parentGroupId"></param>
        /// <returns></returns>
        private static string GetSubTreeGroupIds(int parentGroupId)
        {
            string sTreeOfGroupIds = string.Empty;
            DataTable groupIdsTree = Tvinci.Core.DAL.CatalogDAL.GetAllGroupTree(parentGroupId);
            List<string> oIds = new List<string>();

            if (groupIdsTree != null && groupIdsTree.Rows.Count > 0)
            {
                foreach (DataRow idRow in groupIdsTree.Rows)
                {
                    oIds.Add(Utils.GetStrSafeVal(idRow, "id"));
                }
            }

            if (oIds.Count > 0)
            {
                sTreeOfGroupIds = string.Join(",", oIds);
            }

            return sTreeOfGroupIds;
        }

        private static string GetTagName(int nTagTypeID, Group group)
        {
            string sTagvalue = string.Empty;

            if (group.m_oGroupTags.ContainsKey(nTagTypeID))
            {
                sTagvalue = group.m_oGroupTags[nTagTypeID];
            }

            return sTagvalue;
        }

        #endregion
    }
}

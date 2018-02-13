using ApiLogic;
using ApiObjects;
using ApiObjects.BulkExport;
using ApiObjects.QueueObjects;
using ApiObjects.Response;
using ApiObjects.Roles;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using CachingHelpers;
using EpgBL;
using GroupsCacheManager;
using KLogMonitor;
using QueueWrapper;
using QueueWrapper.Queues.QueueObjects;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using System.Xml;
using Tvinci.Core.DAL;

namespace APILogic.CRUD
{
    public class KSQLChannelsManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Consts

        public const int EPG_ASSET_TYPE = -26;

        public const string ID_REQUIRED = "Channel Identifier is required for this operation";
        public const string CHANNEL_NOT_EXIST = "Channel with given ID does not exist";
        public const string NO_KSQL_CHANNEL_TO_INSERT = "No KSQL Channel was given to insert";
        public const string NAME_REQUIRED = "KSQL Channel must have a name";

        #endregion

        #region Data Members

        protected static string version = string.Empty;

        #endregion

        #region Ctor

        static KSQLChannelsManager()
        {
            version = TVinciShared.WS_Utils.GetTcmConfigValue("Version");
        }

        #endregion
        #region CRUD

        public static KSQLChannelResponse Insert(int groupId, KSQLChannel channel, long userId = 700)
        {
            return Core.Catalog.CatalogManagement.ChannelManager.AddDynamicChannel(groupId, channel, userId);
        }

        public static ApiObjects.Response.Status Delete(int groupId, int channelId, long userId = 700)
        {
            return Core.Catalog.CatalogManagement.ChannelManager.DeleteChannel(groupId, channelId, userId);
        }

        public static void UpdateCatalog(int groupID, int channelId, eAction action = eAction.Update)
        {
            if (!Core.Catalog.Module.UpdateChannelIndex(new List<int> { channelId }, groupID, action))
            {
                log.ErrorFormat("Failed updating channel index for channelId: {0}", channelId);
            }
        }        

        public static KSQLChannelResponse Set(int groupID, KSQLChannel channel, long userId = 700)
        {
            KSQLChannelResponse response = new KSQLChannelResponse();

            try
            {
                if (channel == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NoObjectToInsert, NO_KSQL_CHANNEL_TO_INSERT);
                    return response;
                }

                if (string.IsNullOrEmpty(channel.Name))
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.NameRequired, NAME_REQUIRED);
                    return response;
                }

                // Validate filter query by parsing it
                if (!string.IsNullOrEmpty(channel.FilterQuery))
                {
                    ApiObjects.SearchObjects.BooleanPhraseNode temporaryNode = null;
                    var parseStatus = ApiObjects.SearchObjects.BooleanPhraseNode.ParseSearchExpression(channel.FilterQuery, ref temporaryNode);

                    if (parseStatus == null)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.SyntaxError, "Failed parsing filter query");
                        return response;
                    }
                    else if (parseStatus.Code != (int)eResponseStatus.OK)
                    {
                        response.Status = new ApiObjects.Response.Status(parseStatus.Code, parseStatus.Message);
                        return response;
                    }

                    channel.filterTree = temporaryNode;
                }

                // Validate asset types
                if (channel.AssetTypes != null)
                {
                    Dictionary<int, string> mediaTypesIdToName;
                    Dictionary<string, int> mediaTypesNameToId;
                    Dictionary<int, int> mediaTypeParents;
                    List<int> linearMediaTypes;

                    CatalogDAL.GetMediaTypes(groupID,
                        out mediaTypesIdToName,
                        out mediaTypesNameToId,
                        out mediaTypeParents,
                        out linearMediaTypes);

                    HashSet<int> groupMediaTypes = new HashSet<int>(mediaTypesIdToName.Keys);

                    var channelsMediaTypes = channel.AssetTypes.Where(type => type != EPG_ASSET_TYPE);

                    foreach (int assetType in channelsMediaTypes)
                    {
                        if (!groupMediaTypes.Contains(assetType))
                        {
                            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.InvalidMediaType,
                                string.Format("KSQL Channel media type {0} does not belong to group", assetType));
                            return response;
                        }
                    }
                }

                //check channel exist
                KSQLChannel original = CatalogDAL.GetKSQLChannelById(groupID, channel.ID, null);

                if (original == null || original.ID <= 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ObjectNotExist, CHANNEL_NOT_EXIST);
                    return response;
                }


                channel.GroupID = groupID;

                Group group = GroupsCache.Instance().GetGroup(groupID);

                Dictionary<string, string> metas = null;

                if (group != null && group.m_oMetasValuesByGroupId.ContainsKey(groupID))
                {
                    metas = group.m_oMetasValuesByGroupId[groupID];
                }

                response.Channel = UpdateKSQLChannel(groupID, channel, metas, userId);

                if (response.Channel != null && response.Channel.ID > 0)
                {
                    UpdateCatalog(groupID, response.Channel.ID);

                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "KSQL channel update");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to update new KSQL channel");
                }

                string[] keys = new string[1] 
                { 
                    BuildChannelCacheKey(groupID, channel.ID)
                };

                TVinciShared.QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }
            return response;
        }

        public static KSQLChannelResponseList List(int groupID)
        {
            KSQLChannelResponseList response = new KSQLChannelResponseList();
            try
            {
                response.Channels = CatalogDAL.GetKSQLChannels(groupID);
                if (response.Channels == null || response.Channels.Count == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "no KSQL channels related to group");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new KSQLChannelResponseList();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        public static KSQLChannelResponse Get(int groupID, int channelId)
        {
            KSQLChannelResponse response = new KSQLChannelResponse();

            try
            {
                if (channelId == 0)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.IdentifierRequired, ID_REQUIRED);
                    return response;
                }

                Group group = GroupsCache.Instance().GetGroup(groupID);

                Dictionary<string, string> metas = null;

                if (group != null && group.m_oMetasValuesByGroupId.ContainsKey(groupID))
                {
                    metas = group.m_oMetasValuesByGroupId[groupID];
                }

                response.Channel = CatalogDAL.GetKSQLChannelById(groupID, channelId, metas);
                if (response.Channel == null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ObjectNotExist, CHANNEL_NOT_EXIST);
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }
            catch (Exception ex)
            {
                response = new KSQLChannelResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        } 

        #endregion

        #region Private Methods

        public static string BuildChannelCacheKey(int groupId, int channelId)
        {
            return string.Format("{2}_group_{0}_channel_{1}", groupId, channelId, version);
        }

        public static KSQLChannel UpdateKSQLChannel(int groupID, KSQLChannel channel, Dictionary<string, string> metas, long userId)
        {
            KSQLChannel result = null;

            if (channel != null && channel.ID > 0)
            {
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_KSQLChannel");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@channelId", channel.ID);
                sp.AddParameter("@groupId", groupID);
                sp.AddParameter("@name", channel.Name);
                sp.AddParameter("@isActive", channel.IsActive);
                sp.AddParameter("@UpdaterID", userId);
                sp.AddParameter("@description", channel.Description);
                sp.AddParameter("@Filter", channel.FilterQuery);
                sp.AddParameter("@orderBy", (int)channel.Order.m_eOrderBy);
                sp.AddParameter("@orderDirection", (int)channel.Order.m_eOrderDir + 1);
                sp.AddIDListParameter<int>("@AssetTypes", channel.AssetTypes, "Id");
                sp.AddParameter("@groupBy", channel.GroupBy);

                DataSet ds = sp.ExecuteDataSet();

                if (ds != null && ds.Tables.Count > 0 && ds.Tables[0] != null && ds.Tables[0].Rows.Count > 0)
                {
                    DataTable assetTypes = null;

                    if (ds.Tables.Count > 1)
                    {
                        assetTypes = ds.Tables[1];
                    }

                    result = CatalogDAL.CreateKSQLChannelByDataRow(assetTypes, ds.Tables[0].Rows[0], metas);
                }
            }

            return result;

        }

        #endregion
    }
}

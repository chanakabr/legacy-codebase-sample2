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

        protected const string ID_REQUIRED = "KSQL Channel Identifier is required for this operation";
        protected const string KSQL_CHANNEL_NOT_EXIST = "KSQL Channel with given ID does not exist";
        protected const string NO_KSQL_CHANNEL_TO_INSERT = "No KSQL Channel was given to insert";
        protected const string NAME_REQUIRED = "KSQL Channel must have a name";

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

        public static KSQLChannelResponse Insert(int groupID, KSQLChannel channel)
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

                channel.GroupID = groupID;

                Group group = GroupsCache.Instance().GetGroup(groupID);
                
                Dictionary<string, string> metas = null;

                if (group != null && group.m_oMetasValuesByGroupId.ContainsKey(groupID))
                {
                    metas = group.m_oMetasValuesByGroupId[groupID];
                }

                response.Channel = CatalogDAL.InsertKSQLChannel(groupID, channel, metas);

                if (response.Channel != null && response.Channel.ID > 0)
                {
                    UpdateCatalog(groupID, response.Channel.ID);

                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "new KSQL channel insert");
                }
                else
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "fail to insert new KSQL channel");
                }
            }
            catch (Exception ex)
            {
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}", groupID), ex);
            }

            return response;
        }

        private static void UpdateCatalog(int groupID, int channelId, eAction action = eAction.Update)
        {
            bool updateChannelResult = Core.Catalog.Module.UpdateChannelIndex(new List<int> { channelId }, groupID, action);

            log.DebugFormat("KSQL Channels Manager - WS_Catalog UpdateChannel index result is {0}", updateChannelResult);
        }

        public static ApiObjects.Response.Status Delete(int groupID, int channelId)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            try
            {
                if (channelId == 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.IdentifierRequired, ID_REQUIRED);
                    return response;
                }

                //check if channel exists
                KSQLChannel originalChannel = CatalogDAL.GetKSQLChannelById(groupID, channelId, null);
                if (originalChannel == null || channelId <= 0)
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ObjectNotExist, KSQL_CHANNEL_NOT_EXIST);
                    return response;
                }

                bool isSet = CatalogDAL.DeleteKSQLChannel(groupID, channelId);

                if (isSet)
                {
                    UpdateCatalog(groupID, channelId, eAction.Delete);

                    response = new ApiObjects.Response.Status((int)eResponseStatus.OK, "KSQL channel deleted");
                }
                else
                {
                    response = new ApiObjects.Response.Status((int)eResponseStatus.ObjectNotExist, KSQL_CHANNEL_NOT_EXIST);
                }

                string[] keys = new string[1] 
                { 
                    BuildChannelCacheKey(groupID, channelId)
                };

                TVinciShared.QueueUtils.UpdateCache(groupID, CouchbaseManager.eCouchbaseBucket.CACHE.ToString(), keys);
            }
            catch (Exception ex)
            {
                response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                log.Error(string.Format("Failed groupID={0}, channelId={1}", groupID, channelId), ex);
            }
            return response;
        }

        public static KSQLChannelResponse Set(int groupID, KSQLChannel channel)
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
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ObjectNotExist, KSQL_CHANNEL_NOT_EXIST);
                    return response;
                }


                channel.GroupID = groupID;

                Group group = GroupsCache.Instance().GetGroup(groupID);

                Dictionary<string, string> metas = null;

                if (group != null && group.m_oMetasValuesByGroupId.ContainsKey(groupID))
                {
                    metas = group.m_oMetasValuesByGroupId[groupID];
                }

                response.Channel = CatalogDAL.UpdateKSQLChannel(groupID, channel, metas);

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
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ObjectNotExist, KSQL_CHANNEL_NOT_EXIST);
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

        private static string BuildChannelCacheKey(int groupId, int channelId)
        {
            return string.Format("{2}_group_{0}_channel_{1}", groupId, channelId, version);
        }

        #endregion
    }
}

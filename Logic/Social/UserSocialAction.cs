using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using ApiObjects.Social;
using ApiObjects.Statistics;
using DAL;
using ElasticSearch.Common.DeleteResults;
using KLogMonitor;
using Core.Social.Requests;
using Core.Social.SocialCommands;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Core.Social
{
    public class UserSocialAction
    {
        public UserSocialAction(int groupId)
        {
            SocialSQL = new SocialDAL(groupId);
            SocialBL = BaseSocialBL.GetBaseSocialImpl(groupId) as BaseSocialBL;
        }
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        SocialDAL SocialSQL;
        private BaseSocialBL SocialBL = null;

        public UserSocialActionResponse AddUserSocialAction(int groupID, UserSocialActionRequest actionRequest)
        {
            UserSocialActionResponse response = new UserSocialActionResponse();
            ApiObjects.Response.Status ExternalStatus = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            ApiObjects.Response.Status InternalStatus = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            string dbRecordId = string.Empty;
            string FBObjectID = string.Empty;
            string FBActionID = string.Empty;
            BaseFBExternalCommand oFBCommand;
            SocialActivityDoc doc = null;
            try
            {
               // SocialBL = BaseSocialBL.GetBaseSocialImpl(groupID) as BaseSocialBL;

                // check user is valid
                Core.Users.UserResponseObject uObj = Core.Social.Utils.GetUserDataByID(actionRequest.SiteGuid, groupID);

                if (uObj != null && uObj.m_RespStatus == ApiObjects.ResponseStatus.OK)
                {
                    if (uObj.m_user == null || uObj.m_user.m_oBasicData == null)
                    {
                        response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.UserDoesNotExist, ApiObjects.Response.eResponseStatus.UserDoesNotExist.ToString());
                        return response;
                    }
                    // get user social privacy settings per action 
                    ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
                    SocialPrivacySettings userPrivacySettings = UserSocialPrivacySettings(actionRequest.SiteGuid, groupID, out status);
                    if (userPrivacySettings == null || status.Code == (int)ApiObjects.Response.eResponseStatus.NoUserSocialSettingsFound)
                    {
                        // set default settings
                        userPrivacySettings = SocialPrivacySettings.SetDefultPrivacySettings();
                    }
                    else if (userPrivacySettings.SocialNetworks == null || userPrivacySettings.SocialNetworks.Count == 0)
                    {
                        // set default settings
                        SocialPrivacySettings.SetDefultPrivacySettings(ref userPrivacySettings);
                    }
                    
                    //Like / Watch / Rate – will “publish” to Friend’s activity and other platforms based on the settings for these actions
                    switch (actionRequest.Action)
                    {
                        case eUserAction.LIKE:
                            #region like
                            SocialBL.GetUserSocialAction(actionRequest.SiteGuid, SocialPlatform.UNKNOWN, actionRequest.AssetType, actionRequest.Action, actionRequest.AssetID, out doc);
                            if (doc != null && doc.IsActive == true)
                            {
                                response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.AssetAlreadyLiked, ApiObjects.Response.eResponseStatus.AssetAlreadyLiked.ToString());
                            }
                            else
                            {
                                oFBCommand = new FBLikeExternalCommand(uObj.m_user, groupID, actionRequest.AssetType, actionRequest.AssetID, actionRequest.ExtraParams);
                                ExternalStatus = oFBCommand.ExecuteAction(userPrivacySettings, out FBObjectID, out FBActionID);

                                doc = CreateSocialActivityDoc(uObj.m_user, actionRequest, groupID, ref FBObjectID, ref FBActionID);
                                SocialBL.IncrementAssetLikeCounter(actionRequest.AssetID, actionRequest.AssetType);

                                InternalStatus = SocialBL.InsertUserSocialAction(doc, userPrivacySettings, out dbRecordId);
                                DateTime date = Utils.UnixTimeStampToDateTime(doc.CreateDate);
                                WriteActionToES(actionRequest, groupID, 0, date);
                            }
                            #endregion
                            break;
                        case eUserAction.UNLIKE:
                            #region unlike
                            DeleteUserAction(ref uObj.m_user, userPrivacySettings, eUserAction.LIKE, actionRequest.AssetType, actionRequest.AssetID, groupID, out ExternalStatus, out InternalStatus, out doc);

                            // only if already was a doc we can unactive it 
                            if (doc != null)
                            {
                                DateTime date = Utils.UnixTimeStampToDateTime(doc.CreateDate);
                                DeleteActionFromES(actionRequest, groupID, 0, date);
                            }
                            #endregion
                            break;
                        //External :  Does nothing. The client is required to do the actual “share” implementation.
                        case eUserAction.SHARE:
                            #region Share
                            doc = CreateSocialActivityDoc(uObj.m_user, actionRequest, groupID, ref FBObjectID, ref FBActionID);
                            InternalStatus = SocialBL.InsertUserSocialAction(doc, userPrivacySettings, out dbRecordId);
                            ExternalStatus = null;
                            #endregion
                            break;
                        case eUserAction.WATCHES:
                            #region Watches
                            oFBCommand = new FBWatchExternalCommand(uObj.m_user, groupID, actionRequest.AssetType, actionRequest.AssetID, actionRequest.ExtraParams);
                            ExternalStatus = oFBCommand.ExecuteAction(userPrivacySettings, out FBObjectID, out FBActionID);
                            doc = CreateSocialActivityDoc(uObj.m_user, actionRequest, groupID, ref FBObjectID, ref FBActionID);
                            InternalStatus = SocialBL.InsertUserSocialAction(doc, userPrivacySettings, out dbRecordId);
                            #endregion
                            break;
                        case eUserAction.RATES:
                            #region Rates
                            UserActionRate(ref uObj.m_user, userPrivacySettings, groupID, actionRequest, out ExternalStatus, out InternalStatus, ref doc);
                            if (InternalStatus != null && InternalStatus.Code == (int)ApiObjects.Response.eResponseStatus.AssetAlreadyRated)
                            {
                                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.AssetAlreadyRated, eResponseStatus.AssetAlreadyRated.ToString());
                            }
                            #endregion
                            break;
                        default:
                            break;
                    }

                    log.DebugFormat("External Action Response status:{0}; user={1};ObjectID={2};action={3}", ExternalStatus!=null?ExternalStatus.ToString():string.Empty, uObj.m_user, actionRequest.AssetID, actionRequest.Action.ToString());
                    log.DebugFormat("Internal Action Response status:{0}; user={1};ObjectID={2};action={3}", InternalStatus.ToString(), uObj.m_user, actionRequest.AssetID, actionRequest.Action.ToString());
                }
                else
                {
                    InternalStatus = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, eResponseStatus.UserDoesNotExist.ToString());
                    ExternalStatus = new ApiObjects.Response.Status((int)eResponseStatus.UserDoesNotExist, eResponseStatus.UserDoesNotExist.ToString());
                }

                BuildResponse(ExternalStatus, InternalStatus, actionRequest.Action , doc, ref response);
               
            }
            catch (Exception ex)
            {
                log.ErrorFormat("AddUserSocialAction: Error while get user privacy settings . actionRequest = {0} ex = {2}", actionRequest.ToString(), ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }

           
            return response;
        }

        private void BuildResponse(Status ExternalStatus, Status InternalStatus, eUserAction Action, SocialActivityDoc doc, ref UserSocialActionResponse response)
        {  
            response.NetworksStatus = new List<NetworkActionStatus>();

            if (ExternalStatus != null)
            {
                response.NetworksStatus.Add(new NetworkActionStatus()
                {
                    Network = SocialPlatform.FACEBOOK,
                    Status = ExternalStatus
                });               
            }
            if (InternalStatus != null)
            {
                response.NetworksStatus.Add(new NetworkActionStatus()
                {
                    Status = InternalStatus
                });                
            }

            if (doc != null)
            {
                
                response.UserAction = new UserSocialActionRequest()
                {
                    Id = doc.id,
                    Time = doc.LastUpdate,
                    AssetID = doc.ActivityObject.AssetID,
                    AssetType = doc.ActivityObject.AssetType,             
                    Action = (eUserAction)doc.ActivityVerb.ActionType
                };
                if (response.UserAction.Action == eUserAction.RATES)
                {
                    response.UserAction.ExtraParams = new List<KeyValuePair>();
                    response.UserAction.ExtraParams.Add(new KeyValuePair()
                    {
                        key = "rating:value",
                        value = doc.ActivityVerb.RateValue.ToString()
                    });
                }
            }
        }

        //delete row from  ES due to UNLIKE action 
        private bool DeleteActionFromES(UserSocialActionRequest actionRequest, int groupId, int nRateValue = 0, DateTime date = default(DateTime), eUserAction eAction = eUserAction.LIKE)
        {
            bool result = false;

            string urlV1 = Utils.GetWSURL("ES_URL_V1");
            string urlV2 = Utils.GetWSURL("ES_URL_V2");
            string originalUrl = Utils.GetWSURL("ES_URL");

            HashSet<string> urls = new HashSet<string>();
            urls.Add(urlV1);
            urls.Add(urlV2);
            urls.Add(originalUrl);

            if (urls.Count > 0)
            {
                result = true;
            }

            string statisticsIndex = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(groupId);
            string normalIndex = groupId.ToString();

            int nMediaType = Tvinci.Core.DAL.CatalogDAL.Get_MediaTypeIdByMediaId(actionRequest.AssetID);

            if (date == default(DateTime))
            {
                date = DateTime.UtcNow;
            }

            SocialActionStatistics oActionStats = new SocialActionStatistics()
            {
                Action = eAction.ToString().ToLower(),
                Date = date,
                GroupID = groupId,
                MediaID = actionRequest.AssetID,
                MediaType = nMediaType.ToString(),
                RateValue = nRateValue
            };

            StatisticsActionSearchObj socialSearch = new StatisticsActionSearchObj()
            {
                Action = eAction.ToString().ToLower(),
                Date = date,
                GroupID = groupId,
                MediaID = actionRequest.AssetID,
                MediaType = nMediaType.ToString(),
                RateValue = nRateValue
            };

            string sActionStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(oActionStats);

            if (!string.IsNullOrEmpty(urlV1))
            {
                result &= DeleteActionFromESV1(urlV1, socialSearch, groupId, statisticsIndex);
            }
            // If both are empty it means we have only one URL and that is "ES_URL" (originalURL)
            else if (string.IsNullOrEmpty(urlV2))
            {
                urlV2 = originalUrl;
            }

            if (!string.IsNullOrEmpty(urlV2))
            {
                result &= DeleteActionFromESV2(urlV2, socialSearch, groupId, statisticsIndex);
            }

            return result;
        }

        private static bool DeleteActionFromESV1(string url, StatisticsActionSearchObj socialSearch, int groupId, string index)
        {
            bool result = false;

            try
            {
                ElasticSearch.Common.ElasticSearchApi oESApi = new ElasticSearch.Common.ElasticSearchApi(url);

                if (oESApi.IndexExists(index))
                {
                    ElasticSearch.Searcher.ESStatisticsQueryBuilder queryBuilder = new ElasticSearch.Searcher.ESStatisticsQueryBuilder(groupId, socialSearch);
                    string sQuery = queryBuilder.BuildQuery();

                    string res = oESApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref sQuery);
                    string sID = string.Empty;
                    if (!string.IsNullOrEmpty(res))
                    {
                        int totalItems = 1;
                        List<StatisticsView> lSocialActionView = Utils.DecodeSearchJsonObject(res, ref totalItems);
                        if (lSocialActionView != null && lSocialActionView.Count > 0)
                        {
                            sID = lSocialActionView[0].ID;
                        }
                    }
                    ESDeleteResult esRes = oESApi.DeleteDoc(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, sID);

                    if (esRes == null || !esRes.Ok)
                    {
                        log.Debug("DeleteActionFromESV1 - " + string.Format("Was unable to delete record from ES. index={0};type={1};",
                            index, ElasticSearch.Common.Utils.ES_STATS_TYPE));
                    }
                    else
                    {
                        result = true;
                    }
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("DeleteActionFromES Failed ex={0}, index={1};type={2}", ex, index, ElasticSearch.Common.Utils.ES_STATS_TYPE);
            }

            return result;
        }

        private static bool DeleteActionFromESV2(string url, StatisticsActionSearchObj socialSearch, int groupId, string index)
        {
            bool result = false;

            try
            {
                ElasticSearch.Common.ElasticSearchApi oESApi = new ElasticSearch.Common.ElasticSearchApi(url);

                if (oESApi.IndexExists(index))
                {
                    ElasticSearch.Searcher.ESStatisticsQueryBuilder queryBuilder = new ElasticSearch.Searcher.ESStatisticsQueryBuilder(groupId, socialSearch);
                    string queryString = queryBuilder.BuildQuery();
                    bool deleteDocsByQueryResult = oESApi.DeleteDocsByQuery(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref queryString);

                    string searchResult = oESApi.Search(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref queryString);
                    string id = string.Empty;
                    if (!string.IsNullOrEmpty(searchResult))
                    {
                        int totalItems = 1;
                        List<StatisticsView> lSocialActionView = Utils.DecodeSearchJsonObject(searchResult, ref totalItems);
                        if (lSocialActionView != null && lSocialActionView.Count > 0)
                        {
                            id = lSocialActionView[0].ID;
                        }
                    }

                    if (!string.IsNullOrEmpty(id))
                    {
                        // double try delete
                        ESDeleteResult deleteResult = oESApi.DeleteDoc(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, id);

                        if (deleteResult == null || !deleteResult.Ok)
                        {
                            log.Debug("DeleteActionFromESV2 - " + string.Format("Was unable to delete record from ES. index={0};type={1};",
                                index, ElasticSearch.Common.Utils.ES_STATS_TYPE));
                        }
                        else
                        {
                            result = deleteDocsByQueryResult;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.DebugFormat("DeleteActionFromES Failed ex={0}, index={1};type={2}", ex, index, ElasticSearch.Common.Utils.ES_STATS_TYPE);
            }

            return result;
        }

        private void DeleteUserAction(ref Core.Users.User user, SocialPrivacySettings userPrivacySettings, eUserAction userAction, eAssetType assetType, int assetId, int groupID, out Status ExternalStatus, out Status InternalStatus, out SocialActivityDoc doc)
        {
            //SocialActivityDoc doc;
            ExternalStatus = new Status((int)eResponseStatus.NotAllowed, eResponseStatus.NotAllowed.ToString());
            InternalStatus = new Status((int)eResponseStatus.NotAllowed, eResponseStatus.NotAllowed.ToString());

            SocialBL.GetUserSocialAction(user.m_sSiteGUID, (int)SocialPlatform.UNKNOWN, assetType, userAction, assetId, out doc);
            if (doc != null && doc.IsActive == true)
            {
                BaseFBExternalCommand oFBCommand = new FBDeleteExternalCommand(user, groupID, userAction, assetType, assetId, null);
                string FBObjectID, FBActionID;

                ExternalStatus = oFBCommand.ExecuteAction(userPrivacySettings, out FBObjectID, out FBActionID);

                bool res = SocialBL.DeleteUserSocialAction(user.m_sSiteGUID, assetId, assetType, userAction, SocialPlatform.UNKNOWN);
                if (res)
                {
                    InternalStatus = new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
                else
                {
                    InternalStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                }               
                SocialBL.DecrementAssetLikeCounter(assetId, assetType);
            }
            else
            {
                ExternalStatus = new Status((int)eResponseStatus.AssetNeverLiked, eResponseStatus.NotAllowed.ToString());
                InternalStatus = new Status((int)eResponseStatus.AssetNeverLiked, eResponseStatus.NotAllowed.ToString());
            }
        }

        private SocialActivityDoc CreateSocialActivityDoc(Core.Users.User user, UserSocialActionRequest actionRequest, int groupID, ref string fbObjectID, ref string actionID)
        {
            SocialActivityDoc newActionDoc = new SocialActivityDoc()
            {
                SocialPlatform = (int)SocialPlatform.UNKNOWN,
                DocOwnerSiteGuid = user.m_sSiteGUID,
                DocType = "user_action",
                IsActive = true,
                ActivityObject = new SocialActivityObject()
                {
                    AssetType = actionRequest.AssetType,
                    AssetID = actionRequest.AssetID,
                    ObjectID = fbObjectID
                },
                ActivitySubject = new SocialActivitySubject()
                {
                    ActorSiteGuid = user.m_sSiteGUID,
                    ActorTvinciUsername = string.Format("{0} {1}", user.m_oBasicData.m_sFirstName, user.m_oBasicData.m_sLastName),
                    GroupID = groupID,
                    ActorPicUrl = user.m_oBasicData.m_sFacebookImage,
                    DeviceUdid = actionRequest.DeviceUDID
                },
                ActivityVerb = new SocialActivityVerb()
                {
                    SocialActionID = actionID,
                    ActionName = Enum.GetName(typeof(eUserAction), actionRequest.Action),
                    ActionType = (int)actionRequest.Action
                }
            };           
            return newActionDoc;

        }

        //Write Like action to ES
        private bool WriteActionToES(UserSocialActionRequest actionRequest, int groupId, int nRateValue = 0, DateTime date = default(DateTime))
        {
            bool result = false;

            if (date == default(DateTime))
            {
                date = DateTime.UtcNow;
            }

            int mediaType = Tvinci.Core.DAL.CatalogDAL.Get_MediaTypeIdByMediaId(actionRequest.AssetID);

            try
            {
                result = ElasticSearch.Utilities.ESStatisticsUtilities.InsertSocialActionStatistics(groupId, actionRequest.AssetID, mediaType, actionRequest.Action, nRateValue, date);
            }
            catch (Exception ex)
            {
                string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(groupId);
                log.WarnFormat("WriteLikeToES Was unable to insert record to ES. index={0}; type={1}; id{2}; ex={3}",
                    index, ElasticSearch.Common.Utils.ES_STATS_TYPE, actionRequest.AssetID, ex);
            }

            return result;
            //try
            //{
            //    bool bRes = false;
            //    ElasticSearch.Common.ElasticSearchApi oESApi = new ElasticSearch.Common.ElasticSearchApi();

            //    if (oESApi.IndexExists(index))
            //    {
            //        Guid guid = Guid.NewGuid();

            //        SocialActionStatistics oActionStats = new SocialActionStatistics()
            //        {
            //            Action = actionRequest.Action.ToString().ToLower(),
            //            Date = date,
            //            GroupID = groupId,
            //            MediaID = actionRequest.AssetID,
            //            MediaType = mediaType.ToString(),
            //            RateValue = nRateValue
            //        };

            //        string sActionStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(oActionStats);

            //        bRes = oESApi.InsertRecord(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, guid.ToString(), sActionStatsJson);

            //        if (!bRes)
            //        {
            //        }
            //    }

            //    return bRes;
            //}
            //catch (Exception ex)
            //{
            //    log.ErrorFormat("WriteFavoriteToES Failed ex={0}, index={1};type={2}", ex, index, ElasticSearch.Common.Utils.ES_STATS_TYPE);
            //    return false;
            //}
        }

        private SocialPrivacySettings UserSocialPrivacySettings(string siteGUID, int groupID, out ApiObjects.Response.Status status)
        {
            SocialPrivacySettings settings = new SocialPrivacySettings();
            status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.OK, ApiObjects.Response.eResponseStatus.OK.ToString());
            List<eUserAction> actions = new List<eUserAction>() { eUserAction.LIKE, eUserAction.WATCHES, eUserAction.SHARE, eUserAction.RATES };

            try
            {
                DataTable dtSettings = SocialSQL.GetSocialPrivacySettings(siteGUID, groupID);
                if (dtSettings == null || dtSettings.Rows == null || dtSettings.Rows.Count == 0)
                {
                    settings = SocialPrivacySettings.SetDefultPrivacySettings();
                }
                else
                {
                    int privacy = 0;
                    int socialPrivacy = 0;
                    //int network = 0;
                    int action = 0;
                    SocialActionPrivacy actionPrivecy;
                    DataRow[] drows = dtSettings.AsEnumerable().Where(x => x.Field<int>("social_platform") == 999).ToArray();
                    foreach (DataRow dr in drows)
                    {
                        actionPrivecy = new SocialActionPrivacy();
                        privacy = ODBCWrapper.Utils.GetIntSafeVal(dr, "internal_share");
                        action = ODBCWrapper.Utils.GetIntSafeVal(dr, "action_id");

                        if (Enum.IsDefined(typeof(eSocialActionPrivacy), privacy) && Enum.IsDefined(typeof(eUserAction), action))
                        {
                            actionPrivecy.Action = (eUserAction)action;
                            actionPrivecy.Privacy = (eSocialActionPrivacy)privacy;
                            settings.InternalPrivacy.Add(actionPrivecy);
                        }
                    }
                    // complete default privacy for actions 
                    List<eUserAction> missingActions = actions.Where(a => settings.InternalPrivacy.All(i => i.Action != a)).ToList();
                    if (missingActions != null && missingActions.Count() > 0)
                    {
                        settings.InternalPrivacy.AddRange(SocialPrivacySettings.SetDefaultInternalPrivacy(missingActions));
                    }
                    settings.InternalPrivacy = settings.InternalPrivacy.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();

                    List<int> networks = dtSettings.AsEnumerable().Where(x => x.Field<int>("social_platform") != 999 && x.Field<int>("social_platform") != 0).Select(x => x.Field<int>("social_platform")).Distinct().ToList();
                    SocialNetwork sn;
                    foreach (int network in networks)
                    {
                        drows = dtSettings.AsEnumerable().Where(x => x.Field<int>("social_platform") == network).ToArray();
                        sn = new SocialNetwork();

                        if (Enum.IsDefined(typeof(SocialPlatform), network))
                        {
                            sn.Network = (SocialPlatform)network;
                        }
                        socialPrivacy = ODBCWrapper.Utils.GetIntSafeVal(drows[0], "external_privacy");

                        if (Enum.IsDefined(typeof(eSocialPrivacy), socialPrivacy))
                        {
                            sn.SocialPrivacy = (eSocialPrivacy)socialPrivacy;
                        }
                        sn.SocialAction = new List<SocialActionPrivacy>();

                        foreach (DataRow dr in drows)
                        {
                            actionPrivecy = new SocialActionPrivacy();
                            privacy = ODBCWrapper.Utils.GetIntSafeVal(dr, "external_share");
                            action = ODBCWrapper.Utils.GetIntSafeVal(dr, "action_id");

                            if (Enum.IsDefined(typeof(eSocialActionPrivacy), privacy) && Enum.IsDefined(typeof(eUserAction), action))
                            {
                                actionPrivecy.Action = (eUserAction)action;
                                actionPrivecy.Privacy = (eSocialActionPrivacy)privacy;
                                sn.SocialAction.Add(actionPrivecy);
                            }
                        }

                        if (settings.SocialNetworks == null)
                        {
                            settings.SocialNetworks = new List<SocialNetwork>();
                        }

                        missingActions = actions.Where(a => sn.SocialAction.All(i => i.Action != a)).ToList();
                        if (missingActions != null && missingActions.Count() > 0)
                        {
                            sn.SocialAction.AddRange(SocialPrivacySettings.SetDefaultNetworkPlatformPrivacy(missingActions));
                        }
                        sn.SocialAction = sn.SocialAction.OrderByDescending(x => x.Action == eUserAction.WATCHES).ThenBy(x => x.Action != eUserAction.WATCHES).ToList();

                        settings.SocialNetworks.Add(sn);
                    }

                    if (networks == null || networks.Count() == 0)
                    {
                        settings.SocialNetworks.AddRange(SocialPrivacySettings.SetDefultNetworkPrivacySettings());
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("UserSocialPrivacySettings: Error while get user privacy settings . siteGuid = {0} ex = {2}", siteGUID, ex);
                status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.Error, ApiObjects.Response.eResponseStatus.Error.ToString());
                return null;
            }
            return settings;
        }

        private void UserActionRate(ref Core.Users.User user, SocialPrivacySettings userPrivacySettings, int groupId, UserSocialActionRequest actionRequest, out Status ExternalStatus,
            out Status InternalStatus, ref SocialActivityDoc doc)
        {            
            string FBObjectID = string.Empty;
            string FBActionID = string.Empty;
            SocialBL.GetUserSocialAction(user.m_sSiteGUID, SocialPlatform.UNKNOWN, actionRequest.AssetType, eUserAction.RATES, actionRequest.AssetID, out doc);
            if (doc != null && doc.IsActive)
            {               
                InternalStatus = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.AssetAlreadyRated, ApiObjects.Response.eResponseStatus.AssetAlreadyRated.ToString());
                ExternalStatus = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.AssetAlreadyRated, ApiObjects.Response.eResponseStatus.AssetAlreadyRated.ToString());
            }
            else
            {               
                ExternalStatus = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.InvalidParameters, ApiObjects.Response.eResponseStatus.InvalidParameters.ToString());
                InternalStatus = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.InvalidParameters, ApiObjects.Response.eResponseStatus.InvalidParameters.ToString());
                BaseFBExternalCommand oFBCommand = new FBRateExternalCommand(user, groupId, actionRequest.AssetType, actionRequest.AssetID, actionRequest.ExtraParams);               
                ExternalStatus = oFBCommand.ExecuteAction(userPrivacySettings, out FBObjectID, out FBActionID);

                ApiObjects.KeyValuePair oRatingKVP = ApiObjects.KeyValuePair.GetKVPFromList("rating:value", actionRequest.ExtraParams);
                int nRateVal = 0;
                if (oRatingKVP != null && int.TryParse(oRatingKVP.value, out nRateVal))
                {
                    doc = CreateSocialActivityDoc(user, actionRequest, groupId, ref FBObjectID, ref FBActionID);
                    doc.ActivityVerb.RateValue = nRateVal;

                    string DbID;
                    InternalStatus = SocialBL.InsertUserSocialAction(doc, userPrivacySettings, out DbID);
                    SocialBL.RateAsset(user.m_sSiteGUID, actionRequest.AssetID, actionRequest.AssetType, nRateVal);
                }
            }

        }

        public UserSocialActionResponse DeleteUserSocialAction(int groupID, string siteGuid, string id)
        {
            UserSocialActionResponse response = new UserSocialActionResponse();
            try
            {                
                SocialActivityDoc doc = null;
                // get the doc by doc id 
                bool success = SocialBL.GetUserSocialAction(id, out doc);
                if (!success || doc == null || doc.IsActive == false)
                {
                    response.Status = new ApiObjects.Response.Status((int)ApiObjects.Response.eResponseStatus.SocialActionIdDoseNotExists, ApiObjects.Response.eResponseStatus.SocialActionIdDoseNotExists.ToString());
                }
                else
                {
                    UserSocialActionRequest actionRequest = BuildUserSocialActionRequest(groupID, doc);
                    if (actionRequest.SiteGuid != siteGuid)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.UserNotAllowed, eResponseStatus.UserNotAllowed.ToString());
                    }
                    else if (actionRequest.Action == eUserAction.UNKNOWN)
                    {
                        response.Status = new ApiObjects.Response.Status((int)eResponseStatus.ActionIsNotAllowed, eResponseStatus.ActionIsNotAllowed.ToString());
                    }
                    else
                    {
                        // call AddUserSocialAction with the right action type
                        response = AddUserSocialAction(groupID, actionRequest);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("DeleteUserSocialAction: Error while try delete doc . id = {0} ex = {2}", id, ex);
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }
            return response;
        }

        private UserSocialActionRequest BuildUserSocialActionRequest(int groupID, SocialActivityDoc doc)
        {
            UserSocialActionRequest actionRequest = new UserSocialActionRequest();
            try
            {
                if (doc != null)
                {
                    switch ((eUserAction)doc.ActivityVerb.ActionType)
                    {                       
                        case eUserAction.LIKE:
                            actionRequest.Action = eUserAction.UNLIKE;
                            break;
                        default:
                            actionRequest.Action = eUserAction.UNKNOWN;
                            break;
                    }                  
                    actionRequest.AssetID = doc.ActivityObject.AssetID;
                    actionRequest.DeviceUDID = doc.ActivitySubject.DeviceUdid;
                    actionRequest.SiteGuid = doc.DocOwnerSiteGuid;
                    actionRequest.AssetType = doc.ActivityObject.AssetType;
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("BuildUserSocialActionRequest: Error while try build request for  doc = {0} ex = {2}", doc != null ? doc.ToString() : string.Empty, ex);
                actionRequest = new UserSocialActionRequest();
            }
            return actionRequest;
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Core.Social.Responses;
using Core.Social;
using ApiObjects;
using ApiObjects.Statistics;
using ApiObjects.MediaIndexingObjects;
using Core.Social.SocialCommands;
using ApiObjects.SearchObjects;
using ElasticSearch.Common.DeleteResults;
using KLogMonitor;
using System.Reflection;
using Core.Users;
using ApiObjects.Social;

namespace Core.Social.Requests
{
    public class BaseDoUserActionRequest : SocialBaseRequestWrapper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public BaseDoUserActionRequest()
        {
            m_sSiteGuid = string.Empty;
            m_nGroupID = 0;
            m_eSocialPlatform = SocialPlatform.UNKNOWN;
            m_nAssetID = 0;
            m_sFunctionName = "DoUserSocialAction";
            m_eAction = eUserAction.UNKNOWN;
            m_eAssetType = eAssetType.UNKNOWN;
        }

        private BaseSocialBL m_oSocialBL = null;

        #region Properties
        public eAssetType m_eAssetType { get; set; }

        public string m_sDeviceUDID { get; set; }

        public string m_sSiteGuid { get; set; }

        public int m_nAssetID { get; set; }

        public eUserAction m_eAction { get; set; }



        public override string m_sFunctionName { get; set; }
        #endregion

        public override BaseSocialResponse GetResponse(int nGroupID)
        {
            m_nGroupID = nGroupID;

            DoSocialActionResponse oRes = new DoSocialActionResponse(STATUS_FAIL, SocialActionResponseStatus.ERROR, SocialActionResponseStatus.ERROR);

            if (!ValidityCheck())
            {
                return oRes;
            }

            m_oSocialBL = BaseSocialBL.GetBaseSocialImpl(m_nGroupID) as BaseSocialBL;

            UserResponseObject uObj = Utils.GetUserDataByID(m_sSiteGuid, m_nGroupID);

            if (uObj != null && uObj.m_RespStatus == ResponseStatus.OK)
            {
                if (uObj.m_user == null || uObj.m_user.m_oBasicData == null)
                {
                    oRes.m_eActionResponseStatusIntern = SocialActionResponseStatus.USER_DOES_NOT_EXIST;
                    oRes.m_eActionResponseStatusExtern = SocialActionResponseStatus.USER_DOES_NOT_EXIST;
                    return oRes;
                }

                string sObjectID = string.Empty;
                string sActionID = string.Empty;
                oRes.m_nStatus = STATUS_OK;

                SocialActionResponseStatus eInternalActionStatus, eExternalActionStatus;
                DoUserSocialAction(uObj.m_user, out eInternalActionStatus, out  eExternalActionStatus);
                oRes.m_eActionResponseStatusIntern = eInternalActionStatus;
                oRes.m_eActionResponseStatusExtern = eExternalActionStatus;

                log.Debug("External Action - " + string.Format("Response status:{0}; user={1};ObjectID={2};action={3}", oRes.m_eActionResponseStatusExtern.ToString(), uObj.m_user, sObjectID, m_eAction.ToString()));
                log.Debug("Internal Action - " + string.Format("Response status:{0}; user={1};ObjectID={2};actionID={3};action={4}", oRes.m_eActionResponseStatusIntern.ToString(), uObj.m_user, sObjectID, sActionID, m_eAction.ToString()));
            }
            else
            {
                oRes.m_eActionResponseStatusIntern = SocialActionResponseStatus.USER_DOES_NOT_EXIST;
                oRes.m_eActionResponseStatusExtern = SocialActionResponseStatus.USER_DOES_NOT_EXIST;
            }

            return oRes;
        }

        private SocialActivityDoc CreateSocialActivityDoc(User oUser, ref string sFBObjectID, ref string sActionID)
        {
            SocialActivityDoc newActionDoc = new SocialActivityDoc()
            {
                SocialPlatform = (int)m_eSocialPlatform,
                DocOwnerSiteGuid = oUser.m_sSiteGUID,
                DocType = "user_action",
                IsActive = true,
                ActivityObject = new SocialActivityObject()
                {
                    AssetType = m_eAssetType,
                    AssetID = m_nAssetID,
                    ObjectID = sFBObjectID
                },
                ActivitySubject = new SocialActivitySubject()
                {
                    ActorSiteGuid = oUser.m_sSiteGUID,
                    ActorTvinciUsername = string.Format("{0} {1}", oUser.m_oBasicData.m_sFirstName, oUser.m_oBasicData.m_sLastName),
                    GroupID = m_nGroupID,
                    ActorPicUrl = oUser.m_oBasicData.m_sFacebookImage,
                    DeviceUdid = m_sDeviceUDID
                },
                ActivityVerb = new SocialActivityVerb()
                {
                    SocialActionID = sActionID,
                    ActionName = Enum.GetName(typeof(eUserAction), m_eAction),
                    ActionType = (int)m_eAction
                }
            };

            return newActionDoc;

        }

        //Write Like action to ES
        private bool WriteActionToES(int nRateValue = 0, DateTime date = default(DateTime))
        {
            bool result = false;

            if (date == default(DateTime))
            {
                date = DateTime.UtcNow;
            }

            int mediaType = Tvinci.Core.DAL.CatalogDAL.Get_MediaTypeIdByMediaId(this.m_nAssetID);

            try
            {
                result = ElasticSearch.Utilities.ESStatisticsUtilities.InsertSocialActionStatistics(this.m_nGroupID, this.m_nAssetID, mediaType, this.m_eAction, nRateValue, date);
            }
            catch (Exception ex)
            {
                string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(this.m_nGroupID);
                log.WarnFormat("WriteLikeToES Was unable to insert record to ES. index={0}; type={1}; id{2}; ex={3}",
                    index, ElasticSearch.Common.Utils.ES_STATS_TYPE, this.m_nAssetID, ex);
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

        //delete row from  ES due to UNLIKE action 
        private bool DeleteActionFromES(int nRateValue = 0, DateTime date = default(DateTime), eUserAction eAction = eUserAction.LIKE)
        {
            string index = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(m_nGroupID);
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

            string statisticsIndex = ElasticSearch.Common.Utils.GetGroupStatisticsIndex(this.m_nGroupID);
            string normalIndex = this.m_nGroupID.ToString();

            int nMediaType = Tvinci.Core.DAL.CatalogDAL.Get_MediaTypeIdByMediaId(this.m_nAssetID);

            if (date == default(DateTime))
            {
                date = DateTime.UtcNow;
            }

            SocialActionStatistics oActionStats = new SocialActionStatistics()
            {
                Action = eAction.ToString().ToLower(),
                Date = date,
                GroupID = this.m_nGroupID,
                MediaID = this.m_nAssetID,
                MediaType = nMediaType.ToString(),
                RateValue = nRateValue
            };

            StatisticsActionSearchObj socialSearch = new StatisticsActionSearchObj()
            {
                Action = eAction.ToString().ToLower(),
                Date = date,
                GroupID = this.m_nGroupID,
                MediaID = this.m_nAssetID,
                MediaType = nMediaType.ToString(),
                RateValue = nRateValue
            };

            string sActionStatsJson = Newtonsoft.Json.JsonConvert.SerializeObject(oActionStats);

            if (!string.IsNullOrEmpty(urlV1))
            {
                result &= DeleteActionFromESV1(urlV1, socialSearch, this.m_nGroupID, statisticsIndex);
            }
            else if (string.IsNullOrEmpty(urlV2))
            {
                urlV2 = originalUrl;
            }

            if (!string.IsNullOrEmpty(urlV2))
            {
                result &= DeleteActionFromESV2(urlV2, socialSearch, this.m_nGroupID, statisticsIndex);
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
                        bool deleteDocsByQueryResult = oESApi.DeleteDocsByQuery(index, ElasticSearch.Common.Utils.ES_STATS_TYPE, ref queryString);

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

        private string createFBObject()
        {
            string sObjectID = string.Empty;
            string sUrl = string.Empty;

            ApiObjects.KeyValuePair urlKvp = null;
            if (this.m_oKeyValue != null && this.m_oKeyValue.Count > 0)
            {
                var objUrl = m_oKeyValue.Where(kvp => kvp.key == "obj:url");

                if (objUrl != null && objUrl.Count() > 0)
                    urlKvp = m_oKeyValue.Where(kvp => kvp.key == "obj:url").First();
                else
                {
                    log.Error("Error - cannot create FB Object. missing obj:url");
                }
            }

            if (urlKvp != null)
            {
                FacebookObjectRequest objRequest = new FacebookObjectRequest()
                                            {
                                                m_eAssetType = m_eAssetType,
                                                m_eSocialPlatform = SocialPlatform.FACEBOOK,
                                                m_nAssetID = m_nAssetID,
                                                m_eType = eRequestType.SET
                                            };
                objRequest.m_oKeyValue.Add(urlKvp);

                SocialObjectReponse objResponse = objRequest.GetResponse(m_nGroupID) as SocialObjectReponse;

                if (objResponse != null)
                {
                    sObjectID = objResponse.sID;
                }
            }
            return sObjectID;
        }

        private bool AllowedToPublish(UserDynamicData userDynamicData)
        {
            bool bFBPrivacy = true;

            if (userDynamicData.m_sUserData != null)
            {
                foreach (UserDynamicDataContainer uddc in userDynamicData.m_sUserData)
                {
                    if (uddc != null && uddc.m_sDataType.Equals("FBPrivacy") && uddc.m_sValue.Equals("0"))
                    {
                        bFBPrivacy = false;
                    }
                }
            }

            return bFBPrivacy;
        }

        protected bool ValidityCheck()
        {
            return (m_nGroupID == 0 || m_nAssetID == 0 || string.IsNullOrEmpty(m_sSiteGuid) || m_eAction == eUserAction.UNKNOWN) ? false : true;

        }

        protected void DoUserSocialAction(User oUser, out SocialActionResponseStatus eInternalResponse, out SocialActionResponseStatus eExternalResponse)
        {
            eInternalResponse = SocialActionResponseStatus.NOT_ALLOWED;
            eExternalResponse = SocialActionResponseStatus.NOT_ALLOWED;

            string sDbRecordId = string.Empty;
            string sFBActionID = string.Empty;
            string sFBObjectID = string.Empty;
            BaseFBExternalCommand oFBCommand;
            SocialActivityDoc doc;

            switch (m_eAction)
            {
                case eUserAction.LIKE:
                    #region like
                    m_oSocialBL.GetUserSocialAction(m_sSiteGuid, m_eSocialPlatform, m_eAssetType, m_eAction, m_nAssetID, out doc);
                    if (doc != null && doc.IsActive == true)
                    {
                        eInternalResponse = SocialActionResponseStatus.MEDIA_ALREADY_LIKED;
                        eExternalResponse = SocialActionResponseStatus.MEDIA_ALREADY_LIKED;
                    }
                    else
                    {
                        oFBCommand = new FBLikeExternalCommand(oUser, m_nGroupID, m_eAssetType, m_nAssetID, m_oKeyValue);
                        eExternalResponse = oFBCommand.Execute(out sFBObjectID, out sFBActionID);
                        doc = CreateSocialActivityDoc(oUser, ref sFBObjectID, ref sFBActionID);
                        m_oSocialBL.IncrementAssetLikeCounter(m_nAssetID, m_eAssetType);
                        eInternalResponse = m_oSocialBL.InsertUserSocialAction(doc, out sDbRecordId) ? SocialActionResponseStatus.OK : SocialActionResponseStatus.ERROR;
                        DateTime date = Utils.UnixTimeStampToDateTime(doc.CreateDate);
                        WriteActionToES(0, date);
                    }
                    #endregion
                    break;
                case eUserAction.UNLIKE:
                    #region unlike
                    DeleteUserAction(ref oUser, eUserAction.LIKE, out eInternalResponse, out eExternalResponse, out doc);

                    // only if already was a doc we can unactive it 
                    if (doc != null) 
                    {
                        DateTime date = Utils.UnixTimeStampToDateTime(doc.CreateDate);
                        DeleteActionFromES(0, date);
                        //WriteActionToES();
                    }
                    #endregion
                    break;
                case eUserAction.UNFOLLOW:
                    #region unfollow
                    DeleteUserAction(ref oUser, eUserAction.FOLLOWS, out eInternalResponse, out eExternalResponse, out doc);
                    if (eInternalResponse == SocialActionResponseStatus.OK)
                        WriteActionToES();
                    #endregion
                    break;
                case eUserAction.WATCHES:
                    oFBCommand = new FBWatchExternalCommand(oUser, m_nGroupID, m_eAssetType, m_nAssetID, m_oKeyValue);
                    eExternalResponse = oFBCommand.Execute(out sFBObjectID, out sFBActionID);
                    doc = CreateSocialActivityDoc(oUser, ref sFBObjectID, ref sFBActionID);
                    eInternalResponse = m_oSocialBL.InsertUserSocialAction(doc, out sDbRecordId) ? SocialActionResponseStatus.OK : SocialActionResponseStatus.ERROR;
                    break;
                case eUserAction.SHARE:
                    oFBCommand = new FBShareExternalCommand(oUser, m_nGroupID, m_eAssetType, m_nAssetID, m_oKeyValue);
                    eExternalResponse = oFBCommand.Execute(out sFBObjectID, out sFBActionID);
                    doc = CreateSocialActivityDoc(oUser, ref sFBObjectID, ref sFBActionID);
                    eInternalResponse = m_oSocialBL.InsertUserSocialAction(doc, out sDbRecordId) ? SocialActionResponseStatus.OK : SocialActionResponseStatus.ERROR;
                    break;
                case eUserAction.RATES:
                    DoUserRate(ref oUser, out eInternalResponse, out eExternalResponse);
                    break;

                case eUserAction.FOLLOWS:
                    #region Do Follows

                    m_oSocialBL.GetUserSocialAction(m_sSiteGuid, m_eSocialPlatform, m_eAssetType, m_eAction, m_nAssetID, out doc);
                    if (doc != null && doc.IsActive == true)
                    {
                        eInternalResponse = SocialActionResponseStatus.MEDIA_ALEADY_FOLLOWED;
                        eExternalResponse = SocialActionResponseStatus.MEDIA_ALEADY_FOLLOWED;
                    }
                    else
                    {
                        oFBCommand = new FBFollowsExternalCommand(oUser, m_nGroupID, m_eAssetType, m_nAssetID, m_oKeyValue);
                        eExternalResponse = oFBCommand.Execute(out sFBObjectID, out sFBActionID);
                        doc = CreateSocialActivityDoc(oUser, ref sFBObjectID, ref sFBActionID);
                        eInternalResponse = m_oSocialBL.InsertUserSocialAction(doc, out sDbRecordId) ? SocialActionResponseStatus.OK : SocialActionResponseStatus.ERROR;
                    }
                    #endregion
                    break;
                default:
                    eInternalResponse = SocialActionResponseStatus.UNKNOWN_ACTION;
                    eExternalResponse = SocialActionResponseStatus.UNKNOWN_ACTION;
                    break;
            }
        }

        private void DeleteUserAction(ref User oUser, eUserAction eUserAction, out SocialActionResponseStatus eInternalStatus, out SocialActionResponseStatus eExternalStatus, out SocialActivityDoc doc)
        {
            //SocialActivityDoc doc;
            eInternalStatus = SocialActionResponseStatus.NOT_ALLOWED;
            eExternalStatus = SocialActionResponseStatus.NOT_ALLOWED;

            m_oSocialBL.GetUserSocialAction(m_sSiteGuid, m_eSocialPlatform, m_eAssetType, eUserAction, m_nAssetID, out doc);
            if (doc != null && doc.IsActive == true)
            {
                BaseFBExternalCommand oFBCommand = new FBDeleteExternalCommand(oUser, m_nGroupID, eUserAction.LIKE, m_eAssetType, m_nAssetID, m_oKeyValue);
                string sFBActionID, sFBObjectID;

                eExternalStatus = oFBCommand.Execute(out sFBObjectID, out sFBActionID);
                eInternalStatus = m_oSocialBL.DeleteUserSocialAction(m_sSiteGuid, m_nAssetID, m_eAssetType, eUserAction.LIKE, m_eSocialPlatform) ? SocialActionResponseStatus.OK : SocialActionResponseStatus.ERROR;

                m_oSocialBL.DecrementAssetLikeCounter(m_nAssetID, m_eAssetType);
            }
        }

        private void DoUserRate(ref User oUser, out SocialActionResponseStatus eInternalStatus, out SocialActionResponseStatus eExternalStatus)
        {
            SocialActivityDoc doc;

            m_oSocialBL.GetUserSocialAction(m_sSiteGuid, m_eSocialPlatform, m_eAssetType, m_eAction, m_nAssetID, out doc);
            if (doc != null && doc.IsActive)
            {
                eInternalStatus = SocialActionResponseStatus.MEDIA_ALREADY_RATED;
                eExternalStatus = SocialActionResponseStatus.MEDIA_ALREADY_RATED;
            }
            else
            {
                string sFBObjectID;
                eInternalStatus = SocialActionResponseStatus.INVALID_PARAMETERS;
                BaseFBExternalCommand oFBCommand = new FBRateExternalCommand(oUser, m_nGroupID, m_eAssetType, m_nAssetID, m_oKeyValue);
                string sFBActionID;
                eExternalStatus = oFBCommand.Execute(out sFBObjectID, out sFBActionID);

                ApiObjects.KeyValuePair oRatingKVP = ApiObjects.KeyValuePair.GetKVPFromList("rating:value", m_oKeyValue);
                int nRateVal = 0;
                if (oRatingKVP != null && int.TryParse(oRatingKVP.value, out nRateVal))
                {
                    doc = CreateSocialActivityDoc(oUser, ref sFBObjectID, ref sFBActionID);
                    doc.ActivityVerb.RateValue = nRateVal;

                    string sDbID;
                    eInternalStatus = m_oSocialBL.InsertUserSocialAction(doc, out sDbID) ? SocialActionResponseStatus.OK : SocialActionResponseStatus.ERROR;
                    m_oSocialBL.RateAsset(m_sSiteGuid, m_nAssetID, m_eAssetType, nRateVal);
                }
            }

        }
    }
}

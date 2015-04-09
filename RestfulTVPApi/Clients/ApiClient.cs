using RestfulTVPApi.Clients.Utils;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Extentions;
using RestfulTVPApi.Api;
using RestfulTVPApi.Objects.Responses;


namespace RestfulTVPApi.Clients
{
    public class ApiClient : BaseClient
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiClient));

        #endregion

        #region C'tor
        public ApiClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
           
        }

        public ApiClient()
        {
            // TODO: Complete member initialization
        }

        #endregion C'tor

        #region

        protected RestfulTVPApi.Api.API Api
        {
            get
            {
                return (Module as RestfulTVPApi.Api.API);
            }
        }

        #endregion

        #region Public methods
        public List<RestfulTVPApi.Objects.Responses.GroupOperator> GetGroupOperators(string scope)
        {
            List<RestfulTVPApi.Objects.Responses.GroupOperator> response = null;

            response = Execute(() =>
                {
                    var res = Api.GetGroupOperators(WSUserName, WSPassword, scope);
                    if (res != null)
                        response = res.Where(go => go != null).Select(o => o.ToApiObject()).ToList();

                    return response;
                }) as List<RestfulTVPApi.Objects.Responses.GroupOperator>;

            return response;
        }

        public List<RestfulTVPApi.Objects.Responses.GroupOperator> GetOperators(int[] operatorIds)
        {
            List<RestfulTVPApi.Objects.Responses.GroupOperator> operators = null;

            operators = Execute(() =>
                {

                    var response = Api.GetOperator(WSUserName, WSPassword, operatorIds);
                    if (response != null)
                        operators = response.Where(go => go != null).Select(o => o.ToApiObject()).ToList();

                    return operators;
                }) as List<RestfulTVPApi.Objects.Responses.GroupOperator>;

            return operators;            
        }

        public RestfulTVPApi.Objects.Responses.MediaMarkObject GetMediaMark(string sSiteGuid, int iMediaID)
        {
            RestfulTVPApi.Objects.Responses.MediaMarkObject mediaMark = null;

            mediaMark = Execute(() =>
                {
                    var res = Api.GetMediaMark(WSUserName, WSPassword, iMediaID, sSiteGuid);
                    if (res != null)
                        mediaMark = res.ToApiObject();

                    return mediaMark;
                }) as RestfulTVPApi.Objects.Responses.MediaMarkObject;

            return mediaMark;
        }

        public bool AddUserSocialAction(int iMediaID, string sSiteGuid, SocialAction Action, RestfulTVPApi.Api.SocialPlatform socialPlatform)
        {
            bool bRet = false;

            bRet = Convert.ToBoolean(Execute(() =>
                {
                    bRet = Api.AddUserSocialAction(WSUserName, WSPassword, iMediaID, sSiteGuid, Action, socialPlatform);
                    return bRet;
                }));

            return bRet;
        }

        public RateMediaObject RateMedia(string siteGuid, int mediaId, int rating)
        {
            RateMediaObject res = null;

            res = Execute(() =>
                {
                    res = Api.RateMedia(WSUserName, WSPassword, mediaId, siteGuid, rating);
                    return res;
                }) as RateMediaObject;

            return res;
        }

        public string CheckGeoBlockMedia(int iMediaID, string UserIP)
        {
            string geo = string.Empty;

            geo = Execute(() =>
                {

                    geo = Api.CheckGeoBlockMedia(WSUserName, WSPassword, iMediaID, UserIP);
                    return geo;
                }) as string;

            return geo;
        }

        public List<EPGChannel> GetEPGChannel(string sPicSize)
        {
            List<EPGChannel> objEPGRes = null;

            objEPGRes = Execute(() =>
                {

                    var response = Api.GetEPGChannel(WSUserName, WSPassword, sPicSize);
                    if (response != null)
                        objEPGRes = response.Where(c => c != null).Select(c => c.ToApiObject()).ToList();

                    return objEPGRes;
                }) as List<EPGChannel>;

            return objEPGRes;
        }

        //public List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> GetEPGChannelProgrammeByDates(string sChannelID, string sPicSize, DateTime fromDate, DateTime toDate, int utcOffset)
        //{
        //    List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> objEPGRes = null;

        //    objEPGRes = Execute(() =>
        //        {

        //            var response = Api.GetEPGChannelProgrammeByDates(m_wsUserName, m_wsPassword, sChannelID, sPicSize, fromDate, toDate, utcOffset);
        //            if (response != null)
        //                objEPGRes = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();

        //            return objEPGRes;
        //        }) as List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject>;

        //    return objEPGRes;
        //}

        //public List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> GetEPGChannel(string sChannelID, string sPicSize, RestfulTVPApi.Api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> objEPGProgramRes = null;

        //    objEPGProgramRes = Execute(() =>
        //        {
        //            var response = Api.GetEPGChannelProgramme(m_wsUserName, m_wsPassword, sChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
        //            if (response != null)
        //                objEPGProgramRes = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();

        //            return objEPGProgramRes;
        //        }) as List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject>;

        //    return objEPGProgramRes;
        //}

        //public List<RestfulTVPApi.Objects.Responses.EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(string[] sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    List<RestfulTVPApi.Objects.Responses.EPGMultiChannelProgrammeObject> objEPGProgramRes = null;

        //    objEPGProgramRes = Execute(() =>
        //        {
        //            var response = Api.GetEPGMultiChannelProgramme(m_wsUserName, m_wsPassword, sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
        //            if (response != null)
        //                objEPGProgramRes = response.Where(mcp => mcp != null).Select(p => p.ToApiObject()).ToList();

        //            return objEPGProgramRes;
        //        }) as List<RestfulTVPApi.Objects.Responses.EPGMultiChannelProgrammeObject>;

        //    return objEPGProgramRes;
        //}

        public List<RestfulTVPApi.Objects.Responses.GroupRule> GetGroupMediaRules(int MediaId, int siteGuid, string udid)
        {
            List<RestfulTVPApi.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {

                    var response = Api.GetGroupMediaRules(WSUserName, WSPassword, MediaId, siteGuid, RestfulTVPApi.ServiceInterface.Utils.GetClientIP(), udid);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.GroupRule>;

            return res;            
        }

        public List<RestfulTVPApi.Objects.Responses.GroupRule> GetGroupRules()
        {
            List<RestfulTVPApi.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetGroupRules(WSUserName, WSPassword);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(gr => gr.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.GroupRule>;

            return res;     
        }

        public List<RestfulTVPApi.Objects.Responses.GroupRule> GetUserGroupRules(string siteGuid)
        {
            List<RestfulTVPApi.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetUserGroupRules(WSUserName, WSPassword, siteGuid);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.GroupRule>;

            return res;            
        }

        public bool SetUserGroupRule(string siteGuid, int ruleID, string PIN, int isActive)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.SetUserGroupRule(WSUserName, WSPassword, siteGuid, ruleID, PIN, isActive);
                    return res;
                }));

            return res;
        }

        public bool CheckParentalPIN(string siteGuid, int ruleID, string PIN)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {

                    res = Api.CheckParentalPIN(WSUserName, WSPassword, siteGuid, ruleID, PIN);
                    return res;
                }));

            return res;            
        }

        public List<string> GetAutoCompleteList(int[] mediaTypes, string[] metas, string[] tags, string prefix, string lang, int pageIdx, int pageSize)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var res = Api.GetAutoCompleteList(WSUserName, WSPassword, new RestfulTVPApi.Api.RequestObj()
                    {
                        m_InfoStruct = new RestfulTVPApi.Api.InfoStructObj()
                        {
                            m_MediaTypes = mediaTypes,
                            m_Metas = metas,
                            m_Tags = tags,
                            m_sPrefix = prefix
                        },
                        m_eRuleType = RestfulTVPApi.Api.eCutType.Or,
                        m_sLanguage = lang,
                        m_iPageIndex = pageIdx,
                        m_iPageSize = pageSize
                    });

                    if (res != null)
                        retVal = res.ToList();

                    return retVal;
                }) as List<string>;

            return retVal;            
        }

        public bool SetRuleState(string siteGuid, int domainID, int ruleID, int isActive)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.SetRuleState(WSUserName, WSPassword, domainID, siteGuid, ruleID, isActive);
                    return res;
                }));

            return res;
        }

        public List<RestfulTVPApi.Objects.Responses.GroupRule> GetDomainGroupRules(int domainID)
        {
            List<RestfulTVPApi.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetDomainGroupRules(WSUserName, WSPassword, domainID);

                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.GroupRule>;

            return res;
        }

        public bool SetDomainGroupRule(int domainID, int ruleID, string PIN, int isActive)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.SetDomainGroupRule(WSUserName, WSPassword, domainID, ruleID, PIN, isActive);
                    return res;
                }));

            return res;
        }

        public List<RestfulTVPApi.Objects.Responses.GroupRule> GetEPGProgramRules(int MediaId, int programId, string siteGuid, string IP, string udid)
        {
            List<RestfulTVPApi.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetEPGProgramRules(WSUserName, WSPassword, MediaId, programId, int.Parse(siteGuid), IP, udid);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<RestfulTVPApi.Objects.Responses.GroupRule>;

            return res;
        }

        public List<string> GetUserStartedWatchingMedias(string siteGuid, int numOfItems)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var res = Api.GetUserStartedWatchingMedias(WSUserName, WSPassword, siteGuid, numOfItems);
                    if (res != null)
                        retVal = res.ToList();

                    return retVal;
                }) as List<string>;

            return retVal;
        }

        public bool CleanUserHistory(string siteGuid, int[] mediaIDs)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.CleanUserHistory(WSUserName, WSPassword, siteGuid, mediaIDs);
                    return res;
                }));

            return res;
        }

        //public List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> GetEPGProgramsByScids(string siteGuid, string[] scids, Language language, int duration)
        //{
        //    List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> res = null;

        //    res = Execute(() =>
        //        {
        //            var response = Api.GetEPGProgramsByScids(m_wsUserName, m_wsPassword, scids, language, duration);
        //            if (response != null)
        //                res = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();

        //            return res;
        //        }) as List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject>;

        //    return res;
        //}

        public bool SendToFriend(string senderName, string senderMail, string mailTo, int mediaID)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.SendToFriend(WSUserName, WSPassword, senderName, senderMail, mailTo, mailTo, mediaID);
                    return res;
                }));

            return res;
        }

        //public List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> SearchEPGContent(string searchValue, int nPageIndex, int nPageSize)
        //{
        //    List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject> res = null;

        //    res = Execute(() =>
        //        {
        //            string sKey = string.Format("{0}_{1}_{2}", searchValue, nPageIndex, nPageSize);

        //            // return object from cache if exist
        //            object oFromCache = DataHelper.GetCacheObject(sKey);
        //            if (oFromCache != null && oFromCache is IEnumerable<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject>)
        //                return (oFromCache as List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject>);

        //            var response = Api.SearchEPGContent(m_wsUserName, m_wsPassword, searchValue, nPageIndex, nPageSize);

        //            if (response != null && response.Length > 0)
        //            {
        //                res = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();
        //                DataHelper.SetCacheObject(sKey, res);
        //            }

        //            return res;
        //        }) as List<RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject>;

        //    return res;           
        //}

        //public RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject[] GetEPGProgramsByProgramsIdentefier(string siteGuid, string[] pids, Language language, int duration)
        //{
        //    RestfulTVPApi.Objects.Responses.EPGChannelProgrammeObject[] res = null;
        //    try
        //    {
        //        res = Api.GetEPGProgramsByProgramsIdentefier(m_wsUserName, m_wsPassword, pids, language, duration);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGProgramsByScids, Error Message: {0}, Parameters :  siteGuid: {1}, clientIP: {2}, language: {3}, duration: {4}", ex.Message, siteGuid, Utils.GetClientIP(), language, duration);
        //    }
        //    return res;
        //}

        #endregion
    }
}
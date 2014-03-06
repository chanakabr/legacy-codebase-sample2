using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using TVPPro.SiteManager.Services;
//using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Extentions;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPApiModule.Context;
using TVPPro.SiteManager.Objects;

namespace TVPApiModule.Services
{
    public class ApiApiService : BaseService
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiService));

        //private TVPPro.SiteManager.TvinciPlatform.api.API m_Module;

        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;

        //private int m_groupID;
        //private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiApiService(int groupID, PlatformType platform)
        {
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.api.API();
            //m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.URL;
            //m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultUser;
            //m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiApiService()
        {
            // TODO: Complete member initialization
        }

        #endregion C'tor

        #region

        protected TVPPro.SiteManager.TvinciPlatform.api.API Api
        {
            get
            {
                return (m_Module as TVPPro.SiteManager.TvinciPlatform.api.API);
            }
        }

        #endregion

        //#region Public Static Functions

        //public static ApiApiService Instance(int groupId, PlatformType platform)
        //{
        //    return BaseService.Instance(groupId, platform, eService.ApiService) as ApiApiService;
        //}

        //#endregion
        
        #region Public methods
        public List<TVPApiModule.Objects.Responses.GroupOperator> GetGroupOperators(string scope)
        {
            List<TVPApiModule.Objects.Responses.GroupOperator> response = null;

            response = Execute(() =>
                {
                    var res = Api.GetGroupOperators(m_wsUserName, m_wsPassword, scope);
                    if (res != null)
                        response = res.Where(go => go != null).Select(o => o.ToApiObject()).ToList();

                    return response;
                }) as List<TVPApiModule.Objects.Responses.GroupOperator>;

            return response;
        }

        public List<TVPApiModule.Objects.Responses.GroupOperator> GetOperators(int[] operatorIds)
        {
            List<TVPApiModule.Objects.Responses.GroupOperator> operators = null;

            operators = Execute(() =>
                {

                    var response = Api.GetOperator(m_wsUserName, m_wsPassword, operatorIds);
                    if (response != null)
                        operators = response.Where(go => go != null).Select(o => o.ToApiObject()).ToList();

                    return operators;
                }) as List<TVPApiModule.Objects.Responses.GroupOperator>;

            return operators;            
        }

        public TVPApiModule.Objects.Responses.MediaMarkObject GetMediaMark(string sSiteGuid, int iMediaID)
        {
            TVPApiModule.Objects.Responses.MediaMarkObject mediaMark = null;

            mediaMark = Execute(() =>
                {
                    var res = Api.GetMediaMark(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid);
                    if (res != null)
                        mediaMark = res.ToApiObject();

                    return mediaMark;
                }) as TVPApiModule.Objects.Responses.MediaMarkObject;

            return mediaMark;
        }

        public bool AddUserSocialAction(int iMediaID, string sSiteGuid, SocialAction Action, TVPPro.SiteManager.TvinciPlatform.api.SocialPlatform socialPlatform)
        {
            bool bRet = false;

            bRet = Convert.ToBoolean(Execute(() =>
                {
                    bRet = Api.AddUserSocialAction(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid, Action, socialPlatform);
                    return bRet;
                }));

            return bRet;
        }

        public RateMediaObject RateMedia(string siteGuid, int mediaId, int rating)
        {
            RateMediaObject res = null;

            res = Execute(() =>
                {
                    res = Api.RateMedia(m_wsUserName, m_wsPassword, mediaId, siteGuid, rating);
                    return res;
                }) as RateMediaObject;

            return res;
        }

        public string CheckGeoBlockMedia(int iMediaID, string UserIP)
        {
            string geo = string.Empty;

            geo = Execute(() =>
                {

                    geo = Api.CheckGeoBlockMedia(m_wsUserName, m_wsPassword, iMediaID, UserIP);
                    return geo;
                }) as string;

            return geo;
        }

        public List<EPGChannel> GetEPGChannel(string sPicSize)
        {
            List<EPGChannel> objEPGRes = null;

            objEPGRes = Execute(() =>
                {

                    var response = Api.GetEPGChannel(m_wsUserName, m_wsPassword, sPicSize);
                    if (response != null)
                        objEPGRes = response.Where(c => c != null).Select(c => c.ToApiObject()).ToList();

                    return objEPGRes;
                }) as List<EPGChannel>;

            return objEPGRes;
        }

        //public List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> GetEPGChannelProgrammeByDates(string sChannelID, string sPicSize, DateTime fromDate, DateTime toDate, int utcOffset)
        //{
        //    List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> objEPGRes = null;

        //    objEPGRes = Execute(() =>
        //        {

        //            var response = Api.GetEPGChannelProgrammeByDates(m_wsUserName, m_wsPassword, sChannelID, sPicSize, fromDate, toDate, utcOffset);
        //            if (response != null)
        //                objEPGRes = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();

        //            return objEPGRes;
        //        }) as List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>;

        //    return objEPGRes;
        //}

        //public List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> GetEPGChannel(string sChannelID, string sPicSize, TVPPro.SiteManager.TvinciPlatform.api.EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> objEPGProgramRes = null;

        //    objEPGProgramRes = Execute(() =>
        //        {
        //            var response = Api.GetEPGChannelProgramme(m_wsUserName, m_wsPassword, sChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
        //            if (response != null)
        //                objEPGProgramRes = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();

        //            return objEPGProgramRes;
        //        }) as List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>;

        //    return objEPGProgramRes;
        //}

        //public List<TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject> GetEPGMultiChannelProgram(string[] sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    List<TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject> objEPGProgramRes = null;

        //    objEPGProgramRes = Execute(() =>
        //        {
        //            var response = Api.GetEPGMultiChannelProgramme(m_wsUserName, m_wsPassword, sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
        //            if (response != null)
        //                objEPGProgramRes = response.Where(mcp => mcp != null).Select(p => p.ToApiObject()).ToList();

        //            return objEPGProgramRes;
        //        }) as List<TVPApiModule.Objects.Responses.EPGMultiChannelProgrammeObject>;

        //    return objEPGProgramRes;
        //}

        public List<TVPApiModule.Objects.Responses.GroupRule> GetGroupMediaRules(int MediaId, int siteGuid, string udid)
        {
            List<TVPApiModule.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {

                    var response = Api.GetGroupMediaRules(m_wsUserName, m_wsPassword, MediaId, siteGuid, SiteHelper.GetClientIP(), udid);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.GroupRule>;

            return res;            
        }

        public List<TVPApiModule.Objects.Responses.GroupRule> GetGroupRules()
        {
            List<TVPApiModule.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetGroupRules(m_wsUserName, m_wsPassword);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(gr => gr.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.GroupRule>;

            return res;     
        }

        public List<TVPApiModule.Objects.Responses.GroupRule> GetUserGroupRules(string siteGuid)
        {
            List<TVPApiModule.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetUserGroupRules(m_wsUserName, m_wsPassword, siteGuid);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.GroupRule>;

            return res;            
        }

        public bool SetUserGroupRule(string siteGuid, int ruleID, string PIN, int isActive)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.SetUserGroupRule(m_wsUserName, m_wsPassword, siteGuid, ruleID, PIN, isActive);
                    return res;
                }));

            return res;
        }

        public bool CheckParentalPIN(string siteGuid, int ruleID, string PIN)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {

                    res = Api.CheckParentalPIN(m_wsUserName, m_wsPassword, siteGuid, ruleID, PIN);
                    return res;
                }));

            return res;            
        }

        public List<string> GetAutoCompleteList(int[] mediaTypes, string[] metas, string[] tags, string prefix, string lang, int pageIdx, int pageSize)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var res = Api.GetAutoCompleteList(m_wsUserName, m_wsPassword, new TVPPro.SiteManager.TvinciPlatform.api.RequestObj()
                    {
                        m_InfoStruct = new TVPPro.SiteManager.TvinciPlatform.api.InfoStructObj()
                        {
                            m_MediaTypes = mediaTypes,
                            m_Metas = metas,
                            m_Tags = tags,
                            m_sPrefix = prefix
                        },
                        m_eRuleType = TVPPro.SiteManager.TvinciPlatform.api.eCutType.Or,
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
                    res = Api.SetRuleState(m_wsUserName, m_wsPassword, domainID, siteGuid, ruleID, isActive);
                    return res;
                }));

            return res;
        }

        public List<TVPApiModule.Objects.Responses.GroupRule> GetDomainGroupRules(int domainID)
        {
            List<TVPApiModule.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetDomainGroupRules(m_wsUserName, m_wsPassword, domainID);

                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.GroupRule>;

            return res;
        }

        public bool SetDomainGroupRule(int domainID, int ruleID, string PIN, int isActive)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.SetDomainGroupRule(m_wsUserName, m_wsPassword, domainID, ruleID, PIN, isActive);
                    return res;
                }));

            return res;
        }

        public List<TVPApiModule.Objects.Responses.GroupRule> GetEPGProgramRules(int MediaId, int programId, string siteGuid, string IP, string udid)
        {
            List<TVPApiModule.Objects.Responses.GroupRule> res = null;

            res = Execute(() =>
                {
                    var response = Api.GetEPGProgramRules(m_wsUserName, m_wsPassword, MediaId, programId, int.Parse(siteGuid), IP, udid);
                    if (response != null)
                        res = response.Where(gr => gr != null).Select(r => r.ToApiObject()).ToList();

                    return res;
                }) as List<TVPApiModule.Objects.Responses.GroupRule>;

            return res;
        }

        public List<string> GetUserStartedWatchingMedias(string siteGuid, int numOfItems)
        {
            List<string> retVal = null;

            retVal = Execute(() =>
                {
                    var res = Api.GetUserStartedWatchingMedias(m_wsUserName, m_wsPassword, siteGuid, numOfItems);
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
                    res = Api.CleanUserHistory(m_wsUserName, m_wsPassword, siteGuid, mediaIDs);
                    return res;
                }));

            return res;
        }

        //public List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> GetEPGProgramsByScids(string siteGuid, string[] scids, Language language, int duration)
        //{
        //    List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> res = null;

        //    res = Execute(() =>
        //        {
        //            var response = Api.GetEPGProgramsByScids(m_wsUserName, m_wsPassword, scids, language, duration);
        //            if (response != null)
        //                res = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();

        //            return res;
        //        }) as List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>;

        //    return res;
        //}

        public bool SendToFriend(string senderName, string senderMail, string mailTo, int mediaID)
        {
            bool res = false;

            res = Convert.ToBoolean(Execute(() =>
                {
                    res = Api.SendToFriend(m_wsUserName, m_wsPassword, senderName, senderMail, mailTo, mailTo, mediaID);
                    return res;
                }));

            return res;
        }

        //public List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> SearchEPGContent(string searchValue, int nPageIndex, int nPageSize)
        //{
        //    List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> res = null;

        //    res = Execute(() =>
        //        {
        //            string sKey = string.Format("{0}_{1}_{2}", searchValue, nPageIndex, nPageSize);

        //            // return object from cache if exist
        //            object oFromCache = DataHelper.GetCacheObject(sKey);
        //            if (oFromCache != null && oFromCache is IEnumerable<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>)
        //                return (oFromCache as List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>);

        //            var response = Api.SearchEPGContent(m_wsUserName, m_wsPassword, searchValue, nPageIndex, nPageSize);

        //            if (response != null && response.Length > 0)
        //            {
        //                res = response.Where(cp => cp != null).Select(p => p.ToApiObject()).ToList();
        //                DataHelper.SetCacheObject(sKey, res);
        //            }

        //            return res;
        //        }) as List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>;

        //    return res;           
        //}

        //public TVPApiModule.Objects.Responses.EPGChannelProgrammeObject[] GetEPGProgramsByProgramsIdentefier(string siteGuid, string[] pids, Language language, int duration)
        //{
        //    TVPApiModule.Objects.Responses.EPGChannelProgrammeObject[] res = null;
        //    try
        //    {
        //        res = Api.GetEPGProgramsByProgramsIdentefier(m_wsUserName, m_wsPassword, pids, language, duration);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGProgramsByScids, Error Message: {0}, Parameters :  siteGuid: {1}, clientIP: {2}, language: {3}, duration: {4}", ex.Message, siteGuid, SiteHelper.GetClientIP(), language, duration);
        //    }
        //    return res;
        //}

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.CatalogLoaders;

namespace TVPApiModule.Services
{
    public class ApiApiService : ApiServiceBase
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiService));

        private TVPPro.SiteManager.TvinciPlatform.api.API m_Module;

        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private int m_groupID;
        private PlatformType m_platform;
        #endregion

        #region C'tor
        public ApiApiService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.api.API();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods
        public GroupOperator[] GetGroupOperators(string scope)
        {
            GroupOperator[] response = null;
            try
            {
                response = m_Module.GetGroupOperators(m_wsUserName, m_wsPassword, scope);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupOperators, Error Message: {0}", ex.Message);
            }

            return response;
        }

        public GroupOperator[] GetOperators(string platform, int[] operatorIds)
        {
            GroupOperator[] operators = null;
            try
            {
                operators = m_Module.GetOperator(m_wsUserName, m_wsPassword, operatorIds);                

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetOperators, Error Message: {0}, Parameters : operators: {1}", ex.Message,
                    string.Join(",", operatorIds.Select(x => x.ToString()).ToArray()));
            }

            return operators;
        }

        public MediaMarkObject GetMediaMark(string sSiteGuid, int iMediaID)
        {
            MediaMarkObject mediaMark = null;
            try
            {
                mediaMark = m_Module.GetMediaMark(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaMark, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, iMediaID, sSiteGuid);
            }

            return mediaMark;
        }

        public bool AddUserSocialAction(int iMediaID, string sSiteGuid, SocialAction Action, SocialPlatform socialPlatform)
        {
            bool bRet = false;
            try
            {
                bRet = m_Module.AddUserSocialAction(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid, Action, socialPlatform);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddUserSocialAction, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, iMediaID, sSiteGuid);
            }

            return bRet;
        }

        public RateMediaObject RateMedia(string siteGuid, int mediaId, int rating)
        {
            RateMediaObject res = null;
            try
            {
                res = m_Module.RateMedia(m_wsUserName, m_wsPassword, mediaId, siteGuid, rating);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RateMedia, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, mediaId, siteGuid);
            }

            return res;
        }

        public string CheckGeoBlockMedia(int iMediaID, string UserIP)
        {
            string geo = string.Empty;
            try
            {
                geo = m_Module.CheckGeoBlockMedia(m_wsUserName, m_wsPassword, iMediaID, UserIP);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CheckGEOBlock, Error Message: {0}, Parameters :  Media ID: {1}, User ID: {2}", ex.Message, iMediaID, SiteHelper.GetClientIP());
            }
            return geo;
        }

        public EPGChannelObject[] GetEPGChannel(string sPicSize)
        {
            EPGChannelObject[] objEPGRes = null;
            try
            {
                objEPGRes = m_Module.GetEPGChannel(m_wsUserName, m_wsPassword, sPicSize);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling web service protocol : GetEPGChannel, Error Message: {0}, IP: {1}", ex.Message, SiteHelper.GetClientIP());
            }
            return objEPGRes;
        }

        //public EPGChannelProgrammeObject[] GetEPGChannelProgrammeByDates(string sChannelID, string sPicSize, DateTime fromDate, DateTime toDate, int utcOffset)
        //{
        //    EPGChannelProgrammeObject[] objEPGRes = null;
        //    try
        //    {
        //        objEPGRes = m_Module.GetEPGChannelProgrammeByDates(m_wsUserName, m_wsPassword, sChannelID, sPicSize, fromDate, toDate, utcOffset);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling web service protocol : GetEPGChannelProgrammeByDates, Error Message: {0}, IP: {1}", ex.Message, SiteHelper.GetClientIP());
        //    }
        //    return objEPGRes;
        //}

        //public EPGChannelProgrammeObject[] GetEPGChannel(string sChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    EPGChannelProgrammeObject[] objEPGProgramRes = null;
        //    try
        //    {
        //        objEPGProgramRes = m_Module.GetEPGChannelProgramme(m_wsUserName, m_wsPassword, sChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGChannelProgramme, Error Message: {0}, ChannelID: {1}, IP: {2}", ex.Message, sChannelID, SiteHelper.GetClientIP());
        //    }
        //    return objEPGProgramRes;
        //}

        //public EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgram(string[] sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    EPGMultiChannelProgrammeObject[] objEPGProgramRes = null;
        //    try
        //    {
        //        objEPGProgramRes = m_Module.GetEPGMultiChannelProgramme(m_wsUserName, m_wsPassword, sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
                
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGMultiChannelProgram, Error Message: {0}, ChannelID: {1}, IP: {2}", ex.Message, string.Join(",", sEPGChannelID), SiteHelper.GetClientIP());
        //    }
        //    return objEPGProgramRes;
        //}

        public GroupRule[] GetGroupMediaRules(int MediaId, int siteGuid, string udid)
        {
            GroupRule[] res = null;
            try
            {
                res = m_Module.GetGroupMediaRules(m_wsUserName, m_wsPassword, MediaId, siteGuid, SiteHelper.GetClientIP(), udid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupMediaRules, Error Message: {0}, Parameters :  MediaId: {1}, siteGuid: {2}, clientIP: {3}", ex.Message, MediaId, siteGuid, SiteHelper.GetClientIP());
            }
            return res;
        }

        public GroupRule[] GetGroupRules()
        {
            GroupRule[] res = null;
            try
            {
                res = m_Module.GetGroupRules(m_wsUserName, m_wsPassword);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupRules, Error Message: {0}, Parameters", ex.Message);
            }
            return res;
        }

        public GroupRule[] GetUserGroupRules(string siteGuid)
        {
            GroupRule[] res = null;
            try
            {
                res = m_Module.GetUserGroupRules(m_wsUserName, m_wsPassword, siteGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserGroupRules, Error Message: {0}, Parameters : {1}", ex.Message, siteGuid);
            }
            return res;
        }

        public bool SetUserGroupRule(string siteGuid, int ruleID, string PIN, int isActive)
        {
            bool res = false;
            try
            {
                res = m_Module.SetUserGroupRule(m_wsUserName, m_wsPassword, siteGuid, ruleID, PIN, isActive);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetUserGroupRule, Error Message: {0}, Parameters : {1}", ex.Message, siteGuid);
            }
            return res;
        }

        public bool CheckParentalPIN(string siteGuid, int ruleID, string PIN)
        {
            bool res = false;
            try
            {                
                res = m_Module.CheckParentalPIN(m_wsUserName, m_wsPassword, siteGuid, ruleID, PIN);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CheckParentalPIN, Error Message: {0}, Parameters : {1}", ex.Message, siteGuid);
            }
            return res;
        }

        public string[] GetAutoCompleteList(int[] mediaTypes, string[] metas, string[] tags, string prefix, string lang, int pageIdx, int pageSize)
        {
            string[] res = null;
            try
            {
                res = m_Module.GetAutoCompleteList(m_wsUserName, m_wsPassword, new RequestObj()
                {
                    m_InfoStruct = new InfoStructObj()
                    {
                        m_MediaTypes = mediaTypes,
                        m_Metas = metas,
                        m_Tags = tags,
                        m_sPrefix = prefix
                    },
                    m_eRuleType = eCutType.Or,
                    m_sLanguage = lang,
                    m_iPageIndex = pageIdx,
                    m_iPageSize = pageSize
                });
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetAutoCompleteList, Error Message: {0}, Parameters : prefix {1}", ex.Message, prefix);
            }
            return res;
        }

        public bool SetRuleState(string siteGuid, int domainID, int ruleID, int isActive)
        {
            bool res = false;
            try
            {
                res = m_Module.SetRuleState(m_wsUserName, m_wsPassword, domainID, siteGuid, ruleID, isActive);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetRuleState, Error Message: {0}, Parameters : siteGuid = {1}, domainID = {2}, ruleID = {3}, isActive = {4}", 
                    ex.Message, siteGuid, domainID, ruleID, isActive);
            }
            return res;
        }

        public GroupRule[] GetDomainGroupRules(int domainID)
        {
            GroupRule[] res = null;
            try
            {
                res = m_Module.GetDomainGroupRules(m_wsUserName, m_wsPassword, domainID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainGroupRules, Error Message: {0}, Parameters : domainID: {1}", ex.Message, domainID);
            }
            return res;
        }

        public bool SetDomainGroupRule(int domainID, int ruleID, string PIN, int isActive)
        {
            bool res = false;
            try
            {
                res = m_Module.SetDomainGroupRule(m_wsUserName, m_wsPassword, domainID, ruleID, PIN, isActive);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetDomainGroupRule, Error Message: {0}, Parameters : domainID: {1}, ruleID: {2}, PIN: {3} isActive: {4}",
                    ex.Message, domainID, ruleID, PIN, isActive);
            }
            return res;
        }

        public GroupRule[] GetEPGProgramRules(int MediaId, int programId, int siteGuid, string IP, string udid)
        {
            GroupRule[] res = null;
            try
            {
                res = m_Module.GetEPGProgramRules(m_wsUserName, m_wsPassword, MediaId, programId, siteGuid, IP, udid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetEPGProgramRules, Error Message: {0}, Parameters :  MediaId: {1}, siteGuid: {2}, clientIP: {3}, ProgramID: {4}", ex.Message, MediaId, siteGuid, IP, programId);
            }
            return res;
        }

        public string[] GetUserStartedWatchingMedias(string siteGuid, int numOfItems)
        {
            string[] res = null;
            try
            {
                res = m_Module.GetUserStartedWatchingMedias(m_wsUserName, m_wsPassword, siteGuid, numOfItems);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserStartedWatchingMedias, Error Message: {0}, Parameters :  siteGuid: {2}, clientIP: {3}", ex.Message, siteGuid, SiteHelper.GetClientIP());
            }
            return res;
        }

        public bool CleanUserHistory(string siteGuid, int[] mediaIDs)
        {
            bool res = false;
            try
            {
                res = m_Module.CleanUserHistory(m_wsUserName, m_wsPassword, siteGuid, mediaIDs);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CleanUserHistory, Error Message: {0}, Parameters :  siteGuid: {2}, clientIP: {3}", ex.Message, siteGuid, SiteHelper.GetClientIP());
            }
            return res;
        }

        public bool SendToFriend(string senderName, string senderMail, string mailTo, string nameTo, int mediaID)
        {
            bool res = false;
            try
            {
                res = m_Module.SendToFriend(m_wsUserName, m_wsPassword, senderName, senderMail, mailTo, nameTo, mediaID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SendToFriend, Error Message: {0}, Parameters :  senderName: {1}, senderMail: {2}, mailTo: {3}, nameTo: {4}, mediaID: {5}", ex.Message, senderName, senderMail, mailTo, nameTo, mediaID);
            }
            return res;
        }
        #endregion
    }
}

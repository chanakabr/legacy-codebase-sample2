using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.CatalogLoaders;
using TVPApiModule.Objects.Responses;
using KLogMonitor;
using System.Reflection;

namespace TVPApiModule.Services
{
    public class ApiApiService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private TVPPro.SiteManager.TvinciPlatform.api.API m_Module;
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;
        private int m_groupID;
        private PlatformType m_platform;

        public ApiApiService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.api.API();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }

        public GroupOperator[] GetGroupOperators(string scope)
        {
            GroupOperator[] response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = m_Module.GetGroupOperators(m_wsUserName, m_wsPassword, scope);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupOperators, Error Message: {0}", ex.Message);
            }

            return response;
        }

        public GroupOperator[] GetOperators(int[] operatorIds, PlatformType platform)
        {
            GroupOperator[] operators = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    operators = m_Module.GetOperator(m_wsUserName, m_wsPassword, operatorIds);
                }
                string _plat = platform == PlatformType.ConnectedTV ? "CTV" : platform.ToString();
                foreach (var menu in operators)
                {
                    string key = this[_plat];
                    var relevantMenu = (from m in menu.Groups_operators_menus
                                        where m.key == key
                                        select m).ToArray();
                    menu.Groups_operators_menus = relevantMenu;

                }

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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    mediaMark = m_Module.GetMediaMark(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = m_Module.AddUserSocialAction(m_wsUserName, m_wsPassword, iMediaID, sSiteGuid, Action, socialPlatform);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.RateMedia(m_wsUserName, m_wsPassword, mediaId, siteGuid, rating);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    geo = m_Module.CheckGeoBlockMedia(m_wsUserName, m_wsPassword, iMediaID, UserIP);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    objEPGRes = m_Module.GetEPGChannel(m_wsUserName, m_wsPassword, sPicSize);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetGroupMediaRules(m_wsUserName, m_wsPassword, MediaId, siteGuid, SiteHelper.GetClientIP(), udid);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetGroupRules(m_wsUserName, m_wsPassword);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetUserGroupRules(m_wsUserName, m_wsPassword, siteGuid);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.SetUserGroupRule(m_wsUserName, m_wsPassword, siteGuid, ruleID, PIN, isActive);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.CheckParentalPIN(m_wsUserName, m_wsPassword, siteGuid, ruleID, PIN);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.SetRuleState(m_wsUserName, m_wsPassword, domainID, siteGuid, ruleID, isActive);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetDomainGroupRules(m_wsUserName, m_wsPassword, domainID);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.SetDomainGroupRule(m_wsUserName, m_wsPassword, domainID, ruleID, PIN, isActive);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetEPGProgramRules(m_wsUserName, m_wsPassword, MediaId, programId, siteGuid, IP, udid);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.GetUserStartedWatchingMedias(m_wsUserName, m_wsPassword, siteGuid, numOfItems);
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var status = m_Module.CleanUserHistory(m_wsUserName, m_wsPassword, siteGuid, mediaIDs);
                    if (status != null && status.Code == 0)
                    {
                        res = true;
                    }
                    else
                    {
                        res = false;
                    }
                }
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
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = m_Module.SendToFriend(m_wsUserName, m_wsPassword, senderName, senderMail, mailTo, nameTo, mediaID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SendToFriend, Error Message: {0}, Parameters :  senderName: {1}, senderMail: {2}, mailTo: {3}, nameTo: {4}, mediaID: {5}", ex.Message, senderName, senderMail, mailTo, nameTo, mediaID);
            }
            return res;
        }

        public TVPApiModule.Objects.Responses.RegionsResponse GetRegions(string[] externalRegionIds)
        {
            TVPApiModule.Objects.Responses.RegionsResponse response = null;

            try
            {
                TVPPro.SiteManager.TvinciPlatform.api.RegionsResponse regionsResult = new TVPPro.SiteManager.TvinciPlatform.api.RegionsResponse();
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    regionsResult = m_Module.GetRegions(m_wsUserName, m_wsPassword, externalRegionIds, RegionOrderBy.CreateDateAsc);
                }
                response = new TVPApiModule.Objects.Responses.RegionsResponse();
                if (regionsResult != null && regionsResult.Regions != null)
                {
                    response.Regions = new List<Objects.Responses.Region>();
                    regionsResult.Regions.ToList().ForEach(r => response.Regions.Add(new Objects.Responses.Region(r)));
                    response.Status = new TVPApiModule.Objects.Responses.Status(regionsResult.Status.Code, regionsResult.Status.Message);
                }
                else
                {
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to get regions."), ex);
                response = new TVPApiModule.Objects.Responses.RegionsResponse();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public AdminUserResponse AdminSignIn(string username, string password)
        {
            AdminUserResponse response = new TVPApiModule.Objects.Responses.AdminUserResponse(); ;

            AdminAccountUserResponse result = new AdminAccountUserResponse();
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    result = m_Module.AdminSignIn(m_wsUserName, m_wsPassword, username, password);
                }
                if (result != null && result.m_status == AdminUserStatus.OK)
                {
                    response.AdminUser = new AdminUser(result);
                    response.Status = new TVPApiModule.Objects.Responses.Status((int)eStatus.OK, "OK");
                }
                else
                {
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to get regions."), ex);
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.ParentalRulesResponse GetParentalRules()
        {
            TVPApiModule.Objects.Responses.ParentalRulesResponse response = new Objects.Responses.ParentalRulesResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetParentalRules(m_wsUserName, m_wsPassword);
                    response = new Objects.Responses.ParentalRulesResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetParentalRules."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.ParentalRulesResponse GetDomainParentalRule(int domainId)
        {
            TVPApiModule.Objects.Responses.ParentalRulesResponse response = new Objects.Responses.ParentalRulesResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetDomainParentalRules(m_wsUserName, m_wsPassword, domainId);
                    response = new Objects.Responses.ParentalRulesResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetDomainParentalRule."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.ParentalRulesResponse GetUserParentalRules(string siteGuid, int domainId)
        {
            TVPApiModule.Objects.Responses.ParentalRulesResponse response = new Objects.Responses.ParentalRulesResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetUserParentalRules(m_wsUserName, m_wsPassword, siteGuid, domainId);
                    response = new Objects.Responses.ParentalRulesResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetUserParentalRules."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.Status SetParentalRules(string siteGuid, int domainID, long ruleId, int isActive)
        {
            TVPApiModule.Objects.Responses.Status status = new Objects.Responses.Status();

            try
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = m_Module.SetUserParentalRules(m_wsUserName, m_wsPassword, siteGuid, ruleId, isActive, domainID);
                        status.Code = webServiceRespone.Code;
                        status.Message = webServiceRespone.Message;
                    }
                }
                else
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = m_Module.SetDomainParentalRules(m_wsUserName, m_wsPassword, domainID, ruleId, isActive);
                        status.Code = webServiceRespone.Code;
                        status.Message = webServiceRespone.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to SetParentalRules."), ex);
                status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return status;
        }

        public TVPApiModule.Objects.Responses.PinResponse GetParentalPIN(int domainId, string siteGuid, int? ruleId)
        {
            TVPApiModule.Objects.Responses.PinResponse response = new Objects.Responses.PinResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetParentalPIN(m_wsUserName, m_wsPassword, domainId, siteGuid, ruleId);
                    response = new Objects.Responses.PinResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetParentalPin."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.Status SetParentalPIN(string siteGuid, int domainID, string pin, int? ruleId)
        {
            TVPApiModule.Objects.Responses.Status status = new Objects.Responses.Status();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = m_Module.SetParentalPIN(m_wsUserName, m_wsPassword, domainID, siteGuid, pin, ruleId);
                    status.Code = webServiceRespone.Code;
                    status.Message = webServiceRespone.Message;
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to SetParentalPIN."), ex);
                status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return status;
        }

        public TVPApiModule.Objects.Responses.PurchaseSettingsResponse GetPurchaseSettings(int domainId, string siteGuid)
        {
            TVPApiModule.Objects.Responses.PurchaseSettingsResponse response = new Objects.Responses.PurchaseSettingsResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetPurchaseSettings(m_wsUserName, m_wsPassword, domainId, siteGuid);
                    response = new Objects.Responses.PurchaseSettingsResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetPurchaseSettings."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.Status SetPurchaseSettings(int domainId, string siteGuid, int setting)
        {
            TVPApiModule.Objects.Responses.Status status = new Objects.Responses.Status();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = m_Module.SetPurchaseSettings(m_wsUserName, m_wsPassword, domainId, siteGuid, setting);
                    status.Code = webServiceRespone.Code;
                    status.Message = webServiceRespone.Message;
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to SetPurchaseSettings."), ex);
                status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return status;
        }

        public TVPApiModule.Objects.Responses.PurchaseSettingsResponse GetPurchasePIN(int domainId, string siteGuid)
        {
            TVPApiModule.Objects.Responses.PurchaseSettingsResponse response = new Objects.Responses.PurchaseSettingsResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetPurchasePIN(m_wsUserName, m_wsPassword, domainId, siteGuid);
                    response = new Objects.Responses.PurchaseSettingsResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetPurchasePIN."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.Status SetPurchasePIN(int domainId, string siteGuid, string pin)
        {
            TVPApiModule.Objects.Responses.Status status = new Objects.Responses.Status();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = m_Module.SetPurchasePIN(m_wsUserName, m_wsPassword, domainId, siteGuid, pin);
                    status.Code = webServiceRespone.Code;
                    status.Message = webServiceRespone.Message;
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to SetPurchasePIN."), ex);
                status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return status;
        }

        public TVPApiModule.Objects.Responses.Status ValidateParentalPIN(int domainId, string siteGuid, string pin, int? ruleId)
        {
            TVPApiModule.Objects.Responses.Status status = new Objects.Responses.Status();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = m_Module.ValidateParentalPIN(m_wsUserName, m_wsPassword, siteGuid, pin, domainId, ruleId);
                    status.Code = webServiceRespone.Code;
                    status.Message = webServiceRespone.Message;
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to ValidateParentalPIN."), ex);
                status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return status;
        }

        public TVPApiModule.Objects.Responses.Status ValidatePurchasePIN(int domainId, string siteGuid, string pin)
        {
            TVPApiModule.Objects.Responses.Status status = new Objects.Responses.Status();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = m_Module.ValidatePurchasePIN(m_wsUserName, m_wsPassword, siteGuid, pin, domainId);
                    status.Code = webServiceRespone.Code;
                    status.Message = webServiceRespone.Message;
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to ValidatePurchasePIN."), ex);
                status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return status;
        }

        public TVPApiModule.Objects.Responses.ParentalRulesResponse GetParentalMediaRules(string siteGuid, long mediaId, long domainId)
        {
            TVPApiModule.Objects.Responses.ParentalRulesResponse response = new Objects.Responses.ParentalRulesResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetParentalMediaRules(m_wsUserName, m_wsPassword, siteGuid, mediaId, domainId);
                    response = new Objects.Responses.ParentalRulesResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetParentalMediaRules."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.ParentalRulesResponse GetParentalEPGRules(string siteGuid, long epgId, long domainId)
        {
            TVPApiModule.Objects.Responses.ParentalRulesResponse response = new Objects.Responses.ParentalRulesResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetParentalEPGRules(m_wsUserName, m_wsPassword, siteGuid, epgId, domainId);
                    response = new Objects.Responses.ParentalRulesResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetParentalEPGRules."), ex);
                response.status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public Objects.Responses.Status DisableDefaultParentalRule(string siteGuid, int domainId)
        {
            TVPApiModule.Objects.Responses.Status status = new Objects.Responses.Status();

            try
            {
                if (!string.IsNullOrEmpty(siteGuid))
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = m_Module.DisableUserDefaultParentalRule(m_wsUserName, m_wsPassword, siteGuid, domainId);
                        status.Code = webServiceRespone.Code;
                        status.Message = webServiceRespone.Message;
                    }
                }
                else
                {
                    using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = m_Module.DisableDomainDefaultParentalRule(m_wsUserName, m_wsPassword, domainId);
                        status.Code = webServiceRespone.Code;
                        status.Message = webServiceRespone.Message;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to DisableDefaultParentalRule."), ex);
                status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return status;
        }

        public Objects.Responses.GenericRulesResponse GetMediaRules(string siteGuid, long mediaId, long domainId, string ip, string udid)
        {
            TVPApiModule.Objects.Responses.GenericRulesResponse response = new Objects.Responses.GenericRulesResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetMediaRules(m_wsUserName, m_wsPassword, siteGuid, mediaId, domainId, ip, udid, GenericRuleOrderBy.NameAsc);
                    response = new Objects.Responses.GenericRulesResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetMediaRules."), ex);
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public Objects.Responses.GenericRulesResponse GetEpgRules(string siteGuid, long epgId, long channelMediaId, long domainId, string ip, string udid)
        {
            TVPApiModule.Objects.Responses.GenericRulesResponse response = new Objects.Responses.GenericRulesResponse();

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = m_Module.GetEpgRules(m_wsUserName, m_wsPassword, siteGuid, epgId, channelMediaId, domainId, ip, GenericRuleOrderBy.NameAsc);
                    response = new Objects.Responses.GenericRulesResponse(webServiceResponse);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to GetEpgRules."), ex);
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.CatalogLoaders;
using TVPApiModule.Objects.Responses;
using Phx.Lib.Log;
using System.Reflection;
using ApiObjects;
using TVPApiModule.Manager;

namespace TVPApiModule.Services
{
    public class ApiApiService : ApiBase
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;
        private int m_groupID;
        private PlatformType m_platform;

        public ApiApiService(int groupID, PlatformType platform)
        {
            m_wsUserName = GroupsManager.GetGroup(groupID).ApiCredentials.Username;
            m_wsPassword = GroupsManager.GetGroup(groupID).ApiCredentials.Password;
            //ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultUser;
            //ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.ApiService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }

        public GroupOperator[] GetGroupOperators(string scope)
        {
            GroupOperator[] response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.Api.Module.GetGroupOperators(m_groupID, scope);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupOperators, Error Message: {0}", ex);
            }

            return response;
        }

        public GroupOperator[] GetOperators(int[] operatorIds, PlatformType platform)
        {
            GroupOperator[] result = null;
            try
            {
                List<GroupOperator> operators = null;

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    operators = Core.Api.Module.GetOperator(m_groupID, operatorIds == null ? null : operatorIds.ToList());
                }

                if (operators != null)
                {
                    string _plat = platform == PlatformType.ConnectedTV ? "CTV" : platform.ToString();
                    foreach (var menu in operators)
                    {
                        string key = this[_plat];
                        var relevantMenu = (from m in menu.Groups_operators_menus
                                            where m.key == key
                                            select m).ToList();
                        menu.Groups_operators_menus = relevantMenu;
                    }

                    result = operators.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetOperators, Error Message: {0}, Parameters : operators: {1}", ex,
                    string.Join(",", operatorIds.Select(x => x.ToString()).ToArray()));
            }

            return result;
        }

        public MediaMarkObject GetMediaMark(string sSiteGuid, int iMediaID)
        {
            MediaMarkObject mediaMark = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    mediaMark = Core.Api.Module.GetMediaMark(m_groupID, iMediaID, sSiteGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaMark, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex, iMediaID, sSiteGuid);
            }

            return mediaMark;
        }

        public bool AddUserSocialAction(int iMediaID, string sSiteGuid, SocialAction Action, SocialPlatform socialPlatform)
        {
            bool bRet = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    bRet = Core.Api.Module.AddUserSocialAction(m_groupID, iMediaID, sSiteGuid, Action, socialPlatform);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddUserSocialAction, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex, iMediaID, sSiteGuid);
            }

            return bRet;
        }

        public RateMediaObject RateMedia(string siteGuid, int mediaId, int rating)
        {
            RateMediaObject res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Api.Module.RateMedia(m_groupID, mediaId, siteGuid, rating);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RateMedia, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex, mediaId, siteGuid);
            }

            return res;
        }

        public string CheckGeoBlockMedia(int iMediaID, string UserIP)
        {
            string geo = string.Empty;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    geo = Core.Api.Module.CheckGeoBlockMedia(m_groupID, iMediaID, UserIP);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CheckGEOBlock, Error Message: {0}, Parameters :  Media ID: {1}, User ID: {2}", ex, iMediaID, SiteHelper.GetClientIP());
            }
            return geo;
        }

        public EPGChannelObject[] GetEPGChannel(string sPicSize)
        {
            EPGChannelObject[] objEPGRes = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var temp = Core.Api.Module.GetEPGChannel(m_groupID, sPicSize);

                    objEPGRes = temp == null ? null : temp.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling web service protocol : GetEPGChannel, Error Message: {0}, IP: {1}", ex, SiteHelper.GetClientIP());
            }
            return objEPGRes;
        }

        //public EPGChannelProgrammeObject[] GetEPGChannelProgrammeByDates(string sChannelID, string sPicSize, DateTime fromDate, DateTime toDate, int utcOffset)
        //{
        //    EPGChannelProgrammeObject[] objEPGRes = null;
        //    try
        //    {
        //        objEPGRes = Core.Api.Module.GetEPGChannelProgrammeByDates(m_groupID, sChannelID, sPicSize, fromDate, toDate, utcOffset);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling web service protocol : GetEPGChannelProgrammeByDates, Error Message: {0}, IP: {1}", ex, SiteHelper.GetClientIP());
        //    }
        //    return objEPGRes;
        //}

        //public EPGChannelProgrammeObject[] GetEPGChannel(string sChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    EPGChannelProgrammeObject[] objEPGProgramRes = null;
        //    try
        //    {
        //        objEPGProgramRes = Core.Api.Module.GetEPGChannelProgramme(m_groupID, sChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGChannelProgramme, Error Message: {0}, ChannelID: {1}, IP: {2}", ex, sChannelID, SiteHelper.GetClientIP());
        //    }
        //    return objEPGProgramRes;
        //}

        //public EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgram(string[] sEPGChannelID, string sPicSize, EPGUnit oUnit, int iFromOffset, int iToOffset, int iUTCOffSet)
        //{
        //    EPGMultiChannelProgrammeObject[] objEPGProgramRes = null;
        //    try
        //    {
        //        objEPGProgramRes = Core.Api.Module.GetEPGMultiChannelProgramme(m_groupID, sEPGChannelID, sPicSize, oUnit, iFromOffset, iToOffset, iUTCOffSet);

        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGMultiChannelProgram, Error Message: {0}, ChannelID: {1}, IP: {2}", ex, string.Join(",", sEPGChannelID), SiteHelper.GetClientIP());
        //    }
        //    return objEPGProgramRes;
        //}

        public GroupRule[] GetGroupMediaRules(int MediaId, int siteGuid, string udid)
        {
            GroupRule[] res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var temp = Core.Api.Module.GetGroupMediaRules(m_groupID, MediaId, siteGuid, SiteHelper.GetClientIP(), udid);
                    res = temp == null ? null : temp.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupMediaRules, Error Message: {0}, Parameters :  MediaId: {1}, siteGuid: {2}, clientIP: {3}", ex, MediaId, siteGuid, SiteHelper.GetClientIP());
            }
            return res;
        }

        public GroupRule[] GetGroupRules()
        {
            GroupRule[] res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var temp = Core.Api.Module.GetGroupRules(m_groupID);
                    res = temp == null ? null : temp.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupRules, Error Message: {0}, Parameters", ex);
            }
            return res;
        }

        public GroupRule[] GetUserGroupRules(string siteGuid)
        {
            GroupRule[] res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var temp = Core.Api.Module.GetUserGroupRules(m_groupID, siteGuid);
                    res = temp == null ? null : temp.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserGroupRules, Error Message: {0}, Parameters : {1}", ex, siteGuid);
            }
            return res;
        }

        public bool SetUserGroupRule(string siteGuid, int ruleID, string PIN, int isActive)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Api.Module.SetUserGroupRule(m_groupID, siteGuid, ruleID, PIN, isActive);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetUserGroupRule, Error Message: {0}, Parameters : {1}", ex, siteGuid);
            }
            return res;
        }

        public bool CheckParentalPIN(string siteGuid, int ruleID, string PIN)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Api.Module.CheckParentalPIN(m_groupID, siteGuid, ruleID, PIN);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CheckParentalPIN, Error Message: {0}, Parameters : {1}", ex, siteGuid);
            }
            return res;
        }

        public string[] GetAutoCompleteList(int[] mediaTypes, string[] metas, string[] tags, string prefix, string lang, int pageIdx, int pageSize)
        {
            string[] res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var temp = Core.Api.Module.GetAutoCompleteList(m_groupID, new RequestObj()
                    {
                        m_InfoStruct = new InfoStructObj()
                        {
                            m_MediaTypes = mediaTypes == null ? null : mediaTypes.ToList(),
                            m_Metas = metas == null ? null : metas.ToList(),
                            m_Tags = tags == null ? null : tags.ToList(),
                            m_sPrefix = prefix
                        },
                        m_eRuleType = eCutType.Or,
                        m_sLanguage = lang,
                        m_iPageIndex = pageIdx,
                        m_iPageSize = pageSize
                    });
                    res = temp == null ? null : temp.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetAutoCompleteList, Error Message: {0}, Parameters : prefix {1}", ex, prefix);
            }
            return res;
        }

        public bool SetRuleState(string siteGuid, int domainID, int ruleID, int isActive)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Api.Module.SetRuleState(m_groupID, domainID, siteGuid, ruleID, isActive);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetRuleState, Error Message: {0}, Parameters : siteGuid = {1}, domainID = {2}, ruleID = {3}, isActive = {4}",
                    ex, siteGuid, domainID, ruleID, isActive);
            }
            return res;
        }

        public GroupRule[] GetDomainGroupRules(int domainID)
        {
            GroupRule[] res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var temp = Core.Api.Module.GetDomainGroupRules(m_groupID, domainID);
                    res = temp == null ? null : temp.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainGroupRules, Error Message: {0}, Parameters : domainID: {1}", ex, domainID);
            }
            return res;
        }

        public bool SetDomainGroupRule(int domainID, int ruleID, string PIN, int isActive)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Api.Module.SetDomainGroupRule(m_groupID, domainID, ruleID, PIN, isActive);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetDomainGroupRule, Error Message: {0}, Parameters : domainID: {1}, ruleID: {2}, PIN: {3} isActive: {4}",
                    ex, domainID, ruleID, PIN, isActive);
            }
            return res;
        }

        public GroupRule[] GetEPGProgramRules(int MediaId, int programId, int siteGuid, string IP, string udid)
        {
            GroupRule[] res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var temp= Core.Api.Module.GetEPGProgramRules(m_groupID, MediaId, programId, siteGuid, IP, udid);
                    res = temp == null ? null : temp.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetEPGProgramRules, Error Message: {0}, Parameters :  MediaId: {1}, siteGuid: {2}, clientIP: {3}, ProgramID: {4}", ex, MediaId, siteGuid, IP, programId);
            }
            return res;
        }

        public string[] GetUserStartedWatchingMedias(string siteGuid, int numOfItems)
        {
            string[] res = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Api.Module.GetUserStartedWatchingMedias(m_groupID, siteGuid, numOfItems);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserStartedWatchingMedias, Error Message: {0}, Parameters :  siteGuid: {2}, clientIP: {3}", ex, siteGuid, SiteHelper.GetClientIP());
            }
            return res;
        }

        public bool CleanUserHistory(string siteGuid, int[] mediaIDs)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var status = Core.Api.Module.CleanUserHistory(m_groupID, siteGuid, mediaIDs == null ? null : mediaIDs.ToList());
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
                logger.ErrorFormat("Error calling webservice protocol : CleanUserHistory, Error Message: {0}, Parameters :  siteGuid: {2}, clientIP: {3}", ex, siteGuid, SiteHelper.GetClientIP());
            }
            return res;
        }

        public bool SendToFriend(string senderName, string senderMail, string mailTo, string nameTo, int mediaID)
        {
            bool res = false;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    res = Core.Api.Module.SendToFriend(m_groupID, senderName, senderMail, mailTo, nameTo, mediaID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SendToFriend, Error Message: {0}, Parameters :  senderName: {1}, senderMail: {2}, mailTo: {3}, nameTo: {4}, mediaID: {5}", ex, senderName, senderMail, mailTo, nameTo, mediaID);
            }
            return res;
        }

        public TVPApiModule.Objects.Responses.RegionsResponse GetRegions(string[] externalRegionIds)
        {
            TVPApiModule.Objects.Responses.RegionsResponse response = null;

            try
            {
                var regionsResult = new ApiObjects.RegionsResponse();
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    regionsResult = Core.Api.Module.GetRegions(m_groupID, externalRegionIds == null ? null : externalRegionIds.ToList(), RegionOrderBy.CreateDateAsc);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    result = Core.Api.Module.AdminSignIn(m_groupID, username, password);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetParentalRules(m_groupID);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetDomainParentalRules(m_groupID, domainId);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetUserParentalRules(m_groupID, siteGuid, domainId);
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
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = Core.Api.Module.SetUserParentalRules(m_groupID, siteGuid, ruleId, isActive, domainID);
                        status.Code = webServiceRespone.Code;
                        status.Message = webServiceRespone.Message;
                    }
                }
                else
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = Core.Api.Module.SetDomainParentalRules(m_groupID, domainID, ruleId, isActive);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetParentalPIN(m_groupID, domainId, siteGuid, ruleId);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = Core.Api.Module.SetParentalPIN(m_groupID, domainID, siteGuid, pin, ruleId);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetPurchaseSettings(m_groupID, domainId, siteGuid);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = Core.Api.Module.SetPurchaseSettings(m_groupID, domainId, siteGuid, setting);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetPurchasePIN(m_groupID, domainId, siteGuid);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = Core.Api.Module.SetPurchasePIN(m_groupID, domainId, siteGuid, pin);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = Core.Api.Module.ValidateParentalPIN(m_groupID, siteGuid, pin, domainId, ruleId);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceRespone = Core.Api.Module.ValidatePurchasePIN(m_groupID, siteGuid, pin, domainId);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetParentalMediaRules(m_groupID, siteGuid, mediaId, domainId);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetParentalEPGRules(m_groupID, siteGuid, epgId, domainId);
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
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = Core.Api.Module.DisableUserDefaultParentalRule(m_groupID, siteGuid, domainId);
                        status.Code = webServiceRespone.Code;
                        status.Message = webServiceRespone.Message;
                    }
                }
                else
                {
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                    {
                        var webServiceRespone = Core.Api.Module.DisableDomainDefaultParentalRule(m_groupID, domainId);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetMediaRules(m_groupID, siteGuid, mediaId, domainId, ip, udid, GenericRuleOrderBy.NameAsc);
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
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var webServiceResponse = Core.Api.Module.GetEpgRules(m_groupID, siteGuid, epgId, channelMediaId, domainId, ip, GenericRuleOrderBy.NameAsc);
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

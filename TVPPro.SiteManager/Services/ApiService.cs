using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.api;
using TVPPro.Configuration.PlatformServices;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.Helper;
using TVPPro.Configuration.Media;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.Services
{
    public class ApiService
    {
        #region Members
        private static object lockObject = new object();
        private TvinciPlatform.api.API m_Module;
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string wsUserName = string.Empty;
        private string wsPassword = string.Empty;
        private Dictionary<int, FileTypeContainer[]> m_AllFileTypes = new Dictionary<int, FileTypeContainer[]>();
        private int m_TVMGroupId = 0;
        #endregion Members

        #region Constractor
        private ApiService()
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.api.API();

            m_Module.Url = PlatformServicesConfiguration.Instance.Data.ApiService.URL;

            //Init file types from web service
            InitAvailableFileTypes();

            logger.Info("Starting PrService with URL:" + PlatformServicesConfiguration.Instance.Data.ConditionalAccessService.URL);
        }
        #endregion Constractor

        #region Properties
        private static ApiService m_Instance;
        public static ApiService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (lockObject)
                    {
                        m_Instance = new ApiService();
                    }
                }

                return m_Instance;
            }
        }


        #endregion

        #region Private Methods
        //This method get all the client file types(trailer, main flv ...)
        private void InitAvailableFileTypes()
        {
            FileTypeContainer[] FileTypes = null;

            if (m_TVMGroupId > 0)
            {
                TVMAccountType m_Account = new TVMAccountType();
                m_Account = PageData.Instance.GetTVMAccountByGroupID(m_TVMGroupId);

                try
                {
                    wsUserName = m_Account.APIUserName;
                    wsPassword = m_Account.APIPassword;
                    FileTypes = m_Module.GetAvailableFileTypes(wsUserName, wsPassword);


                    m_AllFileTypes.Add(m_TVMGroupId, FileTypes);
                    logger.Info("Protocol: GetAvailableFileTypes");

                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetAvailableFileTypes Error message : {0}", ex.Message);
                }
            }
        }

        private TVPPro.SiteManager.Context.Enums.eLocaleUserState ParseUserStatusToLocaleUserState(UserStatus status)
        {
            switch (status)
            {
                case UserStatus.Anonymus:
                    return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Anonymous;

                case UserStatus.EXPPVHolder:
                    return TVPPro.SiteManager.Context.Enums.eLocaleUserState.ExPPV;

                case UserStatus.ExSub:
                    return TVPPro.SiteManager.Context.Enums.eLocaleUserState.ExSub;

                case UserStatus.New:
                    return TVPPro.SiteManager.Context.Enums.eLocaleUserState.New;

                case UserStatus.PPVHolder:
                    return TVPPro.SiteManager.Context.Enums.eLocaleUserState.PPV;

                case UserStatus.Sub:
                    return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Sub;

                default:
                    return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Unknown;
            }
        }
        #endregion Private Methods

        #region Public methods
        public Dictionary<int, FileTypeContainer[]> GetFileTypes(int TVMGroupId)
        {

            if (m_AllFileTypes != null && !m_AllFileTypes.Keys.Contains(TVMGroupId))
            {
                m_TVMGroupId = TVMGroupId;
                InitAvailableFileTypes();
            }

            return m_AllFileTypes;
        }

        public Locale GetAdminLocaleValues(string token, string ip)
        {
            Locale retVal = null;
            try
            {
                string deviceName = string.Empty;
                string countryName = string.Empty;
                string language = string.Empty;
                UserStatus userStatus = new UserStatus();
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                if (m_Module.GetAdminTokenValues(wsUserName, wsPassword, ip, token, ref countryName, ref language, ref deviceName, ref userStatus))
                {
                    retVal = new Locale();
                    retVal.IsAdminLocale = true;
                    retVal.LocaleDevice = deviceName;
                    retVal.LocaleLanguage = language;
                    retVal.LocaleCountry = countryName;
                    retVal.LocaleUserState = ParseUserStatusToLocaleUserState(userStatus);
                    logger.InfoFormat("Protocol: GetAdminLocaleValues, Parameters : token : {0}, ip : {1}", token, ip);
                }
                else
                {
                    logger.ErrorFormat("Invalid Token: {0}, Parameters :  token: {1}, ip: {1}", token, ip);
                }

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetAdminLocaleValues, Error Message: {0}, Parameters :  token: {1}, ip: {1}", ex.Message, token, ip);
            }

            return retVal;
        }
        #endregion Public methods

        public MeidaMaper[] GetMediaIDsFromFileIDs(int[] fileIDs)
        {
            MeidaMaper[] mediaMapper = null;

            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;

                mediaMapper = m_Module.MapMediaFiles(wsUserName, wsPassword, fileIDs);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetAdminLocaleValues, Error Message: {0}, Parameters :  File IDs: {1}", ex.Message, fileIDs.ToString());
            }

            return mediaMapper;
        }

        public RateMediaObject RateMedia(int mediaId, int rating)
        {
            string siteGuid = string.Empty;
            RateMediaObject res = null;
            try
            {
                siteGuid = UsersService.Instance.GetUserID();
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;

                res = m_Module.RateMedia(wsUserName, wsPassword, mediaId, siteGuid, rating);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RateMedia, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, mediaId, siteGuid);
            }

            return res;
        }

        public bool CleanUserHistory(int[] mediaIDs)
        {
            bool res = false;
            string siteGuid = string.Empty;
            try
            {
                siteGuid = siteGuid = UsersService.Instance.GetUserID();
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                var status = m_Module.CleanUserHistory(wsUserName, wsPassword, siteGuid, mediaIDs);

                if (status != null && status.Code == 0)
                {
                    res = true;
                }
                else
                {
                    res = false;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CleanUserHistory, Error Message: {0}, Parameters :  siteGuid: {2}, clientIP: {3}", ex.Message, siteGuid, SiteHelper.GetClientIP());
            }
            return res;
        }

        public MediaMarkObject GetMediaMark(int iMediaID)
        {
            MediaMarkObject mediaMark = null;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;

                mediaMark = m_Module.GetMediaMark(wsUserName, wsPassword, iMediaID, UsersService.Instance.GetUserID());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaMark, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, iMediaID, UsersService.Instance.GetUserID());
            }

            return mediaMark;
        }

        public MediaMarkObject GetMediaMarkForUser(int iMediaID, string sUserGuid)
        {
            MediaMarkObject mediaMark = null;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;

                mediaMark = m_Module.GetMediaMark(wsUserName, wsPassword, iMediaID, sUserGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetMediaMark, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, iMediaID, UsersService.Instance.GetUserID());
            }

            return mediaMark;
        }

        public bool AddUserSocialAction(int iMediaID, SocialAction action, SocialPlatform platform)
        {
            bool bRet = false;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;

                bRet = m_Module.AddUserSocialAction(wsUserName, wsPassword, iMediaID, UsersService.Instance.GetUserID(), action, platform);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddUserSocialAction, Error Message: {0}, Parameters :  Media ID: {1}, User id: {2}", ex.Message, iMediaID, UsersService.Instance.GetUserID());
            }

            return bRet;
        }

        public string CheckGeoBlockMedia(int iMediaID, string UserIP)
        {
            string geo = string.Empty;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                geo = m_Module.CheckGeoBlockMedia(wsUserName, wsPassword, iMediaID, UserIP);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CheckGEOBlock, Error Message: {0}, Parameters :  Media ID: {1}, User ID: {2}", ex.Message, iMediaID, UsersService.Instance.GetUserID());
            }
            return geo;
        }

        public EPGChannelObject[] GetEPGChannel(string PicSize)
        {
            EPGChannelObject[] res = null;

            string sKey = string.Format("EPGChannel_{0}", PicSize);

            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is EPGChannelObject[]) return oFromCache as EPGChannelObject[];

            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                res = m_Module.GetEPGChannel(wsUserName, wsPassword, PicSize);
                if (res != null && res.Length > 0)
                    DataHelper.SetCacheObject(sKey, res);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetEPGChannel, Error Message: {0}, Parameters :  PicSize: {1}, User ID: {2}", ex.Message, PicSize, UsersService.Instance.GetUserID());
            }
            return res;
        }

        //public EPGChannelProgrammeObject[] GetEPGChannelProgramme(string EpgId, string PicSize, TvinciPlatform.api.EPGUnit unit, int nFromOffsetDay, int nToOffsetDay, int nUTCOffset)
        //{
        //    EPGChannelProgrammeObject[] res = null;

        //    string sKey = string.Format("{0}_{1}_{2}_{3}_{4}", EpgId, PicSize, DateTime.Now.AddDays(nFromOffsetDay).Date.ToShortDateString(), DateTime.Now.AddDays(nToOffsetDay).Date.ToShortDateString(), nUTCOffset);

        //    // return object from cache if exist
        //    object oFromCache = DataHelper.GetCacheObject(sKey);
        //    if (oFromCache != null && oFromCache is EPGChannelProgrammeObject[]) return oFromCache as EPGChannelProgrammeObject[];

        //    try
        //    {
        //        wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
        //        wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
        //        res = m_Module.GetEPGChannelProgramme(wsUserName, wsPassword, EpgId, PicSize, unit, nFromOffsetDay, nToOffsetDay, nUTCOffset);
        //        if (res.Length > 0)
        //            DataHelper.SetCacheObject(sKey, res);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGChannelProgramme, Error Message: {0}, Parameters :  Epg Id: {1}, User ID: {2}", ex.Message, EpgId, UsersService.Instance.GetUserID());
        //    }
        //    return res;
        //}

        //public EPGMultiChannelProgrammeObject[] GetEPGMultiChannelProgramme(string[] channelID, string PicSize, TvinciPlatform.api.EPGUnit unit, int nFromOffsetDay, int nToOffsetDay, int nUTCOffset)
        //{
        //    EPGMultiChannelProgrammeObject[] res = null;

        //    string sKey = string.Format("{0}_{1}_{2}_{3}_{4}", string.Join(";", channelID), PicSize, DateTime.Now.AddDays(nFromOffsetDay).Date.ToShortDateString(), DateTime.Now.AddDays(nToOffsetDay).Date.ToShortDateString(), nUTCOffset);

        //    // return object from cache if exist
        //    object oFromCache = DataHelper.GetCacheObject(sKey);
        //    if (oFromCache != null && oFromCache is EPGMultiChannelProgrammeObject[]) return oFromCache as EPGMultiChannelProgrammeObject[];

        //    try
        //    {
        //        wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
        //        wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
        //        res = m_Module.GetEPGMultiChannelProgramme(wsUserName, wsPassword, channelID, PicSize, unit, nFromOffsetDay, nToOffsetDay, nUTCOffset);
        //        if (res.Length > 0)
        //            DataHelper.SetCacheObject(sKey, res);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetEPGChannelProgramme, Error Message: {0}, Parameters :  Epg Id: {1}, User ID: {2}", ex.Message, string.Join(";", channelID), UsersService.Instance.GetUserID());
        //    }
        //    return res;
        //}

        public GroupRule[] GetGroupMediaRules(int MediaId, int siteGuid, string deviceUDID)
        {
            GroupRule[] res = null;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                res = m_Module.GetGroupMediaRules(wsUserName, wsPassword, MediaId, siteGuid, SiteHelper.GetClientIP(), deviceUDID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetGroupMediaRules, Error Message: {0}, Parameters :  MediaId: {1}, SiteGuid: {2}, ClientIP: {3}", ex.Message, MediaId, SiteHelper.GetClientIP());
            }
            return res;
        }


        public GroupRule[] GetUserGroupRules()
        {
            GroupRule[] res = null;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                res = m_Module.GetUserGroupRules(wsUserName, wsPassword, UsersService.Instance.GetUserID());
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetUserGroupRules, Error Message: {0}, Parameters : User ID: {1}", ex.Message, UsersService.Instance.GetUserID());
            }
            return res;
        }

        public bool SetUserGroupRule(int ruleID, string pinCode, int isActive)
        {
            bool res = false;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                res = m_Module.SetUserGroupRule(wsUserName, wsPassword, UsersService.Instance.GetUserID(), ruleID, pinCode, isActive);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetUserGroupRule, Error Message: {0}, Parameters : User ID: {1}, Rule ID: {2}, Pin Code: {3}, IsActive: {4}", ex.Message, UsersService.Instance.GetUserID(), ruleID, pinCode, isActive);
            }
            return res;
        }



        public bool CheckParentalPIN(int ruleID, string parentalPIN, string siteGuid)
        {
            bool retVal = false;

            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;

                retVal = m_Module.CheckParentalPIN(wsUserName, wsPassword, siteGuid, ruleID, parentalPIN);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : CheckParentalPIN, Error Message: {0}, Parameters :  SiteGUID : {1}, RuleID : {2}, parentalPIN : {3}", ex.Message, siteGuid, ruleID, parentalPIN);
            }

            return retVal;
        }


        public DeviceAvailabiltyRule GetAvailableDevices(int MediaId)
        {
            DeviceAvailabiltyRule res = null;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                res = m_Module.GetAvailableDevices(wsUserName, wsPassword, MediaId);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetAvailableDevices, Error Message: {0}, Parameters :  Media Id: {1}, User ID: {2}", ex.Message, MediaId, UsersService.Instance.GetUserID());
            }
            return res;
        }

        public bool SetRuleState(int domainID, int ruleID, int isActive)
        {
            bool res = false;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                //res = m_Module.SetRuleState(wsUserName, wsPassword, domainID, UsersService.Instance.GetUserID(), ruleID, isActive);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetRuleState, Error Message: {0}, Parameters : User ID: {1}, Rule ID: {2}, IsActive: {3}", ex.Message, UsersService.Instance.GetUserID(), ruleID, isActive);
            }
            return res;
        }

        public string[] GetAutoCompleteList(string sPrefix)
        {
            string[] ret = { };
            try
            {
                RequestObj req = new RequestObj();
                req.m_InfoStruct = new InfoStructObj();
                req.m_InfoStruct.m_Metas = MediaConfiguration.Instance.Data.TVM.SearchValues.Metadata.ToString().Split(new Char[] { ';' });
                req.m_InfoStruct.m_Tags = MediaConfiguration.Instance.Data.TVM.SearchValues.Tags.ToString().Split(new Char[] { ';' });
                req.m_InfoStruct.m_sPrefix = sPrefix;
                req.m_iPageIndex = 0;
                req.m_iPageSize = 10;
                req.m_sLanguage = SiteManager.Manager.TextLocalization.Instance.UserLanguageKey;
                req.m_eRuleType = eCutType.Or;

                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;

                ret = m_Module.GetAutoCompleteList(wsUserName, wsPassword, req);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetAutoCompleteList, Error Message: {0}, Parameters : User ID: {1}, Prefix: {2}", ex.Message, UsersService.Instance.GetUserID(), sPrefix);
            }

            return ret;
        }

        public bool SendToFriend(string userName, string userEmail, string friendName, string friendEmail, int mediaId)
        {
            bool res = false;
            try
            {
                wsUserName = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultUser;
                wsPassword = PlatformServicesConfiguration.Instance.Data.ApiService.DefaultPassword;
                res = m_Module.SendToFriend(wsUserName, wsPassword, userName, userEmail, friendEmail, friendName, mediaId);

            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SendToFriend, Error Message: {0}, Parameters : User Name: {1}, User Email: {2}, Friend Email: {3}, Friend Name: {4}, Media ID: {5}", ex.Message, userName, userEmail, friendEmail, friendName, mediaId);
            }
            return res;
        }



    }
}

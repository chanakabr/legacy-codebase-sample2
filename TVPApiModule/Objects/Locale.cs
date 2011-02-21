using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.Services;

/// <summary>
/// Summary description for Locale
/// </summary>
/// 

namespace TVPApi
{
    public class Locale
    {
        private string m_LocaleCountry = string.Empty;
        private string m_SiteGuid = string.Empty;
        private string m_localeLanguage = string.Empty;
        private string m_localeDevice = string.Empty;

        public string LocaleLanguage
        {
            get
            {
                return m_localeLanguage;
            }
            set
            {
                m_localeLanguage = value;
            }
        }

        public string SiteGuid
        {
            get
            {
                return m_SiteGuid;
            }
            set
            {
                m_SiteGuid = value;
                // SetUserStateBySiteGuid(value);
            }
        }

        public string LocaleCountry
        {
            get
            {
                return m_LocaleCountry;
            }
            set
            {
                m_LocaleCountry = value;
            }
        }



        public string LocaleDevice
        {
            get
            {
                return m_localeDevice;
            }
            set
            {
                m_localeDevice = value;
            }
        }

        public TVPApi.LocaleUserState LocaleUserState { get; set; }


        //Get User sate from CA service according to Site GUID
        public void SetUserStateBySiteGuid(string siteGUID)
        {
            if (!string.IsNullOrEmpty(SiteGuid))
            {
                TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus status = ConditionalAccessService.Instance.GetUserCAStatus(SiteGuid);
                switch (status)
                {
                    case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.Annonymus:
                        LocaleUserState = TVPApi.LocaleUserState.Anonymous;
                        break;

                    case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.NeverPurchased:
                        LocaleUserState = TVPApi.LocaleUserState.New;
                        break;

                    case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.CurrentPPV:
                        LocaleUserState = TVPApi.LocaleUserState.PPV;
                        break;

                    case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.CurrentSub:
                        LocaleUserState = TVPApi.LocaleUserState.Sub;
                        break;

                    case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.ExPPV:
                        LocaleUserState = TVPApi.LocaleUserState.ExPPV;
                        break;

                    case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.ExSub:
                        LocaleUserState = TVPApi.LocaleUserState.ExSub;
                        break;

                    default:
                        LocaleUserState = TVPApi.LocaleUserState.Unknown;
                        break;
                }
            }
            else
            {
                LocaleUserState = TVPApi.LocaleUserState.Unknown;
            }

        }




    }
}

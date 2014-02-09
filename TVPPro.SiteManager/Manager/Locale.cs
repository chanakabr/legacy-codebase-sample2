using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.Users;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.api;

namespace TVPPro.SiteManager.Manager
{
    [Serializable]
    public class Locale
    {

        private string m_LocaleCountry;
        public string LocaleLanguage { get; set; }
        public bool IsAdminLocale { get; set; }
        public string AdminToken { get; set; }

        public string LocaleCountry
        {
            get
            {
                if (m_LocaleCountry != null)
                {
                    return m_LocaleCountry;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                m_LocaleCountry = value;
            }
        }

        private TVPPro.SiteManager.Context.Enums.eLocaleUserState m_localeUserState;

        public TVPPro.SiteManager.Context.Enums.eLocaleUserState LocaleUserState 
        {
            get
            {
                if (!IsAdminLocale)
                {
                    TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus status = ConditionalAccessService.Instance.GetUserCAStatus();
                    switch (status)
                    {
                        case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.Annonymus:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Anonymous;

                        case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.NeverPurchased:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.New;

                        case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.CurrentPPV:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.PPV;

                        case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.CurrentSub:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Sub;

                        case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.ExPPV:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.ExPPV;

                        case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.UserCAStatus.ExSub:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.ExSub;

                        default:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Unknown;
                    }
                }
                else
                {
                    return m_localeUserState;
                }
            }
            set
            {
                if (IsAdminLocale)
                {
                    m_localeUserState = value;
                }
            }
        }

        public string LocaleDevice { get; set; }
    }
}

using ApiObjects.ConditionalAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;

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
                    UserCAStatus status = ConditionalAccessService.Instance.GetUserCAStatus();
                    switch (status)
                    {
                        case UserCAStatus.Annonymus:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Anonymous;

                        case UserCAStatus.NeverPurchased:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.New;

                        case UserCAStatus.CurrentPPV:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.PPV;

                        case UserCAStatus.CurrentSub:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.Sub;

                        case UserCAStatus.ExPPV:
                            return TVPPro.SiteManager.Context.Enums.eLocaleUserState.ExPPV;

                        case UserCAStatus.ExSub:
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

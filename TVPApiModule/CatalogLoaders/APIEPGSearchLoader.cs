using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApi;
using TVPApiModule.Helper;
using TVPApiModule.Manager;
using TVPPro.SiteManager.CatalogLoaders;
using TVPApiModule.Extentions;
using TVPApiModule.Context;

namespace TVPApiModule.CatalogLoaders
{
    [Serializable]
    public class APIEPGSearchLoader : EPGSearchLoader
    {
        private string m_sCulture;

        public string Culture
        {
            get { return m_sCulture; }
            set
            {
                m_sCulture = value;
                Language = TextLocalizationManager.Instance.GetTextLocalization(GroupID, (PlatformType)Enum.Parse(typeof(PlatformType), Platform)).GetLanguageDBID(value);
            }
        }


        protected override Object ExecuteEPGAdapter(List<BaseObject> programs)
        {
            List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject> retVal = new List<TVPApiModule.Objects.Responses.EPGChannelProgrammeObject>();
            foreach (ProgramObj p in programs)
            {
                retVal.Add(p.m_oProgram.ToApiObject());
            }
            return retVal;
        }

        #region Constructors

        public APIEPGSearchLoader(int groupID, PlatformType platform, string udid, string userIP, string language, int pageSize, int pageIndex, string searchText, DateTime startTime, DateTime endTime)
            : base(groupID, userIP, pageSize, pageIndex, searchText, startTime, endTime)
        {
            Platform = platform.ToString();
            DeviceId = udid;
            Culture = language;
        }
        #endregion

    }
}

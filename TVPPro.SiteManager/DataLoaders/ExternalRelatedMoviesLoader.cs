using System;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols;
using Tvinci.Data.TVMDataLoader.Protocols.SearchRelated;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Context;
using System.Configuration;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Manager;
using System.Collections.Generic;
using TVPPro.SiteManager.Services;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class ExternalRelatedMoviesLoader : RelatedMoviesLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private ExternalRelatedMediaLoader m_oCatalogxternalRelatedLoader;
        private bool m_bShouldUseCache;

        #region Properties 
        public string FreeParam
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "FreeParam", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "FreeParam", value);
            }
        }
        #endregion

        public ExternalRelatedMoviesLoader(long mediaID, string freeParam = null)
            : this(mediaID, string.Empty, string.Empty, freeParam)
        {
        }

        public ExternalRelatedMoviesLoader(long mediaID, string userName, string pass, string freeParam = null) : base(mediaID, userName, pass)
        {
            MediaID = mediaID;
            TvmUser = userName;
            TvmPass = pass;
            FreeParam = freeParam;
        }

        public override dsItemInfo Execute()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oCatalogxternalRelatedLoader = new ExternalRelatedMediaLoader((int)MediaID, new List<int>(), TvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PicSize, FreeParam)
                {
                    DeviceId = DeviceUDID,
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    SiteGuid = SiteGuid
                };
                return m_oCatalogxternalRelatedLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }
    }
}

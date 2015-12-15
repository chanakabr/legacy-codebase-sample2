using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SearchRelated;
using Tvinci.Data.TVMDataLoader.Protocols;
using TVPPro.SiteManager.DataEntities;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;

namespace TVPApi
{
    public class APIExternalSearchMediaLoader : TVPPro.SiteManager.DataLoaders.ExternalSearchMoviesLoader
    {
        private TVPApiModule.CatalogLoaders.APIExternalSearchMediaLoader m_oCatalogExternalSearchLoader;
        private bool m_bShouldUseCache;

        public APIExternalSearchMediaLoader(string query)
            : base(query, string.Empty, string.Empty)
        {
        }

        public APIExternalSearchMediaLoader(string query, string userName, string pass)
            : base(query, userName, pass)
        {

        }

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        public int GroupID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "GroupID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "GroupID", value);
            }
        }

        public PlatformType Platform
        {
            get
            {
                return Parameters.GetParameter<PlatformType>(eParameterType.Retrieve, "Platform", PlatformType.Unknown);
            }
            set
            {
                Parameters.SetParameter<PlatformType>(eParameterType.Retrieve, "Platform", value);
            }
        }
        public string DeviceUDID
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Filter, "DeviceUDID", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Filter, "DeviceUDID", value);
            }
        }

        public int[] MediaTypes
        {
            get
            {
                return Parameters.GetParameter<int[]>(eParameterType.Retrieve, "MediaTypes", null);
            }
            set
            {
                Parameters.SetParameter<int[]>(eParameterType.Retrieve, "MediaTypes", value);
            }
        }

        public string Language
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Language", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Language", value);
            }
        }

        public int DomainID
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "DomainID", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "DomainID", value);
            }
        }

        public override dsItemInfo Execute()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oCatalogExternalSearchLoader = new TVPApiModule.CatalogLoaders.APIExternalSearchMediaLoader(
                    Query,
                    MediaTypes != null ? MediaTypes.ToList() : new List<int>(),
                    SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID,
                    GroupID,
                    Platform.ToString(),
                    SiteHelper.GetClientIP(),
                    PageSize,
                    PageIndex,
                    PicSize)
                {
                    DeviceId = DeviceUDID,
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    Culture = Language,
                    SiteGuid = SiteGuid,
                    DomainId = DomainID
                };
                return m_oCatalogExternalSearchLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }
    }
}

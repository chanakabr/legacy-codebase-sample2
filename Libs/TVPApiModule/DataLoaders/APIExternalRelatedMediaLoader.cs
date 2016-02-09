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
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace TVPApi
{
    public class APIExternalRelatedMediaLoader : TVPPro.SiteManager.DataLoaders.ExternalRelatedMoviesLoader
    {
        private TVPApiModule.CatalogLoaders.APIExternalRelatedMediaLoader m_oCatalogExternalRelatedLoader;
        private bool m_bShouldUseCache;

        public APIExternalRelatedMediaLoader(long mediaID, string freeParam = null)
            : base(mediaID, string.Empty, string.Empty, freeParam)
        {
        }

        public APIExternalRelatedMediaLoader(long mediaID, string userName, string pass, string freeParam = null)
            : base(mediaID, userName, pass, freeParam)
        {

        }

        #region params
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

        public List<string> With { get; set; }
        #endregion

        public override List<BaseObject> Execute()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oCatalogExternalRelatedLoader = new TVPApiModule.CatalogLoaders.APIExternalRelatedMediaLoader(
                    (int)MediaID,
                    MediaTypes != null ? MediaTypes.ToList() : new List<int>(),
                    SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID,
                    GroupID,
                    Platform.ToString(),
                    SiteHelper.GetClientIP(),
                    PageSize,
                    PageIndex,
                    PicSize, FreeParam)
                {
                    DeviceId = DeviceUDID,
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    Culture = Language,
                    SiteGuid = SiteGuid,
                    DomainId = DomainID,
                    UtcOffset = UtcOffset
                };

                List<BaseObject> ret = m_oCatalogExternalRelatedLoader.Execute() as List<BaseObject>;

                this.RequestId = m_oCatalogExternalRelatedLoader.RequestId;
                this.Status = m_oCatalogExternalRelatedLoader.Status;

                return ret;
            }
            else
            {
                return base.Execute();
            }
        }
    }
}

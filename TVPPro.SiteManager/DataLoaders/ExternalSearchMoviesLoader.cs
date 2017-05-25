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
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class ExternalSearchMoviesLoader : TVMAdapter<List<BaseObject>>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private ExternalSearchMediaLoader m_oCatalogExternalSearchLoader;
        private bool m_bShouldUseCache;

        #region Properties
        public string Query
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "sQuery", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "sQuery", value);
            }
        }

        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGuid", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGuid", value);
            }
        }

        public bool WithInfo
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "WithInfo", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "WithInfo", value);
            }
        }

        protected string TvmUser
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmUser", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmUser", value);
            }

        }
        protected string TvmPass
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "TvmPass", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "TvmPass", value);
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

        public Enums.ePlatform Platform
        {
            get
            {
                return Parameters.GetParameter<Enums.ePlatform>(eParameterType.Retrieve, "Platform", Enums.ePlatform.Unknown);
            }
            set
            {
                Parameters.SetParameter<Enums.ePlatform>(eParameterType.Retrieve, "Platform", value);
            }
        }

        public string PicSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PicSize", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "PicSize", value);
            }
        }

        public bool IsPosterPic
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", true);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", value);
            }
        }

        public string RequestId
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "DomainID", "");
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "DomainID", value);
            }
        }

        public Status Status
        {
            get
            {
                return Parameters.GetParameter<Status>(eParameterType.Retrieve, "ResponseStatus", null);
            }
            set
            {
                Parameters.SetParameter<Status>(eParameterType.Retrieve, "ResponseStatus", value);
            }
        }

        public int TotalResults
        {
            get
            {
                return Parameters.GetParameter<int>(eParameterType.Retrieve, "TotalResults", 0);
            }
            set
            {
                Parameters.SetParameter<int>(eParameterType.Retrieve, "TotalResults", value);
            }
        }
        #endregion

        public ExternalSearchMoviesLoader(string query)
            : this(query, string.Empty, string.Empty)
        {
        }

        public ExternalSearchMoviesLoader(string query, string userName, string pass)
        {
            TvmUser = userName;
            TvmPass = pass;
            Query = query;
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override List<BaseObject> Execute()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oCatalogExternalSearchLoader = new ExternalSearchMediaLoader(Query, new List<int>(), TvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex)
                {
                    DeviceId = DeviceUDID,
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    SiteGuid = SiteGuid
                };
                
                UnifiedSearchResponse ret = m_oCatalogExternalSearchLoader.Execute() as UnifiedSearchResponse;
                                
                this.RequestId = m_oCatalogExternalSearchLoader.RequestId;
                this.Status = m_oCatalogExternalSearchLoader.Status;
                this.TotalResults = ret.m_nTotalItems;

                return ret.m_lObj;
            }
            else
            {
                return null;
            }
        }

        protected override IProtocol CreateProtocol()
        {
            Tvinci.Data.TVMDataLoader.Protocols.SearchRelated.SearchRelated protocol = new Tvinci.Data.TVMDataLoader.Protocols.SearchRelated.SearchRelated();
            //protocol.root.request.media.id = MediaID.ToString();

            protocol.root.request.channel.start_index = "0";
            protocol.root.request.channel.number_of_items = PageSize.ToString();
            protocol.root.flashvars.pic_size1 = PicSize;
            protocol.root.request.@params.with_info = "true";
            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();

            //if (IsPosterPic)
            //{
            //    protocol.root.flashvars.pic_size1_format = "POSTER";
            //    protocol.root.flashvars.pic_size1_quality = "HIGH";
            //}

            protocol.root.flashvars.device_udid = DeviceUDID;
            protocol.root.flashvars.platform = (int)Platform;


            if (WithInfo)
            {
                string[] arrMetas = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    protocol.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }
            //if (WithInfo)
            //{
            //    protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Description (Short)" });
            //}

            return protocol;
        }

        protected override List<BaseObject> PreCacheHandling(object retrievedData)
        {
            List<BaseObject> result = new List<BaseObject>();

            SearchRelated data = (SearchRelated)retrievedData;
            return result;
        }

        public override bool TryGetItemsCount(out long count)
        {
            if (m_bShouldUseCache)
            {
                return m_oCatalogExternalSearchLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{7E01D5C6-2A69-4dd6-8415-A49CE3BB4FB0}"); }
        }
    }
}

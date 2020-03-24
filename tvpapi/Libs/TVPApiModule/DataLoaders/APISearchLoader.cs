using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader.Protocols.Search;
using Tvinci.Data.DataLoader;
using TVPApiModule.CatalogLoaders;
using TVPPro.SiteManager.DataEntities;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Manager;
using TVPApiModule.Helper;
using TVPApiModule.Manager;
using ApiObjects.SearchObjects;
using ConfigurationManager;

namespace TVPApi
{
    public class APISearchLoader : TVPPro.SiteManager.DataLoaders.SearchMediaLoader
    {
        
        private APISearchMediaLoader m_oCatalogSearchLoader;

        public APISearchLoader(string TVMUser, string TVMPass)
            : base(TVMUser, TVMPass)
        {

        }

        public APISearchLoader(string TVMUser, string TVMPass, Dictionary<string, string> tags)
            : base(TVMUser, TVMPass, tags)
        {

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

        public string Country
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Country", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Country", value);
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

        public TVPApi.OrderBy OrderBy
        {
            get
            {
                return Parameters.GetParameter<TVPApi.OrderBy>(eParameterType.Retrieve, "OrderBy", TVPApi.OrderBy.None);
            }
            set
            {
                Parameters.SetParameter<TVPApi.OrderBy>(eParameterType.Retrieve, "OrderBy", value);
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

        public bool IgnoreFilter
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "IgnoreFilter", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "IgnoreFilter", value);
            }
        }

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oCatalogSearchLoader = new APISearchMediaLoader(SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID, GroupID, Platform.ToString(), SiteHelper.GetClientIP(), PageSize, PageIndex, PictureSize, Name)
                {
                    And = CutType == eCutType.And ? true : false,
                    Exact = ExactSearch,
                    Metas = CatalogHelper.GetCatalogMetasTags(dictMetas),
                    Tags = CatalogHelper.GetCatalogMetasTags(m_dictTags),
                    MediaTypes = MediaType.HasValue ? new List<int>() { MediaType.Value } : null,
                    Description = Name,
                    Name = Name,
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    UseFinalDate = bool.Parse(UseFinalEndDate),
                    //UseStartDate = bool.Parse(GetFutureStartDate),
                    DeviceId = DeviceUDID,
                    Culture = Language,
                    SiteGuid = SiteGuid,
                    DomainId = DomainID
                };
                if (OrderBy != TVPApi.OrderBy.None)
                {
                    m_oCatalogSearchLoader.OrderBy = APICatalogHelper.GetCatalogOrderBy(OrderBy);
                    // XXX: For specific date sorting, make this by Descending
                    if (m_oCatalogSearchLoader.OrderBy == ApiObjects.SearchObjects.OrderBy.START_DATE || m_oCatalogSearchLoader.OrderBy == ApiObjects.SearchObjects.OrderBy.CREATE_DATE)
                        m_oCatalogSearchLoader.OrderDir = OrderDir.DESC;
                    else
                        m_oCatalogSearchLoader.OrderDir = CatalogHelper.GetCatalogOrderDirection(OrderDirection);
                    m_oCatalogSearchLoader.OrderMetaMame = OrderByMeta;
                }
                return m_oCatalogSearchLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        public override bool TryGetItemsCount(out long count)
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_oCatalogSearchLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }

        protected override void PreExecute()
        {
            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL))
                (base.GetProvider() as Tvinci.Data.TVMDataLoader.TVMProvider).TVMAltURL = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.Servers.AlternativeServer.URL;

            base.PreExecute();

            this.FlashVarsFileFormat = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).FileFormat;
            this.FlashVarsSubFileFormat = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).SubFileFormat;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SearchProtocol protocol = new SearchProtocol();

            protocol.root.request.search_data.channel.start_index = (PageIndex * PageSize).ToString();
            protocol.root.request.search_data.channel.media_count = PageSize.ToString();
            protocol.root.flashvars.file_format = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).FileFormat;
            protocol.root.flashvars.file_quality = Tvinci.Data.TVMDataLoader.Protocols.Search.file_quality.high;
            protocol.root.flashvars.use_final_end_date = UseFinalEndDate;
            protocol.root.flashvars.use_start_date = ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.FutureAssets.UseStartDate;

            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;

            protocol.root.flashvars.no_file_url = ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.EncryptMediaFileURL;

            if (!string.IsNullOrEmpty(GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).SubFileFormat))
            {
                protocol.root.flashvars.sub_file_format = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).SubFileFormat;
            }

            //if (string.IsNullOrEmpty(PictureSize))
            //    throw new Exception("Picture size must be given");
            protocol.root.flashvars.pic_size1 = PictureSize;

            protocol.root.flashvars.lang = Language;

            if (IsPosterPic)
            {
                protocol.root.flashvars.pic_size1_format = "POSTER";
                protocol.root.flashvars.pic_size1_quality = "HIGH";
            }
            protocol.root.flashvars.device_udid = DeviceUDID;

            //Handle response info stuct
            protocol.root.request.@params.with_info = WithInfo.ToString();
            if (WithInfo)
            {
                protocol.root.request.@params.info_struct.statistics = true;
                protocol.root.request.@params.info_struct.personal = true;
                protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
                protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();
                protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();

                string[] MediaInfoStructMetaNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                string[] MediaInfoStructTagNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

                foreach (string meta in MediaInfoStructMetaNames)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
                }

                foreach (string tagName in MediaInfoStructTagNames)
                {
                    protocol.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
                }
            }

            //Handle request cut values
            protocol.root.request.search_data.cut_values.exact = ExactSearch;
            protocol.root.request.search_data.cut_with = CutType.ToString().ToLower();

            if (MediaType.HasValue)
                protocol.root.request.search_data.cut_values.type.value = (MediaType.Value).ToString();

            if (!SideSearch && m_dictTags == null && dictMetas == null)
            {
                protocol.root.request.search_data.cut_values.name.value = Name;

                string[] MetaNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.SearchValues.Metadata.ToString().Split(new Char[] { ';' });
                string[] TagNames = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.SearchValues.Tags.ToString().Split(new Char[] { ';' });

                foreach (string meta in MetaNames)
                {
                    protocol.root.request.search_data.cut_values.metaCollection.Add(new cut_valuesmeta { name = meta, value = Name });
                }

                foreach (string tagName in TagNames)
                {
                    protocol.root.request.search_data.cut_values.tags.Add(new cut_valuestagstag_type { name = tagName, value = Name });
                }
            }
            else
            {
                if (m_dictTags != null && m_dictTags.Count > 0)
                {
                    if (!string.IsNullOrEmpty(Name))
                        protocol.root.request.search_data.cut_values.name.value = Name;

                    foreach (string key in m_dictTags.Keys)
                    {
                        if (!string.IsNullOrEmpty(m_dictTags[key]))
                        {
                            protocol.root.request.search_data.cut_values.tags.Add(new cut_valuestagstag_type { name = key, value = m_dictTags[key] });
                        }
                    }
                }
                if (dictMetas != null && dictMetas.Count > 0) // Hanble with search metas with multi values
                {
                    if (!string.IsNullOrEmpty(Name))
                        protocol.root.request.search_data.cut_values.name.value = Name;

                    foreach (string key in dictMetas.Keys)
                    {
                        if (!string.IsNullOrEmpty(dictMetas[key]))
                        {
                            string[] CutMetaValues = dictMetas[key].Split(new Char[] { ';' });
                            foreach (string MetaItem in CutMetaValues)
                            {
                                protocol.root.request.search_data.cut_values.metaCollection.Add(new cut_valuesmeta { name = key, value = MetaItem });
                            }
                        }
                    }
                }
            }

            // Add tags to filter by ip/country and Device using site configuration
            if (ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.LocaleSearchFilter.SupportFeature && !IgnoreFilter)
            {
                // By Country
                string sCountryTagName = ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.LocaleSearchFilter.CountryByTagName;
                protocol.root.request.search_data.cut_values.tags.Add(new cut_valuestagstag_type { name = sCountryTagName, value = Country, cut_with = eCutType.And.ToString() });

                // By Platform
                string sPlatformTagName = ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.LocaleSearchFilter.DeviceByTagName;
                protocol.root.request.search_data.cut_values.tags.Add(new cut_valuestagstag_type { name = sPlatformTagName, value = Platform.ToString(), cut_with = eCutType.And.ToString() });
            }

            switch (OrderBy)
            {
                case TVPApi.OrderBy.ABC:
                    protocol.root.request.search_data.order_values.name.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case TVPApi.OrderBy.Added:
                    protocol.root.request.search_data.order_values.date.order_dir = eOrderDirection.Desc.ToString();//OrderDirection.ToString().ToLower();
                    break;
                case TVPApi.OrderBy.Views:
                    protocol.root.request.search_data.order_values.views.order_dir = eOrderDirection.Desc.ToString();//OrderDirection.ToString().ToLower();
                    break;
                case TVPApi.OrderBy.Rating:
                    protocol.root.request.search_data.order_values.rate.order_dir = eOrderDirection.Desc.ToString();//OrderDirection.ToString().ToLower();
                    break;
                case TVPApi.OrderBy.None:
                    protocol.root.request.search_data.order_values.name.order_dir = OrderDirection.ToString().ToLower();
                    break;
                default:
                    throw new Exception("Unknown order by value");
            }

            if (!string.IsNullOrEmpty(OrderByMeta))
            {
                protocol.root.request.search_data.order_values.meta.name = OrderByMeta.ToString();
                protocol.root.request.search_data.order_values.meta.order_dir = eOrderDirection.Asc.ToString();
            }

            return protocol;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{092F417F-A6F2-4558-AB90-8CD519DE5F1B}"); }
        }
    }
}

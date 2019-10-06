using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataLoaders;
using TVPApi;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SubscriptionMedia;
using System.Configuration;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.DataEntities;
using TVPApiModule.Manager;
using ConfigurationManager;

namespace TVPApiModule.DataLoaders
{
    public class APITVMSubscriptionMediaLoader : TVMSubscriptionMediaLoader
    {        
        private long m_BaseID;
        private TVPApiModule.CatalogLoaders.APISubscriptionMediaLoader m_oSubscriptionMediaLoader;
        

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

        public APITVMSubscriptionMediaLoader(long BaseID)
            : base(BaseID)
        {
            m_BaseID = BaseID;
        }

        public APITVMSubscriptionMediaLoader(string TVMUser, string TVMPass, long BaseID) : base(TVMUser, TVMPass, BaseID)
        {
            TvmUser = TVMUser;
            TvmPass = TVMPass;
            m_BaseID = BaseID;
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oSubscriptionMediaLoader = new TVPApiModule.CatalogLoaders.APISubscriptionMediaLoader(
                    (int)BaseID,
                    SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID, 
                    GroupID,
                    Platform.ToString(),
                    SiteHelper.GetClientIP(), 
                    PageSize, 
                    PageIndex, 
                    PicSize)
                {
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    Culture = Language,
                    MediaTypes = MediaType.HasValue ? new List<int>() { MediaType.Value } : null,
                    SiteGuid = SiteGuid,
                    DomainId = DomainID
                };
                return m_oSubscriptionMediaLoader.Execute() as dsItemInfo;
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
                return m_oSubscriptionMediaLoader.TryGetItemsCount(out count);
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
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SubscriptionMedia result = new SubscriptionMedia();

            subscription sub = new subscription();
            sub.id = this.m_BaseID;
            sub.number_of_items = PageSize.ToString();
            sub.start_index = (PageIndex * PageSize).ToString();
            result.root.request.subscription = sub;

            result.root.flashvars.player_un = TvmUser;
            result.root.flashvars.player_pass = TvmPass;

            result.root.flashvars.pic_size1 = PicSize;
            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();

            if (WithInfo)
            {
                string[] arrMetas = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    result.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{A0E21BD5-5922-4B40-BE18-42580A70589E}"); }
        }
    }
}

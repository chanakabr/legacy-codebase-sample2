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
using ConfigurationManager;

namespace TVPApi
{
    public class APIRelatedMediaLoader : TVPPro.SiteManager.DataLoaders.RelatedMoviesLoader
    {

        private TVPApiModule.CatalogLoaders.APIRelatedMediaLoader m_oCatalogRelatedLoader;
        

        public APIRelatedMediaLoader(long mediaID)
            : this(mediaID, string.Empty, string.Empty)
        {
        }

        public APIRelatedMediaLoader(long mediaID, string userName, string pass)
            : base(mediaID, userName, pass)
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

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oCatalogRelatedLoader = new TVPApiModule.CatalogLoaders.APIRelatedMediaLoader(
                    (int)MediaID, 
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
                return m_oCatalogRelatedLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        public override bool TryGetItemsCount(out long count)
        {
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_oCatalogRelatedLoader.TryGetItemsCount(out count);
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

        protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            count = 0;

            if (retrievedData == null)
                return false;

            SearchRelated result = retrievedData as SearchRelated;

            if (result.response.channel.media_count == null)
                return false;

            count = long.Parse(result.response.channel.media_count);
            if (count != 0)
            {
                count++;
            }
            return true;
        }

        protected override IProtocol CreateProtocol()
        {
            SearchRelated protocol = new SearchRelated();
            protocol.root.request.media.id = MediaID.ToString();

            protocol.root.request.channel.start_index = PageIndex.ToString();
            protocol.root.request.channel.number_of_items = PageSize.ToString();

            if (MediaTypes != null)
                protocol.root.request.media.media_types = string.Join(";", MediaTypes.Select(x => x.ToString()).ToArray());

            protocol.root.flashvars.pic_size1 = PicSize;
            protocol.root.request.@params.with_info = "true";
            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;
            protocol.root.flashvars.file_format = GroupsManager.GetGroup(GroupID).GetFlashVars(Platform).FileFormat;
            protocol.root.flashvars.file_quality = file_quality.high;
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();
            protocol.root.flashvars.device_udid = DeviceUDID;

            if (IsPosterPic)
            {
                protocol.root.flashvars.pic_size1_format = "POSTER";
                protocol.root.flashvars.pic_size1_quality = "HIGH";
            }

            if (WithInfo)
            {
                string[] arrMetas = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = ConfigManager.GetInstance().GetConfig(GroupID, Platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
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

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{F84E2DE7-317B-4022-95B3-A5C4AB6F699E}"); }
        }
    }
}

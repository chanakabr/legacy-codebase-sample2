using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsMedia;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Context;
using System.Data;
using System.Configuration;
using TVPApiModule.CatalogLoaders;
using TVPPro.SiteManager.Helper;
using TVPApiModule.Manager;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;

namespace TVPApi
{
    public class APIChannelLoader : TVPPro.SiteManager.DataLoaders.TVMChannelLoader
    {
        private bool m_bShouldUseCache;
        private APIChannelMediaLoader m_oCatalogChannelLoader;

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

        private string TvmUser
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
        private string TvmPass
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

        public List<KeyValue> TagsMetas { get; set; }
        public CutWith CutWith { get; set; }

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

        public override bool ShouldExtractItemsCountInSource
        {
            get
            {
                return true;
            }
        }

        protected override bool TryGetItemsCountInSource(object retrievedData, out long count)
        {
            count = 0;

            if (retrievedData == null)
                return false;

            ChannelsMedia result = retrievedData as ChannelsMedia;

            if (result.response.channelCollection.Count == 0)
            {
                count = 0;
                return true;
            }
            count = long.Parse(result.response.channelCollection[0].media_count);

            return true;
        }

        public APIChannelLoader(long channelID, string picSize)
            : base(string.Empty, string.Empty, channelID, picSize)
        {
            // Do nothing.
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oCatalogChannelLoader = new APIChannelMediaLoader((int)ChannelID, SiteMapManager.GetInstance.GetPageData(GroupID, Platform).GetTVMAccountByUser(TvmUser).BaseGroupID, GroupID, Platform.ToString(), SiteHelper.GetClientIP(), PageSize, PageIndex, OrderObj, PicSize, TagsMetas, CutWith)
                {
                    Culture = Language,
                    DeviceId = DeviceUDID,
                    Platform = Platform.ToString(),
                    OnlyActiveMedia = true,
                    UseStartDate = bool.Parse(GetFutureStartDate),
                    SiteGuid = SiteGuid,
                    DomainId = DomainID
                };

                m_oCatalogChannelLoader.OrderObj = OrderObj == null ? new OrderObj() { m_eOrderDir = OrderDir.DESC, m_eOrderBy = CatalogHelper.GetCatalogOrderBy(OrderBy) } : OrderObj;

                return m_oCatalogChannelLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        public override bool TryGetItemsCount(out long count)
        {
            if (m_bShouldUseCache)
            {
                return m_oCatalogChannelLoader.TryGetItemsCount(out count);
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

        public APIChannelLoader(string TVMUser, string TVMPass, long channelID, string picSize)
            : base(TVMUser, TVMPass, channelID, picSize)
        {
            TvmUser = TVMUser;
            TvmPass = TVMPass;

            //if (string.IsNullOrEmpty(picSize))
            //{
            //    throw new Exception("Picture size is null or empty");
            //}

            PicSize = picSize;
            ChannelID = channelID;
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            ChannelsMedia result = new ChannelsMedia();

            channel newChannel = new channel();
            newChannel.id = ChannelID.ToString();
            //newChannel.number_of_items = PageSize;
            //newChannel.start_index = PageSize * PageIndex;

            //switch ((TVPApi.OrderBy)Enum.Parse(typeof(TVPApi.OrderBy), OrderBy.ToString()))
            //{
            //    case TVPApi.OrderBy.ABC:
            //        newChannel.order_values.name.order_dir = order_dir.asc;
            //        break;
            //    case TVPApi.OrderBy.Added:
            //        newChannel.order_values.date.order_dir = order_dir.desc;
            //        break;
            //    case TVPApi.OrderBy.Views:
            //        newChannel.order_values.views.order_dir = order_dir.desc;
            //        break;
            //    case TVPApi.OrderBy.Rating:
            //        newChannel.order_values.rate.order_dir = order_dir.desc;
            //        break;
            //    case TVPApi.OrderBy.None:
            //        break;
            //    default:
            //        throw new Exception("Unknown order by value");
            //}

            result.root.request.channelCollection.Add(newChannel);

            result.root.flashvars.player_un = TvmUser;
            result.root.flashvars.player_pass = TvmPass;

            result.root.flashvars.pic_size1 = PicSize;

            if (IsPosterPic)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            result.root.flashvars.no_file_url = ConfigManager.GetInstance().GetConfig(GroupID, Platform).SiteConfiguration.Data.Features.EncryptMediaFileURL;

            result.root.flashvars.file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.flashvars.device_udid = DeviceUDID;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();

            if (!string.IsNullOrEmpty(ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.SubFileFormat))
            {
                result.root.flashvars.sub_file_format = ConfigManager.GetInstance().GetConfig(GroupID, Platform).TechnichalConfiguration.Data.TVM.FlashVars.SubFileFormat;
            }

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

        protected override dsItemInfo FormatResults(dsItemInfo originalObject)
        {
            if (OrderBy == Enums.eOrderBy.None) return originalObject;

            dsItemInfo copyObject = originalObject.Copy() as dsItemInfo;

            if (copyObject.Item.Rows.Count > 0)
            {
                copyObject.Item.DefaultView.RowFilter = "";
                switch (OrderBy)
                {
                    case (Enums.eOrderBy.Added):
                        copyObject.Item.DefaultView.Sort = "CreationDate desc";
                        break;
                    case (Enums.eOrderBy.Rating):
                        copyObject.Item.DefaultView.Sort = "Rate desc";
                        break;
                    case (Enums.eOrderBy.Views):
                        copyObject.Item.DefaultView.Sort = "ViewCounter desc";
                        break;
                    default:
                        copyObject.Item.DefaultView.Sort = "Title asc";
                        break;
                }

                DataTable dtItemSorted = copyObject.Item.DefaultView.ToTable();
                copyObject.Item.Clear();
                copyObject.Item.Merge(dtItemSorted, true);

                //int iIndex = 0;
                //DataTable dtPaged = copyObject.Item.Clone();
                //foreach (DataRow row in copyObject.Item.Rows)
                //{
                //    if (iIndex >= PageIndex * PageSize && iIndex < (PageIndex + 1) * PageSize)
                //    {
                //        dtPaged.ImportRow(row);
                //    }
                //    iIndex++;
                //}

                //copyObject.Item.Clear();
                //copyObject.Item.Merge(dtPaged, true);

            }

            return copyObject;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{698C7873-35F1-4137-86E9-1C13C9CCD744}"); }
        }
    }
}

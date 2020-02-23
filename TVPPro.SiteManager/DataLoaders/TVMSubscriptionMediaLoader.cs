using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.SubscriptionMedia;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Context;
using System.Data;
using System.Configuration;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Manager;
using ConfigurationManager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMSubscriptionMediaLoader : TVMAdapter<dsItemInfo>
    {

        private SubscriptionMediaLoader m_oSubscriptionMediaLoader;
        

        public enum eOrderDirection
        {
            Asc,
            Desc
        }
        private string m_tvmUser;
        private string m_tvmPass;
        private long m_BaseID;

        #region properties
        public int? MediaType
        {
            get
            {
                return Parameters.GetParameter<int?>(eParameterType.Retrieve, "MediaType", null);
            }
            set
            {
                Parameters.SetParameter<int?>(eParameterType.Retrieve, "MediaType", value);
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

        public Enums.eOrderBy OrderBy
        {
            get
            {
                return Parameters.GetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "OrderBy", Enums.eOrderBy.Added);
            }
            set
            {
                Parameters.SetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "OrderBy", value);
            }
        }

        public string OrderByMeta
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "OrderByMeta", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "OrderByMeta", value);
            }
        }

        public eOrderDirection OrderDirection
        {
            get
            {
                return Parameters.GetParameter<eOrderDirection>(eParameterType.Retrieve, "OrderDirection", eOrderDirection.Asc);
            }
            set
            {
                Parameters.SetParameter<eOrderDirection>(eParameterType.Retrieve, "OrderDirection", value);
            }
        }

        public long BaseID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "BaseID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "BaseID", value);
            }
        }

        #endregion properties

        public TVMSubscriptionMediaLoader(long BaseID)
        {
            m_BaseID = BaseID;
        }

        public TVMSubscriptionMediaLoader(string TVMUser, string TVMPass, long BaseID)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;
            m_BaseID = BaseID;
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oSubscriptionMediaLoader = new SubscriptionMediaLoader((int)BaseID, m_tvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PicSize)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    OrderBy = CatalogHelper.GetCatalogOrderBy(OrderBy),
                    OrderDir = CatalogHelper.GetCatalogOrderDirection(OrderDirection),
                    OrderMetaMame = OrderByMeta,
                    MediaTypes = MediaType.HasValue ? new List<int>() { MediaType.Value } : null,
                    SiteGuid = SiteGuid
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
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_oSubscriptionMediaLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SubscriptionMedia result = new SubscriptionMedia();

            subscription sub = new subscription();
            sub.id = m_BaseID;
            sub.number_of_items = PageSize.ToString();
            sub.start_index = (PageIndex * PageSize).ToString();
            if (MediaType.HasValue)
            {
                sub.cut_values.type.value = MediaType.ToString();
            }

            switch (OrderBy)
            {
                case Enums.eOrderBy.None:
                    break;
                case Enums.eOrderBy.Added:
                    sub.order_values.date.order_dir = eOrderDirection.Desc.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Views:
                    sub.order_values.views.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Rating:
                    sub.order_values.rate.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.ABC:
                    sub.order_values.name.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Meta:
                    sub.order_values.meta.name = OrderByMeta.ToString();
                    sub.order_values.meta.order_dir = eOrderDirection.Asc.ToString().ToLower();
                    break;            
            }
            
            result.root.request.subscription = sub;

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.flashvars.pic_size1 = PicSize;
            result.root.flashvars.file_format = this.FlashVarsFileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();
            result.root.request.@params.info_struct.description.MakeSchemaCompliant();

            if (WithInfo)
            {
                string[] arrMetas = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                foreach (string metaName in arrMetas)
                {
                    result.root.request.@params.info_struct.metaCollection.Add(new meta() { name = metaName });
                }

                string[] arrTags = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
                foreach (string tagName in arrTags)
                {
                    result.root.request.@params.info_struct.tags.tag_typeCollection.Add(new tag_type() { name = tagName });
                }
            }

            return result;
        }


        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            SubscriptionMedia data = retrievedData as SubscriptionMedia;


            dsItemInfo result = new dsItemInfo();
            
            if (data.response.subscription.Count != 0)
            {
                responsesubscription subscription = data.response.subscription;

                if (subscription.channelCollection.Count != 0)
                {
                    foreach (Tvinci.Data.TVMDataLoader.Protocols.SubscriptionMedia.channel channel in subscription.channelCollection)
                    {
                        foreach (media media in channel)
                        {
                            if (string.IsNullOrEmpty(media.id))
                            {
                                // not a valid situation
                                continue;
                            }
                                dsItemInfo.ItemRow itemRow = result.Item.NewItemRow();
                                itemRow.ID = media.id;

                                itemRow.MediaType = media.type.value;
                                itemRow.MediaTypeID = media.type.id;
                                itemRow.Title = media.title;
                                itemRow.DescriptionShort = media.description.value;
                                itemRow.Rate = Convert.ToDouble(media.rating.avg);
                                itemRow.ImageLink = media.pic_size1;
                                itemRow.FileID = media.file_id;
                                itemRow.ViewCounter = Convert.ToInt32(media.views.count);
                                itemRow.Duration = media.duration;
                                itemRow.URL = media.url;
                                itemRow.Likes = media.like_counter.ToString();

                                if (WithInfo)
                                {
                                    DataHelper.CollectMetasInfo(ref result, media);

                                    DataHelper.CollectTagsInfo(ref result, media);

                                }
                                result.Item.AddItemRow(itemRow);
                            
                            
                        }
                    }
                }
            }

            return result;
        }

        protected override dsItemInfo FormatResults(dsItemInfo originalObject)
        {
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

            }

            return copyObject;
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

            SubscriptionMedia result = retrievedData as SubscriptionMedia;

            if (result.response.subscription.Count == 0)
                return false;

            if (result.response.subscription.media_count == null)
                return false;

            count = long.Parse(result.response.subscription.media_count);

            return true;
        }
        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{E35129CF-7718-4BE9-A748-482F61E6260A}"); }
        }
    }
}

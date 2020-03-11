using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.Search;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.DataLoader;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.Helper;
using TVPPro.Configuration.Technical;
using TVPPro.Configuration.Site;
using System.Configuration;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.Services;
using KLogMonitor;
using System.Reflection;
using Core.Catalog.Response;
using Core.Catalog.Request;
using Core.Catalog;
using ConfigurationManager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class ShowsEpisodeLoader : TVMAdapter<dsItemInfo>, ISupportPaging
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        #region Enums
        public enum eOrderDirection
        {
            Asc,
            Desc
        }
        #endregion Enums

        #region Members
        TVPPro.SiteManager.CatalogLoaders.SearchMediaLoader m_CatalogSearchLoader;

        private string m_ShowNameMeta = string.Empty;
        private string m_SeasonNumerMeta = string.Empty;
        private string m_EpisodeNumberMeta = string.Empty;
        private string m_tvmUser;
        private string m_tvmPass;
        #endregion Members

        #region properties
        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "SiteGuid", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "SiteGuid", value);
            }
        }

        public string ShowName
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "ShowName", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "ShowName", value);
            }
        }

        public long SeasonNumber
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "SeasonNumber", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "SeasonNumber", value);
            }
        }

        public string PictureSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PictureSize", null);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "PictureSize", value);
            }
        }

        public bool IsPosterPic
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "IsPosterPic", value);
            }
        }

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

        public Enums.eOrderBy OrderBy
        {
            get
            {
                return Parameters.GetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "Order", Enums.eOrderBy.None);
            }
            set
            {
                Parameters.SetParameter<Enums.eOrderBy>(eParameterType.Retrieve, "Order", value);
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

        public bool OnlyFirstSeasonEpisode
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "OnlyFirstSeasonEpisode", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "OnlyFirstSeasonEpisode", value);
            }
        }

        public string FirstSeasonEpisodeMeta
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "FirstSeasonEpisodeMeta", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "FirstSeasonEpisodeMeta", value);
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



        public string[] TagsList
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "TagsList", new string[] { });
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "TagsList", value);
            }
        }

        public string[] MetasList
        {
            get
            {
                return Parameters.GetParameter<string[]>(eParameterType.Retrieve, "MetasList", new string[] { });
            }
            set
            {
                Parameters.SetParameter<string[]>(eParameterType.Retrieve, "MetasList", value);
            }
        }

        public string GetFutureStartDate
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "GetFutureStartDate", "true");
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "GetFutureStartDate", value);
            }
        }
        #endregion properties

        #region Constractor
        public ShowsEpisodeLoader(string ShowNameMeta, string SeasonIDMeta, string EpisodeNumberMeta)
        {
            m_ShowNameMeta = ShowNameMeta;
            m_SeasonNumerMeta = SeasonIDMeta;
            m_EpisodeNumberMeta = EpisodeNumberMeta;
        }

        public ShowsEpisodeLoader(string TVMUser, string TVMPass, string ShowNameMeta, string SeasonIDMeta, string EpisodeNumberMeta)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;

            m_ShowNameMeta = ShowNameMeta;
            m_SeasonNumerMeta = SeasonIDMeta;
            m_EpisodeNumberMeta = EpisodeNumberMeta;
        }
        #endregion Constractor

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {

                m_CatalogSearchLoader = new TVPPro.SiteManager.CatalogLoaders.SearchMediaLoader(m_tvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PictureSize, null)
                {
                    And = true,
                    Exact = true,
                    MediaTypes = MediaType.HasValue ? new List<int>() { MediaType.Value } : null,
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    UseStartDate = bool.Parse(GetFutureStartDate),
                    SiteGuid = SiteGuid
                };
                m_CatalogSearchLoader.Tags = new List<KeyValue>();
                m_CatalogSearchLoader.Metas = new List<KeyValue>();
                m_CatalogSearchLoader.Tags.Add(new KeyValue() { m_sKey = m_ShowNameMeta, m_sValue = ShowName });
                if (SeasonNumber > 0)
                {
                    m_CatalogSearchLoader.Metas.Add(new KeyValue() { m_sKey = m_SeasonNumerMeta, m_sValue = SeasonNumber.ToString() });
                }
                if (OnlyFirstSeasonEpisode && !string.IsNullOrEmpty(FirstSeasonEpisodeMeta))
                {
                    m_CatalogSearchLoader.Metas.Add(new KeyValue() { m_sKey = FirstSeasonEpisodeMeta, m_sValue = "1" });
                }
                if (OrderBy != Enums.eOrderBy.None)
                {
                    m_CatalogSearchLoader.OrderBy = CatalogHelper.GetCatalogOrderBy(OrderBy);
                    m_CatalogSearchLoader.OrderDir = CatalogHelper.GetCatalogOrderDirection(OrderDirection);
                    m_CatalogSearchLoader.OrderMetaMame = OrderByMeta;
                }
                return m_CatalogSearchLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            SearchProtocol protocol = new SearchProtocol();

            protocol.root.request.search_data.channel.start_index = (PageIndex * PageSize).ToString();
            protocol.root.request.search_data.channel.media_count = PageSize.ToString();

            protocol.root.flashvars.player_un = m_tvmUser;
            protocol.root.flashvars.player_pass = m_tvmPass;
            protocol.root.flashvars.pic_size1 = PictureSize;

            if (IsPosterPic)
            {
                protocol.root.flashvars.pic_size1_format = "POSTER";
                protocol.root.flashvars.pic_size1_quality = "HIGH";
            }

            //Set the response info_struct
            protocol.root.flashvars.file_format = this.FlashVarsFileFormat;
            protocol.root.flashvars.file_quality = file_quality.high;
            protocol.root.request.@params.with_info = "true";
            protocol.root.request.@params.info_struct.statistics = true;
            //protocol.root.request.@params.info_struct.personal = false;
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();
            protocol.root.request.search_data.cut_with = "and";
            protocol.root.flashvars.use_start_date = GetFutureStartDate;

            protocol.root.request.search_data.cut_values.tags.tag_typeCollection.Add(new cut_valuestagstag_type { name = m_ShowNameMeta, value = ShowName });
            if (SeasonNumber > 0)
            {
                protocol.root.request.search_data.cut_values.metaCollection.Add(new cut_valuesmeta() { name = m_SeasonNumerMeta, value = SeasonNumber.ToString() });
            }

            if (OnlyFirstSeasonEpisode && !string.IsNullOrEmpty(FirstSeasonEpisodeMeta))
            {
                protocol.root.request.search_data.cut_values.metaCollection.Add(new cut_valuesmeta() { name = FirstSeasonEpisodeMeta, value = "1" });
            }

            protocol.root.request.search_data.cut_values.exact = true;

            protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = m_SeasonNumerMeta });
            protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = m_EpisodeNumberMeta });
            protocol.root.request.@params.info_struct.tags.Add(new tag_type { name = m_ShowNameMeta });

            foreach (string meta in MetasList)
            {
                protocol.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
            }

            foreach (string tagName in TagsList)
            {
                protocol.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
            }

            if (MediaType.HasValue)
                protocol.root.request.search_data.cut_values.type.value = (MediaType.Value).ToString();

            switch (OrderBy)
            {
                case Enums.eOrderBy.ABC:
                    protocol.root.request.search_data.order_values.name.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Added:
                    protocol.root.request.search_data.order_values.date.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Views:
                    protocol.root.request.search_data.order_values.views.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.Rating:
                    protocol.root.request.search_data.order_values.rate.order_dir = OrderDirection.ToString().ToLower();
                    break;
                case Enums.eOrderBy.None:
                    break;
                case Enums.eOrderBy.Meta:
                    protocol.root.request.search_data.order_values.meta.name = OrderByMeta.ToString();
                    protocol.root.request.search_data.order_values.meta.order_dir = eOrderDirection.Asc.ToString();
                    break;
                default:
                    throw new Exception("Unknown order by value");
            }

            return protocol;
        }

        //public override eCacheMode GetCacheMode()
        //{
        //    return eCacheMode.Never;
        //}

        protected override int CustomCacheDuration()
        {
            return 5;
        }

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            dsItemInfo ret = new dsItemInfo();

            SearchProtocol result = retrievedData as SearchProtocol;

            foreach (media resMedia in result.response.channel.mediaCollection)
            {
                dsItemInfo.ItemRow newRow = ret.Item.NewItemRow();

                newRow.ID = resMedia.id;
                newRow.Title = resMedia.title;
                newRow.ImageLink = resMedia.pic_size1;
                newRow.Rate = Convert.ToDouble(resMedia.rating.avg);
                newRow.MediaType = resMedia.type.value;
                newRow.ViewCounter = Convert.ToInt32(resMedia.views.count);
                newRow.MediaTypeID = resMedia.type.id;
                newRow.DescriptionShort = resMedia.description.value;

                //Add create date.
                try
                {
                    // For backward compatability
                    if (GetFutureStartDate.ToLower().Equals("true"))
                    {
                        string[] date = resMedia.date.Split('/');
                        newRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                    }
                    else
                    {
                        newRow.AddedDate = DateTime.ParseExact(resMedia.date, "dd/MM/yyyy HH:mm:ss", null);
                    }
                }
                catch (Exception ex)
                {
                    logger.Error("", ex);
                }

                ret.Item.AddItemRow(newRow);
                DataHelper.CollectMetasInfo(ref ret, resMedia);
                DataHelper.CollectTagsInfo(ref ret, resMedia);
            }

            return ret;
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
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_CatalogSearchLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = 0;

                if (retrievedData == null)
                    return false;

                SearchProtocol result = retrievedData as SearchProtocol;

                if (result.response.channel.media_count == null)
                    return false;

                count = long.Parse(result.response.channel.media_count);

                return true;
            }
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{B34519E1-5401-4c67-B742-E89462F4DE96}"); }
        }

        #region ISupportPaging method

        public bool TryGetItemsCount(out long count)
        {
            if (ApplicationConfiguration.Current.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                return m_CatalogSearchLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = 0;

                count = base.GetItemsInSource();

                return true;
            }
        }
        #endregion
    }
}

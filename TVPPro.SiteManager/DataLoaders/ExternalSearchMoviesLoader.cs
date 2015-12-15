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
    public class ExternalSearchMoviesLoader : TVMAdapter<dsItemInfo>
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

        public override dsItemInfo Execute()
        {
            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oCatalogExternalSearchLoader = new ExternalSearchMediaLoader(Query, new List<int>(), TvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PicSize)
                {
                    DeviceId = DeviceUDID,
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    Platform = Platform.ToString(),
                    SiteGuid = SiteGuid
                };
                return m_oCatalogExternalSearchLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
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

        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            dsItemInfo result = new dsItemInfo();

            SearchRelated data = (SearchRelated)retrievedData;

            if (data != null)
            {
                if (data.response != null && data.response.channel.mediaCollection.Count > 0)
                {
                    foreach (responsechannelmedia media in data.response.channel.mediaCollection)
                    {
                        // Info DataTable
                        dsItemInfo.ItemRow mediasRow = result.Item.NewItemRow();

                        mediasRow.ID = media.id.ToString();
                        mediasRow.Title = media.title;
                        mediasRow.ImageLink = media.pic_size1;
                        mediasRow.MediaType = media.type.value;
                        mediasRow.MediaTypeID = media.type.id;
                        mediasRow.DescriptionShort = !string.IsNullOrEmpty(media.description.value) ? media.description.value : string.Empty;
                        mediasRow.Rate = Convert.ToDouble(media.rating.avg);
                        mediasRow.ViewCounter = Convert.ToInt32(media.views.count);
                        mediasRow.URL = media.url;
                        mediasRow.Duration = media.duration;
                        mediasRow.FileID = media.file_id;
                        mediasRow.Likes = media.like_counter.ToString();

                        if (WithInfo)
                        {
                            DataHelper.CollectMetasInfo(ref result, media);

                            DataHelper.CollectTagsInfo(ref result, media);
                        }

                        //Add create date.
                        try
                        {
                            string[] date = media.date.Split('/');
                            mediasRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                        }
                        catch (Exception ex)
                        {
                            logger.Error("", ex);
                        }
                        //if (WithInfo)
                        //{
                        //    mediasRow.DescriptionShort = media.META5_STR_NAME.value;
                        //}

                        result.Item.AddItemRow(mediasRow);
                    }
                }
            }

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

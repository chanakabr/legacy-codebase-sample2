using System;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.MostViewd;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class MostViewedLoader : TVMAdapter<dsItemInfo>
    {
        #region members
        private string m_tvmUser;
        private string m_tvmPass;
        #endregion members

        #region Properties
        public string Duration
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "Duration", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Retrieve, "Duration", value);
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
        #endregion Properties

        public MostViewedLoader(string TVMUser, string TVMPass)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{A33A35A9-3EAC-4c79-AC95-8B667DC4ABF1}"); }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            Tvinci.Data.TVMDataLoader.Protocols.MostViewd.MostViewd protocol = new Tvinci.Data.TVMDataLoader.Protocols.MostViewd.MostViewd();

            protocol.root.request.channel.id = "";
            protocol.root.request.@params.hours = Duration;
            protocol.root.request.channel.start_index = "0";
            protocol.root.request.channel.number_of_items = PageSize.ToString();
            protocol.root.flashvars.pic_size1 = PicSize;
            protocol.root.request.@params.with_info = "true";
            protocol.root.request.@params.with_file_types = "false";
            protocol.root.request.@params.info_struct.statistics = true;
            //protocol.root.request.@params.info_struct.personal = false;
            protocol.root.request.@params.action = "first_play";
            protocol.root.request.@params.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();

            if (IsPosterPic)
            {
                protocol.root.flashvars.pic_size1_format = "POSTER";
                protocol.root.flashvars.pic_size1_quality = "HIGH";
            }

            if (WithInfo)
            {
                protocol.root.request.@params.info_struct.metaCollection.Add(new meta() { name = "Description (Short)" });

                string[] MetaNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
                string[] TagNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });

                foreach (string meta in MetaNames)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
                }

                foreach (string tagName in TagNames)
                {
                    protocol.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
                }
            }

            protocol.root.flashvars.player_un = m_tvmUser;//"ocontent_site_prod";
            protocol.root.flashvars.player_pass = m_tvmPass;//"ocontent_site_prod";
            protocol.root.flashvars.no_cache = "0";
            protocol.root.flashvars.file_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            protocol.root.flashvars.file_quality = file_quality.high;
            //protocol.root.flashvars.lang = "heb";

            return protocol;
        }


        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            dsItemInfo result = new dsItemInfo();

            MostViewd data = (MostViewd)retrievedData;

            if (data != null)
            {
                if (data.response != null && data.response.channel.period_viewsCollection.Count > 0)
                {
                    foreach (period_views item in data.response.channel.period_viewsCollection)
                    {
                        // Info DataTable
                        dsItemInfo.ItemRow mediasRow = result.Item.NewItemRow();

                        // Metas DateTable
                        DataHelper.CollectMetasInfo(ref result, item.media);

                        // Tags DataTable
                        DataHelper.CollectTagsInfo(ref result, item.media);

                        mediasRow.ID = item.media.id.ToString();
                        mediasRow.Title = item.media.title;
                        mediasRow.ImageLink = item.media.pic_size1;
                        mediasRow.MediaType = item.media.type.value;
                        mediasRow.MediaTypeID = item.media.type.id;

                        if (WithInfo)
                        {
                            mediasRow.DescriptionShort = item.media.META5_STR_NAME.value;
                        }

                        result.Item.AddItemRow(mediasRow);
                    }
                }
            }

            return result;
        }

        protected override int CustomCacheDuration()
        {
            return 60;
        }

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Custom;
        }
    }
}

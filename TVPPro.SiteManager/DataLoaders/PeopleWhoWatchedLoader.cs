using System;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.PeopleWhoWatched;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using System.Configuration;
using TVPPro.SiteManager.Manager;
using ConfigurationManager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class PeopleWhoWatchedLoader : TVMAdapter<dsItemInfo>
    {
        private TVPPro.SiteManager.CatalogLoaders.PeopleWhoWatchedLoader m_oPeopleWhoWatchedLoader;
        

        private string m_tvmUser;
        private string m_tvmPass;


        #region Properties
        public PeopleWhoWatchedLoader(string tvmUser, string tvmPass, long mediaID, string pictureSize)
        {
            MediaID = mediaID;
            PictureSize = pictureSize;
            m_tvmUser = tvmUser;
            m_tvmPass = tvmPass;
        }

        public PeopleWhoWatchedLoader(long mediaID, string pictureSize)
        {
            MediaID = mediaID;
            PictureSize = pictureSize;
        }



        public long MediaID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "MediaID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "MediaID", value);

            }
        }

        public string PictureSize
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Retrieve, "PictureSize", string.Empty);
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
        #endregion Properties

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oPeopleWhoWatchedLoader = new TVPPro.SiteManager.CatalogLoaders.PeopleWhoWatchedLoader((int)MediaID, 0, m_tvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PictureSize)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true, 
                    SiteGuid = SiteGuid
                };
                return m_oPeopleWhoWatchedLoader.Execute() as dsItemInfo;
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
                return m_oPeopleWhoWatchedLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            PeopleWhoWatched result = new PeopleWhoWatched();

            result.root.request.media.id = MediaID;

            result.root.flashvars.pic_size1 = PictureSize;

            result.root.request.@params.with_info = "true";


            // views / rating
            result.root.request.@params.info_struct.statistics = true;
            // Type
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();

            if (IsPosterPic)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            result.root.flashvars.file_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;
            result.root.request.@params.with_info = WithInfo.ToString();
            result.root.request.@params.info_struct.statistics = true;
            result.root.request.@params.info_struct.type.MakeSchemaCompliant();

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
            PeopleWhoWatched data = retrievedData as PeopleWhoWatched;

            if (data == null)
            {
                throw new Exception("");
            }

            dsItemInfo result = new dsItemInfo();
            

            if (data.response.channelCollection.Count != 0)
            {
                var channel = data.response.channelCollection[0];

                if (channel.mediaCollection.Count != 0)
                {
                    foreach (channelmedia media in channel.mediaCollection)
                    {
                        if (string.IsNullOrEmpty(media.id))
                        {
                            // not a valid situation
                            continue;
                        }

                        dsItemInfo.ItemRow itemRow = result.Item.NewItemRow();
                        itemRow.ID = media.id.ToString();
                        itemRow.MediaType = media.type.value;
                        itemRow.MediaTypeID = media.type.id;
                        itemRow.Title = media.title;
                        itemRow.ViewCounter = Convert.ToInt32(media.views.count);
                        itemRow.Rate = Convert.ToDouble(media.rating.avg);
                        itemRow.ImageLink = media.pic_size1;
                        itemRow.URL = media.url;
                        if (WithInfo)
                        {
                            DataHelper.CollectMetasInfo(ref result, media);

                            DataHelper.CollectTagsInfo(ref result, media);
                        }
                        result.Item.AddItemRow(itemRow);
                    }
                }
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{F6E73414-359D-4747-B66C-F5216CCAA492}"); }
        }

    }
}

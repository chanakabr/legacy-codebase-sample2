using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.TVMDataLoader;
using TVPPro.SiteManager.DataEntities;
using Tvinci.Data.TVMDataLoader.Protocols.PersonalRecommended;
using Tvinci.Data.DataLoader;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.Helper;
using TVPPro.Configuration.Technical;
using System.Configuration;
using TVPPro.SiteManager.Manager;
using ConfigurationManager;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class PersonalRecommendedLoader : TVMAdapter<dsItemInfo>
    {
        #region members
        private TVPPro.SiteManager.CatalogLoaders.PersonalRecommendedLoader m_oPersonalRecommendedLoader;
        

        private string m_tvmUser;
        private string m_tvmPass;
        #endregion members

        #region properties
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
        #endregion properties

        public PersonalRecommendedLoader(string TVMUser, string TVMPass)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;
        }

        public PersonalRecommendedLoader()
        {

        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (ApplicationConfiguration.TVPApiConfiguration.ShouldUseNewCache.Value)
            {
                m_oPersonalRecommendedLoader = new TVPPro.SiteManager.CatalogLoaders.PersonalRecommendedLoader(SiteGuid, m_tvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PicSize)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true
                };
                return m_oPersonalRecommendedLoader.Execute() as dsItemInfo;
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
                return m_oPersonalRecommendedLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            PersonalRecommended protocol = new PersonalRecommended();

            protocol.root.request.channel.start_index = PageIndex.ToString();
            protocol.root.request.channel.number_of_items = PageSize.ToString();
            protocol.root.request.channel.id = "";
            protocol.root.flashvars.no_cache = "0";
            protocol.root.flashvars.pic_size1 = PicSize;

            protocol.root.request.@params.with_info = WithInfo.ToString();

            protocol.root.request.@params.info_struct.statistics = false;
            //protocol.root.request.@params.info_struct.personal = true;
            protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.site_guid = SiteGuid;

            protocol.root.flashvars.player_un = m_tvmUser;
            protocol.root.flashvars.player_pass = m_tvmPass;
            
            protocol.root.flashvars.file_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            protocol.root.flashvars.file_quality = file_quality.high;
            protocol.root.flashvars.zip = "0";


            string[] MetaNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
            string[] TagNames = MediaConfiguration.Instance.Data.TVM.GalleryMediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });


            if (WithInfo)
            {
                foreach (string meta in MetaNames)
                {
                    protocol.root.request.@params.info_struct.metaCollection.Add(new meta { name = meta });
                }

                foreach (string tagName in TagNames)
                {
                    protocol.root.request.@params.info_struct.tags.Add(new tag_type { name = tagName });
                }
            }

            return protocol;
        }


        protected override dsItemInfo PreCacheHandling(object retrievedData)
        {
            PersonalRecommended resualt = (PersonalRecommended)retrievedData;

            dsItemInfo result = new dsItemInfo();

            foreach (media media in resualt.response.channel.mediaCollection)
            {
                if (string.IsNullOrEmpty(media.id))
                {
                    // not a valid situation
                    continue;
                }

                dsItemInfo.ItemRow itemRow = result.Item.NewItemRow();
                // Metas DateTable
                DataHelper.CollectMetasInfo(ref result, media);

                // Tags DataTable
                DataHelper.CollectTagsInfo(ref result, media);

                itemRow.ID = media.id;

                itemRow.MediaType = media.type.value;
                itemRow.MediaTypeID = media.type.id;
                itemRow.Title = media.title;
                itemRow.ImageLink = media.pic_size1;
                itemRow.DescriptionShort = media.description.value;
                DateTime tempVal = DateTime.MaxValue;
                if (DateTime.TryParse(media.date, out tempVal))
                {
                    itemRow.AddedDate = tempVal;
                }
                if (media.rating.avg != string.Empty && media.rating.avg != null)
                {
                    itemRow.Rate = int.Parse(media.rating.avg);
                }
                if (media.views.count != string.Empty && media.views.count  != null)
                {
                    itemRow.ViewCounter = int.Parse(media.views.count);    
                }
                

                result.Item.AddItemRow(itemRow);
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{667D57BD-94B9-4965-B8A1-D3C97C9DFE2B}"); }
        }

         public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }

        protected override bool ShouldStoreInCache(LoaderAdapterItem result)
        {
            return false;
        }
    }
}

using System;
using Tvinci.Data.DataLoader;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.PersonalLastWatched;
using TVPPro.Configuration.Media;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using System.Globalization;
using TVPPro.SiteManager.CatalogLoaders;
using System.Configuration;
using TVPPro.SiteManager.Manager;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class LastWatchedLoader : TVMAdapter<dsItemInfo>
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private PersonalLastWatchedLoader m_oPersonalLastWatchedLoader;
        private bool m_bShouldUseCache;

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

        public bool Statistics
        {
            get
            {
                return Parameters.GetParameter<bool>(eParameterType.Retrieve, "Statistics", false);
            }
            set
            {
                Parameters.SetParameter<bool>(eParameterType.Retrieve, "Statistics", value);
            }

        }

        public override eCacheMode GetCacheMode()
        {
            return eCacheMode.Never;
        }
        public LastWatchedLoader()
        {
        }

        public LastWatchedLoader(string tvmUn, string tvmPass)
        {
            TvmUser = tvmPass;
            TvmPass = tvmUn;
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

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {
            if (bool.TryParse(System.Configuration.ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oPersonalLastWatchedLoader = new PersonalLastWatchedLoader(SiteGuid, TvmUser, SiteHelper.GetClientIP(), PageSize, PageIndex, PicSize)
                {
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                };
                return m_oPersonalLastWatchedLoader.Execute() as dsItemInfo;
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
                return m_oPersonalLastWatchedLoader.TryGetItemsCount(out count);
            }
            else
            {
                count = base.GetItemsInSource();
                return true;
            }
        }
        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            PersonalLastWatched protocol = new PersonalLastWatched();
            protocol.root.request.channel.start_index = PageIndex.ToString();
            protocol.root.request.channel.number_of_items = PageSize.ToString();
            protocol.root.request.channel.id = "";
            protocol.root.flashvars.no_cache = "1";
            protocol.root.flashvars.pic_size1 = PicSize;

            protocol.root.flashvars.player_un = TvmUser;
            protocol.root.flashvars.player_pass = TvmPass;

            protocol.root.request.@params.with_info = WithInfo.ToString();

            protocol.root.request.@params.info_struct.statistics = Statistics;
            //protocol.root.request.@params.info_struct.personal = true;
            protocol.root.request.@params.info_struct.name.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.description.MakeSchemaCompliant();
            protocol.root.request.@params.info_struct.type.MakeSchemaCompliant();
            protocol.root.request.@params.site_guid = SiteGuid;



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
            PersonalLastWatched retProt = (PersonalLastWatched)retrievedData;

            dsItemInfo result = new dsItemInfo();

            foreach (media med in retProt.response.channel.mediaCollection)
            {
                if (string.IsNullOrEmpty(med.id))
                {
                    // not a valid situation
                    continue;
                }

                dsItemInfo.ItemRow itemRow = result.Item.NewItemRow();
                // Metas DateTable
                DataHelper.CollectMetasInfo(ref result, med);

                // Tags DataTable
                DataHelper.CollectTagsInfo(ref result, med);

                //itemRow.ID = med.id;

                //itemRow.MediaType = med.type.value;
                //itemRow.MediaTypeID = med.type.id;
                //itemRow.Title = med.title;
                //itemRow.ImageLink = med.pic_size1;
                //itemRow.Rate = med.rating.avg != null ? double.Parse(med.rating.avg) : 0;
                //itemRow.LastWatchedDeviceName = med.last_watched_device_name;

                //try
                //{
                //    string[] date = med.date.Split('/');
                //    itemRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                //}
                //catch
                //{ }

                itemRow.ID = med.id;
                itemRow.ItemType = med.type.value;
                itemRow.MediaTypeID = med.type.id;
                itemRow.MediaType = med.type.value;
                itemRow.FileFormat = med.file_format;
                itemRow.ViewCounter = Convert.ToInt32(med.views.count);
                itemRow.Name = med.title;
                itemRow.Title = med.title;
                itemRow.Brief = !string.IsNullOrEmpty(med.description.value) ? System.Web.HttpUtility.HtmlDecode(med.description.value).Replace(@"\", "/") : string.Empty;
                itemRow.DescriptionShort = !string.IsNullOrEmpty(med.description.value) ? med.description.value : string.Empty;
                itemRow.Rate = Convert.ToDouble(med.rating.avg);
                itemRow.FileID = med.file_id;
                itemRow.ImageLink = med.pic_size1;
                //itemRow.BrandingSmallImage = med.pic_size3;
                itemRow.Duration = med.duration;
                //itemRow.BrandingSpaceHight = med.pic_size2_bh;
                //itemRow.BrandingRecurring = med.pic_size2_br;
                //itemRow.BrandingBodyImage = med.pic_size2;
                itemRow.URL = med.url;
                itemRow.LastWatchedDeviceName = med.last_watched_device_name;
                itemRow.GeoBlock = med.block;
                //itemRow.Likes = med.like_counter.ToString();

                //Add create date.
                try
                {
                    //string[] date = med.date.Split('/');
                    //itemRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                    string[] date = med.last_watched_date.Split(' ')[0].Split('/');
                    string[] hour = med.last_watched_date.Split(' ')[1].Split(':');
                    itemRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]), int.Parse(hour[0]), int.Parse(hour[1]), 0);
                }
                catch (Exception ex)
                {
                    logger.Error("", ex);
                }


                try
                {
                    string[] date = med.last_watched_date.Split(' ')[0].Split('/');
                    string[] hour = med.last_watched_date.Split(' ')[1].Split(':');
                    itemRow.LastWatchedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]), int.Parse(hour[0]), int.Parse(hour[1]), 0);
                }
                catch (Exception ex)
                {
                    logger.Error("", ex);
                }

                result.Item.AddItemRow(itemRow);
            }

            return result;
        }

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{210CDCF1-A2EE-4ba2-A45E-F55AF2CD179B}"); }
        }
    }
}

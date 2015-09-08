using System;
using System.Data;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using Tvinci.Data.TVMDataLoader;
using Tvinci.Data.TVMDataLoader.Protocols.ChannelsMedia;
using TVPPro.Configuration.Media;
using TVPPro.Configuration.Technical;
using TVPPro.SiteManager.Context;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using TVPPro.Configuration.Site;
using System.Configuration;
using Tvinci.Data.Loaders;
using TVPPro.SiteManager.CatalogLoaders;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.Services;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.DataLoaders
{
    [Serializable]
    public class TVMChannelLoader : TVMAdapter<dsItemInfo>
    {
        private ChannelMediaLoader m_oCatalogChannelLoader;
        private bool m_bShouldUseCache;

        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private string m_tvmUser;
        private string m_tvmPass;

        #region Loader properties
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

        public long ChannelID
        {
            get
            {
                return Parameters.GetParameter<long>(eParameterType.Retrieve, "ChannelID", 0);
            }
            set
            {
                Parameters.SetParameter<long>(eParameterType.Retrieve, "ChannelID", value);
            }
        }

        public OrderObj OrderObj
        {
            get
            {
                return Parameters.GetParameter<OrderObj>(eParameterType.Retrieve, "OrderObj", null);
            }
            set
            {
                Parameters.SetParameter<OrderObj>(eParameterType.Retrieve, "OrderObj", value);
            }
        }

        //public Enums.eGalleryType GalleryType 
        //{
        //    get
        //    {
        //        return Parameters.GetParameter <Enums.eGalleryType>(eParameterType.Retrieve, "GalleryType", Enums.eGalleryType.Movie);
        //    }
        //    set
        //    {
        //        Parameters.SetParameter < Enums.eGalleryType>(eParameterType.Retrieve, "GalleryType", value) ;
        //    }
        //}

        public Enums.eOrderBy OrderBy
        {
            get
            {
                return Parameters.GetParameter<Enums.eOrderBy>(eParameterType.Filter, "OrderBy", Enums.eOrderBy.Added);
            }
            set
            {
                Parameters.SetParameter<Enums.eOrderBy>(eParameterType.Filter, "OrderBy", value);
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

        public string SiteGuid
        {
            get
            {
                return Parameters.GetParameter<string>(eParameterType.Filter, "SiteGuid", string.Empty);
            }
            set
            {
                Parameters.SetParameter<string>(eParameterType.Filter, "SiteGuid", value);
            }
        }
        #endregion

        public TVMChannelLoader(long channelID, string picSize)
            : this(string.Empty, string.Empty, channelID, picSize)
        {
            // Do nothing.
        }

        public TVMChannelLoader(string TVMUser, string TVMPass, long channelID, string picSize)
        {
            m_tvmUser = TVMUser;
            m_tvmPass = TVMPass;

            //if (string.IsNullOrEmpty(picSize))
            //{
            //    throw new Exception("Picture size is null or empty");
            //}

            PicSize = picSize;
            ChannelID = channelID;
        }

        public override object BCExecute(eExecuteBehaivor behaivor)
        {
            return Execute();
        }

        public override dsItemInfo Execute()
        {

            if (bool.TryParse(ConfigurationManager.AppSettings["ShouldUseNewCache"], out m_bShouldUseCache) && m_bShouldUseCache)
            {
                m_oCatalogChannelLoader = new ChannelMediaLoader((int)ChannelID, m_tvmUser, SiteHelper.GetClientIP(), PageSize, PageSize != 0 ? PageIndex / PageSize : PageIndex, PicSize, OrderObj)
                {
                    DeviceId = DeviceUDID,
                    Platform = Platform.ToString(),
                    Language = int.Parse(TechnicalManager.GetLanguageID().ToString()),
                    OnlyActiveMedia = true,
                    UseStartDate = bool.Parse(GetFutureStartDate),
                    SiteGuid = SiteGuid
                };
                return m_oCatalogChannelLoader.Execute() as dsItemInfo;
            }
            else
            {
                return base.Execute();
            }
        }

        protected override Tvinci.Data.TVMDataLoader.Protocols.IProtocol CreateProtocol()
        {
            ChannelsMedia result = new ChannelsMedia();

            channel newChannel = new channel();
            newChannel.id = int.Parse(ChannelID.ToString());
            newChannel.number_of_items = PageSize;
            newChannel.start_index = PageIndex;

            switch (OrderBy)
            {
                case Enums.eOrderBy.ABC:
                    newChannel.order_values.name.order_dir = order_dir.desc;
                    break;
                case Enums.eOrderBy.Added:
                    newChannel.order_values.date.order_dir = order_dir.desc;
                    break;
                case Enums.eOrderBy.Views:
                    newChannel.order_values.views.order_dir = order_dir.desc;
                    break;
                case Enums.eOrderBy.Rating:
                    newChannel.order_values.rate.order_dir = order_dir.desc;
                    break;
                case Enums.eOrderBy.None:
                    break;
                default:
                    throw new Exception("Unknown order by value");
            }

            result.root.request.channelCollection.Add(newChannel);

            result.root.flashvars.player_un = m_tvmUser;
            result.root.flashvars.player_pass = m_tvmPass;

            result.root.flashvars.pic_size1 = PicSize;

            if (IsPosterPic)
            {
                result.root.flashvars.pic_size1_format = "POSTER";
                result.root.flashvars.pic_size1_quality = "HIGH";
            }

            result.root.flashvars.use_start_date = GetFutureStartDate;
            result.root.flashvars.file_format = TechnicalConfiguration.Instance.Data.TVM.FlashVars.FileFormat;
            result.root.flashvars.file_quality = file_quality.high;

            result.root.flashvars.device_udid = DeviceUDID;
            result.root.flashvars.platform = (int)Platform;

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
            DateTime dtStart = DateTime.Now;

            bool handleSingleTypeOnly = string.IsNullOrEmpty(PicSize);
            ChannelsMedia data = retrievedData as ChannelsMedia;

            if (data == null)
            {
                throw new Exception("");
            }
            dsItemInfo result = new dsItemInfo();

            if (data.response.channelCollection.Count != 0)
            {
                responsechannel channel = data.response.channelCollection[0];

                if (channel.mediaCollection.Count != 0)
                {
                    foreach (media media in channel.mediaCollection)
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

                        //Add create date.
                        try
                        {
                            // For backward compatability
                            if (GetFutureStartDate.ToLower().Equals("true"))
                            {
                                string[] date = media.date.Split('/');
                                itemRow.AddedDate = new DateTime(int.Parse(date[2]), int.Parse(date[1]), int.Parse(date[0]));
                            }
                            else
                            {
                                itemRow.AddedDate = DateTime.ParseExact(media.date, "dd/MM/yyyy HH:mm:ss", null);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.Error("", ex);
                        }

                        // add sub file format info
                        if (media.inner_medias.Count > 0)
                        {
                            itemRow.SubFileID = media.inner_medias[0].file_id;
                            itemRow.SubFileFormat = media.inner_medias[0].file_format;
                            itemRow.SubDuration = media.inner_medias[0].duration;
                            itemRow.SubURL = media.inner_medias[0].url;

                            foreach (inner_mediasmedia file in media.inner_medias)
                            {
                                dsItemInfo.FilesRow rowFile = result.Files.NewFilesRow();
                                rowFile.ID = media.id;
                                rowFile.FileID = file.file_id;
                                rowFile.URL = file.url;
                                rowFile.Duration = file.duration;
                                rowFile.Format = file.orig_file_format;

                                result.Files.AddFilesRow(rowFile);
                            }
                        }

                        if (WithInfo)
                        {
                            DataHelper.CollectMetasInfo(ref result, media);

                            DataHelper.CollectTagsInfo(ref result, media);

                            /*dsItemInfo.TagsRow rowTag = result.Tags.AddTagsRow(media.id);

                            foreach (tags_collectionstag_type tagType in media.tags_collections)
                            {
                                String sTagType = tagType.name;
                                foreach (tag tagElement in tagType.tagCollection)
                                {
                                    if (!result.Tags.Columns.Contains(sTagType))
                                    {
                                        System.Data.DataColumn colTagName = result.Tags.Columns.Add(sTagType, typeof(string));

                                        rowTag[colTagName] = tagElement.name;
                                    }
                                    else
                                    {
                                        rowTag[sTagType] += (!String.IsNullOrEmpty(rowTag[sTagType].ToString())) ? string.Concat(rowTag[sTagType], "|", tagElement.name) : tagElement.name;
                                    }
                                }
                            }
                            */
                            //
                        }

                        //// Add external IDs
                        foreach (System.Reflection.PropertyInfo property in media.external_ids.GetType().GetProperties())
                        {
                            if (property.CanRead)
                            {
                                string sValue = property.GetValue(media.external_ids, null).ToString();
                                if (!string.IsNullOrEmpty(sValue))
                                {
                                    // add column if not exist
                                    if (!result.ExtIDs.Columns.Contains(property.Name))
                                        result.ExtIDs.Columns.Add(property.Name);

                                    dsItemInfo.ExtIDsRow rowExtID = result.ExtIDs.NewExtIDsRow();
                                    rowExtID[property.Name] = sValue;
                                    rowExtID["ID"] = media.id;
                                    result.ExtIDs.AddExtIDsRow(rowExtID);
                                }
                            }
                        }

                        result.Item.AddItemRow(itemRow);
                    }
                }

                dsItemInfo.ChannelRow channelRow = result.Channel.NewChannelRow();

                channelRow.ChannelId = channel.id;
                channelRow.Title = channel.title;
                channelRow.Description = channel.description;
                channelRow.EnableRssFeed = channel.rss;

                result.Channel.AddChannelRow(channelRow);
            }

            DateTime dtEnd = DateTime.Now;
            TimeSpan span = dtEnd - dtStart;



            logger.Info("Tag Reflaction - ChannelID: " + ChannelID + ", Tags: " + result.Tags.Rows.Count + ", Total Time: " + span.TotalMilliseconds.ToString() + "ms");

            return result;
        }

        //protected override dsItemInfo FormatResults(dsItemInfo originalObject)
        //{
        //    if (OrderBy == Enums.eOrderBy.None) return originalObject;

        //    dsItemInfo copyObject = originalObject.Copy() as dsItemInfo;

        //    if (copyObject.Item.Rows.Count > 0)
        //    {
        //        copyObject.Item.DefaultView.RowFilter = "";
        //        switch (OrderBy)
        //        {
        //            case (Enums.eOrderBy.Added):
        //                copyObject.Item.DefaultView.Sort = "AddedDate desc";
        //                break;
        //            case (Enums.eOrderBy.Rating):
        //                copyObject.Item.DefaultView.Sort = "Rate desc";
        //                break;
        //            case (Enums.eOrderBy.Views):
        //                copyObject.Item.DefaultView.Sort = "ViewCounter desc";
        //                break;
        //            default:
        //                copyObject.Item.DefaultView.Sort = "Title asc";
        //                break;
        //        }

        //        DataTable dtItemSorted = copyObject.Item.DefaultView.ToTable();
        //        copyObject.Item.Clear();
        //        copyObject.Item.Merge(dtItemSorted, true);

        //    }

        //    return copyObject;
        //}

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
                return false;

            if (result.response.channelCollection[0].media_count == null)
                return false;

            count = long.Parse(result.response.channelCollection[0].media_count);

            return true;
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

        protected override Guid UniqueIdentifier
        {
            get { return new Guid("{C90246A5-0A45-4f18-8762-37C29BCE35AD}"); }
        }
    }
}

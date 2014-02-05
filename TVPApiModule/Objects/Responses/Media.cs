using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using System.Configuration;
using TVPApiModule.Services;
using log4net;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.Manager;
using TVPApiModule.Context;
using TVPApiModule.Helper;

/// <summary>
/// Summary description for Media
/// </summary>
/// 

namespace TVPApiModule.Objects.Responses
{
    public class ExtIDPair
    {
        public string key { get; set; }
        public string value { get; set; }
    }

    public class File
    {
        public string file_id { get; set; }
        public string url { get; set; }
        public string duration { get; set; }
        public string format { get; set; }
        public AdvertisingProvider pre_provider { get; set; }
        public AdvertisingProvider post_provider { get; set; }
        public AdvertisingProvider break_provider { get; set; }
        public AdvertisingProvider overlay_provider { get; set; }
        public string[] break_points { get; set; }
        public string[] overlay_points { get; set; }
        public string co_guid { get; set; }
    }

    public class Picture
    {
        public string pic_size { get; set; }
        public string url { get; set; }
    }

    public class TagMetaPair
    {
        public string key { get; set; }
        public string value { get; set; }

        public TagMetaPair(string key, string value)
        {
            this.key = key;
            this.value = value;
        }
    }

    public class Media
    {
        #region Properties

        private readonly ILog logger = LogManager.GetLogger(typeof(Media));

        public string media_id;
        public string media_name;
        public string media_type_id;
        public string media_type_name;
        public double rating;
        public int view_counter;
        public string description;
        public DateTime creation_date;
        public DateTime? last_watch_date;
        public DateTime start_date;
        public DateTime catalog_start_date;
        public string pic_url;
        public string url;
        public string mediaWeb_link;
        public string duration;
        public string file_id;
        private List<TagMetaPair> m_tags;
        private List<TagMetaPair> m_metas;
        private List<File> m_files;
        private List<TagMetaPair> m_adParams;

        private List<Picture> m_pictures;

        private List<ExtIDPair> m_externalIDs;

        //public DynamicData mediaDynamicData;
        public string sub_duration;
        public string sub_file_format;
        public string sub_file_id;
        public string sub_url;
        public string geo_block;
        public long total_items;
        public int? like_counter;

        public List<TagMetaPair> tags
        {
            get
            {
                if (m_tags == null)
                {
                    m_tags = new List<TagMetaPair>();
                }
                return m_tags;
            }
        }

        public List<TagMetaPair> metas
        {
            get
            {
                if (m_metas == null)
                {
                    m_metas = new List<TagMetaPair>();
                }
                return m_metas;
            }
        }

        public List<TagMetaPair> advertising_parameters
        {
            get
            {
                if (m_adParams == null)
                {
                    m_adParams = new List<TagMetaPair>();
                }
                return m_adParams;
            }
        }

        public List<File> files
        {
            get
            {
                if (m_files == null)
                {
                    m_files = new List<File>();
                }
                return m_files;
            }
        }

        public List<Picture> pictures
        {
            get
            {
                if (m_pictures == null)
                {
                    m_pictures = new List<Picture>();
                }
                return m_pictures;
            }
        }

        public List<ExtIDPair> external_ids
        {
            get
            {
                if (m_externalIDs == null)
                {
                    m_externalIDs = new List<ExtIDPair>();
                }
                return m_externalIDs;
            }
        }
        #endregion

        #region Constructors
        public Media()
        {

        }

        public Media(MediaObj mediaObj, string picSize, long totalItems, int groupID, PlatformType platform)//, bool withDynamic)
        {
            InitMediaObj(mediaObj, picSize, totalItems, groupID, platform);//, withDynamic);
        }



        //public Media(dsItemInfo.ItemRow itemRow, InitializationObject initObj, int groupID, bool withDynamic)
        //{
        //    //InitMediaObj(itemRow, initObj, groupID, withDynamic, 0);
        //}

        //public Media(dsItemInfo.ItemRow itemRow, InitializationObject initObj, int groupID, bool withDynamic, long iMediaCount)
        //{
        //    //InitMediaObj(itemRow, initObj, groupID, withDynamic, iMediaCount);
        //}
        #endregion

        #region private functions

        private string GetMediaWebLink(int groupID, PlatformType platform)
        {
            string retVal = string.Empty;
            string baseUrl = ConfigurationManager.AppSettings[string.Format("{0}_BaseURL", groupID.ToString())];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                if (ConfigManager.GetInstance().GetConfig(groupID, platform).SiteConfiguration.Data.Features.FriendlyURL.SupportFeature)
                {
                    string sMediaName = media_name.Replace("/", "");

                    sMediaName = sMediaName.Replace(" ", "-");

                    sMediaName = HttpUtility.UrlEncode(sMediaName);

                    retVal = string.Format("{0}/{1}/{2}/{3}", baseUrl, media_type_name, sMediaName, media_id);
                }
                else
                {
                    retVal = string.Format("{0}/MediaPage.aspx?MediaID={1}&MediaType={2}", baseUrl, media_id, media_type_id);
                }
            }
            return retVal;
        }

        private void InitMediaObj(MediaObj mediaObj, string picSize, long totalItems, int groupID, PlatformType platform)//, bool withDynamic)
        {
            if (mediaObj != null)
            {
                media_id = mediaObj.m_nID.ToString();
                media_name = mediaObj.m_sName;
                media_type_id = mediaObj.m_oMediaType.m_nTypeID.ToString();
                media_type_name = mediaObj.m_oMediaType.m_sTypeName;
                rating = mediaObj.m_oRatingMedia.m_nRatingAvg;
                view_counter = mediaObj.m_oRatingMedia.m_nViwes;
                description = mediaObj.m_sDescription;
                creation_date = mediaObj.m_dCreationDate;
                last_watch_date = mediaObj.m_dLastWatchedDate;
                start_date = mediaObj.m_dStartDate;
                catalog_start_date = mediaObj.m_dCatalogStartDate;
                like_counter = mediaObj.m_nLikeCounter;

                totalItems = totalItems;

                if (!string.IsNullOrEmpty(picSize))
                    pic_url = (from pic in mediaObj.m_lPicture where pic.m_sSize.ToLower() == picSize.ToLower() select pic.m_sURL).FirstOrDefault();


                //media.GeoBlock = No data... 

                // Problems...
                //media.MediaDynamicData;

                //MediaWebLink
                mediaWeb_link = GetMediaWebLink(groupID, platform);

                //Files
                buildFiles(mediaObj.m_lFiles, mediaObj.m_lBranding, groupID, platform);

                //Metas & Tags
                BuildTagMetas(mediaObj.m_lMetas, mediaObj.m_lTags, groupID, platform);


                //ExternalIDs
                if (!string.IsNullOrEmpty(mediaObj.m_ExternalIDs))
                    external_ids.Add(new ExtIDPair() { key = "epg_id", value = mediaObj.m_ExternalIDs });

                //Pictures
                buildPictures(mediaObj.m_lPicture);
            }
        }


        //Fill properies according to media row
        //private void InitMediaObj(dsItemInfo.ItemRow row, InitializationObject initObj, int groupID, bool withDynamic, long iMediaCount)
        //{
        //    MediaID = row.ID;
        //    if (!row.IsMediaTypeIDNull())
        //    {
        //        MediaTypeID = row.MediaTypeID;
        //    }

        //    MediaName = row.Title;

        //    if (!row.IsMediaTypeNull())
        //    {
        //        MediaTypeName = row.MediaType;
        //    }
        //    if (!row.IsDescriptionShortNull())
        //    {
        //        Description = row.DescriptionShort;
        //    }

        //    if (!row.IsImageLinkNull())
        //    {
        //        PicURL = row.ImageLink;
        //    }
        //    if (!row.IsURLNull())
        //    {
        //        URL = row.URL;
        //    }

        //    if (!row.IsFileIDNull())
        //    {
        //        FileID = row.FileID;
        //    }

        //    if (!row.IsDurationNull())
        //    {
        //        Duration = row.Duration;
        //    }

        //    if (!row.IsRateNull())
        //    {
        //        Rating = row.Rate;
        //    }
        //    if (!row.IsViewCounterNull())
        //    {
        //        ViewCounter = row.ViewCounter;
        //    }
        //    if (!row.IsCreationDateNull())
        //    {
        //        CreationDate = row.CreationDate;
        //    }
        //    else
        //        CreationDate = new DateTime(1970, 1, 1);

        //    if (!row.IsStartDateNull())
        //    {
        //        StartDate = row.StartDate;
        //    }
        //    else
        //        StartDate = new DateTime(1970, 1, 1);

        //    if (!row.IsCatalogStartDateNull())
        //    {
        //        CatalogStartDate = row.CatalogStartDate;
        //    }
        //    else
        //        CatalogStartDate = new DateTime(1970, 1, 1);

        //    // add sub file foramt info
        //    if (!row.IsSubDurationNull())
        //    {
        //        SubDuration = row.SubDuration;
        //    }
        //    if (!row.IsSubFileFormatNull())
        //    {
        //        SubFileFormat = row.SubFileFormat;
        //    }
        //    if (!row.IsSubFileIDNull())
        //    {
        //        SubFileID = row.SubFileID;
        //    }
        //    if (!row.IsSubURLNull())
        //    {
        //        SubURL = row.SubURL;
        //    }
        //    if (!row.IsGeoBlockNull())
        //    {
        //        GeoBlock = row.GeoBlock;
        //    }
        //    if (!String.IsNullOrEmpty(row.Likes))
        //    {
        //        like_counter = Convert.ToInt32(row.Likes);
        //    }
        //    if (!row.IsLastWatchedDateNull())
        //    {
        //        LastWatchDate = row.LastWatchedDate;
        //    }
        //    MediaWebLink = GetMediaWebLink(groupID, initObj.Platform);

        //    BuildTagMetas(groupID, row, initObj.Platform);

        //    buildFiles(row);

        //    builtExternalIDs(row);

        //    buildPictures(row);

        //    buildPictures(row);

        //    if (withDynamic && initObj.Locale != null)
        //    {
        //        logger.InfoFormat("Start Media dynamic build GroupID:", groupID);

        //        BuildDynamicObj(initObj, groupID);
        //    }

        //    TotalItems = iMediaCount;
        //}


        //private void builtExternalIDs(dsItemInfo.ItemRow row)
        //{
        //    System.Data.DataRow[] rowExtIDs = row.GetChildRows("Item_ExtIDs");
        //    if (rowExtIDs != null && rowExtIDs.Length > 0)
        //    {
        //        foreach (System.Data.DataRow rowExt in rowExtIDs)
        //        {
        //            foreach (System.Data.DataColumn dc in rowExt.Table.Columns)
        //            {
        //                if (!dc.ColumnName.Equals("ID"))
        //                    ExternalIDs.Add(new ExtIDPair() { Key = dc.ColumnName, Value = rowExt[dc.ColumnName].ToString() });
        //            }
        //        }
        //    }
        //}



        private void buildFiles(List<FileMedia> mediaFiles, List<Branding> brandings, int groupID, PlatformType platform)
        {
            // Get file formats from configuration
            var techConfigFlashVars = ConfigManager.GetInstance().GetConfig(groupID, platform).TechnichalConfiguration.Data.TVM.FlashVars;
            string fileFormat = techConfigFlashVars.FileFormat;
            string subFileFormat = (techConfigFlashVars.SubFileFormat.Split(';')).FirstOrDefault();

            file_id = "0"; // default value

            if (mediaFiles != null && mediaFiles.Count > 0)
            {
                File mediaFile;
                foreach (FileMedia file in mediaFiles)
                {
                    mediaFile = new File();

                    mediaFile.file_id = file.m_nFileId.ToString();
                    mediaFile.url = file.m_sUrl;
                    mediaFile.duration = file.m_nDuration.ToString();
                    mediaFile.format = file.m_sFileFormat;
                    mediaFile.co_guid = file.m_sCoGUID;

                    if (file.m_sFileFormat.ToLower() == fileFormat.ToLower())
                    {
                        url = file.m_sUrl;
                        duration = file.m_nDuration.ToString();
                        file_id = file.m_nFileId.ToString();
                    }
                    if (file.m_sFileFormat.ToLower() == subFileFormat.ToLower())
                    {
                        sub_duration = file.m_nDuration.ToString();
                        subFileFormat = file.m_sFileFormat;
                        sub_file_id = file.m_nFileId.ToString();
                        sub_url = file.m_sUrl;
                    }

                    if (file.m_oPreProvider != null)
                        mediaFile.pre_provider = new AdvertisingProvider(file.m_oPreProvider.ProviderID, file.m_oPreProvider.ProviderName);

                    if (file.m_oPostProvider != null)
                        mediaFile.post_provider = new AdvertisingProvider(file.m_oPostProvider.ProviderID, file.m_oPostProvider.ProviderName);

                    if (file.m_oBreakProvider != null)
                    {
                        mediaFile.break_provider = new AdvertisingProvider(file.m_oBreakProvider.ProviderID, file.m_oBreakProvider.ProviderName);
                        if (!string.IsNullOrEmpty(file.m_sBreakpoints))
                            mediaFile.break_points = file.m_sBreakpoints.ToString().Split(';');
                    }

                    if (file.m_oOverlayProvider != null)
                    {
                        mediaFile.overlay_provider = new AdvertisingProvider(file.m_oOverlayProvider.ProviderID, file.m_oOverlayProvider.ProviderName);
                        if (!string.IsNullOrEmpty(file.m_sOverlaypoints))
                            mediaFile.overlay_points = file.m_sOverlaypoints.ToString().Split(';');
                    }

                    files.Add(mediaFile);
                }
            }

            if (brandings != null && brandings.Count > 0)
            {
                File mediaFile;
                foreach (Branding branding in brandings)
                {
                    mediaFile = new File();

                    mediaFile.file_id = branding.m_nFileId.ToString();
                    mediaFile.url = branding.m_sUrl;
                    mediaFile.duration = branding.m_nDuration.ToString();
                    mediaFile.format = branding.m_sFileFormat;
                    mediaFile.co_guid = branding.m_sCoGUID;

                    files.Add(mediaFile);
                }
            }
        }

        //private void buildFiles(dsItemInfo.ItemRow row)
        //{
        //    System.Data.DataRow[] rowFiles = row.GetChildRows("Item_files");
        //    if (rowFiles != null && rowFiles.Length > 0)
        //    {
        //        foreach (System.Data.DataRow rowFile in rowFiles)
        //        {
        //            File file = new File();
        //            file.FileID = rowFile["FileID"].ToString();
        //            file.URL = rowFile["URL"].ToString();
        //            file.Duration = rowFile["Duration"].ToString();
        //            file.Format = rowFile["Format"].ToString();

        //            int preProviderID = Convert.ToInt32(rowFile["PreProviderID"].ToString());

        //            if (preProviderID != 0)
        //                file.PreProvider = new AdvertisingProvider(preProviderID, rowFile["PostProviderName"].ToString());

        //            int postProviderID = Convert.ToInt32(rowFile["PostProviderID"].ToString());
                    
        //            if (postProviderID != 0)
        //                file.PostProvider = new AdvertisingProvider(postProviderID, rowFile["PostProviderName"].ToString());

        //            int breakProviderID = Convert.ToInt32(rowFile["BreakProviderID"].ToString());
                    
        //            if (breakProviderID != 0)
        //            {
        //                file.BreakProvider = new AdvertisingProvider(breakProviderID, rowFile["BreakProviderName"].ToString());

        //                if (rowFile["BreakPoints"] != null)
        //                    file.BreakPoints = rowFile["BreakPoints"].ToString().Split(';');
        //            }

        //            int overlayPoviderID = Convert.ToInt32(rowFile["OverlayProviderID"].ToString());

        //            if (overlayPoviderID != 0)
        //            {
        //                file.OverlayProvider = new AdvertisingProvider(overlayPoviderID, rowFile["OverlayProviderName"].ToString());

        //                if (rowFile["OverlayPoints"] != null)
        //                    file.OverlayPoints = rowFile["OverlayPoints"].ToString().Split(';');
        //            }

        //            Files.Add(file);
        //        }
        //    }
        //}

        private void buildPictures(List<Tvinci.Data.Loaders.TvinciPlatform.Catalog.Picture> mediaPictures)
        {
            if (mediaPictures != null)
            {
                Picture picture;
                foreach (Tvinci.Data.Loaders.TvinciPlatform.Catalog.Picture pic in mediaPictures)
                {
                    picture = new Picture();
                    picture.pic_size = pic.m_sSize;
                    picture.url = pic.m_sURL;
                    pictures.Add(picture);
                }
            }
        }


        //private void buildPictures(dsItemInfo.ItemRow row)
        //{
        //    System.Data.DataRow[] rowPictures = row.GetChildRows("Pictures_Item");
        //    if (rowPictures != null && rowPictures.Length > 0)
        //    {
        //        foreach (System.Data.DataRow rowPicture in rowPictures)
        //        {
        //            Picture pic = new Picture();
        //            pic.PicSize = rowPicture["PicSize"].ToString();
        //            pic.URL = rowPicture["URL"].ToString();
        //            Pictures.Add(pic);
        //        }
        //    }
        //}

        private void BuildTagMetas(List<Metas> mediaMetas, List<Tags> mediaTags, int groupID, PlatformType platform)
        {
            TagMetaPair pair;

            string[] adMetas = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Metas.Split(';');

            if (mediaMetas != null)
            {
                foreach (Metas meta in mediaMetas)
                {
                    if (meta.m_oTagMeta.m_sName != "ID")
                    {
                        pair = new TagMetaPair(meta.m_oTagMeta.m_sName, meta.m_sValue);
                        metas.Add(pair);

                        if (adMetas.Contains(pair.key))
                            advertising_parameters.Add(pair);
                    }
                }
            }
            //Copy Tags

            if (mediaTags != null)
            {
                foreach (Tags tag in mediaTags)
                {
                    if (tag.m_oTagMeta.m_sName != "ID")
                    {
                        foreach (string tagValue in tag.m_lValues)
                        {
                            TagMetaPair mediaTag = tags.Where(t => t.key == tag.m_oTagMeta.m_sName).FirstOrDefault();
                            if (mediaTag.key == null && mediaTag.value == null) // Change this!!!
                            {
                                pair = new TagMetaPair(tag.m_oTagMeta.m_sName, tagValue);
                                tags.Add(pair);
                            }
                            else
                            {
                                mediaTag.value = (!String.IsNullOrEmpty(mediaTag.value.ToString())) ? string.Concat(mediaTag.value.ToString(), "|", tagValue) : tagValue;
                            }
                        }
                    }
                    string[] adTags = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Tags.Split(';');

                    foreach (TagMetaPair mediaTag in tags)
                    {
                        if (adTags.Contains(mediaTag.key))
                            advertising_parameters.Add(mediaTag);
                    }
                }
            }
        }

        //private void BuildTagMetas(int groupID, dsItemInfo.ItemRow row, PlatformType platform)
        //{
        //    System.Data.DataRow[] tagsRow = row.GetChildRows("Item_Tags");
        //    if (tagsRow != null && tagsRow.Length > 0)
        //    {
        //        string[] adTags = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Tags.Split(';');

        //        //Create tag meta pair objects list for all tags
        //        foreach (System.Data.DataColumn tag in tagsRow[0].Table.Columns)
        //        {
        //            if (tag.ColumnName != "ID")
        //            {
        //                TagMetaPair pair = new TagMetaPair(tag.ColumnName, tagsRow[0][tag.ColumnName].ToString());
        //                Tags.Add(pair);

        //                if (adTags.Contains(pair.Key))
        //                    AdvertisingParameters.Add(pair);
        //            }
        //        }
        //    }
        //    System.Data.DataRow[] metasRow = row.GetChildRows("Item_Metas");
        //    if (metasRow != null && metasRow.Length > 0)
        //    {
        //        string[] adMetas = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Metas.Split(';');

        //        //Create tag meta pair objects list for all metas
        //        foreach (System.Data.DataColumn meta in metasRow[0].Table.Columns)
        //        {
        //            if (meta.ColumnName != "ID")
        //            {
        //                TagMetaPair pair = new TagMetaPair(meta.ColumnName, metasRow[0][meta.ColumnName].ToString());
        //                Metas.Add(pair);

        //                if (adMetas.Contains(pair.Key))
        //                    AdvertisingParameters.Add(pair);
        //            }
        //        }
        //    }
        //}

        private long GetMediaMark()
        {
            long retVal = 0;
            int groupID = WSUtils.GetGroupIDByMediaType(int.Parse(media_type_id));

            return retVal;
        }

        //Build dynamic data if needed (is favorite, price, price status, notifications..)
        //public void BuildDynamicObj(InitializationObject initObj, int groupID)
        //{
        //    this.MediaDynamicData = new DynamicData();

        //    PermittedMediaContainer[] MediaItems = new ApiConditionalAccessService(groupID, initObj.Platform).GetUserPermittedItems(initObj.SiteGuid);
        //    int mediID = int.Parse(MediaID);
        //    if (MediaItems != null)
        //    {
        //        var ItemPermited = (from m in MediaItems where m.m_nMediaID == mediID select m).FirstOrDefault();
        //        if (ItemPermited != null)
        //        {
        //            MediaDynamicData.Price = "Free";
        //            MediaDynamicData.PriceType = PriceReason.PPVPurchased;
        //            if (ItemPermited.m_dEndDate != null)
        //            {
        //                MediaDynamicData.ExpirationDate = ItemPermited.m_dEndDate;
        //            }
        //        }
        //        else
        //        {
        //            string price;
        //            PriceReason reason;
        //            GetPrice(out price, out reason, groupID, initObj.SiteGuid, initObj.Platform);
        //            MediaDynamicData.PriceType = reason;
        //            MediaDynamicData.Price = price;
        //        }
        //    }
        //    else
        //    {
        //        string price;
        //        PriceReason reason;
        //        GetPrice(out price, out reason, groupID, initObj.SiteGuid, initObj.Platform);
        //        MediaDynamicData.PriceType = reason;
        //        MediaDynamicData.Price = price;

        //    }

        //    if (!string.IsNullOrEmpty(initObj.SiteGuid))
        //    {
        //        if (MediaHelper.AreMediasFavorite(initObj, groupID, new List<int>() { mediID })[0].Value)
        //        {
        //            MediaDynamicData.IsFavorite = true;
        //        }
        //        else
        //        {
        //            MediaDynamicData.IsFavorite = false;
        //        }
        //    }

        //}

        ////private bool IsItemFavorite(int mediaID, string userGuid,int iDomainID, string sUDID, int groupID, PlatformType platform)
        ////{
        ////    bool retVal = false;
        ////    FavoritObject[] favoriteObj = new ApiUsersService(groupID, platform).GetUserFavorites(userGuid, string.Empty, iDomainID, sUDID);
        ////    if (favoriteObj != null)
        ////    {
        ////        for (int i = 0; i < favoriteObj.Length; i++)
        ////        {
        ////            if (favoriteObj[i].m_sItemCode == mediaID.ToString())
        ////            {
        ////                retVal = true;
        ////                break;
        ////            }
        ////        }
        ////    }
        ////    return retVal;
        ////}

        //private void GetPrice(out string price, out PriceReason reason, int groupID, string userGuid, PlatformType platform)
        //{
        //    //string MediaPrice = string.Empty;
        //    price = "Free";
        //    reason = PriceReason.Free;
        //    if (!string.IsNullOrEmpty(file_id))
        //    {
        //        int MediaFileID = int.Parse(file_id);

        //        int[] MediasArray = new int[1];
        //        MediasArray[0] = MediaFileID;

        //        //Get media price from conditional access.
        //        IEnumerable<TVPApiModule.Objects.Responses.MediaFileItemPricesContainer> MediasPrices = new ApiConditionalAccessService(groupID, platform).GetItemsPrice(MediasArray, userGuid, true);

        //        if (MediasPrices != null)
        //        {
        //            //Locating the media inside the array
        //            TVPApiModule.Objects.Responses.MediaFileItemPricesContainer mediaPriceCont = null;
        //            foreach (TVPApiModule.Objects.Responses.MediaFileItemPricesContainer mp in MediasPrices)
        //            {
        //                if (mp.media_file_id == MediaFileID)
        //                    mediaPriceCont = mp;
        //            }

        //            if (mediaPriceCont != null)
        //            {
        //                if (mediaPriceCont.item_prices != null)
        //                {
        //                    price = mediaPriceCont.item_prices[0].price.price.ToString();
        //                    switch (mediaPriceCont.item_prices[0].price_reason)
        //                    {
        //                        case TVPApiModule.Objects.Responses.PriceReason.ForPurchase:
        //                            {
        //                                reason = PriceReason.ForPurchase;
        //                                break;
        //                            }
        //                        case TVPApiModule.Objects.Responses.PriceReason.Free:
        //                            {
        //                                reason = PriceReason.Free;
        //                                break;
        //                            }
        //                        case TVPApiModule.Objects.Responses.PriceReason.PPVPurchased:
        //                        case TVPApiModule.Objects.Responses.PriceReason.SubscriptionPurchased:
        //                            {
        //                                reason = PriceReason.PPVPurchased;
        //                                break;
        //                            }
        //                        default:
        //                            {
        //                                reason = PriceReason.Free;
        //                                break;
        //                            }
        //                    }
        //                }
        //                else
        //                {
        //                    price = "Free";
        //                    reason = PriceReason.Free;
        //                }
        //            }
        //            else
        //            {
        //                price = "Free";
        //                reason = PriceReason.Free;
        //            }
        //        }
        //        else
        //        {
        //            price = "Free";
        //            reason = PriceReason.Free;
        //        }
        //        //Extract price from response.
        //        //MediaPrice = PriceHelper.GetSingleFullMediaPrice(MediaFileID, MediasPrices);

        //    }

        //    //return MediaPrice;
        //}

        //private PriceReason GetMediaPriceReason(PermittedMediaContainer[] MediaItems)
        //{
        //    PriceReason retVal = PriceReason.UnKnown;
        //    if (!string.IsNullOrEmpty(file_id))
        //    {
        //        int MediaFileID = int.Parse(file_id);
        //        if (MediaFileID > 0)
        //        {
        //            string Reason = PriceHelper.GetItemPriceReason(MediaFileID);

        //            switch (Reason)
        //            {
        //                case "PPVPurchased":
        //                    retVal = PriceReason.PPVPurchased;
        //                    break;
        //                case "Free":
        //                    retVal = PriceReason.Free;
        //                    break;
        //                case "ForPurchaseSubscriptionOnly":
        //                    retVal = PriceReason.ForPurchaseSubscriptionOnly;
        //                    break;
        //                case "SubscriptionPurchased":
        //                    retVal = PriceReason.SubscriptionPurchased;
        //                    break;
        //                case "ForPurchase":
        //                    retVal = PriceReason.ForPurchase;
        //                    break;
        //                case "UnKnown":
        //                    retVal = PriceReason.UnKnown;
        //                    break;
        //                case "SubscriptionPurchasedWrongCurrency":
        //                    retVal = PriceReason.SubscriptionPurchasedWrongCurrency;
        //                    break;
        //            }
        //        }
        //    }

        //    return retVal;
        //}

        #endregion
    }
}

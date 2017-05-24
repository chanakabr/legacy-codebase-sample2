using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using System.Configuration;
using TVPApiModule.Services;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Users;
using KLogMonitor;
using System.Reflection;

/// <summary>
/// Summary description for Media
/// </summary>
/// 

namespace TVPApi
{
    public class Media
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string MediaID;
        public string MediaName;
        public string MediaTypeID;
        public string MediaTypeName;
        public double Rating;
        public int ViewCounter;
        public string Description;
        public DateTime CreationDate;
        public DateTime? LastWatchDate;
        public DateTime StartDate;
        public DateTime CatalogStartDate;
        public string PicURL;
        public string URL;
        public string MediaWebLink;
        public string Duration;
        public string FileID;
        private List<TagMetaPair> m_tags;
        private List<TagMetaPair> m_metas;
        private List<File> m_files;
        private List<TagMetaPair> m_adParams;

        private List<Picture> m_pictures;

        private List<ExtIDPair> m_externalIDs;

        public DynamicData MediaDynamicData;
        public string SubDuration;
        public string SubFileFormat;
        public string SubFileID;
        public string SubURL;
        public string GeoBlock;
        public long TotalItems;
        public int? like_counter;

        public string EntryId;

        public struct ExtIDPair
        {
            public string Key;
            public string Value;
        }

        public struct File
        {
            public string FileID;
            public string URL;
            public string Duration;
            public string Format;
            public AdvertisingProvider PreProvider;
            public AdvertisingProvider PostProvider;
            public AdvertisingProvider BreakProvider;
            public AdvertisingProvider OverlayProvider;
            public string[] BreakPoints;
            public string[] OverlayPoints;
            public string Language;
            public bool IsDefaultLang;
            public string CoGuid;
        }

        public struct Picture
        {
            public string PicSize;
            public string URL;
            public string ID;
            public int Version;
            public string Ratio;
            public bool IsDefault;
        }

        public List<TagMetaPair> Tags
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

        public List<TagMetaPair> Metas
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

        public List<TagMetaPair> AdvertisingParameters
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

        public List<File> Files
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

        public List<Picture> Pictures
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

        public List<ExtIDPair> ExternalIDs
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

        public Media()
        {

        }

        public Media(dsItemInfo.ItemRow itemRow, InitializationObject initObj, int groupID, bool withDynamic)
        {
            InitMediaObj(itemRow, initObj, groupID, withDynamic, 0);
        }

        public Media(dsItemInfo.ItemRow itemRow, InitializationObject initObj, int groupID, bool withDynamic, long iMediaCount)
        {
            InitMediaObj(itemRow, initObj, groupID, withDynamic, iMediaCount);
        }


        private string GetMediaWebLink(int groupID, PlatformType platform)
        {
            string retVal = string.Empty;
            string baseUrl = ConfigurationManager.AppSettings[string.Format("{0}_BaseURL", groupID.ToString())];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                if (ConfigManager.GetInstance().GetConfig(groupID, platform).SiteConfiguration.Data.Features.FriendlyURL.SupportFeature)
                {
                    //string sMediaName = MediaName.Replace("/", "");

                    //sMediaName = sMediaName.Replace(" ", "-");

                    //sMediaName = HttpUtility.UrlEncode(sMediaName);

                    string sMediaName = MediaName.Replace("/", "");
                    sMediaName = SiteHelper.ReplaceSpecialChars(sMediaName);

                    if (groupID == 153)
                    {
                        if (MediaTypeName == "Series")
                            retVal = string.Format("{0}/ShowPage/{1}/{2}", baseUrl, sMediaName, MediaID);
                        else if (MediaTypeName == "Linear")
                            retVal = string.Format("{0}/ChannelPage/{1}/{2}", baseUrl, sMediaName, MediaID);
                        else
                            retVal = string.Format("{0}/MediaPage/{1}/{2}", baseUrl, sMediaName, MediaID);
                    }
                    else if (groupID == 147)
                    {
                        switch (MediaTypeName)
                        {
                            case "Linear":
                                retVal = string.Format("{0}/channels/{1}/{2}", baseUrl, sMediaName, MediaID);
                                break;
                            case "Series":
                            case "Episode":
                                retVal = string.Format("{0}/series/{1}/{2}", baseUrl, sMediaName, MediaID);
                                break;
                            case "Extra":
                                if (Tags != null && Tags.Count > 0)
                                {
                                    var extraTag = Tags.Where(t => t.Key == "Extra Type").FirstOrDefault();
                                    var tag = Tags.Where(t => t.Key == "Extra Virtual Media Link").FirstOrDefault();
                                    if (!string.IsNullOrEmpty(tag.Value))
                                    {

                                        retVal = string.Format("{0}/series/{1}/{2}/{3}", baseUrl, sMediaName, extraTag.Value, MediaID);
                                    }
                                    else
                                    {
                                        tag = Tags.Where(t => t.Key == "Extra Regular Media Link").FirstOrDefault();
                                        if (!string.IsNullOrEmpty(tag.Value))
                                        {
                                            retVal = string.Format("{0}/movies/{1}/{2}/{3}", baseUrl, sMediaName, extraTag.Value, MediaID);
                                        }
                                    }
                                }
                                break;
                            case "Movie":
                                retVal = string.Format("{0}/movies/{1}/{2}", baseUrl, sMediaName, MediaID);
                                break;
                            case "News Series":
                            case "News":
                                retVal = string.Format("{0}/news/{1}/{2}", baseUrl, sMediaName, MediaID);
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        retVal = string.Format("{0}/{1}/{2}/{3}", baseUrl, MediaTypeName, sMediaName, MediaID);
                    }
                }
                else
                {
                    retVal = string.Format("{0}/MediaPage.aspx?MediaID={1}&MediaType={2}", baseUrl, MediaID, MediaTypeID);
                }
            }
            return retVal;
        }
        //Fill properties according to media row
        private void InitMediaObj(dsItemInfo.ItemRow row, InitializationObject initObj, int groupID, bool withDynamic, long iMediaCount)
        {
            MediaID = row.ID;
            if (!row.IsMediaTypeIDNull())
            {
                MediaTypeID = row.MediaTypeID;
            }

            if (!row.IsNameNull())
            {
                MediaName = row.Title;
            }

            if (!row.IsMediaTypeNull())
            {
                MediaTypeName = row.MediaType;
            }
            if (!row.IsDescriptionShortNull())
            {
                Description = row.DescriptionShort;
            }

            if (!row.IsImageLinkNull())
            {
                PicURL = row.ImageLink;
            }
            if (!row.IsURLNull())
            {
                URL = row.URL;
            }

            if (!row.IsFileIDNull())
            {
                FileID = row.FileID;
            }

            if (!row.IsDurationNull())
            {
                Duration = row.Duration;
            }

            if (!row.IsRateNull())
            {
                Rating = row.Rate;
            }
            if (!row.IsViewCounterNull())
            {
                ViewCounter = row.ViewCounter;
            }
            if (!row.IsCreationDateNull())
            {
                CreationDate = row.CreationDate;
            }
            else
                CreationDate = new DateTime(1970, 1, 1);

            if (!row.IsStartDateNull())
            {
                StartDate = row.StartDate;
            }
            else
                StartDate = new DateTime(1970, 1, 1);

            if (!row.IsCatalogStartDateNull())
            {
                CatalogStartDate = row.CatalogStartDate;
            }
            else
                CatalogStartDate = new DateTime(1970, 1, 1);

            // add sub file foramt info
            if (!row.IsSubDurationNull())
            {
                SubDuration = row.SubDuration;
            }
            if (!row.IsSubFileFormatNull())
            {
                SubFileFormat = row.SubFileFormat;
            }
            if (!row.IsSubFileIDNull())
            {
                SubFileID = row.SubFileID;
            }
            if (!row.IsSubURLNull())
            {
                SubURL = row.SubURL;
            }
            if (!row.IsGeoBlockNull())
            {
                GeoBlock = row.GeoBlock;
            }
            if (!String.IsNullOrEmpty(row.Likes))
            {
                like_counter = Convert.ToInt32(row.Likes);
            }
            if (!row.IsLastWatchedDateNull())
            {
                LastWatchDate = row.LastWatchedDate;
            }

            BuildTagMetas(groupID, row, initObj.Platform);

            buildFiles(row);

            builtExternalIDs(row);

            buildPictures(row);

            if (withDynamic && initObj.Locale != null)
            {
                logger.InfoFormat("Start Media dynamic build GroupID:", groupID);

                BuildDynamicObj(initObj, groupID);
            }

            TotalItems = iMediaCount;

            MediaWebLink = GetMediaWebLink(groupID, initObj.Platform);

            if (!row.IsEntryIdNull())
            {
                EntryId = row.EntryId;
            }
        }

        private void builtExternalIDs(dsItemInfo.ItemRow row)
        {
            System.Data.DataRow[] rowExtIDs = row.GetChildRows("Item_ExtIDs");
            if (rowExtIDs != null && rowExtIDs.Length > 0)
            {
                foreach (System.Data.DataRow rowExt in rowExtIDs)
                {
                    foreach (System.Data.DataColumn dc in rowExt.Table.Columns)
                    {
                        if (!dc.ColumnName.Equals("ID"))
                            ExternalIDs.Add(new ExtIDPair() { Key = dc.ColumnName, Value = rowExt[dc.ColumnName].ToString() });
                    }
                }
            }
        }

        private void buildFiles(dsItemInfo.ItemRow row)
        {
            System.Data.DataRow[] rowFiles = row.GetChildRows("Item_files");
            if (rowFiles != null && rowFiles.Length > 0)
            {
                foreach (System.Data.DataRow rowFile in rowFiles)
                {
                    File file = new File();
                    file.FileID = rowFile["FileID"].ToString();
                    file.URL = rowFile["URL"].ToString();
                    file.Duration = rowFile["Duration"].ToString();
                    file.Format = rowFile["Format"].ToString();
                    file.Language = rowFile["Language"].ToString();
                    file.IsDefaultLang = Convert.ToBoolean(Convert.ToInt16(rowFile["IsDefaultLang"].ToString()));
                    file.CoGuid = rowFile["CoGuid"].ToString();

                    int preProviderID = Convert.ToInt32(rowFile["PreProviderID"].ToString());

                    if (preProviderID != 0)
                        file.PreProvider = new AdvertisingProvider(preProviderID, rowFile["PreProviderName"].ToString());

                    int postProviderID = Convert.ToInt32(rowFile["PostProviderID"].ToString());

                    if (postProviderID != 0)
                        file.PostProvider = new AdvertisingProvider(postProviderID, rowFile["PostProviderName"].ToString());

                    int breakProviderID = Convert.ToInt32(rowFile["BreakProviderID"].ToString());

                    if (breakProviderID != 0)
                    {
                        file.BreakProvider = new AdvertisingProvider(breakProviderID, rowFile["BreakProviderName"].ToString());

                        if (rowFile["BreakPoints"] != null)
                            file.BreakPoints = rowFile["BreakPoints"].ToString().Split(';');
                    }

                    int overlayPoviderID = Convert.ToInt32(rowFile["OverlayProviderID"].ToString());

                    if (overlayPoviderID != 0)
                    {
                        file.OverlayProvider = new AdvertisingProvider(overlayPoviderID, rowFile["OverlayProviderName"].ToString());

                        if (rowFile["OverlayPoints"] != null)
                            file.OverlayPoints = rowFile["OverlayPoints"].ToString().Split(';');
                    }

                    Files.Add(file);
                }
            }
        }

        private void buildPictures(dsItemInfo.ItemRow row)
        {
            System.Data.DataRow[] rowPictures = row.GetChildRows("Pictures_Item");
            if (rowPictures != null && rowPictures.Length > 0)
            {
                foreach (System.Data.DataRow rowPicture in rowPictures)
                {
                    Picture pic = new Picture();
                    pic.PicSize = rowPicture["PicSize"].ToString();
                    pic.URL = rowPicture["URL"].ToString();
                    pic.Version = Convert.ToInt32(rowPicture["Version"].ToString());
                    pic.ID = rowPicture["ImageId"].ToString();
                    pic.Ratio = rowPicture["Ratio"].ToString();
                    pic.IsDefault = Convert.ToBoolean(rowPicture["IsDefault"]);

                    Pictures.Add(pic);
                }
            }
        }

        private void BuildTagMetas(int groupID, dsItemInfo.ItemRow row, PlatformType platform)
        {
            System.Data.DataRow[] tagsRow = row.GetChildRows("Item_Tags");
            if (tagsRow != null && tagsRow.Length > 0)
            {
                string[] adTags = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Tags.Split(';');

                //Create tag meta pair objects list for all tags
                foreach (System.Data.DataColumn tag in tagsRow[0].Table.Columns)
                {
                    if (tag.ColumnName != "ID")
                    {
                        TagMetaPair pair = new TagMetaPair(tag.ColumnName, tagsRow[0][tag.ColumnName].ToString());
                        Tags.Add(pair);

                        if (adTags.Contains(pair.Key))
                            AdvertisingParameters.Add(pair);
                    }
                }
            }
            System.Data.DataRow[] metasRow = row.GetChildRows("Item_Metas");
            if (metasRow != null && metasRow.Length > 0)
            {
                string[] adMetas = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.AdvertisingValues.Metas.Split(';');

                //Create tag meta pair objects list for all metas
                foreach (System.Data.DataColumn meta in metasRow[0].Table.Columns)
                {
                    if (meta.ColumnName != "ID")
                    {
                        TagMetaPair pair = new TagMetaPair(meta.ColumnName, metasRow[0][meta.ColumnName].ToString());
                        Metas.Add(pair);

                        if (adMetas.Contains(pair.Key))
                            AdvertisingParameters.Add(pair);
                    }
                }
            }
        }

        private long GetMediaMark()
        {
            long retVal = 0;
            int groupID = WSUtils.GetGroupIDByMediaType(int.Parse(MediaTypeID));

            return retVal;
        }

        //Build dynamic data if needed (is favorite, price, price status, notifications..)
        private void BuildDynamicObj(InitializationObject initObj, int groupID)
        {
            this.MediaDynamicData = new DynamicData();

            PermittedMediaContainer[] MediaItems = new ApiConditionalAccessService(groupID, initObj.Platform).GetUserPermittedItems(initObj.SiteGuid);
            int mediID = int.Parse(MediaID);
            if (MediaItems != null)
            {
                var ItemPermited = (from m in MediaItems where m.m_nMediaID == mediID select m).FirstOrDefault();
                if (ItemPermited != null)
                {
                    MediaDynamicData.Price = "Free";
                    MediaDynamicData.PriceType = PriceReason.PPVPurchased;
                    if (ItemPermited.m_dEndDate != null)
                    {
                        MediaDynamicData.ExpirationDate = ItemPermited.m_dEndDate;
                    }
                }
                else
                {
                    string price;
                    PriceReason reason;
                    GetPrice(out price, out reason, groupID, initObj.SiteGuid, initObj.Platform);
                    MediaDynamicData.PriceType = reason;
                    MediaDynamicData.Price = price;
                }
            }
            else
            {
                string price;
                PriceReason reason;
                GetPrice(out price, out reason, groupID, initObj.SiteGuid, initObj.Platform);
                MediaDynamicData.PriceType = reason;
                MediaDynamicData.Price = price;

            }

            if (!string.IsNullOrEmpty(initObj.SiteGuid))
            {
                if (MediaHelper.IsFavoriteMedia(initObj, groupID, int.Parse(MediaID)))
                {
                    MediaDynamicData.IsFavorite = true;
                }
                else
                {
                    MediaDynamicData.IsFavorite = false;
                }
            }

        }

        //private bool IsItemFavorite(int mediaID, string userGuid,int iDomainID, string sUDID, int groupID, PlatformType platform)
        //{
        //    bool retVal = false;
        //    FavoritObject[] favoriteObj = new ApiUsersService(groupID, platform).GetUserFavorites(userGuid, string.Empty, iDomainID, sUDID);
        //    if (favoriteObj != null)
        //    {
        //        for (int i = 0; i < favoriteObj.Length; i++)
        //        {
        //            if (favoriteObj[i].m_sItemCode == mediaID.ToString())
        //            {
        //                retVal = true;
        //                break;
        //            }
        //        }
        //    }
        //    return retVal;
        //}

        private void GetPrice(out string price, out PriceReason reason, int groupID, string userGuid, PlatformType platform)
        {
            //string MediaPrice = string.Empty;
            price = "Free";
            reason = PriceReason.Free;
            if (!string.IsNullOrEmpty(FileID))
            {
                int MediaFileID = int.Parse(FileID);

                int[] MediasArray = new int[1];
                MediasArray[0] = MediaFileID;

                //Get media price from conditional access.
                MediaFileItemPricesContainer[] MediasPrices = new ApiConditionalAccessService(groupID, platform).GetItemsPrice(MediasArray, userGuid, true);

                if (MediasPrices != null)
                {
                    //Locating the media inside the array
                    MediaFileItemPricesContainer mediaPriceCont = null;
                    foreach (MediaFileItemPricesContainer mp in MediasPrices)
                    {
                        if (mp.m_nMediaFileID == MediaFileID)
                            mediaPriceCont = mp;
                    }

                    if (mediaPriceCont != null)
                    {
                        if (mediaPriceCont.m_oItemPrices != null)
                        {
                            price = mediaPriceCont.m_oItemPrices[0].m_oPrice.m_dPrice.ToString();
                            switch (mediaPriceCont.m_oItemPrices[0].m_PriceReason)
                            {
                                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PriceReason.ForPurchase:
                                    {
                                        reason = PriceReason.ForPurchase;
                                        break;
                                    }
                                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PriceReason.Free:
                                    {
                                        reason = PriceReason.Free;
                                        break;
                                    }
                                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PriceReason.PPVPurchased:
                                case TVPPro.SiteManager.TvinciPlatform.ConditionalAccess.PriceReason.SubscriptionPurchased:
                                    {
                                        reason = PriceReason.PPVPurchased;
                                        break;
                                    }
                                default:
                                    {
                                        reason = PriceReason.Free;
                                        break;
                                    }
                            }
                        }
                        else
                        {
                            price = "Free";
                            reason = PriceReason.Free;
                        }
                    }
                    else
                    {
                        price = "Free";
                        reason = PriceReason.Free;
                    }
                }
                else
                {
                    price = "Free";
                    reason = PriceReason.Free;
                }
                //Extract price from response.
                //MediaPrice = PriceHelper.GetSingleFullMediaPrice(MediaFileID, MediasPrices);

            }

            //return MediaPrice;
        }

        private PriceReason GetMediaPriceReason(PermittedMediaContainer[] MediaItems)
        {
            PriceReason retVal = PriceReason.UnKnown;
            if (!string.IsNullOrEmpty(FileID))
            {
                int MediaFileID = int.Parse(FileID);
                if (MediaFileID > 0)
                {
                    string Reason = PriceHelper.GetItemPriceReason(MediaFileID);

                    switch (Reason)
                    {
                        case "PPVPurchased":
                            retVal = PriceReason.PPVPurchased;
                            break;
                        case "Free":
                            retVal = PriceReason.Free;
                            break;
                        case "ForPurchaseSubscriptionOnly":
                            retVal = PriceReason.ForPurchaseSubscriptionOnly;
                            break;
                        case "SubscriptionPurchased":
                            retVal = PriceReason.SubscriptionPurchased;
                            break;
                        case "ForPurchase":
                            retVal = PriceReason.ForPurchase;
                            break;
                        case "UnKnown":
                            retVal = PriceReason.UnKnown;
                            break;
                        case "SubscriptionPurchasedWrongCurrency":
                            retVal = PriceReason.SubscriptionPurchasedWrongCurrency;
                            break;
                    }
                }
            }

            return retVal;
        }

    }
}

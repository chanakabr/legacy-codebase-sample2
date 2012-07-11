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

/// <summary>
/// Summary description for Media
/// </summary>
/// 

namespace TVPApi
{
    public class Media
    {
        #region Properties

        private readonly ILog logger = LogManager.GetLogger(typeof(Media));

        public string MediaID;
        public string MediaName;
        public string MediaTypeID;
        public string MediaTypeName;
        public double Rating;
        public int ViewCounter;
        public string Description;
        public DateTime CreationDate;
        public DateTime? LastWatchDate;
        public string PicURL;
        public string URL;
        public string MediaWebLink;
        public string Duration;
        public string FileID;
        private List<TagMetaPair> m_tags;
        private List<TagMetaPair> m_metas;
        private List<File> m_files;

        public DynamicData MediaDynamicData;
        public string SubDuration;
        public string SubFileFormat;
        public string SubFileID;
        public string SubURL;
        public string GeoBlock;
        public long TotalItems;
        public int? like_counter;

        public struct File
        {
            public string FileID;
            public string URL;
            public string Duration;
            public string Format;
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
        #endregion

        #region Constructors
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
                    MediaName = MediaName.Replace("/", "");
                    retVal = string.Format("{0}/{1}/{2}/{3}", baseUrl, MediaTypeName, MediaName, MediaID);
                }
                else
                {
                    retVal = string.Format("{0}/MediaPage.aspx?MediaID={1}&MediaType={2}", baseUrl, MediaID, MediaTypeID);
                }
            }
            return retVal;
        }
        //Fill properies according to media row
        private void InitMediaObj(dsItemInfo.ItemRow row, InitializationObject initObj, int groupID, bool withDynamic, long iMediaCount)
        {
            MediaID = row.ID;
            if (!row.IsMediaTypeIDNull())
            {
                MediaTypeID = row.MediaTypeID;
            }

            MediaName = row.Title;

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
            if (!row.IsAddedDateNull())
            {
                CreationDate = row.AddedDate;
            }

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
            MediaWebLink = GetMediaWebLink(groupID, initObj.Platform);

            BuildTagMetas(groupID, row, initObj.Platform);

            buildFiles(row);

            if (withDynamic && initObj.Locale != null)
            {
                logger.InfoFormat("Start Media dynamic build GroupID:", groupID);

                BuildDynamicObj(initObj, groupID);
            }

            TotalItems = iMediaCount;
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

                    Files.Add(file);
                }
            }
        }

        private void BuildTagMetas(int groupID, dsItemInfo.ItemRow row, PlatformType platform)
        {
            string[] TagNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
            System.Data.DataRow[] tagsRow = row.GetChildRows("Item_Tags");
            if (tagsRow != null && tagsRow.Length > 0)
            {
                //Create tag meta pair objects list for all tags
                foreach (string tagName in TagNames)
                {
                    if (tagsRow[0].Table.Columns.Contains(tagName) && !string.IsNullOrEmpty(tagsRow[0][tagName].ToString()))
                    {
                        TagMetaPair pair = new TagMetaPair(tagName, tagsRow[0][tagName].ToString());
                        Tags.Add(pair);
                    }
                }
            }
            string[] MetaNames = ConfigManager.GetInstance().GetConfig(groupID, platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
            System.Data.DataRow[] metasRow = row.GetChildRows("Item_Metas");
            if (metasRow != null && metasRow.Length > 0)
            {
                //Create tag meta pair objects list for all metas
                foreach (string metaName in MetaNames)
                {
                    if (metasRow[0].Table.Columns.Contains(metaName) && !string.IsNullOrEmpty(metasRow[0][metaName].ToString()))
                    {
                        TagMetaPair pair = new TagMetaPair(metaName, metasRow[0][metaName].ToString());
                        Metas.Add(pair);
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

        #endregion
    }
}

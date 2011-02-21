using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Helper;
using TVPPro.SiteManager.Services;
using TVPApiModule.users;
using System.Configuration;

/// <summary>
/// Summary description for Media
/// </summary>
/// 

namespace TVPApi
{
    public class Media
    {
        #region Properties

        public string MediaID;
        public string MediaName;
        public string MediaTypeID;
        public string MediaTypeName;
        public double Rating;
        public int ViewCounter;
        public string Description;
        public DateTime CreationDate;
        public string PicURL;
        public string URL;
        public string MediaWebLink;
        public string Duration;
        public string FileID;
        private List<TagMetaPair> m_tags;
        private List<TagMetaPair> m_metas;

        public DynamicData MediaDynamicData;

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

        #endregion

        #region Constructors
        public Media()
        {

        }

        public Media(dsItemInfo.ItemRow itemRow, InitializationObject initObj, int groupID, bool withDynamic)
        {
            InitMediaObj(itemRow, initObj, groupID, withDynamic);
        }

        #endregion

        #region private functions

        private string GetMediaWebLink(int groupID)
        {
            string retVal = string.Empty;
            string baseUrl = ConfigurationManager.AppSettings[string.Format("{0}_BaseURL", groupID.ToString())];
            if (!string.IsNullOrEmpty(baseUrl))
            {
                retVal = string.Format("{0}/{1}/{2}/{3}", baseUrl, MediaTypeName, MediaName, MediaID);
            }
            return retVal;
        }
        //Fill properies according to media row
        private void InitMediaObj(dsItemInfo.ItemRow row, InitializationObject initObj, int groupID, bool withDynamic)
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

            MediaWebLink = GetMediaWebLink(groupID);

            BuildTagMetas(groupID, row, initObj.Platform.ToString());
            if (withDynamic && initObj.Locale != null)
            {
                LogManager.Instance.Log(groupID, "Media", "Start dynamic build");
                BuildDynamicObj(initObj.Locale.SiteGuid, initObj.Platform.ToString(), groupID);
            }
        }

        private void BuildTagMetas(int groupID, dsItemInfo.ItemRow row, string platform)
        {
            string[] TagNames = ConfigManager.GetInstance(groupID, platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Tags.ToString().Split(new Char[] { ';' });
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
            string[] MetaNames = ConfigManager.GetInstance(groupID, platform).MediaConfiguration.Data.TVM.MediaInfoStruct.Metadata.ToString().Split(new Char[] { ';' });
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
        private void BuildDynamicObj(string guid, string platform, int groupID)
        {
            this.MediaDynamicData = new DynamicData();
            LogManager.Instance.Log(93, "Media", string.Format("GUID is {0}", guid));
            PermittedMediaContainer[] MediaItems = TVPPro.SiteManager.Services.ConditionalAccessService.Instance.GetUserPermittedItems(guid, string.Format("conditionalaccess_{0}", groupID.ToString()), "11111");
            int mediID = int.Parse(MediaID);
            if (MediaItems != null)
            {
                var ItemPermited = (from m in MediaItems where m.m_nMediaID == mediID select m).FirstOrDefault();
                if (ItemPermited != null)
                {
                    LogManager.Instance.Log(93, "Media", "Found purchased item");
                    MediaDynamicData.Price = "Free";
                    MediaDynamicData.PriceType = PriceReason.PPVPurchased;
                    if (ItemPermited.m_dEndDate != null)
                    {
                        MediaDynamicData.ExpirationDate = ItemPermited.m_dEndDate;
                        LogManager.Instance.Log(93, "Media", string.Format("ExpirationDate is {0}", ItemPermited.m_dEndDate));
                    }
                }
                else
                {
                    string price;
                    PriceReason reason;
                    GetPrice(out price, out reason, groupID, guid);
                    MediaDynamicData.PriceType = reason;
                    MediaDynamicData.Price = price;
                }
            }
            else
            {
                string price;
                PriceReason reason;
                GetPrice(out price, out reason, groupID, guid);
                MediaDynamicData.PriceType = reason;
                MediaDynamicData.Price = price;

            }
            LogManager.Instance.Log(93, "Media", "Try get is favorite");
            if (!string.IsNullOrEmpty(guid))
            {
                if (IsItemFavorite(int.Parse(MediaID), guid))
                {
                    MediaDynamicData.IsFavorite = true;
                }
                else
                {
                    MediaDynamicData.IsFavorite = false;
                }
            }
            LogManager.Instance.Log(93, "Media", string.Format("IsItemInFavorite:{0}", MediaDynamicData.IsFavorite));
        }

        private bool IsItemFavorite(int mediaID, string userGuid)
        {
            bool retVal = false;
            TVPApiModule.users.UsersService userService = new TVPApiModule.users.UsersService();
            int groupID = WSUtils.GetGroupIDByMediaType(int.Parse(MediaTypeID));
            FavoritObject[] favoriteObj = userService.GetUserFavorites(string.Format("users_{0}", groupID.ToString()), "11111", userGuid, string.Empty, string.Empty);
            if (favoriteObj != null)
            {
                for (int i = 0; i < favoriteObj.Length; i++)
                {
                    if (favoriteObj[i].m_sItemCode == mediaID.ToString())
                    {
                        retVal = true;
                        break;
                    }
                }
            }
            return retVal;
        }

        private void GetPrice(out string price, out PriceReason reason, int groupID, string userGuid)
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
                Dictionary<int, MediaFileItemPricesContainer> MediasPrices = ConditionalAccessService.Instance.GetItemsPrice(MediasArray,string.Format("conditionalaccess_{0}", groupID), "11111", userGuid, true);
                
                if (MediasPrices != null && MediasPrices.ContainsKey(MediaFileID))
                {
                    MediaFileItemPricesContainer mediaPriceCont = MediasPrices[MediaFileID];
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
            LogManager.Instance.Log(93, "Media", "Try get price reason");
            PriceReason retVal = PriceReason.UnKnown;
            if (!string.IsNullOrEmpty(FileID))
            {
                LogManager.Instance.Log(93, "Media", string.Format("File ID is:{0}", FileID));
                int MediaFileID = int.Parse(FileID);
                if (MediaFileID > 0)
                {
                    string Reason = PriceHelper.GetItemPriceReason(MediaFileID);
                    LogManager.Instance.Log(93, "Media", string.Format("Reason is:{0}", Reason));
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

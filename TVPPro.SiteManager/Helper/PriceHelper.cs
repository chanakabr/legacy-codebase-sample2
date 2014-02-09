using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.Context;
using TVPPro.Configuration.Site;
using TVPPro.SiteManager.Services;

namespace TVPPro.SiteManager.Helper
{
    public class PriceHelper
    {
        public static string GetSingleFullMediaPrice(int FileId, Dictionary<int, MediaFileItemPricesContainer> MediasPrices)
        {
            string price = string.Empty;

            if (MediasPrices != null && MediasPrices.ContainsKey(FileId))
            {
                if (FileId > 0)
                {
                    MediaFileItemPricesContainer MediaFileItemPrices = MediasPrices[FileId];
                    if (MediaFileItemPrices.m_oItemPrices != null)
                    {
                        ItemPriceContainer item = MediaFileItemPrices.m_oItemPrices[0];
                        if (item != null && item.m_PriceReason != PriceReason.Free)
                        {
                            //Get the currency sign location.
                            Enums.eAddToSide AddToSide = (Enums.eAddToSide)Enum.Parse(typeof(Enums.eAddToSide), SiteConfiguration.Instance.Data.Global.Price.Location.ToString());

                            if (AddToSide == Enums.eAddToSide.Left)
                                price = string.Format("{0} {1}", item.m_oFullPrice.m_oCurrency.m_sCurrencySign, string.Format("{0:" + SiteConfiguration.Instance.Data.Global.Price.Format.ToString() + "}", item.m_oFullPrice.m_dPrice));
                            else
                                price = string.Format("{0} {1}", string.Format("{0:" + SiteConfiguration.Instance.Data.Global.Price.Format.ToString() + "}", item.m_oFullPrice.m_dPrice), item.m_oFullPrice.m_oCurrency.m_sCurrencySign);
                        }
                    }
                }
            }
            return price;
        }

        public static List<string> GetFullMultiMediaPrice(int FileId, Dictionary<int, MediaFileItemPricesContainer> MediasPrices)
        {
            List<string> prices = new List<string>();

            if (FileId > 0)
            {
                MediaFileItemPricesContainer MediaFileItemPrices = MediasPrices[FileId];
                if (MediaFileItemPrices.m_oItemPrices != null)
                {
                    //Get the currency sign location.
                    Enums.eAddToSide AddToSide = (Enums.eAddToSide)Enum.Parse(typeof(Enums.eAddToSide), SiteConfiguration.Instance.Data.Global.Price.Location.ToString());

                    foreach (ItemPriceContainer priceItem in MediaFileItemPrices.m_oItemPrices)
                    {
                        if (AddToSide == Enums.eAddToSide.Left)
                            prices.Add(string.Format("{0}{1}", priceItem.m_oFullPrice.m_oCurrency.m_sCurrencySign, priceItem.m_oFullPrice.m_dPrice.ToString()));
                        else
                            prices.Add(string.Format("{0}{1}", priceItem.m_oFullPrice.m_dPrice.ToString(), priceItem.m_oFullPrice.m_oCurrency.m_sCurrencySign));
                    }
                }
            }
            return prices;
        }

        public static string GetSubscriptionDiscountPrice(string SubscriptionID, SubscriptionsPricesContainer[] SubscriptionPrices)
        {
            string price = string.Empty;

            if (SubscriptionPrices != null && SubscriptionPrices.Length > 0)
            {
                SubscriptionsPricesContainer item = SubscriptionPrices[0];

                //Get the currency sign location.
                Enums.eAddToSide AddToSide = (Enums.eAddToSide)Enum.Parse(typeof(Enums.eAddToSide), SiteConfiguration.Instance.Data.Global.Price.Location.ToString());

                if (item != null && item.m_oPrice != null)
                {
                    if (AddToSide == Enums.eAddToSide.Left)
                        price = string.Format("{0} {1}", item.m_oPrice.m_oCurrency.m_sCurrencySign, string.Format("{0:" + SiteConfiguration.Instance.Data.Global.Price.Format.ToString() + "}", item.m_oPrice.m_dPrice));
                    else
                        price = string.Format("{0} {1}", string.Format("{0:" + SiteConfiguration.Instance.Data.Global.Price.Format.ToString() + "}", item.m_oPrice.m_dPrice), item.m_oPrice.m_oCurrency.m_sCurrencySign);

                    //if (AddToSide == Enums.eAddToSide.Left)
                    //    price = string.Format("{0}{1}", item.m_oPrice.m_oCurrency.m_sCurrencySign, item.m_oPrice.m_dPrice.ToString());
                    //else
                    //    price = string.Format("{0}{1}", item.m_oPrice.m_dPrice.ToString(), item.m_oPrice.m_oCurrency.m_sCurrencySign);
                }
            }
            return price;
        }

        public static string GetSingleDiscountMediaPrice(int FileId, Dictionary<int, MediaFileItemPricesContainer> MediasPrices)
        {
            string price = string.Empty;

            if (MediasPrices != null && MediasPrices.ContainsKey(FileId))
            {
                if (FileId > 0)
                {
                    MediaFileItemPricesContainer MediaFileItemPrices = MediasPrices[FileId];
                    if (MediaFileItemPrices != null && MediaFileItemPrices.m_oItemPrices != null && MediaFileItemPrices.m_oItemPrices.Length > 0)
                    {
                        ItemPriceContainer items = MediaFileItemPrices.m_oItemPrices[0];

                        //Get the currency sign location.
                        Enums.eAddToSide AddToSide = (Enums.eAddToSide)Enum.Parse(typeof(Enums.eAddToSide), SiteConfiguration.Instance.Data.Global.Price.Location.ToString());

                        if (AddToSide == Enums.eAddToSide.Left)
                            price = string.Format("{0}{1}", items.m_oPrice.m_oCurrency.m_sCurrencySign, items.m_oPrice.m_dPrice.ToString());
                        else
                            price = string.Format("{0}{1}", items.m_oPrice.m_dPrice.ToString(), items.m_oPrice.m_oCurrency.m_sCurrencySign);
                    }
                }
            }

            return price;
        }

        public static List<string> GetDiscountMultiMediaPrice(int FileId, Dictionary<int, MediaFileItemPricesContainer> MediasPrices)
        {
            List<string> prices = new List<string>();

            if (MediasPrices != null && MediasPrices.ContainsKey(FileId))
            {
                if (FileId > 0)
                {
                    MediaFileItemPricesContainer MediaFileItemPrices = MediasPrices[FileId];
                    if (MediaFileItemPrices.m_oItemPrices != null)
                    {
                        //Get the currency sign location.
                        Enums.eAddToSide AddToSide = (Enums.eAddToSide)Enum.Parse(typeof(Enums.eAddToSide), SiteConfiguration.Instance.Data.Global.Price.Location.ToString());

                        foreach (ItemPriceContainer priceItem in MediaFileItemPrices.m_oItemPrices)
                        {
                            if (AddToSide == Enums.eAddToSide.Left)
                                prices.Add(string.Format("{0}{1}", priceItem.m_oPrice.m_oCurrency.m_sCurrencySign, priceItem.m_oPrice.m_dPrice.ToString()));
                            else
                                prices.Add(string.Format("{0}{1}", priceItem.m_oPrice.m_dPrice.ToString(), priceItem.m_oPrice.m_oCurrency.m_sCurrencySign));
                        }
                    }
                }
            }

            return prices;
        }

        public static string GetItemPriceReason(int FileId)
        {
            string PriceReason = string.Empty;
            Dictionary<int, MediaFileItemPricesContainer> MediasPrices;

            int[] MediasArray = new int[1];
            MediasArray[0] = FileId;

            //Get media price from conditional access.
            MediasPrices = ConditionalAccessService.Instance.GetItemsPrice(MediasArray, true);

            if (MediasPrices != null && MediasPrices.ContainsKey(FileId))
            {
                if (FileId > 0)
                {
                    MediaFileItemPricesContainer MediaFileItemPrices = MediasPrices[FileId];
                    if (MediaFileItemPrices.m_oItemPrices != null && MediaFileItemPrices.m_oItemPrices.Count() > 0)
                    {
                        ItemPriceContainer item = MediaFileItemPrices.m_oItemPrices[0];

                        PriceReason = item.m_PriceReason.ToString();

                    }
                }
            }

            return PriceReason;
        }

        public static string FormatPriceAndSymbol(double price)
        {
            if (SiteConfiguration.Instance.Data.Global.Price.Location.ToString() == "Left")
                return string.Format("{0}{1}", SiteConfiguration.Instance.Data.Global.Price.Symbol, price.ToString(SiteConfiguration.Instance.Data.Global.Price.Format));
            else
                return string.Format("{0}{1}", price.ToString(SiteConfiguration.Instance.Data.Global.Price.Format), SiteConfiguration.Instance.Data.Global.Price.Symbol);
        }
    }
}

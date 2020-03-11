using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.Manager;
using TVPPro.SiteManager.DataLoaders;
using TVPPro.SiteManager.Context;
using TVPPro.Configuration.Technical;

namespace TVPPro.SiteManager.Helper
{
    //Struct containing Branding Info
    public struct BrandingInfo
    {
        private string m_recurringImage;
        private string m_brandingImage;
        private string m_brandingHeight;
        private Enums.eBrandingRecurringType m_recurringType;

        public BrandingInfo(string recurringImage, string brandingImage, string brandingHeight, Enums.eBrandingRecurringType recType)
        {
            m_recurringImage = recurringImage;
            m_brandingImage = brandingImage;
            m_brandingHeight = brandingHeight;
            m_recurringType = recType;
        }

        public string RecurringImage
        {
            get
            {
                return m_recurringImage;
            }
        }

        public string BrandingImage
        {
            get
            {
                return m_brandingImage;
            }
        }

        public string BrandingHeight
        {
            get
            {
                return m_brandingHeight;
            }
        }

        public Enums.eBrandingRecurringType RecurringType
        {
            get
            {
                return m_recurringType;
            }
        }
    }


    public class BrandingHelper
    {
        //Get media branding
        public static BrandingInfo GetBrandingInfo(dsItemInfo mediaInfo, string brandingImageSize)
        {
            BrandingInfo retVal;
            PageContext PC = PageData.Instance.GetCurrentPage();
            SerializableDictionary<string, string> PicDict = new SerializableDictionary<string, string>();
            List<string> BrandingPicIDs = new List<string>();
            string brandingImage = string.Empty;
            string recurringImage = string.Empty;
            string brandingHeight = string.Empty;
            Enums.eBrandingRecurringType recType = Enums.eBrandingRecurringType.None;
            
            if (mediaInfo != null)
            {
                //Try to get branding image from media
                dsItemInfo.ItemRow drMediaDetails = mediaInfo.Item[0];
                bool isSmallPicAdded = false;
                //First Check if the media has branded image, whether has take it
                if (!drMediaDetails.IsBrandingSmallImageNull() && !string.IsNullOrEmpty(drMediaDetails.BrandingSmallImage))
                {
                    brandingImage = drMediaDetails.BrandingSmallImage.ToString();
                    isSmallPicAdded = true;
                }
                //If the media has no branded image search it on the page branding image
                else if (PC.BrandingSmallImageID > 0)
                {
                    BrandingPicIDs.Add(PC.BrandingSmallImageID.ToString());
                }

                //Set body branding
                if (!drMediaDetails.IsBrandingBodyImageNull() && !string.IsNullOrEmpty(drMediaDetails.BrandingBodyImage))
                {
                    if (!string.IsNullOrEmpty(drMediaDetails.BrandingSpaceHight))
                    {
                        brandingHeight = drMediaDetails.BrandingSpaceHight;
                    }
                    recurringImage = drMediaDetails.BrandingBodyImage;
                    recType = (Enums.eBrandingRecurringType)int.Parse(drMediaDetails.BrandingRecurring);
                }

                //Media doesnt have branding recurring image - try to get from page
                else if (PC.BrandingBigImageID > 0)
                {

                    BrandingPicIDs.Add(PC.BrandingBigImageID.ToString());
                    //User pic loader to get pic url from pic ids
                    if (BrandingPicIDs.Count > 0)
                    {
                        PicDict = new PicLoader(BrandingPicIDs.ToArray(), brandingImageSize) { PicsIDArr = BrandingPicIDs.ToArray(), ContainBranding = true }.Execute();
                        if (PicDict.Keys.Contains(PC.BrandingSmallImageID.ToString()))
                        {
                            string BodyImageUrl = (from p in PicDict
                                                   where p.Key == PC.BrandingSmallImageID.ToString()
                                                   select p.Value).First();

                            if (!string.IsNullOrEmpty(BodyImageUrl) && BodyImageUrl.Contains(';'))
                            {
                                string[] ImageUrls = new string[2];
                                ImageUrls = BodyImageUrl.Split(';');
                                if (!string.IsNullOrEmpty(ImageUrls[0]))
                                {
                                    recurringImage = ImageUrls[0];
                                    recType = GetRecurringType(PC.BrandingRecurringHorizonal, PC.BrandingRecurringVertical);
                                }
                            }
                        }

                        if (PicDict.Keys.Contains(PC.BrandingBigImageID.ToString()))
                        {
                            string SmallImageUrl = (from p in PicDict
                                                    where p.Key == PC.BrandingBigImageID.ToString()
                                                    select p.Value).First();

                            if (!string.IsNullOrEmpty(SmallImageUrl) && SmallImageUrl.Contains(';') && !isSmallPicAdded)
                            {
                                brandingImage = SmallImageUrl.Substring(0, SmallImageUrl.IndexOf(';'));
                                brandingHeight = PC.BrandingPixelHeigt.ToString();

                            }
                        }
                    }
                }


                retVal = new BrandingInfo(recurringImage, brandingImage, brandingHeight, recType);
            }
            else
            {
                //No media - try to get from page
                retVal = GetBrandingInfoFromPage(brandingImageSize);
            }
            return retVal;
        }


        //Get branding info from page (disregarding media info)
        public static BrandingInfo GetBrandingInfoFromPage(string brandingPicSize)
        {
            BrandingInfo retVal;
            PageContext PC = PageData.Instance.GetCurrentPage();
            SerializableDictionary<string, string> PicDict = new SerializableDictionary<string, string>();
            List<string> BrandingPicIDs = new List<string>();
            string brandingImage = string.Empty;
            string recurringImage = string.Empty;
            string brandingHeight = string.Empty;
            Enums.eBrandingRecurringType recType = Enums.eBrandingRecurringType.None;

            if (PC.BrandingSmallImageID > 0)
            {
                BrandingPicIDs.Add(PC.BrandingSmallImageID.ToString());
            }


            if (PC.BrandingBigImageID > 0 || BrandingPicIDs.Count > 0 )
            {

                BrandingPicIDs.Add(PC.BrandingBigImageID.ToString());


                if (BrandingPicIDs.Count > 0)
                {
                    PicDict = new PicLoader(BrandingPicIDs.ToArray(), brandingPicSize, TechnicalConfiguration.Instance.Data.TVM.Configuration.User, TechnicalConfiguration.Instance.Data.TVM.Configuration.Password) { PicsIDArr = BrandingPicIDs.ToArray(), ContainBranding = true }.Execute();
                    brandingHeight = PC.BrandingPixelHeigt.ToString();
                    if (PicDict.Keys.Contains(PC.BrandingSmallImageID.ToString()))
                    {
                        string BodyImageUrl = (from p in PicDict
                                               where p.Key == PC.BrandingSmallImageID.ToString()
                                               select p.Value).First();

                        if (!string.IsNullOrEmpty(BodyImageUrl))
                        {

                            recurringImage = BodyImageUrl;
                            recType = GetRecurringType(PC.BrandingRecurringHorizonal, PC.BrandingRecurringVertical);

                        }
                    }

                    if (PicDict.Keys.Contains(PC.BrandingBigImageID.ToString()))
                    {
                        string SmallImageUrl = (from p in PicDict
                                                where p.Key == PC.BrandingBigImageID.ToString()
                                                select p.Value).First();

                        if (!string.IsNullOrEmpty(SmallImageUrl) && !SmallImageUrl.Contains(';'))
                        {
                            brandingImage = SmallImageUrl;
                        }
                        else if (!string.IsNullOrEmpty(SmallImageUrl))
                        {
                            brandingImage = SmallImageUrl.Substring(0, SmallImageUrl.IndexOf(';'));
                        }
                    }
                }
            }
            retVal = new BrandingInfo(recurringImage, brandingImage, brandingHeight, recType);
            return retVal;
        }


        private static Enums.eBrandingRecurringType GetRecurringType(int horizontal, int vertical)
        {
            Enums.eBrandingRecurringType retVal = Enums.eBrandingRecurringType.None;
            if (horizontal > 0)
            {
                if (vertical > 0)
                {
                    retVal = Enums.eBrandingRecurringType.Both;
                }
                else
                {
                    retVal = Enums.eBrandingRecurringType.Horizontal;
                }
            }
            else
            {
                if (vertical > 0)
                {
                    retVal = Enums.eBrandingRecurringType.Vertical;
                }
            }
            return retVal;
        }
        public static string GetFileFormatRegular()
        {
            return TechnicalConfiguration.Instance.Data.TVM.FlashVars.Branding.BrandingRegular.BrandingMainFormat;
        }
        public static string GetFileFormatVirtual()
        {
            return TechnicalConfiguration.Instance.Data.TVM.FlashVars.Branding.BrandingVirtual.BrandingMainFormat;
        }
        public static string GetRepeatFileFormatVirtual()
        {
            return TechnicalConfiguration.Instance.Data.TVM.FlashVars.Branding.BrandingVirtual.BrandingRepeatFormat;
        }
        public static string GetRepeatFileFormatRegular()
        {
            return TechnicalConfiguration.Instance.Data.TVM.FlashVars.Branding.BrandingRegular.BrandingRepeatFormat;
        }

    }
}

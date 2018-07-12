using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
using TVPPro.SiteManager.TvinciPlatform.Pricing;
using TVPPro.SiteManager.DataEntities;
using System.Collections;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.api;


namespace TVPPro.SiteManager.Helper
{
    public class PackageHelper
    {
        

        public static dsPackages GetPermittedPackagesAsdsPackages(PermittedSubscriptionContainer[] PermitedPackages)
        {   
            dsPackages Packages = new dsPackages();
            
            //Gett Permited packages and return it as dsPackages typed dataset
            var PackagesPermited = from package in PermitedPackages
                                   select new { package.m_sSubscriptionCode, package.m_dCurrentDate, package.m_dEndDate, package.m_dLastViewDate,
                                   package.m_dPurchaseDate, package.m_nCurrentUses, package.m_nMaxUses};


            foreach (var item in PackagesPermited)
            {
                Packages.UserPackages.Rows.Add(item.m_sSubscriptionCode, item.m_dCurrentDate, item.m_dEndDate, item.m_dLastViewDate, item.m_dPurchaseDate,
                    item.m_nCurrentUses, item.m_nMaxUses);
            }

            return Packages;
        }

        public static string GetPackageTitle(string SubscriptionCode, string lang, string DefaultTitle)
        {
            string m_Title = DefaultTitle;

            //Check if recived subscription code and bring subsicption data
            if (!string.IsNullOrEmpty(SubscriptionCode))
            {
                TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription SubscriptionObj = PricingService.Instance.GetSubscriptionDetailsByCode(SubscriptionCode);
                if (SubscriptionObj != null && SubscriptionObj.m_sName != null)
                {
                        //Get package title, if the title is empty or nul will return a default title
                    try
                    {
                        m_Title = (from s in SubscriptionObj.m_sName
                                   where s.LanguageCode.ToLower() == lang.ToLower()
                                   select s.Value).First();
                    }
                    catch (Exception)
                    {

                    }
                    if(string.IsNullOrEmpty(m_Title))
                        m_Title = DefaultTitle;
                }
            }
            
            return m_Title;
        }

        public static TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription GetPackageDataById(string SubscriptionCode)
        {
            TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription SubscriptionObj = null;

            //Check if recived subscription code and bring subsicption data
            if (!string.IsNullOrEmpty(SubscriptionCode))
            {
                SubscriptionObj = PricingService.Instance.GetSubscriptionDetailsByCode(SubscriptionCode);
            }

            return SubscriptionObj;
        }

        public static int GetFileTypeIdByName(string FileTypeName, int GroupID)
        {
            int FileTypeId = 0;

            if (!string.IsNullOrEmpty(FileTypeName))
            {
                //Get all Files type(trailer, poster , main flv...)
                Dictionary<int, FileTypeContainer[]> FileType = ApiService.Instance.GetFileTypes(GroupID);

                if (FileType != null && FileType.Count > 0 && FileType.Keys.Contains(GroupID))
                {
                    FileTypeContainer[] TypeContainer = FileType[GroupID];

                    //Get File type id By file type name
                    FileTypeId = (from f in TypeContainer
                                  where f.m_sType == FileTypeName
                                  select f.m_nFileTypeID).FirstOrDefault();
                }
            }

            return FileTypeId;
        }

        public static dsPackages GetSuscriptionsForItem(int MediaId, int FileType, string lang)
        {
            dsPackages ItemPackages = new dsPackages();
            string sKey = string.Format("{0}_{1}_{2}", MediaId.ToString(), FileType.ToString(), lang.ToString());
            
            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is dsPackages) return (oFromCache as dsPackages);

            // if no cache get data from webservice
            if (MediaId > 0 && FileType > 0)
            {
                TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] ItemSubscriptions = PricingService.Instance.GetSubscriptionsForSingleItem(MediaId, FileType);
                
                ItemPackages = SubscriptionsToDataset(ItemSubscriptions, lang);
                // save return data to cache
                DataHelper.SetCacheObject(sKey, ItemPackages);
            }
            return ItemPackages;
        }

        public static string GetSuscriptionsForItemSTR(int MediaId, int FileType, string lang)
        {
            string ItemPackages = string.Empty;
            string sKey = string.Format("{0}_{1}_{2}", MediaId.ToString(), FileType.ToString(), lang.ToString());

            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is string) return (oFromCache as string);

            // if no cache get data from webservice
            if (MediaId > 0 && FileType > 0)
            {
                ItemPackages = PricingService.Instance.GetSubscriptionsContainingMediaSTR(MediaId, FileType);

                //ItemPackages = SubscriptionsToDataset(ItemSubscriptions, lang);
                // save return data to cache
                DataHelper.SetCacheObject(sKey, ItemPackages);
            }
            return ItemPackages;
        }

        public static dsPackages GetIndexedSuscriptionsForItem(int MediaId, int FileType, string lang, int iCount)
        {
            dsPackages ItemPackages = new dsPackages();
            string sKey = string.Format("{0}_{1}_{2}", MediaId.ToString(), FileType.ToString(), lang.ToString());

            // return object from cache if exist
            object oFromCache = DataHelper.GetCacheObject(sKey);
            if (oFromCache != null && oFromCache is dsPackages) return (oFromCache as dsPackages);

            // if no cache get data from webservice
            if (MediaId > 0 && FileType > 0)
            {
                TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] ItemSubscriptions = PricingService.Instance.GetIndexedSubscriptionsForSingleItem(MediaId, FileType, iCount);

                ItemPackages = SubscriptionsToDataset(ItemSubscriptions, lang);
                // save return data to cache
                DataHelper.SetCacheObject(sKey, ItemPackages);
            }
            return ItemPackages;
        }

        public static dsPackages SubscriptionsToDataset(TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription[] ItemSubscriptions, string lang)
        {
            dsPackages dsPackagesRet = new dsPackages();

            string PackageName = string.Empty;
            string PackageDescription = string.Empty;
            string SubscriptionId = string.Empty;
            string PackageID = string.Empty;

            if (ItemSubscriptions != null && ItemSubscriptions.Length > 0)
            {
                foreach (TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription sub in ItemSubscriptions)
                {
                    PackageName = string.Empty;
                    PackageDescription = string.Empty;
                    SubscriptionId = string.Empty;
                    PackageID = string.Empty;

                    dsPackages.PackagesRow pack = dsPackagesRet.Packages.NewPackagesRow();
                    //Get package name from subscription
                    PackageName = (from p in sub.m_sName
                                   where p.LanguageCode.Contains(lang)
                                   select p.Value).FirstOrDefault();

                    //Get description name from subscription
                    PackageDescription = (from p in sub.m_sDescription
                                          where p.LanguageCode.Contains(lang)
                                          select p.Value).FirstOrDefault();


                    if (sub.m_sCodes != null && sub.m_sCodes.Count() > 0)
                    {
                        PackageID = sub.m_sCodes[0].m_sCode;
                    }

                    pack.Title = PackageName;
                    pack.Description = PackageDescription;
                    pack.SubscriptionId = sub.m_sObjectCode;
                    pack.PackageID = PackageID;
                    pack.StartDate = sub.m_dStartDate;
                    pack.EndDate = sub.m_dEndDate;

                    dsPackagesRet.Packages.Rows.Add(pack);
                }
            }

            return dsPackagesRet;
        }

        public static int[] GetMediaListFromSubscriptionID(string SubscriptionID)
        {
            int[] mediaListRet = new int[] { };
            TVPPro.SiteManager.TvinciPlatform.Pricing.Subscription ItemSubscriptions = GetPackageDataById(SubscriptionID);
            if (ItemSubscriptions != null)
            {
                int[] fileTypes = ItemSubscriptions.m_sFileTypes;
                if (fileTypes != null && fileTypes.Length > 0)
                {
                    mediaListRet = PricingService.Instance.GetSubscriptionMediaList(SubscriptionID, fileTypes[0], string.Empty);
                }
            }
            return mediaListRet;
        }
    }
}

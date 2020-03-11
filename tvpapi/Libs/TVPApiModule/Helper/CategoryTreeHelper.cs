using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPApiModule.CatalogLoaders;
using TVPPro.SiteManager.Helper;

/// <summary>
/// Summary description for CategoryTreeHelper
/// </summary>
/// 

namespace TVPApi
{
    public class CategoryTreeHelper
    {

        static ILoaderCache m_dataCaching = LoaderCacheLite.Current;

        //public static Category GetCategoryTree(int categoryID, int groupID, PlatformType platform)
        //{
        //    Category retVal = null;
        //    if (m_dataCaching.TryGetData<Category>(GetUniqueCacheKey(categoryID, groupID), out retVal))
        //    {
        //        return retVal;
        //    }
        //    TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular);
        //    dsCategory categoryDS = (new CategoryTreeLoader(account.TVMUser, account.TVMUser, categoryID)).Execute();
        //    if (categoryDS != null)
        //    {
        //        CategoryTreeBuilder catBuilder = new CategoryTreeBuilder(categoryID.ToString(), categoryDS);
        //        retVal = catBuilder.BuildCategoryTree();
        //    }
        //    m_dataCaching.AddData(GetUniqueCacheKey(categoryID, groupID), retVal, new string[] { }, 3600);
        //    return retVal;
        //}

        public static Category GetCategoryTree(int categoryID, int groupID, PlatformType platform, string language)
        {
            Category retVal = null;

            if (m_dataCaching.TryGetData<Category>(GetUniqueCacheKey(categoryID, groupID), out retVal))
            {
                return retVal;
            }

            CategoryResponse categoryResponse = new APICategoryLoader(groupID, platform.ToString(), SiteHelper.GetClientIP(), categoryID, language).Execute() as CategoryResponse;

            if (categoryResponse != null)
            {
                CategoryTreeBuilder catBuilder = new CategoryTreeBuilder(categoryID.ToString(), categoryResponse, string.Empty);
                retVal = catBuilder.BuildCategoryTree();
            }
            m_dataCaching.AddData(GetUniqueCacheKey(categoryID, groupID), retVal, new string[] { }, 3600);
            return retVal;




        }

        private static string GetUniqueCacheKey(int categoryID, int groupID)
        {
            return string.Format("{0}_{1}_{2}", "Category", categoryID.ToString(), groupID.ToString());
        }

        public static Category GetFullCategoryTree(int categoryID, string picSize, int groupID, PlatformType platformType, string language)
        {
            Category retVal = null;
            //if (false && m_dataCaching.TryGetData<Category>(GetUniqueCacheKey(categoryID, groupID), out retVal))
            //{
            //    return retVal;
            //}
            CategoryResponse categoryResponse = new APICategoryLoader(groupID, platformType.ToString(), SiteHelper.GetClientIP(), categoryID, language).Execute() as CategoryResponse;
            
            if (categoryResponse != null)
            {
                CategoryTreeBuilder catBuilder = new CategoryTreeBuilder(categoryID.ToString(), categoryResponse, picSize);
                retVal = catBuilder.BuildFullCategoryTree();                
            }
            m_dataCaching.AddData(GetUniqueCacheKey(categoryID, groupID), retVal, new string[] { }, 3600);
            return retVal;
        }
    }
}

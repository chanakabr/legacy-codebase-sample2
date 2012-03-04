using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataEntities;
using TVPPro.SiteManager.DataLoaders;
using Tvinci.Data.DataLoader;

/// <summary>
/// Summary description for CategoryTreeHelper
/// </summary>
/// 

namespace TVPApi
{
    public class CategoryTreeHelper
    {

        static ILoaderCache m_dataCaching = LoaderCacheLite.Current;

        public static Category GetCategoryTree(int categoryID, int groupID, PlatformType platform)
        {
            Category retVal = null;
            if (m_dataCaching.TryGetData<Category>(GetUniqueCacheKey(categoryID, groupID), out retVal))
            {
                return retVal;
            }
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, platform).GetTVMAccountByAccountType(AccountType.Regular);
            dsCategory categoryDS = (new CategoryTreeLoader(account.TVMUser, account.TVMUser, categoryID)).Execute();
            if (categoryDS != null)
            {
                CategoryTreeBuilder catBuilder = new CategoryTreeBuilder(categoryID.ToString(), categoryDS);
                retVal = catBuilder.BuildCategoryTree();
            }
            m_dataCaching.AddData(GetUniqueCacheKey(categoryID, groupID), retVal, new string[] { }, 3600);
            return retVal;
        }

        private static string GetUniqueCacheKey(int categoryID, int groupID)
        {
            return string.Format("{0}_{1}_{2}", "Category", categoryID.ToString(), groupID.ToString());
        }

        public static Category GetFullCategoryTree(int categoryID, int groupID, PlatformType platformType)
        {
            Category retVal = null;
            if (false && m_dataCaching.TryGetData<Category>(GetUniqueCacheKey(categoryID, groupID), out retVal))
            {
                return retVal;
            }
            TVMAccountType account = SiteMapManager.GetInstance.GetPageData(groupID, platformType).GetTVMAccountByAccountType(AccountType.Regular);
            dsCategory categoryDS = (new FullCategoryTreeLoader(account.TVMUser, account.TVMUser, categoryID)).Execute();
            if (categoryDS.Categories[0].ID == "0")
            {
                account = SiteMapManager.GetInstance.GetPageData(groupID, platformType).GetTVMAccountByAccountType(AccountType.Parent);
                categoryDS = (new FullCategoryTreeLoader(account.TVMUser, account.TVMUser, categoryID)).Execute();
            }
            if (categoryDS != null)
            {
                CategoryTreeBuilder catBuilder = new CategoryTreeBuilder(categoryID.ToString(), categoryDS);
                retVal = catBuilder.BuildFullCategoryTree();                
            }
            m_dataCaching.AddData(GetUniqueCacheKey(categoryID, groupID), retVal, new string[] { }, 3600);
            return retVal;
        }
    }
}

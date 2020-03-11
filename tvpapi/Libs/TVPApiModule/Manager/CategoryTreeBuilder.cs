using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.DataEntities;

/// <summary>
/// Summary description for CategoryTreeBuilder
/// </summary>
/// 
namespace TVPApi
{
    public class CategoryTreeBuilder
    {
        private string m_rootID;
        private CategoryResponse m_categoryResponse;
        private string m_picSize;

        public CategoryTreeBuilder(string rootID, CategoryResponse categoryResponse, string picSize)
        {
            m_rootID = rootID;
            m_categoryResponse = categoryResponse;
            m_picSize = picSize;
        }

        public CategoryTreeBuilder()
        {
            m_rootID = string.Empty;
            m_categoryResponse = null;
            m_picSize = string.Empty;
        }

        public Category BuildCategoryTree()
        {
            Category retVal = null;
            CategoryResponse rootCategory = m_categoryResponse;
            IEnumerable<CategoryResponse> innerCategories = m_categoryResponse.m_oChildCategories;

            if (rootCategory != null)
            {
                retVal = CreateCategory(rootCategory);
                foreach (CategoryResponse cat in innerCategories)
                {
                    Category innerCat = CreateCategory(cat);
                    if (retVal.InnerCategories == null)
                    {
                        retVal.InnerCategories = new List<Category>();
                    }
                    retVal.InnerCategories.Add(innerCat);
                }
            }
            return retVal;
        }

        private Category CreateCategory(CategoryResponse categoryResponse)
        {
            Category retVal = null;
            retVal = new Category(categoryResponse, m_picSize);
            retVal.m_pictures = categoryResponse.m_lPics;
            retVal.Channels = new List<Channel>();
            foreach (channelObj channel in categoryResponse.m_oChannels)
                retVal.Channels.Add(new Channel(channel, m_picSize));

            return retVal;
        }

        public Category BuildFullCategoryTree()
        {
            Category retVal = null;
            CategoryResponse rootCategory = m_categoryResponse;

            if (rootCategory != null)
                retVal = BuildRecursive(rootCategory);

            return retVal;
        }

        private Category BuildRecursive(CategoryResponse categoryResponse)
        {
            if (categoryResponse == null)
                return null;

            Category currentCat = CreateCategory(categoryResponse);
            currentCat.InnerCategories = new List<Category>();

            foreach (CategoryResponse child in categoryResponse.m_oChildCategories)
                currentCat.InnerCategories.Add(BuildRecursive(child));

            return currentCat;
        }
    }
}

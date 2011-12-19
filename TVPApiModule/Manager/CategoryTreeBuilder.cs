using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
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
        private dsCategory m_categoryDS;

        public CategoryTreeBuilder(string rootID, dsCategory categoryDS)
        {
            m_rootID = rootID;
            m_categoryDS = categoryDS;
        }

        public CategoryTreeBuilder()
        {
            m_rootID = string.Empty;
            m_categoryDS = null;
        }

        public Category BuildCategoryTree()
        {
            Category retVal = null;
            dsCategory.CategoriesRow rootRow = (from categories in m_categoryDS.Categories
                                                where categories.ID.Equals(m_rootID)
                                                select categories).FirstOrDefault();


            IEnumerable<dsCategory.CategoriesRow> innerCategories = (from categories in m_categoryDS.Categories
                                                                     where !(categories.ID.Equals(m_rootID))
                                                                     select categories);

            if (rootRow != null)
            {
                retVal = CreateCategory(rootRow);
                foreach (dsCategory.CategoriesRow catRow in innerCategories)
                {
                    Category innerCat = CreateCategory(catRow);
                    if (retVal.InnerCategories == null)
                    {
                        retVal.InnerCategories = new List<Category>();
                    }
                    retVal.InnerCategories.Add(innerCat);
                }
            }
            return retVal;
        }

        private Category CreateCategory(dsCategory.CategoriesRow catRow)
        {
            Category retVal = null;
            retVal = new Category(catRow);
            dsCategory.ChannelsRow[] channels = catRow.GetChannelsRows();
            foreach (dsCategory.ChannelsRow channel in channels)
            {
                if (retVal.Channels == null)
                {
                    retVal.Channels = new List<Channel>();
                }
                retVal.Channels.Add(new Channel(channel));
            }
            return retVal;
        }

        public Category BuildFullCategoryTree()
        {
            Category retVal = null;
            dsCategory.CategoriesRow rootRow = (from categories in m_categoryDS.Categories
                                                where categories.ID.Equals(m_rootID)
                                                select categories).FirstOrDefault();

            if (rootRow != null)                            
                retVal = BuildRecursive(rootRow);
            
            return retVal;
        }

        private Category BuildRecursive(dsCategory.CategoriesRow row)
        {
            if (row == null)
                return null;

            Category currentCat = CreateCategory(row);

            if (row.GetChildRows("CategoryToParent").Count() > 0)
                currentCat.InnerCategories = new List<Category>();

            foreach (dsCategory.CategoriesRow child in row.GetChildRows("CategoryToParent"))            
                currentCat.InnerCategories.Add(BuildRecursive(child));            

            return currentCat;
        }
    }
}

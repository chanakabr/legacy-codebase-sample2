using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataEntities;

/// <summary>
/// Summary description for Category
/// </summary>
/// 

namespace TVPApi
{
    public class Category
    {
        private List<Category> m_innerCategories;
        private List<Channel> m_channels;
        public string Title { get; set; }
        public string ID { get; set; }

        public Category(dsCategory.CategoriesRow catRow)
        {
            Title = catRow.Title;
            ID = catRow.ID;
        }

        public Category()
        {
            Title = string.Empty;
            ID = string.Empty;
        }



        public List<Channel> Channels
        {
            get
            {
                return m_channels;
            }
            set
            {
                m_channels = new List<Channel>();
            }
        }

        public List<Category> InnerCategories
        {
            get
            {
                return m_innerCategories;
            }
            set
            {
                m_innerCategories = value;
            }
        }

    }
}

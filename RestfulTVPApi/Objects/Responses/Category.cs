using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPPro.SiteManager.DataEntities;

/// <summary>
/// Summary description for Category
/// </summary>
/// 

namespace RestfulTVPApi.Objects.Responses
{
    public class Category
    {
        private List<Category> m_innerCategories;
        private List<Channel> m_channels;
        public string title { get; set; }
        public string id { get; set; }
        public string pic_url { get; set; }

        public Category(dsCategory.CategoriesRow catRow)
        {
            title = catRow.Title;
            id = catRow.ID;
            if (!catRow.IsPicURLNull())
            {
                pic_url = catRow.PicURL;
            }
        }

        public Category()
        {
            title = string.Empty;
            id = string.Empty;
            pic_url = string.Empty;
        }



        public List<Channel> channels
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

        public List<Category> inner_categories
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

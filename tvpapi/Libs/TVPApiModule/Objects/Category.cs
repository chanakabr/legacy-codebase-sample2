using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
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
        public string PicURL { get; set; }
        public string CoGuid { get; set; }
        public List<Picture> m_pictures;


        public Category(CategoryResponse categoryResponse, string picSize)
        {
            Title = categoryResponse.m_sTitle;
            ID = categoryResponse.ID.ToString();
            CoGuid = categoryResponse.m_sCoGuid;
            if (!string.IsNullOrEmpty(picSize) && categoryResponse.m_lPics != null)
            {
                var pic = categoryResponse.m_lPics.Where(p => p.m_sSize.ToLower() == picSize.ToLower()).FirstOrDefault();
                PicURL = pic == null ? string.Empty : pic.m_sURL;
            }
        }

        public Category()
        {
            Title = string.Empty;
            ID = string.Empty;
            PicURL = string.Empty;
            CoGuid = string.Empty;
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

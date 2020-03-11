using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;

/// <summary>
/// Summary description for PageInfo
/// </summary>
/// 

namespace TVPApi
{
    //A shallow page object - holds gallery ids instead of actual galleries
    public class PageInfo
    {

        public long ID { get; set; }
        public long PageMetadataID { get; set; }
        public string BreadCrumbText { get; set; }
        public string URL { get; set; }
        public bool HasPlayer { get; set; }
        public bool HasCarousel { get; set; }
        public bool HasMiddleFooter { get; set; }
        public bool PlayerAutoPlay { get; set; }
        public TVPApi.Pages PageToken { get; set; }

        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public long PlayerChannel { get; set; }
        public long PlayerTreeCategory { get; set; }
        public long CarouselChannel { get; set; }

        public long SideProfileID { get; set; }
        public long BottomProfileID { get; set; }
        public long MenuID { get; set; }
        public long FooterID { get; set; }
        public long MiddleFooterID { get; set; }
        public long ProfileID { get; set; }
        public bool IsProtected { get; set; }

        private PageContext m_Page;
        private List<long> m_Children = new List<long>();

        public List<long> Children
        {
            get
            {
                return m_Children;
            }
            set
            {
                m_Children = value;
            }
        }

        public void SetPage(PageContext page)
        {
            m_Page = page;
        }

        //Return gallery IDs instead of full gallery objects
        public List<long> MainGalleriesIDs
        {
            get
            {
                List<long> retVal = null;
                if (m_Page != null && m_Page.MainGalleries != null)
                {
                    IEnumerable<long> galleryList = from galleries in m_Page.MainGalleries
                                                    select galleries.GalleryID;
                    if (galleryList != null && galleryList.Count() > 0)
                    {
                        retVal = galleryList.ToList<long>();
                    }

                }
                return retVal;
            }
            set
            {
            }
        }

        //Return gallery IDs instead of full gallery objects
        public List<long> TopGalleriesIDs
        {
            get
            {
                List<long> retVal = null;
                if (m_Page != null && m_Page.TopGalleries != null)
                {
                    IEnumerable<long> galleryList = from galleries in m_Page.TopGalleries
                                                    select galleries.GalleryID;
                    if (galleryList != null && galleryList.Count() > 0)
                    {
                        retVal = galleryList.ToList<long>();
                    }

                }
                return retVal;
            }
            set
            {
            }
        }


        public PageInfo()
        {

        }
    }
}

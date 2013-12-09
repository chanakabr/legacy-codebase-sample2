using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;

/// <summary>
/// Summary description for SiteMap
/// </summary>
/// 

namespace TVPApi
{
    public class SiteMap
    {
        //List of site pages
        private List<PageContext> m_Pages = new List<PageContext>();
        //List of full side profiles
        private List<Profile> m_SideProfiles = new List<Profile>();
        //List of full bottom profiles
        private List<Profile> m_BottomProfiles = new List<Profile>();
        //List of site menues
        private List<Menu> m_Menues;
        //List of site shallow page info
        private List<PageInfo> m_pagesInfo;
        //List of site footers
        private List<Menu> m_Footers;
        //List of shallow side profile info
        private List<ProfileInfo> m_SideProfileInfo;
        //List of shallow bottom profile info
        private List<ProfileInfo> m_BottomProfileInfo;

        public SiteMap()
        {

        }

        public List<Menu> Menues
        {
            get
            {
                if (m_Menues == null)
                {
                    m_Menues = new List<Menu>();
                }
                return m_Menues;
            }
            set
            {
                m_Menues = value;
            }
        }

        public List<PageInfo> PagesInfo
        {
            get
            {
                if (m_pagesInfo == null)
                {
                    m_pagesInfo = new List<PageInfo>();
                }
                return m_pagesInfo;
            }
            set
            {
                m_pagesInfo = new List<PageInfo>();
            }
        }

        public List<Menu> Footers
        {
            get
            {
                if (m_Footers == null)
                {
                    m_Footers = new List<Menu>();
                }
                return m_Footers;
            }
            set
            {
                m_Footers = value;
            }
        }

        public List<ProfileInfo> SideProfilesInfo
        {
            get
            {
                if (m_SideProfileInfo == null)
                {
                    m_SideProfileInfo = new List<ProfileInfo>();
                }
                return m_SideProfileInfo;
            }
            set
            {
                m_SideProfileInfo = value;
            }
        }

        public List<ProfileInfo> BottomProfileInfo
        {
            get
            {
                if (m_BottomProfileInfo == null)
                {
                    m_BottomProfileInfo = new List<ProfileInfo>();
                }
                return m_BottomProfileInfo;
            }
            set
            {
                m_BottomProfileInfo = value;
            }
        }



        public List<PageContext> GetPages()
        {
            return m_Pages;
        }

        public List<Profile> GetSideProfiles()
        {
            return m_SideProfiles;
        }

        public List<Profile> GetBottomProfiles()
        {
            return m_BottomProfiles;
        }
    }
}

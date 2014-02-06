using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

/// <summary>
/// Summary description for ProfileInfo
/// </summary>
/// 
namespace TVPApi
{
    //A shallow profile object to return in site map (holds gallery ids instead of galleries)
    public class ProfileInfo
    {

        public long ProfileID { get; set; }
        private Profile m_Profile { get; set; }



        public ProfileInfo()
        {

        }

        public void SetProfile(Profile profile)
        {
            m_Profile = profile;
        }

        //Return gallery IDs instead of full gallery object
        public List<long> GalleryIDs
        {
            get
            {
                List<long> retVal = null;
                if (m_Profile != null && m_Profile.Galleries != null)
                {
                    retVal = (from galleries in m_Profile.Galleries
                              select galleries.GalleryID).ToList<long>();
                }
                return retVal;
            }
        }
    }
}

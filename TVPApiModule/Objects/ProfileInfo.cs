using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApiModule.Objects;

/// <summary>
/// Summary description for ProfileInfo
/// </summary>
/// 
namespace TVPApiModule.Objects
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
                              select galleries.gallery_id).ToList<long>();
                }
                return retVal;
            }
        }
    }
}

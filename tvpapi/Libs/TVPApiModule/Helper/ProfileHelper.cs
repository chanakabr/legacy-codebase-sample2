using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TVPApi;
/// <summary>
/// Summary description for ProfileHelper
/// </summary>
/// 

namespace TVPApi
{
    public class ProfileHelper
    {
        public ProfileHelper()
        {
        }

        public static List<Profile> GetSideProfiles(InitializationObject initObj, int groupID)
        {
            List<Profile> retVal = null;
            SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            if (siteMap != null)
            {
                retVal = siteMap.GetSideProfiles();
            }
            return retVal;
        }

        public static List<Profile> GetBottomProfiles(InitializationObject initObj, int groupID)
        {
            List<Profile> retVal = null;
            SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            if (siteMap != null)
            {
                retVal = siteMap.GetBottomProfiles();
            }
            return retVal;
        }

        public static Profile GetBottomProfile(InitializationObject initObj, long ID, int groupID)
        {
            Profile retVal = null;
            SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            if (siteMap != null)
            {
                List<Profile> bottomProfiles = siteMap.GetBottomProfiles();
                retVal = GetProfileFromList(bottomProfiles, ID);
            }
            return retVal;
        }

        public static Profile GetSideProfile(InitializationObject initObj, long ID, int groupID)
        {
            Profile retVal = null;
            SiteMap siteMap = SiteMapManager.GetInstance.GetSiteMapInstance(groupID, initObj.Platform, initObj.Locale);
            if (siteMap != null)
            {
                List<Profile> sideProfiles = siteMap.GetSideProfiles();
                retVal = GetProfileFromList(sideProfiles, ID);
            }
            return retVal;
        }

        public static ProfileInfo ParseProfileToProfileInfo(Profile profile)
        {
            ProfileInfo retVal = new ProfileInfo();
            retVal.ProfileID = profile.ProfileID;
            retVal.SetProfile(profile);
            return retVal;
        }

        private static Profile GetProfileFromList(List<Profile> profileList, long ID)
        {
            Profile retVal = null;

            var profile = from profiles in profileList
                          where profiles.ProfileID == ID
                          select profiles;


            retVal = profile as Profile;

            return retVal;
        }


    }
}

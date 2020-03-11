using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class PeopleWhoLikedLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int MediaID { get; set; }
        public int CountryID { get; set; }
        public int MediaFileID { get; set; }
        public int SocialAction { get; set; }
        public int SocialPlatform { get; set; }

        #region Constructors
        public PeopleWhoLikedLoader(int mediaID, int mediaFileID, int countryID, int socialAction, int socialPlatform, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            MediaID = mediaID;
            MediaFileID = mediaFileID;
            CountryID = countryID;
            SocialAction = socialAction;
            SocialPlatform = socialPlatform;
        }

        public PeopleWhoLikedLoader(int mediaID, int mediaFileID, int countryID, int socialAction, int socialPlatform, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(mediaID, mediaFileID, countryID, socialAction, socialPlatform, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new PWLALProtocolRequest()
            {
                m_nMediaID = MediaID,
                m_nMediaFileID = MediaFileID,
                m_nCountryID = CountryID,
                m_nSocialAction = SocialAction,
                m_nSocialPlatform = SocialPlatform,
            };
        }

        public override string GetLoaderCachekey()
        {
            return string.Format("people_who_liked_mediaid{0}_fileid{1}_country{2}_action{3}_platform{4}_index{5}_size{6}_group{7}", MediaID, MediaFileID, CountryID, SocialAction, SocialPlatform, PageIndex, PageSize, GroupID);
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.PWLALProtocolRequest":
                        PWLALProtocolRequest peopleWhoLikedRequest = obj as PWLALProtocolRequest;
                        sText.AppendFormat("PWLALProtocolRequest: MediaID = {0}, MediaFileID = {1}, CountryID = {2}, SocialAction = {3}, SocialPlatform = {4}, GroupID = {5}, PageIndex = {6}, PageSize = {7}",
                            peopleWhoLikedRequest.m_nMediaID, peopleWhoLikedRequest.m_nMediaFileID, peopleWhoLikedRequest.m_nCountryID, peopleWhoLikedRequest.m_nSocialAction, peopleWhoLikedRequest.m_nSocialPlatform, peopleWhoLikedRequest.m_nGroupID, peopleWhoLikedRequest.m_nPageIndex, peopleWhoLikedRequest.m_nPageSize);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.MediaIdsResponse":
                        MediaIdsResponse mediaIDsResponse = obj as MediaIdsResponse;
                        sText.AppendFormat("MediaIdsResponse: TotalItems = {0}, ", mediaIDsResponse.m_nTotalItems);
                        sText.AppendLine(mediaIDsResponse.m_nMediaIds.ToStringEx());
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
            //logger.Info(sText.ToString());
        }


    }
}

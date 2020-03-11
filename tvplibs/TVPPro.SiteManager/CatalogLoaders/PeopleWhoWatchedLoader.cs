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
    [Serializable]
    public class PeopleWhoWatchedLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public int MediaID { get; set; }
        public int CountryID { get; set; }

        #region Constructors
        public PeopleWhoWatchedLoader(int mediaID, int countryID, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            MediaID = mediaID;
            CountryID = countryID;
        }

        public PeopleWhoWatchedLoader(int mediaID, int countryID, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(mediaID, countryID, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new PWWAWProtocolRequest()
            {
                m_nMediaID = MediaID,
                m_nCountryID = CountryID,
            };
        }
        public override string GetLoaderCachekey()
        {
            return string.Format("people_who_watched_mediaid{0}country{1}_index{2}_size{3}_group{4}", MediaID, CountryID, PageIndex, PageSize, GroupID);
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.PWWAWProtocolRequest":
                        PWWAWProtocolRequest peopleWhoWatchedRequest = obj as PWWAWProtocolRequest;
                        sText.AppendFormat("PWWAWProtocolRequest: MediaID = {0}, CountryID = {1}, GroupID = {2}, PageIndex = {3}, PageSize = {4}", peopleWhoWatchedRequest.m_nMediaID, peopleWhoWatchedRequest.m_nCountryID, peopleWhoWatchedRequest.m_nGroupID, peopleWhoWatchedRequest.m_nPageIndex, peopleWhoWatchedRequest.m_nPageSize);
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

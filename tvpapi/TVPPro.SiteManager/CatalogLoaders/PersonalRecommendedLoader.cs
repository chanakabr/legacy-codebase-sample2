using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;
using KLogMonitor;
using System.Reflection;
using Core.Catalog.Response;
using Core.Catalog.Request;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class PersonalRecommendedLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Constructors
        public PersonalRecommendedLoader(string siteGuid, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            SiteGuid = siteGuid;
        }

        public PersonalRecommendedLoader(string siteGuid, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(siteGuid, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new PersonalRecommendedRequest();
        }
        public override string GetLoaderCachekey()
        {
            return string.Format("personal_recommended_siteguid{0}_index{1}_size{2}_group{3}", SiteGuid, PageIndex, PageSize, GroupID);
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.PersonalRecommendedRequest":
                        PersonalRecommendedRequest personalRecommendedRequest = obj as PersonalRecommendedRequest;
                        sText.AppendFormat("PersonalRecommendedRequest: SiteGuid = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}", personalRecommendedRequest.m_sSiteGuid, personalRecommendedRequest.m_nGroupID, personalRecommendedRequest.m_nPageIndex, personalRecommendedRequest.m_nPageSize);
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

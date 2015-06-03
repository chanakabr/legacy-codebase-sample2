using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Manager;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using Tvinci.Data.Loaders;
using KLogMonitor;
using System.Reflection;

namespace TVPPro.SiteManager.CatalogLoaders
{
    [Serializable]
    public class PersonalLastWatchedLoader : MultiMediaLoader
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Constructors
        public PersonalLastWatchedLoader(string siteGuid, int groupID, string userIP, int pageSize, int pageIndex, string picSize)
            : base(groupID, userIP, pageSize, pageIndex, picSize)
        {
            SiteGuid = siteGuid;
        }

        public PersonalLastWatchedLoader(string siteGuid, string userName, string userIP, int pageSize, int pageIndex, string picSize)
            : this(siteGuid, PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, pageSize, pageIndex, picSize)
        {
        }
        #endregion


        protected override object Process()
        {
            List<BaseObject> lMediaObj = null;
            if (m_oResponse != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds != null && ((MediaIdsResponse)m_oResponse).m_nMediaIds.Count > 0)
            {
                m_oMediaCache = new MediaCache(((MediaIdsResponse)m_oResponse).m_nMediaIds, GroupID, m_sUserIP, m_oFilter);
                m_oMediaCache.BuildRequest();
                lMediaObj = (List<BaseObject>)m_oMediaCache.Execute();
                BaseResponse deviceResponse;
                BaseRequest deviceRequest = new PersonalLasDeviceRequest()
                {
                    m_nMediaIDs = ((MediaIdsResponse)m_oResponse).m_nMediaIds.Select(mediaRes => mediaRes.assetID).ToList(),
                    m_nGroupID = GroupID,
                    m_oFilter = m_oFilter,
                    m_sSignature = m_sSignature,
                    m_sSignString = m_sSignString,
                    m_sSiteGuid = SiteGuid,
                    m_sUserIP = m_sUserIP
                };
                if (m_oProvider.TryExecuteGetBaseResponse(deviceRequest, out deviceResponse) == eProviderResult.Success && deviceResponse is PersonalLastDeviceResponse &&
                    ((PersonalLastDeviceResponse)deviceResponse).m_lPersonalLastWatched != null && ((PersonalLastDeviceResponse)deviceResponse).m_lPersonalLastWatched.Count > 0)
                {
                    foreach (MediaObj media in lMediaObj)
                    {
                        if (media != null)
                        {
                            var mediaLastWatched = ((PersonalLastDeviceResponse)deviceResponse).m_lPersonalLastWatched.Where(lastWatched => lastWatched.m_nID.ToString() == media.AssetId).FirstOrDefault();
                            if (mediaLastWatched != null)
                            {
                                media.m_dLastWatchedDate = mediaLastWatched.m_dLastWatchedDate;
                                media.m_sLastWatchedDevice = mediaLastWatched.m_sLastWatchedDevice;
                            }
                        }
                    }
                }
            }
            return lMediaObj;
        }

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new PersonalLastWatchedRequest();
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.PersonalLastWatchedRequest":
                        PersonalLastWatchedRequest personalLastWatchedRequest = obj as PersonalLastWatchedRequest;
                        sText.AppendFormat("PersonalLastWatchedRequest: SiteGuid = {0}, GroupID = {1}, PageIndex = {2}, PageSize = {3}",
                            personalLastWatchedRequest.m_sSiteGuid, personalLastWatchedRequest.m_nGroupID, personalLastWatchedRequest.m_nPageIndex, personalLastWatchedRequest.m_nPageSize);
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

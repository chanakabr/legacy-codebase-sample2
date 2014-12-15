using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tvinci.Data.DataLoader;
using Tvinci.Data.Loaders;
using Tvinci.Data.Loaders.TvinciPlatform.Catalog;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.CatalogLoaders
{
    public class DomainLastPositionLoader : CatalogRequestManager, ILoaderAdapter
    {
        private static ILog logger = log4net.LogManager.GetLogger(typeof(DomainLastPositionLoader));        
        
        public int MediaID { get; set; }                
        public string UDID { get; set; }        
        public int DomainID { get; set; }

        #region Constructors
        public DomainLastPositionLoader(int groupID, string userIP, string siteGuid, string udid, int mediaID )
            : base(groupID, userIP, 0, 0)
        {            
            MediaID = mediaID;            
            SiteGuid = siteGuid;
            UDID = udid;            
        }

        public DomainLastPositionLoader(string userName, string userIP, string siteGuid, string udid, int mediaID)
            : this(PageData.Instance.GetTVMAccountByUserName(userName).BaseGroupID, userIP, siteGuid, udid, mediaID)
        {
        }

        public DomainLastPositionLoader(int groupID, string userIP, string siteGuid, string udid, int mediaID, Provider provider)
            : this(groupID, userIP, siteGuid, udid, mediaID)
        {
            m_oProvider = provider;
        }
        #endregion

        protected override void BuildSpecificRequest()
        {
            m_oRequest = new DomainLastPositionRequest()
            {
                data = new MediaLastPositionRequestData()
                {                    
                    m_nMediaID = MediaID,
                    m_sSiteGuid = SiteGuid,
                    m_sUDID = UDID                    
                },
                m_nDomainID = DomainID                
            };           
        }

        public object Execute()
        {
            DomainLastPositionResponse retVal = null;
            BuildRequest();
            Log("TryExecuteGetBaseResponse:", m_oRequest);
            if (m_oProvider.TryExecuteGetBaseResponse(m_oRequest, out m_oResponse) == eProviderResult.Success)
            {
                Log("Got:", m_oResponse);
                retVal = m_oResponse as DomainLastPositionResponse;
            }
            return retVal;
        }

        protected override void Log(string message, object obj)
        {
            StringBuilder sText = new StringBuilder();
            sText.AppendLine(message);
            if (obj != null)
            {
                switch (obj.GetType().ToString())
                {
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.DomainLastPositionRequest":
                        sText.AppendFormat("MediaHitRequest: groupID = {0}, userIP = {1}, siteGuid = {2}, udid = {3}, mediaID = {4}",
                            GroupID, m_sUserIP, SiteGuid, UDID, MediaID);
                        break;
                    case "Tvinci.Data.Loaders.TvinciPlatform.Catalog.DomainLastPositionResponse":
                        DomainLastPositionResponse mediaMarkResponse = obj as DomainLastPositionResponse;
                        sText.AppendFormat("MediaHitResponse: Status = {0}, ", mediaMarkResponse.m_sStatus);
                        break;
                    default:
                        break;
                }
            }
            logger.Debug(sText.ToString());
        }

        #region ILoaderAdapter not implemented methods
        public bool IsPersist()
        {
            throw new NotImplementedException();
        }

        public object Execute(eExecuteBehaivor behaivor)
        {
            throw new NotImplementedException();
        }

        public object LastExecuteResult
        {
            get { throw new NotImplementedException(); }
        }
        #endregion
    }
}

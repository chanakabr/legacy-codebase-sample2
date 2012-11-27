using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using log4net;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiServices
{
    /// <summary>
    /// Summary description for Service
    /// </summary>
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DomainService : System.Web.Services.WebService, IDomainService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(DomainService));

        #region public methods

        [WebMethod(EnableSession = true, Description = "Reset Domain")]
        public DomainResponseObject ResetDomain(InitializationObject initObj)
        {
            DomainResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ResetDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("ResetDomain-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    response = new ApiDomainsService(groupID, initObj.Platform).ResetDomain(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    logger.Error("ResetDomain->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("ResetDomain-> 'Unknown group' Username: {0}, Password: {1}, domainID: {2}", initObj.ApiUser, initObj.ApiPass, initObj.DomainID);
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Set device info")]
        public bool SetDeviceInfo(InitializationObject initObj, string deviceName)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SetDeviceInfo-> [{0}, {1}]", groupID, initObj.Platform);

            if (groupID > 0)
            {
                try
                {
                    response = new ApiDomainsService(groupID, initObj.Platform).SetDeviceInfo(initObj.UDID, deviceName);
                }
                catch (Exception ex)
                {
                    logger.Error("SetDeviceInfo->", ex);
                }
            }
            else
            {
                logger.ErrorFormat("SetDeviceInfo-> 'Unknown group' Username: {0}, Password: {1}, udid: {2}", initObj.ApiUser, initObj.ApiPass, initObj.UDID);
            }

            return response;
        }

        #endregion
    }
}

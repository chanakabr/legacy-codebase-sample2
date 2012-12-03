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


        [WebMethod(EnableSession = true, Description = "Add device to domain")]
        public DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDeviceToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("AddDeviceToDomain-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDeviceToDomain(initObj.DomainID, initObj.UDID, sDeviceName, iDeviceBrandID);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : AddDeviceToDomain, Error Message: {0} Parameters: iDomainID: {1}, sUDID: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, initObj.DomainID, initObj.UDID, sDeviceName, iDeviceBrandID);
                }
            }

            return resDomain;
        }
        
        [WebMethod(EnableSession = true, Description = "Add a user to domain")]
        public DomainResponseObject AddUserToDomain(InitializationObject initObj, bool bMaster)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddUserToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("AddUserToDomain-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {

                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddUserToDomain(initObj.DomainID, initObj.SiteGuid, bMaster);                    
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : AddUserToDomain, Error Message: {0} Parameters: sSiteGuid: {1}, bMaster: {2}", ex.Message, initObj.SiteGuid, bMaster);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Remove a user from domain")]
        public Domain RemoveUserFromDomain(InitializationObject initObj)
        {
            Domain domain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveUserFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("RemoveUserFromDomain-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveUserFromDomain(initObj.DomainID, initObj.SiteGuid);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : RemoveUserFromDomain, Error Message: {0} Parameters: iDomainID: {1}, sSiteGUID: {2}", ex.Message, initObj.DomainID, initObj.SiteGuid);
                }
            }

            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Remove device from domain")]
        public DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDeviceFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("RemoveDeviceFromDomain-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDeviceToDomain(initObj.DomainID, initObj.UDID);                    
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : RemoveDeviceFromDomain, Error Message: {0} Parameters: iDomainID: {1}, sUDID: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, initObj.DomainID, initObj.UDID, sDeviceName, iDeviceBrandID);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Activate/Deactivate a device in domain")]
        public DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeDeviceDomainStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("ChangeDeviceDomainStatus-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).ChangeDeviceDomainStatus(initObj.DomainID, initObj.UDID, bActive);                    
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : ChangeDeviceDomainStatus, Error Message: {0} Parameters: iDomainID: {1}, bActive: {2}", ex.Message, initObj.DomainID, initObj.UDID, bActive);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Get device/user domain info")]
        public Domain GetDomainInfo(InitializationObject initObj)
        {
            Domain domain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetDomainInfo-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainInfo(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetDomainInfo, Error Message: {0} Parameters: iDomainID: {1}", ex.Message, initObj.DomainID);
                }
            }

            return domain;
        }

        //XXX: Move it to domain Service
        [WebMethod(EnableSession = true, Description = "Set device/user domain info")]
        public DomainResponseObject SetDomainInfo(InitializationObject initObj, string sDomainName, string sDomainDescription)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("SetDomainInfo-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).SetDomainInfo(initObj.DomainID, sDomainName, sDomainDescription);                    
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : SetDomainInfo, Error Message: {0} Parameters: iDomainID: {1}, sDomainName: {2}, sDomainDescription: {3}", ex.Message, initObj.DomainID, sDomainName, sDomainDescription);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Get device domains")]
        public TVPApiModule.Services.ApiDomainsService.DeviceDomain[] GetDeviceDomains(InitializationObject initObj)
        {
            Domain[] domains = null;
            TVPApiModule.Services.ApiDomainsService.DeviceDomain[] devDomains = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetDeviceDomains-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    domains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDeviceDomains(initObj.UDID);

                    if (domains == null || domains.Count() == 0)
                        return devDomains;

                    devDomains = new TVPApiModule.Services.ApiDomainsService.DeviceDomain[domains.Count()];

                    for (int i = 0; i < domains.Count(); i++)
                        devDomains[i] = new TVPApiModule.Services.ApiDomainsService.DeviceDomain() { DomainID = domains[i].m_nDomainID, DomainName = domains[i].m_sName, SiteGuid = domains[i].m_masterGUIDs[0].ToString() };
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetDeviceDomains, Error Message: {0} Parameters: udid: {1}", ex.Message, initObj.UDID);
                }
            }

            return devDomains;
        }

        [WebMethod(EnableSession = true, Description = "Get PIN Code for a new device")]
        public string GetPINForDevice(InitializationObject initObj, int devBrandID)
        {
            string pin = string.Empty;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPINForDevice", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("GetPINForDevice-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    pin = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetPINForDevice(initObj.UDID, devBrandID);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : GetPINForDevice, Error Message: {0} Parameters: udid: {1}", ex.Message, initObj.UDID);
                }
            }

            return pin;
        }

        [WebMethod(EnableSession = true, Description = "Register a device to domain by PIN code")]
        public TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin)
        {
            TVPApiModule.Services.ApiDomainsService.DeviceRegistration deviceRes = new TVPApiModule.Services.ApiDomainsService.DeviceRegistration();
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RegisterDeviceByPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("RegisterDeviceByPIN-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiDomainsService service = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform);
                    DeviceResponseObject device = service.RegisterDeviceByPIN(initObj.UDID, initObj.DomainID, pin);

                    if (device == null || device.m_oDeviceResponseStatus == DeviceResponseStatus.Error)
                        deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Error;
                    else if (device.m_oDeviceResponseStatus == DeviceResponseStatus.DuplicatePin || device.m_oDeviceResponseStatus == DeviceResponseStatus.DeviceNotExists)
                        deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Invalid;
                    else
                    {
                        deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Success;
                        deviceRes.UDID = device.m_oDevice.m_deviceUDID;
                    }
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : RegisterDeviceByPIN, Error Message: {0} Parameters: udid: {1}", ex.Message, initObj.UDID);
                }
            }

            return deviceRes;
        }

        [WebMethod(EnableSession = true, Description = "Add domain to master site user")]
        public DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid)
        {
            DomainResponseObject domainRes = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            logger.InfoFormat("AddDomain-> [{0}, {1}], Params:[siteGuid: {2}]", groupID, initObj.Platform, initObj.SiteGuid);

            if (groupID > 0)
            {
                try
                {
                    new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDomain(domainName, domainDesc, masterGuid);
                }
                catch (Exception ex)
                {
                    logger.ErrorFormat("Error calling webservice protocol : AddDomain, Error Message: {0} Parameters: udid: {1}", ex.Message, initObj.UDID);
                }
            }

            return domainRes;
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using TVPApi;
using TVPPro.SiteManager.Helper;
using System.Web.Services;
using TVPPro.SiteManager.TvinciPlatform.Users;
using log4net;
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using System.Web;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects.Responses;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DomainService : System.Web.Services.WebService, IDomainService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(DomainService));

        [WebMethod(EnableSession = true, Description = "Reset Domain")]
        public DomainResponseObject ResetDomain(InitializationObject initObj)
        {
            DomainResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ResetDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new ApiDomainsService(groupID, initObj.Platform).ResetDomain(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Set device info")]
        public bool SetDeviceInfo(InitializationObject initObj, string deviceName)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    response = new ApiDomainsService(groupID, initObj.Platform).SetDeviceInfo(initObj.UDID, deviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Add device to domain")]
        public DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDeviceToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    resDomain = impl.AddDeviceToDomain(sDeviceName, iDeviceBrandID);

                    //resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDeviceToDomain(initObj.DomainID, initObj.UDID, sDeviceName, iDeviceBrandID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Add a user to domain")]
        public DomainResponseObject AddUserToDomain(InitializationObject initObj, int AddedUserGuid)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddUserToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddUserToDomain(initObj.DomainID, Convert.ToInt32(initObj.SiteGuid), AddedUserGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Remove a user from domain")]
        public DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove)
        {
            DomainResponseObject domain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveUserFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveUserFromDomain(initObj.DomainID, userGuidToRemove);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Remove device from domain")]
        public DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, int domainID, string sDeviceName, int iDeviceBrandID, string sUdid)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDeviceFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    if (!string.IsNullOrEmpty(sUdid))
                        resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDeviceToDomain(domainID, sUdid);
                    else
                        resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDeviceToDomain(domainID, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Activate/Deactivate a device in domain")]
        public DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeDeviceDomainStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).ChangeDeviceDomainStatus(initObj.DomainID, initObj.UDID, bActive);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Get device/user domain info")]
        public Domain GetDomainInfo(InitializationObject initObj)
        {
            Domain domain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainInfo(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Set device/user domain info")]
        public DomainResponseObject SetDomainInfo(InitializationObject initObj, string sDomainName, string sDomainDescription)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).SetDomainInfo(initObj.DomainID, sDomainName, sDomainDescription);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
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

            if (groupID > 0)
            {
                try
                {
                    domains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDeviceDomains(initObj.UDID);

                    if (domains == null || domains.Count() == 0)
                        return devDomains;

                    devDomains = new TVPApiModule.Services.ApiDomainsService.DeviceDomain[domains.Count()];

                    for (int i = 0; i < domains.Count(); i++)
                        devDomains[i] = new TVPApiModule.Services.ApiDomainsService.DeviceDomain()
                        {
                            DomainID = domains[i].m_nDomainID,
                            DomainName = domains[i].m_sName,
                            SiteGuid = domains[i].m_masterGUIDs[0].ToString(),
                            DefaultUser = domains[i].m_DefaultUsersIDs[0].ToString(),
                            DomainStatus = domains[i].m_DomainStatus
                        };
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return devDomains;
        }

        [WebMethod(EnableSession = true, Description = "Get PIN Code for a new device")]
        public string GetPINForDevice(InitializationObject initObj, int devBrandID)
        {
            string pin = string.Empty;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPINForDevice", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    pin = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetPINForDevice(initObj.UDID, devBrandID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return pin;
        }

        [WebMethod(EnableSession = true, Description = "Register a device to domain by PIN code")]
        public TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin)
        {
            TVPApiModule.Services.ApiDomainsService.DeviceRegistration deviceRes = new TVPApiModule.Services.ApiDomainsService.DeviceRegistration();
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RegisterDeviceByPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

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
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return deviceRes;
        }

        [WebMethod(EnableSession = true, Description = "Add domain to master site user")]
        public DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid)
        {
            DomainResponseObject domainRes = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    domainRes = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDomain(domainName, domainDesc, masterGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return domainRes;
        }

        [WebMethod(EnableSession = true, Description = "Add domain with Co-GUID to master site user")]
        public DomainResponseObject AddDomainWithCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid, string coGuid)
        {
            DomainResponseObject domainRes = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDomainWithCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    domainRes = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDomainWithCoGuid(domainName, domainDesc, masterGuid, coGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return domainRes;
        }

        //[WebMethod(EnableSession = true, Description = "Get domain CoGuid")]
        //public string GetDomainCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid)
        //{
        //    string res = string.Empty;
        //    int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

        //    if (groupID > 0)
        //    {
        //        try
        //        {
        //            ApiUsersService usersService = new ApiUsersService(groupID, initObj.Platform);
        //            UserResponseObject userResponseObject = usersService.GetUserData(initObj.SiteGuid);
        //            if (userResponseObject.m_RespStatus == ResponseStatus.OK)
        //            {
        //              res = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainCoGuid(userResponseObject.m_user.m_domianID);  
        //            }

        //        }
        //        catch (Exception ex)
        //        {
        //            HttpContext.Current.Items.Add("Error", ex);
        //        }
        //    }

        //    return res;
        //}

        [WebMethod(EnableSession = true, Description = "Get domain object by CoGuid")]
        public DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid)
        {
            DomainResponseObject res = null;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    res = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainByCoGuid(coGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get domain ID by CoGuid")]
        public int GetDomainIDByCoGuid(InitializationObject initObj, string coGuid)
        {
            int res = 0;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDByCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    res = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainIDByCoGuid(coGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Submit user to a domain request")]
        public DomainResponseObject SubmitAddUserToDomainRequest(InitializationObject initObj, string masterUsername)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SubmitAddUserToDomainRequest", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                try
                {
                    int siteGuid = 0;
                    if (int.TryParse(initObj.SiteGuid, out siteGuid))
                        resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).SubmitAddUserToDomainRequest(siteGuid, masterUsername);
                    else
                        throw new Exception("Site guid is not a valid number");
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Remove Domain")]
        public string RemoveDomain(InitializationObject initObj)
        {
            DomainResponseStatus resDomain = DomainResponseStatus.UnKnown;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDomain(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomain.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Get DomainIDs By Operator CoGuid")]
        public int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid)
        {
            int[] resDomains = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDsByOperatorCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                try
                {
                    resDomains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainIDsByOperatorCoGuid(operatorCoGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomains;
        }

        [WebMethod(EnableSession = true, Description = "Get Device Info")]
        public DeviceResponseObject GetDeviceInfo(InitializationObject initObj, string sId, bool bIsUDID)
        {
            DeviceResponseObject deviceInfo = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    deviceInfo = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).GetDeviceInfo(sId, bIsUDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return deviceInfo;

        }

        [WebMethod(EnableSession = true, Description = "Sets Domain Restriction")]
        public bool SetDomainRestriction(InitializationObject initObj, int restriction)
        {
            bool passed = false;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    passed = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).SetDomainRestriction(initObj.DomainID, restriction);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return passed;
        }

        [WebMethod(EnableSession = true, Description = "Adds a device to a domain")]
        public DomainResponseObject SubmitAddDeviceToDomainRequest(InitializationObject initObj, string deviceName, int brandId)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    int siteGuid = 0;
                    if (int.TryParse(initObj.SiteGuid, out siteGuid))
                        domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).SubmitAddDeviceToDomainRequest(initObj.UDID, initObj.DomainID, siteGuid, deviceName, brandId);
                    else
                        logger.WarnFormat("Illegal site-guid for device name: {0}, barndId: {1}", deviceName, brandId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Confirms a device by master")]
        public DomainResponseObject ConfirmDeviceByDomainMaster(InitializationObject initObj, string udid, string masterUn, string token)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ConfirmDeviceByDomainMaster(udid, masterUn, token);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return domain;
        }


        [WebMethod(EnableSession = true, Description = "Adds Home network to domain")]
        public NetworkResponseObject AddHomeNetworkToDomain(InitializationObject initObj, string networkId, string networkName, string networkDesc)
        {
            NetworkResponseObject network = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "AddHomeNetworkToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    network = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).AddHomeNetworkToDomain(Convert.ToInt64(initObj.DomainID), networkId, networkName, networkDesc);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return network;
        }

        [WebMethod(EnableSession = true, Description = "Updates the home network's state")]
        public NetworkResponseObject UpdateDomainHomeNetwork(InitializationObject initObj, string networkId, string networkName, string networkDesc, bool isActive)
        {
            NetworkResponseObject network = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "UpdateDomainHomeNetwork", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    network = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).UpdateDomainHomeNetwork(Convert.ToInt64(initObj.DomainID), networkId, networkName, networkDesc, isActive);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return network;
        }

        [WebMethod(EnableSession = true, Description = "Removes domain's home network")]
        public NetworkResponseObject RemoveDomainHomeNetwork(InitializationObject initObj, string networkId)
        {
            NetworkResponseObject network = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "RemoveDomainHomeNetwork", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    network = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).RemoveDomainHomeNetwork(Convert.ToInt64(initObj.DomainID), networkId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return network;
        }

        [WebMethod(EnableSession = true, Description = "Gets the domain's home networks")]
        public HomeNetwork[] GetDomainHomeNetworks(InitializationObject initObj)
        {
            HomeNetwork[] homeNetworks = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainHomeNetworks", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    homeNetworks = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).GetDomainHomeNetworks(Convert.ToInt64(initObj.DomainID));
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return homeNetworks;
        }

        [WebMethod(EnableSession = true, Description = "Change the domain master")]
        public DomainResponseObject ChangeDomainMaster(InitializationObject initObj, int currentMasterID, int newMasterID)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "ChangeDomainMaster", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ChangeDomainMaster(initObj.DomainID, currentMasterID, newMasterID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Reset the domain frequency")]
        public DomainResponseObject ResetDomainFrequency(InitializationObject initObj, int frequencyType)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "ResetDomainFrequency", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ResetDomainFrequency(initObj.DomainID, frequencyType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }
            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Suspends a domain for the given domain ID")]
        public ClientResponseStatus SuspendDomain(InitializationObject initObj, int domainId)
        {
            ClientResponseStatus clientResponse;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "SuspendDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    clientResponse = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).SuspendDomain(domainId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse();
                }
            }
            else
            {
                clientResponse = ResponseUtils.ReturnBadCredentialsClientResponse();
            }
            return clientResponse;
        }

        [WebMethod(EnableSession = true, Description = "Resuming a suspended domain for the given domain ID")]
        public ClientResponseStatus ResumeDomain(InitializationObject initObj, int domainId)
        {
            ClientResponseStatus clientResponse;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "SuspendDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    clientResponse = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ResumeDomain(domainId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse();
                }
            }
            else
            {
                clientResponse = ResponseUtils.ReturnBadCredentialsClientResponse();
            }
            return clientResponse;
        }

        [WebMethod(EnableSession = true, Description = "Gets the domain limitation module by ID")]
        public DomainLimitationModuleResponse GetDomainLimitationModule(InitializationObject initObj, int domainLimitationID)
        {
            DomainLimitationModuleResponse response = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainLimitationModule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                try
                {
                    response = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).GetDomainLimitationModule(domainLimitationID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                    response = new DomainLimitationModuleResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items.Add("Error", "Unknown group");
                response = new DomainLimitationModuleResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }
            return response;
        }
    }
}

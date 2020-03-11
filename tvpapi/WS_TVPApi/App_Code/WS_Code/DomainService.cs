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
using TVPApiModule.Services;
using TVPPro.SiteManager.Context;
using TVPApiModule.Objects;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using System.Web;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Manager;
using TVPApiModule.Objects.Authorization;
using KLogMonitor;
using System.Reflection;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DomainService : System.Web.Services.WebService, IDomainService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [WebMethod(EnableSession = true, Description = "Reset Domain")]
        public DomainResponseObject ResetDomain(InitializationObject initObj)
        {
            DomainResponseObject response = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ResetDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }

                try
                {
                    response = new ApiDomainsService(groupID, initObj.Platform).ResetDomain(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Set device info")]
        [PrivateMethod]
        public bool SetDeviceInfo(InitializationObject initObj, string deviceName)
        {
            bool response = false;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain and device
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, initObj.UDID, groupID, initObj.Platform))
                {
                    return false;
                }

                try
                {
                    response = new ApiDomainsService(groupID, initObj.Platform).SetDeviceInfo(initObj.UDID, deviceName);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
            }

            return response;
        }

        [WebMethod(EnableSession = true, Description = "Add device to domain")]
        [PrivateMethod]
        public DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddDeviceToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    // Tokenization: validate domain
                    if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                    {
                        return null;
                    }

                    IImplementation impl = WSUtils.GetImplementation(groupID, initObj);
                    resDomain = impl.AddDeviceToDomain(sDeviceName, iDeviceBrandID);

                    //resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddDeviceToDomain(initObj.DomainID, initObj.UDID, sDeviceName, iDeviceBrandID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Add a user to domain")]
        [PrivateMethod]
        public DomainResponseObject AddUserToDomain(InitializationObject initObj, int AddedUserGuid)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddUserToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddUserToDomain(initObj.DomainID, Convert.ToInt32(initObj.SiteGuid), AddedUserGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Remove a user from domain")]
        [PrivateMethod]
        public DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove)
        {
            DomainResponseObject domain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveUserFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain and siteGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, userGuidToRemove, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveUserFromDomain(initObj.DomainID, userGuidToRemove);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Remove device from domain")]
        [PrivateMethod]
        public DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, int domainID, string sDeviceName, int iDeviceBrandID, string sUdid)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDeviceFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    // Tokenization: validate domain and udid
                    if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, initObj.UDID, groupID, initObj.Platform))
                    {
                        return null;
                    }

                    if (!string.IsNullOrEmpty(sUdid))
                        resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDeviceToDomain(domainID, sUdid);
                    else
                        resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDeviceToDomain(domainID, initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Activate/Deactivate a device in domain")]
        [PrivateMethod]
        public DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "ChangeDeviceDomainStatus", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain and udid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, initObj.UDID, groupID, initObj.Platform))
                {
                    return null;
                }

                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).ChangeDeviceDomainStatus(initObj.DomainID, initObj.UDID, bActive);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Get device/user domain info")]
        [PrivateMethod]
        public Domain GetDomainInfo(InitializationObject initObj)
        {
            Domain domain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainInfo(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Set device/user domain info")]
        [PrivateMethod]
        public DomainResponseObject SetDomainInfo(InitializationObject initObj, string sDomainName, string sDomainDescription)
        {
            DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SetDomainInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).SetDomainInfo(initObj.DomainID, sDomainName, sDomainDescription);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Get device domains")]
        [PrivateMethod]
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

                    // if tokenization enabled 
                    if (AuthorizationManager.IsTokenizationEnabled())
                    {
                        // Tokenization: validate domains - find siteGuid's domain
                        Domain siteGuidsDomain = AuthorizationManager.GetSiteGuidsDomain(initObj.SiteGuid, domains);
                        if (siteGuidsDomain == null)
                        {
                            AuthorizationManager.Instance.returnError(403);
                            return null;
                        }

                        // device in siteGuid's domain return only the relevant domain
                        else
                        {
                            devDomains = new TVPApiModule.Services.ApiDomainsService.DeviceDomain[1]
                            {
                                new TVPApiModule.Services.ApiDomainsService.DeviceDomain()
                                {
                                    DomainID = siteGuidsDomain.m_nDomainID,
                                    DomainName = siteGuidsDomain.m_sName,
                                    SiteGuid = siteGuidsDomain.m_masterGUIDs != null && siteGuidsDomain.m_masterGUIDs.Count() > 0 ? siteGuidsDomain.m_masterGUIDs[0].ToString() : string.Empty,
                                    DefaultUser = siteGuidsDomain.m_DefaultUsersIDs != null && siteGuidsDomain.m_DefaultUsersIDs.Count() > 0 ? siteGuidsDomain.m_DefaultUsersIDs[0].ToString() : string.Empty,
                                    DomainStatus = siteGuidsDomain.m_DomainStatus
                                }
                            };
                        }
                    }
                    // tokenization is disabled - return the same value as before
                    else
                    {
                        devDomains = new ApiDomainsService.DeviceDomain[domains.Count()];
                        for (int i = 0; i < domains.Count(); i++)
                        {
                            devDomains[i] = new ApiDomainsService.DeviceDomain()
                            {
                                DomainID = domains[i].m_nDomainID,
                                DomainName = domains[i].m_sName,
                                SiteGuid = domains[i].m_masterGUIDs != null && domains[i].m_masterGUIDs.Count() > 0 ? domains[i].m_masterGUIDs[0].ToString() : string.Empty,
                                DefaultUser = domains[i].m_DefaultUsersIDs != null && domains[i].m_DefaultUsersIDs.Count() > 0 ? domains[i].m_DefaultUsersIDs[0].ToString() : string.Empty,
                                DomainStatus = domains[i].m_DomainStatus
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return devDomains;
        }

        [WebMethod(EnableSession = true, Description = "Get PIN Code for a new device")]
        [PrivateMethod]
        public string GetPINForDevice(InitializationObject initObj, int devBrandID)
        {
            string pin = string.Empty;
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetPINForDevice", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    pin = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetPINForDevice(initObj.UDID, devBrandID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return pin;
        }

        [WebMethod(EnableSession = true, Description = "Register a device to domain by PIN code")]
        [PrivateMethod]
        public TVPApiModule.Services.ApiDomainsService.DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin)
        {
            TVPApiModule.Services.ApiDomainsService.DeviceRegistration deviceRes = new TVPApiModule.Services.ApiDomainsService.DeviceRegistration();
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RegisterDeviceByPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return default(TVPApiModule.Services.ApiDomainsService.DeviceRegistration);
                }
                try
                {
                    TVPApiModule.Services.ApiDomainsService service = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform);
                    DeviceResponseObject device = service.RegisterDeviceByPIN(initObj.UDID, initObj.DomainID, pin);

                    if (device == null)
                    {
                        deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Error;
                    }
                    else
                    {
                        switch (device.m_oDeviceResponseStatus)
                        {
                            case DeviceResponseStatus.UnKnown:
                            case DeviceResponseStatus.Error:
                                deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Error;
                                break;
                            case DeviceResponseStatus.DuplicatePin:
                            case DeviceResponseStatus.DeviceNotExists:
                                deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Invalid;
                                break;
                            case DeviceResponseStatus.ExceededLimit:
                                deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.ExceededLimit;
                                break;
                            case DeviceResponseStatus.OK:
                                deviceRes.RegStatus = TVPApiModule.Services.ApiDomainsService.eDeviceRegistrationStatus.Success;
                                deviceRes.UDID = device.m_oDevice.m_deviceUDID;
                                break;
                            default:
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return deviceRes;
        }

        [WebMethod(EnableSession = true, Description = "Add domain to master site user")]
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return domainRes;
        }

        [WebMethod(EnableSession = true, Description = "Add domain with Co-GUID to master site user")]
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
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
        //            HttpContext.Current.Items["Error"] = ex;
        //        }
        //    }

        //    return res;
        //}

        [WebMethod(EnableSession = true, Description = "Get domain object by CoGuid")]
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
                // Tokenization: validate coGuid
                if (AuthorizationManager.IsTokenizationEnabled() && res != null && res.m_oDomain != null &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, res.m_oDomain.m_nDomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Get domain ID by CoGuid")]
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
                // Tokenization: validate coGuid
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, res, null, groupID, initObj.Platform))
                {
                    return 0;
                }
            }

            return res;
        }

        [WebMethod(EnableSession = true, Description = "Submit user to a domain request")]
        [PrivateMethod]
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
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Remove Domain")]
        [PrivateMethod]
        public string RemoveDomain(InitializationObject initObj)
        {
            DomainResponseStatus resDomain = DomainResponseStatus.UnKnown;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, groupID, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).RemoveDomain(initObj.DomainID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomain.ToString();
        }

        [WebMethod(EnableSession = true, Description = "Get DomainIDs By Operator CoGuid")]
        [PrivateMethod]
        public int[] GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid)
        {
            int[] resDomains = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDomainIDsByOperatorCoGuid", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                // Tokenization: authorized only for admin
                if (AuthorizationManager.IsTokenizationEnabled())
                {
                    return null;
                }
                try
                {
                    resDomains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDomainIDsByOperatorCoGuid(operatorCoGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }

            return resDomains;
        }

        [WebMethod(EnableSession = true, Description = "Get Device Info")]
        [PrivateMethod]
        public DeviceResponseObject GetDeviceInfo(InitializationObject initObj, string sId, bool bIsUDID)
        {
            DeviceResponseObject deviceInfo = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate device                
                if (AuthorizationManager.IsTokenizationEnabled() && bIsUDID &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, 0, sId, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    deviceInfo = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).GetDeviceInfo(sId, bIsUDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }

                // Tokenization: validate device in domain               
                if (AuthorizationManager.IsTokenizationEnabled() && !bIsUDID && deviceInfo != null && deviceInfo.m_oDevice != null &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, deviceInfo.m_oDevice.m_domainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
            }

            return deviceInfo;

        }

        [WebMethod(EnableSession = true, Description = "Sets Domain Restriction")]
        [PrivateMethod]
        public bool SetDomainRestriction(InitializationObject initObj, int restriction)
        {
            bool passed = false;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return false;
                }
                try
                {
                    passed = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).SetDomainRestriction(initObj.DomainID, restriction);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return passed;
        }

        [WebMethod(EnableSession = true, Description = "Adds a device to a domain")]
        [PrivateMethod]
        public DomainResponseObject SubmitAddDeviceToDomainRequest(InitializationObject initObj, string deviceName, int brandId)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    int siteGuid = 0;
                    if (int.TryParse(initObj.SiteGuid, out siteGuid))
                        domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).SubmitAddDeviceToDomainRequest(initObj.UDID, initObj.DomainID, siteGuid, deviceName, brandId);
                    else
                        logger.WarnFormat("Illegal site-guid for device name: {0}, barndId: {1}", true, null, deviceName, brandId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Confirms a device by master")]
        [PrivateMethod]
        public DomainResponseObject ConfirmDeviceByDomainMaster(InitializationObject initObj, string udid, string masterUn, string token)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceInfo", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate device
                if (AuthorizationManager.IsTokenizationEnabled() &&
                        !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, 0, udid, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ConfirmDeviceByDomainMaster(udid, masterUn, token);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return domain;
        }


        [WebMethod(EnableSession = true, Description = "Adds Home network to domain")]
        [PrivateMethod]
        public NetworkResponseObject AddHomeNetworkToDomain(InitializationObject initObj, string networkId, string networkName, string networkDesc)
        {
            NetworkResponseObject network = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "AddHomeNetworkToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    network = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).AddHomeNetworkToDomain(Convert.ToInt64(initObj.DomainID), networkId, networkName, networkDesc);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return network;
        }

        [WebMethod(EnableSession = true, Description = "Updates the home network's state")]
        [PrivateMethod]
        public NetworkResponseObject UpdateDomainHomeNetwork(InitializationObject initObj, string networkId, string networkName, string networkDesc, bool isActive)
        {
            NetworkResponseObject network = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "UpdateDomainHomeNetwork", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    network = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).UpdateDomainHomeNetwork(Convert.ToInt64(initObj.DomainID), networkId, networkName, networkDesc, isActive);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return network;
        }

        [WebMethod(EnableSession = true, Description = "Removes domain's home network")]
        [PrivateMethod]
        public NetworkResponseObject RemoveDomainHomeNetwork(InitializationObject initObj, string networkId)
        {
            NetworkResponseObject network = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "RemoveDomainHomeNetwork", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    network = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).RemoveDomainHomeNetwork(Convert.ToInt64(initObj.DomainID), networkId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return network;
        }

        [WebMethod(EnableSession = true, Description = "Gets the domain's home networks")]
        [PrivateMethod]
        public HomeNetwork[] GetDomainHomeNetworks(InitializationObject initObj)
        {
            HomeNetwork[] homeNetworks = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainHomeNetworks", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    homeNetworks = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).GetDomainHomeNetworks(Convert.ToInt64(initObj.DomainID));
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return homeNetworks;
        }

        [WebMethod(EnableSession = true, Description = "Change the domain master")]
        [PrivateMethod]
        public DomainResponseObject ChangeDomainMaster(InitializationObject initObj, int currentMasterID, int newMasterID)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "ChangeDomainMaster", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain and newMasterID
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, newMasterID.ToString(), initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ChangeDomainMaster(initObj.DomainID, currentMasterID, newMasterID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Reset the domain frequency")]
        [PrivateMethod]
        public DomainResponseObject ResetDomainFrequency(InitializationObject initObj, int frequencyType)
        {
            DomainResponseObject domain = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "ResetDomainFrequency", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    domain = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ResetDomainFrequency(initObj.DomainID, frequencyType);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                }
            }
            return domain;
        }

        [WebMethod(EnableSession = true, Description = "Suspends a domain for the given domain ID")]
        [PrivateMethod]
        public ClientResponseStatus SuspendDomain(InitializationObject initObj, int domainId)
        {
            ClientResponseStatus clientResponse;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "SuspendDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    clientResponse = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).SuspendDomain(domainId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
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
        [PrivateMethod]
        public ClientResponseStatus ResumeDomain(InitializationObject initObj, int domainId)
        {
            ClientResponseStatus clientResponse;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "SuspendDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    return null;
                }
                try
                {
                    clientResponse = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).ResumeDomain(domainId);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
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
        [PrivateMethod]
        public DomainLimitationModuleResponse GetDomainLimitationModule(InitializationObject initObj, int domainLimitationID)
        {
            DomainLimitationModuleResponse response = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "GetDomainLimitationModule", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    response = new DomainLimitationModuleResponse();
                    response.Status = new TVPApiModule.Objects.Responses.Status((int)eStatus.Unauthorized, "User is not in domain");
                }
                try
                {
                    response = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).GetDomainLimitationModule(domainLimitationID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    response = new DomainLimitationModuleResponse();
                    response.Status = ResponseUtils.ReturnGeneralErrorStatus();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                response = new DomainLimitationModuleResponse();
                response.Status = ResponseUtils.ReturnBadCredentialsStatus();
            }
            return response;
        }

        [WebMethod(EnableSession = true, Description = "Set region for domain")]
        [PrivateMethod]
        public ClientResponseStatus SetDomainRegion(InitializationObject initObj, int domain_id, string ext_region_id, string lookup_key)
        {
            ClientResponseStatus clientResponse = null;

            int nGroupId = ConnectionHelper.GetGroupID("tvpapi", "SetDomainRegion", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (nGroupId > 0)
            {
                // Tokenization: validate domain
                if (AuthorizationManager.IsTokenizationEnabled() &&
                    !AuthorizationManager.Instance.ValidateRequestParameters(initObj.SiteGuid, null, initObj.DomainID, null, nGroupId, initObj.Platform))
                {
                    clientResponse = new TVPApiModule.Objects.Responses.ClientResponseStatus((int)eStatus.Unauthorized, "User is not in domain");
                }
                try
                {
                    clientResponse = new TVPApiModule.Services.ApiDomainsService(nGroupId, initObj.Platform).SetDomainRegion(domain_id, ext_region_id, lookup_key);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items["Error"] = ex;
                    clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse();
                }
            }
            else
            {
                HttpContext.Current.Items["Error"] = "Unknown group";
                clientResponse = ResponseUtils.ReturnBadCredentialsClientResponse();
            }
            return clientResponse;
        }
    }
}



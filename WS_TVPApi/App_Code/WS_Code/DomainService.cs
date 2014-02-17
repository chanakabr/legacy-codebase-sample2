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
using System.Web;
using TVPApiModule.Interfaces;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Helper;

namespace TVPApiServices
{
    [WebService(Namespace = "http://platform-us.tvinci.com/tvpapi/ws")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class DomainService : System.Web.Services.WebService//, IDomainService
    {
        private readonly ILog logger = LogManager.GetLogger(typeof(DomainService));

        [WebMethod(EnableSession = true, Description = "Reset Domain")]
        public TVPApiModule.Objects.Responses.DomainResponseObject ResetDomain(InitializationObject initObj)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;

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
        public TVPApiModule.Objects.Responses.DomainResponseObject AddDeviceToDomain(InitializationObject initObj, string sDeviceName, int iDeviceBrandID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject resDomain = null;

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
        public TVPApiModule.Objects.Responses.DomainResponseObject AddUserToDomain(InitializationObject initObj, int AddedUserGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "AddUserToDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).AddUserToDomain(initObj.DomainID, initObj.SiteGuid, AddedUserGuid);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return resDomain;
        }

        [WebMethod(EnableSession = true, Description = "Remove a user from domain")]
        public TVPApiModule.Objects.Responses.DomainResponseObject RemoveUserFromDomain(InitializationObject initObj, string userGuidToRemove)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

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
        public TVPApiModule.Objects.Responses.DomainResponseObject RemoveDeviceFromDomain(InitializationObject initObj, int domainID, string sDeviceName, int iDeviceBrandID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RemoveDeviceFromDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
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
        public TVPApiModule.Objects.Responses.DomainResponseObject ChangeDeviceDomainStatus(InitializationObject initObj, bool bActive)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject resDomain = null;

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
        public TVPApiModule.Objects.Responses.Domain GetDomainInfo(InitializationObject initObj)
        {
            TVPApiModule.Objects.Responses.Domain domain = null;

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
        public TVPApiModule.Objects.Responses.DomainResponseObject SetDomainInfo(InitializationObject initObj, string sDomainName, string sDomainDescription)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject resDomain = null;

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
        public IEnumerable<DeviceDomain> GetDeviceDomains(InitializationObject initObj)
        {
            IEnumerable<DeviceDomain> domains = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "GetDeviceDomain", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    domains = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).GetDeviceDomains(initObj.UDID);
                }
                catch (Exception ex)
                {
                    HttpContext.Current.Items.Add("Error", ex);
                }
            }

            return domains;
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
        public DeviceRegistration RegisterDeviceByPIN(InitializationObject initObj, string pin)
        {
            DeviceRegistration deviceRes = new DeviceRegistration();
            int groupID = ConnectionHelper.GetGroupID("tvpapi", "RegisterDeviceByPIN", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {
                try
                {
                    TVPApiModule.Services.ApiDomainsService service = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform);
                    TVPApiModule.Objects.Responses.DeviceResponseObject device = service.RegisterDeviceByPIN(initObj.UDID, initObj.DomainID, pin);

                    if (device == null || device.device_response_status == TVPApiModule.Objects.Responses.DeviceResponseStatus.Error)
                        deviceRes.reg_status = eDeviceRegistrationStatus.Error;
                    else if (device.device_response_status == DeviceResponseStatus.DuplicatePin || device.device_response_status == TVPApiModule.Objects.Responses.DeviceResponseStatus.DeviceNotExists)
                        deviceRes.reg_status = eDeviceRegistrationStatus.Invalid;
                    else
                    {
                        deviceRes.reg_status = eDeviceRegistrationStatus.Success;
                        deviceRes.udid = device.device.device_udid;
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
        public TVPApiModule.Objects.Responses.DomainResponseObject AddDomain(InitializationObject initObj, string domainName, string domainDesc, int masterGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domainRes = null;
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
        public TVPApiModule.Objects.Responses.DomainResponseObject AddDomainWithCoGuid(InitializationObject initObj, string domainName, string domainDesc, int masterGuid, string coGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domainRes = null;
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
        public TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(InitializationObject initObj, string coGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject res = null;
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
        public TVPApiModule.Objects.Responses.DomainResponseObject SubmitAddUserToDomainRequest(InitializationObject initObj, string masterUsername)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject resDomain = null;

            int groupID = ConnectionHelper.GetGroupID("tvpapi", "SubmitAddUserToDomainRequest", initObj.ApiUser, initObj.ApiPass, SiteHelper.GetClientIP());

            if (groupID > 0)
            {

                try
                {
                    resDomain = new TVPApiModule.Services.ApiDomainsService(groupID, initObj.Platform).SubmitAddUserToDomainRequest(initObj.SiteGuid, masterUsername);
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
            TVPApiModule.Objects.Responses.DomainResponseStatus resDomain = TVPApiModule.Objects.Responses.DomainResponseStatus.UnKnown;

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
        public IEnumerable<int> GetDomainIDsByOperatorCoGuid(InitializationObject initObj, string operatorCoGuid)
        {
            IEnumerable<int> resDomains = null;

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

        
    }
}

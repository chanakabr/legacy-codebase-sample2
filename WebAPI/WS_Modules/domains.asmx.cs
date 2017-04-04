using ApiObjects;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Services;
using Core.Users;

namespace WS_Domains
{
    /// <summary>
    /// Summary description for Service1
    /// </summary>
    [WebService(Namespace = "http://domains.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class module : System.Web.Services.WebService
    {
        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DeviceContainer))]
        public DomainStatusResponse AddDomain(string sWSUserName, string sWSPassword, string sDomainName, string sDomainDescription, Int32 nMasterUserGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nMasterUserGuid;

            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.AddDomain(nGroupID, sDomainName, sDomainDescription, nMasterUserGuid);
            }
            else
            {
                if (nGroupID == 0)
                {
                    // error finding group ID 
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatusResponse))]
        [System.Xml.Serialization.XmlInclude(typeof(DeviceContainer))]
        public DomainStatusResponse AddDomainWithCoGuid(string sWSUserName, string sWSPassword, string sDomainName, string sDomainDescription, Int32 nMasterUserGuid, string sCoGuid)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);

            if (nGroupID != 0)
            {
                return Core.Domains.Module.AddDomain(nGroupID, sDomainName, sDomainDescription, nMasterUserGuid);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseStatus))]
        public DomainResponseStatus RemoveDomain(string sWSUserName, string sWSPassword, int nDomainID)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.RemoveDomain(nGroupID, nDomainID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return DomainResponseStatus.Error;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatusResponse))]
        public DomainStatusResponse SetDomainInfo(string sWSUserName, string sWSPassword, Int32 nDomainID, string sDomainName, string sDomainDescription)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.SetDomainInfo(nGroupID, nDomainID, sDomainName, sDomainDescription);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainStatusResponse AddUserToDomain(string sWSUserName, string sWSPassword, int nDomainID, int nUserGuid, int nMasterUserGuid, bool bIsMaster)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserGuid;

            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (nUserGuid <= 0)
            {
                return response;
            }
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.AddUserToDomain(nGroupID, nDomainID, nUserGuid, nMasterUserGuid, bIsMaster);
            }
            else
            {
                HttpContext.Current.Response.StatusCode = 404;
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainStatusResponse SubmitAddUserToDomainRequest(string sWSUserName, string sWSPassword, int nUserID, string sMasterUsername)
        {
            DomainStatusResponse response = new DomainStatusResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString()),
                DomainResponse = new DomainResponseObject() { m_oDomain = new Domain() { m_DomainStatus = DomainStatus.Error }, m_oDomainResponseStatus = DomainResponseStatus.Error }
            };

            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserID;

            if (nUserID > 0)
            {
                Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
                if (nGroupID != 0)
                {
                    return Core.Domains.Module.SubmitAddUserToDomainRequest(nGroupID, nUserID, sMasterUsername);
                }
                else
                {
                    if (nGroupID == 0)
                    {
                        HttpContext.Current.Response.StatusCode = 404;
                    }
                }
            }

            return response;
        }

        [WebMethod]
        public List<string> GetDomainUserList(string sWSUserName, string sWSPassword, int nDomainID)
        {
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDomainUserList(nDomainID, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainStatusResponse RemoveUserFromDomain(string sWSUserName, string sWSPassword, Int32 nDomainID, string sUserGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (string.IsNullOrEmpty(sUserGUID))
            {
                return response;
            }

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.RemoveUserFromDomain(nGroupID, nDomainID, sUserGUID);
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainStatusResponse AddDeviceToDomain(string sWSUserName, string sWSPassword, int nDomainID, string udid, string deviceName, int deviceBrandID)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.AddDeviceToDomain(nGroupID, nDomainID, udid, deviceName, deviceBrandID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        [WebMethod]
        public DeviceResponse AddDevice(string sWSUserName, string sWSPassword, int nDomainID, string udid, string deviceName, int deviceBrandID)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Domains.Module.AddDevice(nGroupID, nDomainID, udid, deviceName, deviceBrandID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainStatusResponse RemoveDeviceFromDomain(string sWSUserName, string sWSPassword, int nDomainID, string udid)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.RemoveDeviceFromDomain(nGroupID, nDomainID, udid);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainStatusResponse ChangeDeviceDomainStatus(string sWSUserName, string sWSPassword, int nDomainID, string deviceUDID, bool activate)
        {
            DomainStatusResponse response = new DomainStatusResponse();

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ChangeDeviceDomainStatus(nGroupID, nDomainID, deviceUDID, activate);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(HomeNetwork))]
        public DomainResponse GetDomainInfo(string sWSUserName, string sWSPassword, Int32 nDomainID)
        {
            DomainResponse response = new DomainResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDomainInfo(nGroupID, nDomainID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.Domain = new Domain() { m_DomainStatus = DomainStatus.Error };
            }

            return response;
        }

        [WebMethod]
        public int GetDomainIDByCoGuid(string sWSUserName, string sWSPassword, string sCoGuid)
        {

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDomainIDByCoGuid(nGroupID, sCoGuid);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return 0;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public DomainStatusResponse GetDomainByCoGuid(string sWSUserName, string sWSPassword, string sCoGuid)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDomainByCoGuid(nGroupID, sCoGuid);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public List<Domain> GetDeviceDomains(string sWSUserName, string sWSPassword, string udid)
        {

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDeviceDomains(nGroupID, udid);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return null;
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public DevicePinResponse GetPINForDevice(string sWSUserName, string sWSPassword, string sDeviceUDID, int nBrandID)
        {
            DevicePinResponse response = new DevicePinResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetPINForDevice(nGroupID, sDeviceUDID, nBrandID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DeviceResponseObject))]
        public DeviceResponse RegisterDeviceToDomainWithPIN(string sWSUserName, string sWSPassword, string sPID, int nDomainID, string sDeviceName)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.RegisterDeviceToDomainWithPIN(nGroupID, sPID, nDomainID, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                response.Device = new DeviceResponseObject(new Device() { m_state = DeviceState.Error }, DeviceResponseStatus.Error);
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [Obsolete]
        public ApiObjects.Response.Status SetDeviceInfo(string sWSUserName, string sWSPassword, string sDeviceUDID, string sDeviceName)
        {
            Core.Users.BaseDevice t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUserName, sWSPassword, "SetDeviceInfo", ref t);
            if (nGroupID != 0)
            {
                return t.SetDeviceInfo(nGroupID, sDeviceUDID, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
            }
        }

        [WebMethod]
        public DomainResponseObject ResetDomain(string sWSUserName, string sWSPassword, int nDomainID)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ResetDomain(nDomainID, 0);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            Domain domain = new Domain();
            domain.m_DomainStatus = DomainStatus.Error;

            return new DomainResponseObject(domain, DomainResponseStatus.Error);
        }

        [WebMethod]
        public DomainStatusResponse ResetDomainFrequency(string sWSUserName, string sWSPassword, int nDomainID, int nFrequencyType)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ResetDomainFrequency(nGroupID, nDomainID, nFrequencyType);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }

                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        [WebMethod]
        public DomainResponseObject ChangeDomainMaster(string sWSUserName, string sWSPassword, int nDomainID, int nCurrentMasterID, int nNewMasterID)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ChangeDomainMaster(nGroupID, nDomainID, nCurrentMasterID, nNewMasterID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            Domain domain = new Domain();
            domain.m_DomainStatus = DomainStatus.Error;

            return new DomainResponseObject(domain, DomainResponseStatus.Error);
        }

        [WebMethod]
        public int[] GetDomainIDsByOperatorCoGuid(string sWSUserName, string sWSPassword, string sOperatorCoGuid)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDomainIDsByOperatorCoGuid(nGroupID, sOperatorCoGuid);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return new int[] { };
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(DeviceResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(Device))]
        public DeviceResponseObject GetDeviceInfo(string sWSUserName, string sWSPassword, string sID, bool bIsUDID)
        {
            Core.Users.BaseDevice t = null;
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDeviceInfo(nGroupID, sID, bIsUDID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            Device device = new Device(0);
            device.m_state = DeviceState.Error;

            return new DeviceResponseObject(device, DeviceResponseStatus.Error);
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainStatusResponse SubmitAddDeviceToDomainRequest(string sWSUserName, string sWSPassword, int nDomainID, int nUserID, string sDeviceUdid, string sDeviceName, int nBrandID)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserID;
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.SubmitAddDeviceToDomainRequest(nGroupID, nDomainID, nUserID, sDeviceUdid, sDeviceName, nBrandID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }

                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseObject))]
        public DomainResponseObject ConfirmDeviceByDomainMaster(string sWSUserName, string sWSPassword, string sMasterUN, string sDeviceUDID, string sToken)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ConfirmDeviceByDomainMaster(nGroupID, sMasterUN, sDeviceUDID, sToken);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            Domain domain = new Domain();
            domain.m_DomainStatus = DomainStatus.Error;

            return new DomainResponseObject(domain, DomainResponseStatus.Error);
        }

        [WebMethod]
        public bool SetDomainRestriction(string sWSUserName, string sWSPassword, int nDomainID, int nRestriction)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.SetDomainRestriction(nGroupID, nDomainID, nRestriction);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return false;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseObject))]
        public NetworkResponseObject AddHomeNetworkToDomain(string sWSUsername, string sWSPassword, long lDomainID,
            string sNetworkID, string sNetworkName, string sNetworkDesc)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.AddHomeNetworkToDomain(nGroupID, lDomainID, sNetworkID, sNetworkName, sNetworkDesc);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return new NetworkResponseObject(false, NetworkResponseStatus.Error);

        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseObject))]
        [Obsolete]
        public ApiObjects.Response.Status UpdateDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID,
            string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive)
        {
            HomeNetworkResponse response = new HomeNetworkResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            Core.Users.BaseDomain t = null;
            Int32 nGroupID = Utils.GetGroupID(sWSUsername, sWSPassword, "UpdateDomainHomeNetwork", ref t);
            if (nGroupID != 0 && t != null)
            {
                response = t.UpdateDomainHomeNetwork(lDomainID, sNetworkID, sNetworkName, sNetworkDesc, bIsActive);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response.Status;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseObject))]
        public HomeNetworkResponse SetDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID,
            string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive)
        {
            HomeNetworkResponse response = new HomeNetworkResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Domains.Module.SetDomainHomeNetwork(nGroupID, lDomainID, sNetworkID, sNetworkName, sNetworkDesc, bIsActive);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(NetworkResponseObject))]
        public ApiObjects.Response.Status RemoveDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID,
            string sNetworkID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Domains.Module.RemoveDomainHomeNetwork(nGroupID, lDomainID, sNetworkID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(HomeNetwork))]
        public HomeNetworksResponse GetDomainHomeNetworks(string sWSUsername, string sWSPassword, long lDomainID)
        {
            HomeNetworksResponse response = new HomeNetworksResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDomainHomeNetworks(nGroupID, lDomainID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ValidationType))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(ValidationResponseObject))]
        public ValidationResponseObject ValidateLimitationModule(string sWSUsername, string sWSPassword, string sUDID, int nDeviceBrandID,
            long lSiteGuid, long lDomainID, ValidationType eValidation, int nRuleID = 0, int nMediaConcurrencyLimit = 0, int nMediaID = 0)
        {
            // add siteguid to logs/monitor
            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                HttpContext.Current.Items[Constants.USER_ID] = lSiteGuid;
            }

            if (lDomainID < 1 && lSiteGuid < 1)
                return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ValidateLimitationModule(nGroupID, sUDID, nDeviceBrandID, lSiteGuid, lDomainID, eValidation, nRuleID, nMediaConcurrencyLimit, nMediaID);
            }
            return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ValidationType))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(ValidationResponseObject))]
        public ValidationResponseObject ValidateLimitationNpvr(string sWSUsername, string sWSPassword, string sUDID, int nDeviceBrandID,
            long lSiteGuid, long lDomainID, ValidationType eValidation, int nNpvrConcurrencyLimit = 0, string sNpvrID = default(string))
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = lSiteGuid;

            if (lDomainID < 1 && lSiteGuid < 1)
                return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ValidateLimitationNpvr(nGroupID, sUDID, nDeviceBrandID, lSiteGuid, lDomainID, eValidation, nNpvrConcurrencyLimit, sNpvrID);
            }
            return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
        }


        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ApiObjects.Response.Status))]
        public ApiObjects.Response.Status RemoveDLM(string sWSUsername, string sWSPassword, int nDlmID)
        {
            ApiObjects.Response.Status resp = new ApiObjects.Response.Status();
            if (nDlmID < 1)
            {
                resp.Code = (int)eResponseStatus.DlmNotExist;
                return resp;
            }
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.RemoveDLM(nGroupID, nDlmID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                resp.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                return resp;
            }
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(ChangeDLMObj))]
        public ChangeDLMObj ChangeDLM(string sWSUsername, string sWSPassword, int nDomainID, int nDlmID)
        {
            ChangeDLMObj oChangeDLMObj = new ChangeDLMObj();
            if (nDlmID < 1)
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DlmNotExist, string.Empty);
            if (nDomainID < 1)
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DomainNotExists, string.Empty);
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.ChangeDLM(nDomainID, nDlmID, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.WrongPasswordOrUserName, string.Empty);
            }

            return oChangeDLMObj;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(DLMResponse))]
        public DLMResponse GetDLM(string sWSUsername, string sWSPassword, int nDlmID)
        {
            DLMResponse oDLMObj = new DLMResponse();
            if (nDlmID < 1)
                oDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DlmNotExist, string.Empty);
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDLM(nDlmID, nGroupID);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                oDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.WrongPasswordOrUserName, string.Empty);
            }

            return oDLMObj;
        }

        [WebMethod]
        public ApiObjects.Response.Status SuspendDomain(string sWSUserName, string sWSPassword, int nDomainID)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
                return Core.Domains.Module.SuspendDomain(nGroupID, nDomainID);
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error }; // suspend failed
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status ResumeDomain(string sWSUserName, string sWSPassword, int nDomainID)
        {
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
                return Core.Domains.Module.ResumeDomain(nGroupID, nDomainID);
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error }; // suspend failed
            }
        }

        [WebMethod]
        public ApiObjects.Response.Status SetDomainRegion(string sWSUserName, string sWSPassword, int domainId, string extRegionId, string lookupKey)
        {
            Int32 groupId = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (groupId != 0)
            {
                return Core.Domains.Module.SetDomainRegion(groupId, domainId, extRegionId, lookupKey);
            }
            else
            {
                if (groupId == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
            }
        }

        [WebMethod]
        public DomainResponse GetDomainByUser(string sWSUserName, string sWSPassword, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            DomainResponse response = new DomainResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Int32 groupId = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (groupId != 0)
            {
                return Core.Domains.Module.GetDomainByUser(groupId, siteGuid);
            }
            else
            {
                if (groupId == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }
            
            return response;
        }

        [WebMethod]
        public DeviceRegistrationStatusResponse GetDeviceRegistrationStatus(string sWSUserName, string sWSPassword, string udid, int domainId)
        {
            DeviceRegistrationStatusResponse response = new DeviceRegistrationStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDeviceRegistrationStatus(nGroupID, udid, domainId);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(Domain))]
        [System.Xml.Serialization.XmlInclude(typeof(DomainStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(User))]
        [System.Xml.Serialization.XmlInclude(typeof(UserBasicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicData))]
        [System.Xml.Serialization.XmlInclude(typeof(UserDynamicDataContainer))]
        [System.Xml.Serialization.XmlInclude(typeof(ResponseStatus))]
        [System.Xml.Serialization.XmlInclude(typeof(BaseUsers))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        [System.Xml.Serialization.XmlInclude(typeof(UserResponseObject))]
        public DeviceResponse SetDevice(string sWSUserName, string sWSPassword, string sDeviceUDID, string sDeviceName)
        {
            Core.Users.BaseDevice t = null;
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.SetDevice(nGroupID, sDeviceUDID, sDeviceName);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
                DeviceResponse response = new DeviceResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }
        }

        [WebMethod]
        public DeviceResponse GetDevice(string sWSUserName, string sWSPassword, string udid, int domainId)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                return Core.Domains.Module.GetDevice(nGroupID, udid, domainId);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseStatus))]
        public ApiObjects.Response.Status RemoveDomainById(string sWSUserName, string sWSPassword, int nDomainID)
        {

            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                DomainResponseStatus domainResponseStatus = Core.Domains.Module.RemoveDomain(nGroupID, nDomainID);
                status = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        [WebMethod]
        [System.Xml.Serialization.XmlInclude(typeof(DomainResponseStatus))]
        public ApiObjects.Response.Status RemoveDomainByCoGuid(string sWSUserName, string sWSPassword, string coGuid)
        {

            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                int householdId = Core.Domains.Module.GetDomainIDByCoGuid(nGroupID, coGuid);
                if (householdId > 0)
                {
                    DomainResponseStatus domainResponseStatus = Core.Domains.Module.RemoveDomain(nGroupID, householdId);
                    status = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
                }
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return status;
        }

        [WebMethod]
        public HomeNetworkResponse AddDomainHomeNetwork(string sWSUsername, string sWSPassword, long lDomainID,
            string sNetworkID, string sNetworkName, string sNetworkDesc, bool isActive)
        {
            HomeNetworkResponse response = new HomeNetworkResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };
                        
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Domains.Module.AddDomainHomeNetwork(nGroupID, lDomainID, sNetworkID, sNetworkName, sNetworkDesc, isActive);
            }
            else
            {
                if (nGroupID == 0)
                    HttpContext.Current.Response.StatusCode = 404;
            }

            return response;
        }

        [WebMethod]
        public DeviceResponse SubmitAddDeviceToDomain(string sWSUserName, string sWSPassword, int domainID, string userID, string deviceUdid, string deviceName, int brandID)
        {
            DeviceResponse response = new DeviceResponse() { Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() } };

            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userID;

            
            Int32 nGroupID = Utils.GetDomainGroupID(sWSUserName, sWSPassword);
            if (nGroupID != 0)
            {
                response = Core.Domains.Module.SubmitAddDeviceToDomain(nGroupID, domainID, userID, deviceUdid, deviceName, brandID);
            }
            else
            {
                if (nGroupID == 0)
                {
                    HttpContext.Current.Response.StatusCode = 404;
                }
            }

            return response;
        }

        [WebMethod]
        public bool VerifyDRMDevice(string sWSUsername, string sWSPassword, string userId, string udid, string drmId)
        {
            Int32 groupId = Utils.GetDomainGroupID(sWSUsername, sWSPassword);
            if (groupId != 0)
            {
                return Core.Domains.Module.VerifyDRMDevice(groupId, userId, udid, drmId);
            }
            else
            {
                if (groupId == 0)
                    HttpContext.Current.Response.StatusCode = 404;               
                return false;
            } 
        }
       
    }
}
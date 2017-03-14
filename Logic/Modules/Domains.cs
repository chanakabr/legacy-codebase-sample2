using ApiObjects;
using ApiObjects.Response;
using Core.Users;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Core.Domains
{
    public class Module
    {
        
        public static DomainStatusResponse AddDomain(int nGroupID, string sDomainName, string sDomainDescription, Int32 nMasterUserGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nMasterUserGuid;

            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.AddDomain(sDomainName, sDomainDescription, nMasterUserGuid, nGroupID);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        
        public static DomainStatusResponse AddDomainWithCoGuid(int nGroupID, string sDomainName, string sDomainDescription, Int32 nMasterUserGuid, string sCoGuid)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);

            if (t != null)
            {
                response.DomainResponse = t.AddDomain(sDomainName, sDomainDescription, nMasterUserGuid, nGroupID, sCoGuid);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        
        public static DomainResponseStatus RemoveDomain(int nGroupID, int nDomainID)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.RemoveDomain(nDomainID);
            }

            return DomainResponseStatus.Error;
        }

        
        public static DomainStatusResponse SetDomainInfo(int nGroupID, Int32 nDomainID, string sDomainName, string sDomainDescription)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {

                response.DomainResponse = t.SetDomainInfo(nDomainID, sDomainName, nGroupID, sDomainDescription);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        
        public static DomainStatusResponse AddUserToDomain(int nGroupID, int nDomainID, int nUserGuid, int nMasterUserGuid, bool bIsMaster)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserGuid;

            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (nUserGuid <= 0)
            {
                return response;
            }

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.AddUserToDomain(nGroupID, nDomainID, nUserGuid, nMasterUserGuid, bIsMaster);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
                else
                {
                    response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
                }
            }
            return response;
        }

        
        public static DomainStatusResponse SubmitAddUserToDomainRequest(int nGroupID, int nUserID, string sMasterUsername)
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

                Core.Users.BaseDomain t = null;
                Utils.GetBaseImpl(ref t, nGroupID);
                if (t != null)
                {
                    response.DomainResponse = t.SubmitAddUserToDomainRequest(nGroupID, nUserID, sMasterUsername);
                    if (response.DomainResponse != null)
                    {
                        // convert response status
                        response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                    }
                }
            }

            return response;
        }

        
        public static List<string> GetDomainUserList(int nGroupID, int nDomainID)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDomainUserList(nDomainID, nGroupID);
            }
            return null;
        }

        
        public static DomainStatusResponse RemoveUserFromDomain(int nGroupID, Int32 nDomainID, string sUserGUID)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = sUserGUID != null ? sUserGUID : "null";
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            if (string.IsNullOrEmpty(sUserGUID))
            {
                return response;
            }

            int nUserGUID = int.Parse(sUserGUID);
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.RemoveUserFromDomain(nGroupID, nDomainID, nUserGUID);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }

                else
                {
                    response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
                }
            }

            return response;
        }

        
        public static DomainStatusResponse AddDeviceToDomain(int nGroupID, int nDomainID, string udid, string deviceName, int deviceBrandID)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.AddDeviceToDomain(nGroupID, nDomainID, udid, deviceName, deviceBrandID);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        
        public static DeviceResponse AddDevice(int nGroupID, int nDomainID, string udid, string deviceName, int deviceBrandID)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.AddDevice(nGroupID, nDomainID, udid, deviceName, deviceBrandID);
            }
            return response;
        }

        
        public static DomainStatusResponse RemoveDeviceFromDomain(int nGroupID, int nDomainID, string udid)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.RemoveDeviceFromDomain(nDomainID, udid);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        
        public static DomainStatusResponse ChangeDeviceDomainStatus(int nGroupID, int nDomainID, string deviceUDID, bool activate)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.ChangeDeviceDomainStatus(nDomainID, deviceUDID, activate);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }
            return response;
        }

        
        public static DomainResponse GetDomainInfo(int nGroupID, Int32 nDomainID)
        {
            DomainResponse response = new DomainResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.Domain = t.GetDomainInfo(nDomainID, nGroupID);
                if (response.Domain != null)
                {
                    response.Status = Utils.ConvertDomainStatusToResponseObject(response.Domain.m_DomainStatus);
                }
            }
            else
            {
                response.Domain = new Domain() { m_DomainStatus = DomainStatus.Error };
            }

            return response;
        }

        
        public static int GetDomainIDByCoGuid(int nGroupID, string sCoGuid)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDomainIDByCoGuid(sCoGuid);
            }

            return 0;
        }

        
        public static DomainStatusResponse GetDomainByCoGuid(int nGroupID, string sCoGuid)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.GetDomainByCoGuid(sCoGuid, nGroupID);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        
        public static List<Domain> GetDeviceDomains(int nGroupID, string udid)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDeviceDomains(udid);
            }
            return null;
        }


        
        public static DevicePinResponse GetPINForDevice(int nGroupID, string sDeviceUDID, int nBrandID)
        {
            DevicePinResponse response = new DevicePinResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            Core.Users.BaseDevice t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.Pin = t.GetPINForDevice(nGroupID, sDeviceUDID, nBrandID);
                if (!string.IsNullOrEmpty(response.Pin))
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        
        public static DeviceResponse RegisterDeviceToDomainWithPIN(int nGroupID, string sPID, int nDomainID, string sDeviceName)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.Device = t.RegisterDeviceToDomainWithPIN(nGroupID, sPID, nDomainID, sDeviceName);

                if (response.Device != null)
                {
                    response.Status = Utils.ConvertDeviceResponseStatusToResponseObject(response.Device.m_oDeviceResponseStatus);
                }
            }
            else
            {
                response.Device = new DeviceResponseObject(new Device() { m_state = DeviceState.Error }, DeviceResponseStatus.Error);
            }

            return response;
        }

        
        public static DomainResponseObject ResetDomain(int nGroupID, int nDomainID)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ResetDomain(nDomainID, 0);
            }

            Domain domain = new Domain();
            domain.m_DomainStatus = DomainStatus.Error;

            return new DomainResponseObject(domain, DomainResponseStatus.Error);
        }

        
        public static DomainStatusResponse ResetDomainFrequency(int nGroupID, int nDomainID, int nFrequencyType)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.ResetDomain(nDomainID, nFrequencyType);

                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }
            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        
        public static DomainResponseObject ChangeDomainMaster(int nGroupID, int nDomainID, int nCurrentMasterID, int nNewMasterID)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChangeDomainMaster(nDomainID, nCurrentMasterID, nNewMasterID);
            }

            Domain domain = new Domain();
            domain.m_DomainStatus = DomainStatus.Error;

            return new DomainResponseObject(domain, DomainResponseStatus.Error);
        }

        
        public static int[] GetDomainIDsByOperatorCoGuid(int nGroupID, string sOperatorCoGuid)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDomainIDsByOperatorCoGuid(sOperatorCoGuid);
            }

            return new int[] { };
        }

        
        public static DeviceResponseObject GetDeviceInfo(int nGroupID, string sID, bool bIsUDID)
        {
            Core.Users.BaseDevice t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDeviceInfo(nGroupID, sID, bIsUDID);
            }

            Device device = new Device(0);
            device.m_state = DeviceState.Error;

            return new DeviceResponseObject(device, DeviceResponseStatus.Error);
        }

        
        public static DomainStatusResponse SubmitAddDeviceToDomainRequest(int nGroupID, int nDomainID, int nUserID, string sDeviceUdid, string sDeviceName, int nBrandID)
        {
            DomainStatusResponse response = new DomainStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = nUserID;

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.DomainResponse = t.SubmitAddDeviceToDomainRequest(nGroupID, nDomainID, nUserID, sDeviceUdid, sDeviceName, nBrandID);
                if (response.DomainResponse != null)
                {
                    // convert response status
                    response.Status = Utils.ConvertDomainResponseStatusToResponseObject(response.DomainResponse.m_oDomainResponseStatus);
                }

            }
            else
            {
                response.DomainResponse = new DomainResponseObject(new Domain() { m_DomainStatus = DomainStatus.Error }, DomainResponseStatus.Error);
            }

            return response;
        }

        
        public static DomainResponseObject ConfirmDeviceByDomainMaster(int nGroupID, string sMasterUN, string sDeviceUDID, string sToken)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ConfirmDeviceByDomainMaster(sMasterUN, sDeviceUDID, sToken);
            }

            Domain domain = new Domain();
            domain.m_DomainStatus = DomainStatus.Error;

            return new DomainResponseObject(domain, DomainResponseStatus.Error);
        }

        
        public static bool SetDomainRestriction(int nGroupID, int nDomainID, int nRestriction)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.SetDomainRestriction(nDomainID, (DomainRestriction)nRestriction);
            }

            return false;
        }

        
        public static NetworkResponseObject AddHomeNetworkToDomain(int nGroupID, long lDomainID,
            string sNetworkID, string sNetworkName, string sNetworkDesc)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.AddHomeNetworkToDomain(lDomainID, sNetworkID, sNetworkName, sNetworkDesc);
            }

            return new NetworkResponseObject(false, NetworkResponseStatus.Error);

        }

        
        public static HomeNetworkResponse SetDomainHomeNetwork(int nGroupID, long lDomainID,
            string sNetworkID, string sNetworkName, string sNetworkDesc, bool bIsActive)
        {
            HomeNetworkResponse response = new HomeNetworkResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.UpdateDomainHomeNetwork(lDomainID, sNetworkID, sNetworkName, sNetworkDesc, bIsActive);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status RemoveDomainHomeNetwork(int nGroupID, long lDomainID,
            string sNetworkID)
        {
            ApiObjects.Response.Status response = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.RemoveDomainHomeNetwork(lDomainID, sNetworkID);
            }

            return response;
        }

        
        public static HomeNetworksResponse GetDomainHomeNetworks(int nGroupID, long lDomainID)
        {
            HomeNetworksResponse response = new HomeNetworksResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response.HomeNetworks = t.GetDomainHomeNetworks(lDomainID);
                if (response.HomeNetworks != null)
                {
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
                }
            }

            return response;
        }

        
        public static ValidationResponseObject ValidateLimitationModule(int nGroupID, string sUDID, int nDeviceBrandID,
            long lSiteGuid, long lDomainID, ValidationType eValidation, int nRuleID = 0, int nMediaConcurrencyLimit = 0, int nMediaID = 0)
        {
            // add siteguid to logs/monitor
            if (HttpContext.Current != null && HttpContext.Current.Items != null)
            {
                HttpContext.Current.Items[Constants.USER_ID] = lSiteGuid;
            }

            if (lDomainID < 1 && lSiteGuid < 1)
                return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ValidateLimitationModule(sUDID, nDeviceBrandID, lSiteGuid, lDomainID, eValidation, nRuleID, nMediaConcurrencyLimit, nMediaID);
            }
            return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
        }

        
        public static ValidationResponseObject ValidateLimitationNpvr(int nGroupID, string sUDID, int nDeviceBrandID,
            long lSiteGuid, long lDomainID, ValidationType eValidation, int nNpvrConcurrencyLimit = 0, string sNpvrID = default(string))
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = lSiteGuid;

            if (lDomainID < 1 && lSiteGuid < 1)
                return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ValidateLimitationNpvr(sUDID, nDeviceBrandID, lSiteGuid, lDomainID, eValidation, nNpvrConcurrencyLimit, sNpvrID);
            }
            return new ValidationResponseObject(DomainResponseStatus.UnKnown, lDomainID);
        }


        
        public static ApiObjects.Response.Status RemoveDLM(int nGroupID, int nDlmID)
        {
            ApiObjects.Response.Status resp = new ApiObjects.Response.Status();
            if (nDlmID < 1)
            {
                resp.Code = (int)eResponseStatus.DlmNotExist;
                return resp;
            }
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.RemoveDLM(nDlmID);
            }
            else
            {
                resp.Code = (int)eResponseStatus.WrongPasswordOrUserName;
                return resp;
            }
        }

        
        public static ChangeDLMObj ChangeDLM(int nGroupID, int nDomainID, int nDlmID)
        {
            ChangeDLMObj oChangeDLMObj = new ChangeDLMObj();
            if (nDlmID < 1)
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DlmNotExist, string.Empty);
            if (nDomainID < 1)
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DomainNotExists, string.Empty);
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.ChangeDLM(nDomainID, nDlmID, nGroupID);
            }
            else
            {
                oChangeDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.WrongPasswordOrUserName, string.Empty);
            }

            return oChangeDLMObj;
        }

        
        public static DLMResponse GetDLM(int nGroupID, int nDlmID)
        {
            DLMResponse oDLMObj = new DLMResponse();
            if (nDlmID < 1)
                oDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.DlmNotExist, string.Empty);
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDLM(nDlmID, nGroupID);
            }
            else
            {
                oDLMObj.resp = new ApiObjects.Response.Status((int)eResponseStatus.WrongPasswordOrUserName, string.Empty);
            }

            return oDLMObj;
        }

        
        public static ApiObjects.Response.Status SuspendDomain(int nGroupID, int nDomainID)
        {
            Core.Users.BaseDomain d = null;
            Utils.GetBaseImpl(ref d, nGroupID);
            if (d != null)
                return d.SuspendDomain(nDomainID);
            else
            {
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error }; // suspend failed
            }
        }

        
        public static ApiObjects.Response.Status ResumeDomain(int nGroupID, int nDomainID)
        {
            Core.Users.BaseDomain d = null;
            Utils.GetBaseImpl(ref d, nGroupID);
            if (d != null)
                return d.ResumeDomain(nDomainID);
            else
            {
                return new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error }; // suspend failed
            }
        }

        
        public static ApiObjects.Response.Status SetDomainRegion(int nGroupID, int domainId, string extRegionId, string lookupKey)
        {
            Core.Users.BaseDomain d = null;
            Utils.GetBaseImpl(ref d, nGroupID);
            if (d != null)
            {
                return d.SetDomainRegion(nGroupID, domainId, extRegionId, lookupKey);
            }
            else
            {
                return new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
            }
        }

        
        public static DomainResponse GetDomainByUser(int nGroupID, string siteGuid)
        {
            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = siteGuid != null ? siteGuid : "null";

            DomainResponse response = new DomainResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain d = null;
            Utils.GetBaseImpl(ref d, nGroupID);
            if (d != null)
            {
                response.Domain = d.GetDomainByUser(nGroupID, siteGuid);
                if (response.Domain != null)
                {
                    response.Status = Utils.ConvertDomainStatusToResponseObject(response.Domain.m_DomainStatus);
                }
            }
            return response;
        }

        
        public static DeviceRegistrationStatusResponse GetDeviceRegistrationStatus(int nGroupID, string udid, int domainId)
        {
            DeviceRegistrationStatusResponse response = new DeviceRegistrationStatusResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDeviceRegistrationStatus(udid, domainId);
            }

            return response;
        }

        
        public static DeviceResponse SetDevice(int nGroupID, string sDeviceUDID, string sDeviceName)
        {
            Core.Users.BaseDevice t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                DeviceResponseObject responseObject = t.SetDevice(nGroupID, sDeviceUDID, sDeviceName);

                DeviceResponse response = new DeviceResponse()
                {
                    Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()),
                    Device = responseObject
                };

                if (responseObject.m_oDeviceResponseStatus != DeviceResponseStatus.OK)
                    response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, responseObject.m_oDeviceResponseStatus.ToString());

                return response;
            }
            else
            {
                DeviceResponse response = new DeviceResponse();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
                return response;
            }
        }

        
        public static DeviceResponse GetDevice(int nGroupID, string udid, int domainId)
        {
            DeviceResponse response = new DeviceResponse();
            response.Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                return t.GetDevice(udid, domainId);
            }

            return response;
        }

        
        public static ApiObjects.Response.Status RemoveDomainById(int nGroupID, int nDomainID)
        {

            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                DomainResponseStatus domainResponseStatus = t.RemoveDomain(nDomainID);
                status = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
            }

            return status;
        }

        
        public static ApiObjects.Response.Status RemoveDomainByCoGuid(int nGroupID, string coGuid)
        {

            ApiObjects.Response.Status status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                int householdId = t.GetDomainIDByCoGuid(coGuid);
                if (householdId > 0)
                {
                    DomainResponseStatus domainResponseStatus = t.RemoveDomain(householdId);
                    status = Utils.ConvertDomainResponseStatusToResponseObject(domainResponseStatus);
                }
            }

            return status;
        }

        
        public static HomeNetworkResponse AddDomainHomeNetwork(int nGroupID, long lDomainID,
            string sNetworkID, string sNetworkName, string sNetworkDesc, bool isActive)
        {
            HomeNetworkResponse response = new HomeNetworkResponse()
            {
                Status = new ApiObjects.Response.Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString())
            };

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.AddDomainHomeNetwork(lDomainID, sNetworkID, sNetworkName, sNetworkDesc, isActive);
            }

            return response;
        }

        
        public static DeviceResponse SubmitAddDeviceToDomain(int nGroupID, int domainID, string userID, string deviceUdid, string deviceName, int brandID)
        {
            DeviceResponse response = new DeviceResponse() { Status = new ApiObjects.Response.Status() { Code = (int)eResponseStatus.Error, Message = eResponseStatus.Error.ToString() } };

            // add siteguid to logs/monitor
            HttpContext.Current.Items[Constants.USER_ID] = userID;

            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, nGroupID);
            if (t != null)
            {
                response = t.SubmitAddDeviceToDomain(nGroupID, domainID, userID, deviceUdid, deviceName, brandID);
            }

            return response;
        }

        public static bool VerifyDRMDevice(int groupId, string deviceUdid, string drmId)
        {
            Core.Users.BaseDomain t = null;
            Utils.GetBaseImpl(ref t, groupId);
            if (t != null)
            {
                return t.VerifyDRMDevice(deviceUdid, drmId);
            }

            return false;
        }
    }
}

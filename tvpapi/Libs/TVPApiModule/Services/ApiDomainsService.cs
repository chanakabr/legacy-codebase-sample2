using Core.Users;
using KLogMonitor;
using System;
using System.Reflection;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Objects;
using Domain = TVPApiModule.Objects.Domain;
using System.Linq;
using DomainResponseObject = TVPApiModule.Objects.DomainResponseObject;
using TVPApiModule.Manager;

namespace TVPApiModule.Services
{
    public class ApiDomainsService : ApiBase
    {
        #region Variables
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        private string m_wsUserName = string.Empty;
        private string m_wsPassword = string.Empty;

        private int m_groupID;
        private PlatformType m_platform;

        [Serializable]
        public struct DeviceDomain
        {
            public string SiteGuid;
            public int DomainID;
            public string DomainName;
            public string DefaultUser;
            public Objects.DomainStatus DomainStatus;
        }

        [Serializable]
        public enum eDeviceRegistrationStatus { Success = 0, Invalid = 1, Error = 2, ExceededLimit = 3 }

        [Serializable]
        public struct DeviceRegistration
        {
            public string UDID;
            public eDeviceRegistrationStatus RegStatus;
        }
        #endregion

        public ApiDomainsService(int groupID, PlatformType platform)
        {
            m_wsUserName = GroupsManager.GetGroup(groupID).DomainsCredentials.Username;
            m_wsPassword = GroupsManager.GetGroup(groupID).DomainsCredentials.Password;

            m_groupID = groupID;
            m_platform = platform;
        }

        public DomainResponseObject AddUserToDomain(int domainID, int masterSiteGuid, int AddedUserGuid)
        {
            DomainResponseObject domain = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.AddUserToDomain(m_groupID, domainID, AddedUserGuid, masterSiteGuid, false);
                    if (res != null && res.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddUserToDomain, Error Message: {0} Parameters: AddedUserGuid: {1}, masterSiteGuid: {2}", ex.Message, AddedUserGuid, masterSiteGuid);
            }

            return domain;
        }

        public DomainResponseObject RemoveUserFromDomain(int iDomainID, string userGuidToRemove)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.RemoveUserFromDomain(m_groupID, iDomainID, userGuidToRemove);
                    if (res != null && res.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(res.DomainResponse);
                    }
                }
                //if (res.m_oDomainResponseStatus == DomainResponseStatus.OK)
                //    domain = res.m_oDomain;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RemoveUserFromDomain, Error Message: {0} Parameters: iDomainID: {1}, userGuidToRemove: {2}", ex.Message, iDomainID, userGuidToRemove);
            }

            return domain;
        }

        public DomainResponseObject AddDeviceToDomain(int iDomainID, string sUDID, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.AddDeviceToDomain(m_groupID, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
                    if (res != null && res.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDeviceToDomain, Error Message: {0} Parameters: iDomainID: {1}, sUDID: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
            }

            return domain;
        }

        public bool SetDomainRestriction(int iDomainID, int nRestriction)
        {
            bool passed = false;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    passed = Core.Domains.Module.SetDomainRestriction(m_groupID, iDomainID, nRestriction);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RemoveDeviceToDomain, Error Message: {0} Parameters: iDomainID: {1}, nRestriction: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, iDomainID, nRestriction);
            }

            return passed;
        }

        public DomainResponseObject SubmitAddDeviceToDomainRequest(string sUDID, int domainId, int userId, string deviceName, int brandId)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res  = Core.Domains.Module.SubmitAddDeviceToDomainRequest(m_groupID, domainId, userId, sUDID, deviceName, brandId);
                    if (res != null && res.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SubmitAddDeviceToDomainRequest, Error Message: {0} Parameters: domainId: {1}, userId: {2}, deviceName: {3}, brandId: {4}", ex.Message, domainId, userId, deviceName, brandId);
            }

            return domain;
        }

        public DomainResponseObject ConfirmDeviceByDomainMaster(string udid, string masterUn, string token)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.ConfirmDeviceByDomainMaster(m_groupID, masterUn, udid, token);
                    if (res != null)
                    {
                        domain = new DomainResponseObject(res);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ConfirmDeviceByDomainMaster, Error Message: {0} Parameters: masterUn: {1}, udid: {2}, token: {3}", ex.Message, masterUn, udid, token);
            }

            return domain;
        }

        public NetworkResponseObject AddHomeNetworkToDomain(long domainId, string networkId, string networkName, string networkDesc)
        {
            NetworkResponseObject network = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    network = Core.Domains.Module.AddHomeNetworkToDomain(m_groupID, domainId, networkId, networkName, networkDesc);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddHomeNetworkToDomain, Error Message: {0} Parameters: domainId: {1}, networkId: {2}, networkName: {3}, networkDesc: {4}", ex.Message, domainId, networkId, networkName, networkDesc);
            }

            return network;
        }

        public NetworkResponseObject UpdateDomainHomeNetwork(long domainId, string networkId, string networkName, string networkDesc, bool isActive)
        {
            NetworkResponseObject network = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.SetDomainHomeNetwork(m_groupID, domainId, networkId, networkName, networkDesc, isActive);

                    if (res != null && res.Status != null)
                    {
                        network = ConvertStatusToNetworkResponseObject(res.Status);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : UpdateDomainHomeNetwork, Error Message: {0} Parameters: domainId: {1}, networkId: {2}, networkName: {3}, networkDesc: {4}, isActive: {5}", ex.Message, domainId, networkId, networkName, networkDesc, isActive);
            }

            return network;
        }

        private NetworkResponseObject ConvertStatusToNetworkResponseObject(ApiObjects.Response.Status res)
        {
            NetworkResponseObject response = new NetworkResponseObject()
            {
                bSuccess = false,
                eReason = NetworkResponseStatus.Error
            };

            if (res != null)
            {
                switch (res.Code)
                {
                    case 0:
                        {
                            response.bSuccess = true;
                            response.eReason = NetworkResponseStatus.OK;
                        }
                        break;
                    case 6016:
                        {
                            response.bSuccess = false;
                            response.eReason = NetworkResponseStatus.InvalidInput;
                        }
                        break;
                    case 1031:
                        {
                            response.bSuccess = false;
                            response.eReason = NetworkResponseStatus.NetworkExists;
                        }
                        break;
                    case 1032:
                        {
                            response.bSuccess = false;
                            response.eReason = NetworkResponseStatus.QuantityLimitation;
                        }
                        break;
                    case 1033:
                        {
                            response.bSuccess = false;
                            response.eReason = NetworkResponseStatus.NetworkDoesNotExist;
                        }
                        break;
                    case 1034:
                        {
                            response.bSuccess = false;
                            response.eReason = NetworkResponseStatus.FrequencyLimitation;
                        }
                        break;
                    default:
                        {
                            response.bSuccess = false;
                            response.eReason = NetworkResponseStatus.Error;
                        }
                        break;
                }
            }

            return response;
        }

        public NetworkResponseObject RemoveDomainHomeNetwork(long domainId, string networkId)
        {
            NetworkResponseObject network = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.RemoveDomainHomeNetwork(m_groupID, domainId, networkId);
                    network = ConvertStatusToNetworkResponseObject(res);

                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RemoveDomainHomeNetwork, Error Message: {0} Parameters: domainId: {1}, networkId: {2}", ex.Message, domainId, networkId);
            }

            return network;
        }

        public HomeNetwork[] GetDomainHomeNetworks(long domainId)
        {
            HomeNetwork[] homeNetworks = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.GetDomainHomeNetworks(m_groupID, domainId);
                    if (res != null && res.HomeNetworks != null)
                        homeNetworks = res.HomeNetworks.ToArray();
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainHomeNetworks, Error Message: {0} Parameters: domainId: {1}", ex.Message, domainId);
            }

            return homeNetworks;
        }

        public DomainResponseObject RemoveDeviceToDomain(int iDomainID, string sUDID)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.RemoveDeviceFromDomain(m_groupID, iDomainID, sUDID);
                    if (res != null && res.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDeviceToDomain, Error Message: {0} Parameters: iDomainID: {1}, sUDID: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, iDomainID, sUDID);
            }

            return domain;
        }

        public DomainResponseObject ChangeDeviceDomainStatus(int iDomainID, string sUDID, bool bActive)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.ChangeDeviceDomainStatus(m_groupID, iDomainID, sUDID, bActive);
                    if (res != null && res.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChangeDeviceDomainStatus, Error Message: {0} Parameters: iDomainID: {1}, bActive: {2}", ex.Message, iDomainID, sUDID, bActive);
            }

            return domain;
        }

        public Domain GetDomainInfo(int iDomainID)
        {
            Domain domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = Core.Domains.Module.GetDomainInfo(m_groupID, iDomainID);
                    if (response != null && response.Domain != null)
                    {
                        domain = new Domain(response.Domain);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainInfo, Error Message: {0} Parameters: iDomainID: {1}", ex.Message, iDomainID);
            }

            return domain;
        }

        public DomainResponseObject SetDomainInfo(int iDomainID, string sDomainName, string sDomainDescription)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = Core.Domains.Module.SetDomainInfo(m_groupID, iDomainID, sDomainName, sDomainDescription, null);

                    if (response != null && response.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(response.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetDomainInfo, Error Message: {0} Parameters: iDomainID: {1}, sDomainName: {2}, sDomainDescription: {3}", ex.Message, iDomainID, sDomainName, sDomainDescription);
            }

            return domain;
        }

        public Domain[] GetDeviceDomains(string udid)
        {
            Domain[] domains = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var list = Core.Domains.Module.GetDeviceDomains(m_groupID, udid);

                    if (list != null)
                    {
                        domains = list.Select(d => new Domain(d)).ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDeviceDomains, Error Message: {0} Parameters: udid: {1}", ex.Message, udid);
            }

            return domains;
        }

        public string GetPINForDevice(string udid, int devBrandID)
        {
            string pin = string.Empty;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = Core.Domains.Module.GetPINForDevice(m_groupID, udid, devBrandID);
                    if (response != null)
                        pin = response.Pin;
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetPINForDevice, Error Message: {0} Parameters: udid: {1}, brand: {2}", ex.Message, udid, devBrandID);
            }

            return pin;
        }

        public DeviceResponseObject RegisterDeviceByPIN(string udid, int domainID, string pin)
        {
            DeviceResponseObject device = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var deviceRes = Core.Domains.Module.RegisterDeviceToDomainWithPIN(m_groupID, pin, domainID, string.Empty);
                    if (deviceRes != null)
                    {
                        device = deviceRes.Device;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RegisterDeviceByPIN, Error Message: {0} Parameters: udid: {1}, pin: {2}", ex.Message, udid, pin);
            }

            return device;
        }

        public DomainResponseObject ResetDomain(int domainID)
        {
            DomainResponseObject response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.ResetDomain(m_groupID, domainID);

                    if (res != null)
                    {
                        response = new DomainResponseObject(res);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RegisterDeviceByPIN, Error Message: {0} Parameters: domainID: {1}", ex.Message, domainID);
            }

            return response;
        }

        public bool SetDeviceInfo(string udid, string deviceName)
        {
            bool response = false;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.Domains.Module.SetDeviceInfo(m_groupID, udid, deviceName);
                    if (result != null && result.Code == 0)
                    {
                        response = true;
                    }
                    else
                    {
                        response = false;
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetDeviceInfo, Error Message: {0} Parameters: udid: {1}", ex.Message, udid);
            }

            return response;
        }

        public DomainResponseObject AddDomain(string domainName, string domainDesc, int masterGuid)
        {
            DomainResponseObject response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.AddDomain(m_groupID, domainName, domainDesc, masterGuid, null);

                    if (res != null && res.DomainResponse != null)
                    {
                        response = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDomain, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, masterGuid);
            }

            return response;
        }

        public DomainResponseObject AddDomainWithCoGuid(string domainName, string domainDesc, int masterGuid, string CoGuid)
        {
            DomainResponseObject response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.AddDomainWithCoGuid(m_groupID, domainName, domainDesc, masterGuid, CoGuid, null);

                    if (res != null && res.DomainResponse != null)
                    {
                        response = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDomainWithCoGuid, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, masterGuid);
            }
            return response;
        }


        //public string GetDomainCoGuid(int nDomainID)
        //{
        //    string resp = string.Empty;
        //    try
        //    {
        //        //resp = Core.Domains.Module.GetDomainCoGuid(m_groupID, nDomainID);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.ErrorFormat("Error calling webservice protocol : GetDomainCoGuid, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, nDomainID);
        //    }
        //    return resp;
        //}

        public DomainResponseObject GetDomainByCoGuid(string coGuid)
        {
            DomainResponseObject response = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.GetDomainByCoGuid(m_groupID, coGuid);

                    if (res != null && res.DomainResponse != null)
                    {
                        response = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainByCoGuid, Error Message: {0} Parameters: coGuid: {1}", ex.Message, coGuid);
            }
            return response;
        }

        public int GetDomainIDByCoGuid(string coGuid)
        {
            int domainID = 0;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    domainID = Core.Domains.Module.GetDomainIDByCoGuid(m_groupID, coGuid);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainIDByCoGuid, Error Message: {0} Parameters: coGuid: {1}", ex.Message, coGuid);
            }
            return domainID;
        }

        public DomainResponseObject SubmitAddUserToDomainRequest(int userID, string masterUsername)
        {
            DomainResponseObject res = null;
            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var response = Core.Domains.Module.SubmitAddUserToDomainRequest(m_groupID, userID, masterUsername);

                    if (response != null && response.DomainResponse != null)
                    {
                        res = new DomainResponseObject(response.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SubmitAddUserToDomainRequest, Error Message: {0} Parameters: UserID: {1}", ex.Message, userID);
            }
            return res;
        }

        public DomainResponseStatus RemoveDomain(int domainID)
        {
            DomainResponseStatus response = DomainResponseStatus.UnKnown;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.Domains.Module.RemoveDomain(m_groupID, domainID, false);
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error in RemoveDomain, Error : {0} Parameters : Domain ID: {1}", e.Message, domainID);
            }

            return response;
        }

        public int[] GetDomainIDsByOperatorCoGuid(string operatorCoGuid)
        {
            int[] response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    response = Core.Domains.Module.GetDomainIDsByOperatorCoGuid(m_groupID, operatorCoGuid);
                }
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error in GetDomainIDsByOperatorCoGuid, Error : {0} Parameters : operatorCoGuid: {1}", e.Message, operatorCoGuid);
            }

            return response;
        }


        public DeviceResponseObject GetDeviceInfo(string sId, bool bIsUDID)
        {
            DeviceResponseObject deviceInfo = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    deviceInfo = Core.Domains.Module.GetDeviceInfo(m_groupID, sId, bIsUDID);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error in GetDeviceInfo, Error : {0} Parameters : Id: {1} : isUDID : {2}", ex.Message, sId, bIsUDID);
            }

            return deviceInfo;
        }

        public DomainResponseObject ChangeDomainMaster(int domainID, int currentMasterID, int newMasterID)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.ChangeDomainMaster(m_groupID, domainID, currentMasterID, newMasterID);

                    if (res != null)
                    {
                        domain = new DomainResponseObject(res);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error in ChangeDomainMaster, Error : {0} Parameters : domainID: {1}, : currentMasterID : {2}, newMasterID : {3}", ex.Message, domainID, currentMasterID, newMasterID);
            }

            return domain;
        }

        public DomainResponseObject ResetDomainFrequency(int domainID, int frequencyType)
        {
            DomainResponseObject domain = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.ResetDomainFrequency(m_groupID, domainID, frequencyType);

                    if (res != null && res.DomainResponse != null)
                    {
                        domain = new DomainResponseObject(res.DomainResponse);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error in ResetDomainFrequency, Error : {0} Parameters : domainID: {1}, : frequencyType : {2}", ex.Message, domainID, frequencyType);
            }

            return domain;
        }

        public ClientResponseStatus SuspendDomain(int domainId)
        {
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.Domains.Module.SuspendDomain(m_groupID, domainId);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to suspend a domain. Domain ID: {0}", domainId), ex);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

        public ClientResponseStatus ResumeDomain(int domainId)
        {
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.Domains.Module.ResumeDomain(m_groupID, domainId);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to resume a suspended domain. Domain ID: {0}", domainId), ex);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

        public DomainLimitationModuleResponse GetDomainLimitationModule(int dlmID)
        {
            DomainLimitationModuleResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.Domains.Module.Instance.GetDLM(m_groupID, dlmID);
                    response = new DomainLimitationModuleResponse();
                    response.DLM = new Objects.Responses.LimitationsManager(result.dlm);
                    response.Status = new TVPApiModule.Objects.Responses.Status(result.resp.Code, result.resp.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to get domain limitation module. DLM ID: {0}", dlmID), ex);
                response = new DomainLimitationModuleResponse();
                response.Status = ResponseUtils.ReturnGeneralErrorStatus("Error while calling webservice");
            }

            return response;
        }

        public ClientResponseStatus SetDomainRegion(int domainId, string extRegionId, string lookupKey)
        {
            ClientResponseStatus clientResponse;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var result = Core.Domains.Module.SetDomainRegion(m_groupID, domainId, extRegionId, lookupKey);
                    clientResponse = new ClientResponseStatus(result.Code, result.Message);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while trying to get domain limitation module. domainId: {0}, extRegionId: {1}, lookupKey: {2}", domainId, extRegionId, lookupKey), ex);
                clientResponse = ResponseUtils.ReturnGeneralErrorClientResponse("Error while calling webservice");
            }

            return clientResponse;
        }

        public Domain GetDomainByUser(string siteGuid)
        {
            Domain result = null;

            try
            {
                using (KMonitor km = new KMonitor(KLogMonitor.Events.eEvent.EVENT_WS, null, null, null, null))
                {
                    var res = Core.Domains.Module.GetDomainByUser(m_groupID, siteGuid);
                    if (res != null && res.Domain != null)
                        result = new Domain(res.Domain);
                }
            }
            catch (Exception ex)
            {
                logger.Error(string.Format("Error while GetDomainByUser. siteGuid: {0}", siteGuid), ex);
            }

            return result;
        }
    }
}

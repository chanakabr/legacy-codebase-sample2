using KLogMonitor;
using System;
using System.Reflection;
using TVPApi;
using TVPApiModule.Objects.Responses;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiModule.Services
{
    public class ApiDomainsService : ApiBase
    {
        #region Variables
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());


        private TVPPro.SiteManager.TvinciPlatform.Domains.module m_Module;

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
            public DomainStatus DomainStatus;
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
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Domains.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultPassword;

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
                    var res = m_Module.AddUserToDomain(m_wsUserName, m_wsPassword, domainID, AddedUserGuid, masterSiteGuid, false);
                    if (res != null)
                    {
                        domain = res.DomainResponse;
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
                    var res = m_Module.RemoveUserFromDomain(m_wsUserName, m_wsPassword, iDomainID, userGuidToRemove);
                    if (res != null)
                    {
                        domain = res.DomainResponse;
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
                    var res = m_Module.AddDeviceToDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
                    if (res != null)
                    {
                        domain = res.DomainResponse;
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
                    passed = m_Module.SetDomainRestriction(m_wsUserName, m_wsPassword, iDomainID, nRestriction);
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
                    domain = m_Module.SubmitAddDeviceToDomainRequest(m_wsUserName, m_wsPassword, domainId, userId, sUDID, deviceName, brandId);
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
                    domain = m_Module.ConfirmDeviceByDomainMaster(m_wsUserName, m_wsPassword, masterUn, udid, token);
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
                    network = m_Module.AddHomeNetworkToDomain(m_wsUserName, m_wsPassword, domainId, networkId, networkName, networkDesc);
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
                    var res = m_Module.UpdateDomainHomeNetwork(m_wsUserName, m_wsPassword, domainId, networkId, networkName, networkDesc, isActive);
                    network = ConvertStatusToNetworkResponseObject(res);
                }
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : UpdateDomainHomeNetwork, Error Message: {0} Parameters: domainId: {1}, networkId: {2}, networkName: {3}, networkDesc: {4}, isActive: {5}", ex.Message, domainId, networkId, networkName, networkDesc, isActive);
            }

            return network;
        }

        private NetworkResponseObject ConvertStatusToNetworkResponseObject(TVPPro.SiteManager.TvinciPlatform.Domains.Status res)
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
                    var res = m_Module.RemoveDomainHomeNetwork(m_wsUserName, m_wsPassword, domainId, networkId);
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
                    var res = m_Module.GetDomainHomeNetworks(m_wsUserName, m_wsPassword, domainId);
                    if (res != null)
                        homeNetworks = res.HomeNetworks;
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
                    var res = m_Module.RemoveDeviceFromDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID);
                    if (res != null)
                    {
                        domain = res.DomainResponse;
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
                    var res = m_Module.ChangeDeviceDomainStatus(m_wsUserName, m_wsPassword, iDomainID, sUDID, bActive);
                    if (res != null)
                    {
                        domain = res.DomainResponse;
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
                    var response = m_Module.GetDomainInfo(m_wsUserName, m_wsPassword, iDomainID);
                    if (response != null)
                    {
                        domain = response.Domain;
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
                    var response = m_Module.SetDomainInfo(m_wsUserName, m_wsPassword, iDomainID, sDomainName, sDomainDescription);

                    if (response != null)
                    {
                        domain = response.DomainResponse;
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
                    domains = m_Module.GetDeviceDomains(m_wsUserName, m_wsPassword, udid);
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
                    var response = m_Module.GetPINForDevice(m_wsUserName, m_wsPassword, udid, devBrandID);
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
                    var deviceRes = m_Module.RegisterDeviceToDomainWithPIN(m_wsUserName, m_wsPassword, pin, domainID, string.Empty);
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
                    response = m_Module.ResetDomain(m_wsUserName, m_wsPassword, domainID);
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
                    var result = m_Module.SetDeviceInfo(m_wsUserName, m_wsPassword, udid, deviceName);
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
                    var res = m_Module.AddDomain(m_wsUserName, m_wsPassword, domainName, domainDesc, masterGuid);

                    if (res != null)
                    {
                        response = res.DomainResponse;
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
                    var res = m_Module.AddDomainWithCoGuid(m_wsUserName, m_wsPassword, domainName, domainDesc, masterGuid, CoGuid);

                    if (res != null)
                    {
                        response = res.DomainResponse;
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
        //        //resp = m_Module.GetDomainCoGuid(m_wsUserName, m_wsPassword, nDomainID);
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
                    var res = m_Module.GetDomainByCoGuid(m_wsUserName, m_wsPassword, coGuid);

                    if (res != null)
                    {
                        response = res.DomainResponse;
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
                    domainID = m_Module.GetDomainIDByCoGuid(m_wsUserName, m_wsPassword, coGuid);
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
                    res = m_Module.SubmitAddUserToDomainRequest(m_wsUserName, m_wsPassword, userID, masterUsername);
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
                    response = m_Module.RemoveDomain(m_wsUserName, m_wsPassword, domainID);
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
                    response = m_Module.GetDomainIDsByOperatorCoGuid(m_wsUserName, m_wsPassword, operatorCoGuid);
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
                    deviceInfo = m_Module.GetDeviceInfo(m_wsUserName, m_wsPassword, sId, bIsUDID);
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
                    domain = m_Module.ChangeDomainMaster(m_wsUserName, m_wsPassword, domainID, currentMasterID, newMasterID);
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
                    var res = m_Module.ResetDomainFrequency(m_wsUserName, m_wsPassword, domainID, frequencyType);

                    if (res != null)
                    {
                        domain = res.DomainResponse;
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
                    var result = m_Module.SuspendDomain(m_wsUserName, m_wsPassword, domainId);
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
                    var result = m_Module.ResumeDomain(m_wsUserName, m_wsPassword, domainId);
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
                    var result = m_Module.GetDLM(m_wsUserName, m_wsPassword, dlmID);
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
                    var result = m_Module.SetDomainRegion(m_wsUserName, m_wsPassword, domainId, extRegionId, lookupKey);
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
                    var res = m_Module.GetDomainByUser(m_wsUserName, m_wsPassword, siteGuid);
                    if (res != null)
                        result = res.Domain;
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

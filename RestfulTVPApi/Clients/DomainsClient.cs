using RestfulTVPApi.Objects.Responses;
using RestfulTVPApi.Objects.Responses.Enums;
using ServiceStack.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using RestfulTVPApi.Objects.Extentions;
using RestfulTVPApi.Clients.ClientsCache;

namespace RestfulTVPApi.Clients
{
    public class DomainsClient : BaseClient
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(DomainsClient));

        #region Old Code - Will be removed later
        //private TVPPro.SiteManager.TvinciPlatform.Domains.module m_Module;

        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;

        //private int m_groupID;
        //private PlatformType m_platform;
        #endregion

        #endregion

        #region Ctor

        public DomainsClient(int groupID, RestfulTVPApi.Objects.Enums.PlatformType platform)
        {
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.Domains.module();
            //m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.URL;
            //m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultUser;
            //m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public DomainsClient()
        { }

        #endregion

        #region Properties

        protected RestfulTVPApi.Domains.module Domains
        {
            get
            {
                return (Module as RestfulTVPApi.Domains.module);
            }
        }

        #endregion

        //#region Public Static Functions

        //public static ApiDomainsService Instance(int groupId, PlatformType platform)
        //{
        //    return BaseService.Instance(groupId, platform, eService.DomainsService) as ApiDomainsService;
        //}

        //#endregion

        public DomainResponseObject AddUserToDomain(int domainID, string masterSiteGuid, int AddedUserGuid)
        {
            DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.AddUserToDomain(WSUserName, WSPassword, domainID, AddedUserGuid, int.Parse(masterSiteGuid), false);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public DomainResponseObject RemoveUserFromDomain(int iDomainID, string userGuidToRemove)
        {
            DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.RemoveUserFromDomain(WSUserName, WSPassword, iDomainID, userGuidToRemove);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public DomainResponseObject AddDeviceToDomain(int iDomainID, string sUDID, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject domain = null;

            domain = Execute(() =>
            {
                var res = Domains.AddDeviceToDomain(WSUserName, WSPassword, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
                if (res != null)
                    domain = res.ToApiObject();

                return domain;
            }) as DomainResponseObject;

            return domain;
        }

        public DomainResponseObject RemoveDeviceToDomain(int iDomainID, string sUDID)
        {
            DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.RemoveDeviceFromDomain(WSUserName, WSPassword, iDomainID, sUDID);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public DomainResponseObject ChangeDeviceDomainStatus(int iDomainID, string sUDID, bool bActive)
        {
            DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.ChangeDeviceDomainStatus(WSUserName, WSPassword, iDomainID, sUDID, bActive);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public DomainResponseObject GetDomainInfo(int iDomainID)
        {
            DomainResponseObject domainResponse = new DomainResponseObject();

            domainResponse = Execute(() =>
                {
                    Domain domain = new Domain();                    
                    var res = Domains.GetDomainInfo(WSUserName, WSPassword, iDomainID);
                    if (res != null)
                        domain = res.ToApiObject();

                    domainResponse.domain = domain;
                    domainResponse.domain_response_status = (DomainResponseStatus)domain.domain_status;

                    return domainResponse;
                }) as DomainResponseObject;

            return domainResponse;
        }

        public DomainResponseObject SetDomainInfo(int iDomainID, string sDomainName, string sDomainDescription)
        {
            DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.SetDomainInfo(WSUserName, WSPassword, iDomainID, sDomainName, sDomainDescription);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public List<DeviceDomain> GetDeviceDomains(string udid)
        {
            List<DeviceDomain> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Domains.GetDeviceDomains(WSUserName, WSPassword, udid);

                    if (response != null)
                    {
                        IEnumerable<Domain> domains = response.Where(d => d != null).Select(d => d.ToApiObject());

                        retVal = domains.Select(m => new DeviceDomain()
                        {
                            domain_id = m.domain_id,
                            domain_name = m.name,
                            site_guid = m.master_guids[0].ToString()
                        }).ToList();
                    }

                    return retVal;
                }) as List<DeviceDomain>;

            return retVal;
        }

        public string GetPINForDevice(string udid, int devBrandID)
        {
            string pin = string.Empty;

            pin = Execute(() =>
                {
                    return Domains.GetPINForDevice(WSUserName, WSPassword, udid, devBrandID);
                }) as string;

            return pin;
        }

        public DeviceResponseObject RegisterDeviceByPIN(string deviceName, int domainID, string pin)
        {
            DeviceResponseObject device = null;

            device = Execute(() =>
                {
                    var res = Domains.RegisterDeviceToDomainWithPIN(WSUserName, WSPassword, pin, domainID, deviceName);
                    if (res != null)
                        device = res.ToApiObject();

                    return device;
                }) as DeviceResponseObject;

            return device;
        }

        public DomainResponseObject ResetDomain(int domainID)
        {
            DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.ResetDomain(WSUserName, WSPassword, domainID);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as DomainResponseObject;

            return response;
        }

        public bool SetDeviceInfo(string udid, string deviceName)
        {
            bool response = false;

            response = Convert.ToBoolean(Execute(() =>
                {
                    return Domains.SetDeviceInfo(WSUserName, WSPassword, udid, deviceName);                    
                }));
            return response;
        }

        public DomainResponseObject AddDomain(string domainName, string domainDesc, int masterGuid)
        {
            DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.AddDomain(WSUserName, WSPassword, domainName, domainDesc, masterGuid);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as DomainResponseObject;

            return response;
        }

        public DomainResponseObject AddDomainWithCoGuid(string domainName, string domainDesc, int masterGuid, string CoGuid)
        {
            DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.AddDomainWithCoGuid(WSUserName, WSPassword, domainName, domainDesc, masterGuid, CoGuid);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as DomainResponseObject;

            return response;
        }

        public string GetDomainCoGuid(int nDomainID)
        {
            string resp = string.Empty;

            resp = Execute(() =>
                {
                    try
                    {
                        //resp = m_Module.GetDomainCoGuid(m_wsUserName, m_wsPassword, nDomainID);
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorFormat("Error calling webservice protocol : GetDomainCoGuid, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, nDomainID);
                    }
                    return resp;
                }) as string;

            return resp;
        }

        public DomainResponseObject GetDomainByCoGuid(string coGuid)
        {
            DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.GetDomainByCoGuid(WSUserName, WSPassword, coGuid);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as DomainResponseObject;

            return response;
        }

        public int GetDomainIDByCoGuid(string coGuid)
        {
            int domainID = 0;

            domainID = Convert.ToInt32(Execute(() =>
                {
                    domainID = Domains.GetDomainIDByCoGuid(WSUserName, WSPassword, coGuid);
                    return domainID;
                }));

            return domainID;
        }

        public DomainResponseObject SubmitAddUserToDomainRequest(string siteGuid, string masterUsername)
        {
            DomainResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Domains.SubmitAddUserToDomainRequest(WSUserName, WSPassword, int.Parse(siteGuid), masterUsername);
                        if (response != null)
                            res = response.ToApiObject();
                    
                    return res;
                }) as DomainResponseObject;

            return res;
        }

        public DomainResponseStatus RemoveDomain(int domainID)
        {
            DomainResponseStatus response = DomainResponseStatus.UnKnown;

            response = (DomainResponseStatus)Enum.Parse(typeof(DomainResponseStatus), Execute(() =>
                {
                    response = (DomainResponseStatus)Domains.RemoveDomain(WSUserName, WSPassword, domainID);
                    
                    return response;
                }).ToString());

            return response;
        }

        public List<int> GetDomainIDsByOperatorCoGuid(string operatorCoGuid)
        {
            List<int> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Domains.GetDomainIDsByOperatorCoGuid(WSUserName, WSPassword, operatorCoGuid);
                    if (response != null)
                        retVal = response.ToList();

                    return retVal;
                }) as List<int>;

            return retVal;
        }

        public bool SetDomainRestriction(int domainId, int restriction)
        {
            bool retVal = false;
            retVal = Convert.ToBoolean(Execute(() =>
            {
                return Domains.SetDomainRestriction(WSUserName, WSPassword, domainId, restriction);                
            }));

            return retVal;            
        }

        public DomainResponseObject SubmitAddDeviceToDomainRequest(string udid, int domainId, int siteGuid, string deviceName, int brandId)
        {
            DomainResponseObject retVal = null;

            retVal = Execute(() =>
                {
                    if (siteGuid > 0)
                    {
                        var res = Domains.SubmitAddDeviceToDomainRequest(WSUserName, WSPassword, domainId, siteGuid, udid, deviceName, brandId);
                        if (res != null)
                        {
                            retVal = res.ToApiObject();
                        }                        
                    }
                    else
                    {
                        retVal = null;
                    }

                    return retVal;

                }) as DomainResponseObject;            

            return retVal;
        }

        public NetworkResponseObject AddHomeNetworkToDomain(long domainId, string networkId, string networkName, string networkDescription)
        {
            NetworkResponseObject network = null;

            network = Execute(() =>
            {
                var res = Domains.AddHomeNetworkToDomain(WSUserName, WSPassword, domainId, networkId, networkName, networkDescription);
                    if (res != null)
                    {
                        network = res.ToApiObject();
                    }                

                return network;

            }) as NetworkResponseObject;

            return network;
        }

        public NetworkResponseObject UpdateDomainHomeNetwork(long domainId, string networkId, string networkName, string networkDescription, bool isActive)
        {
            NetworkResponseObject network = null;

            network = Execute(() =>
            {
                var res = Domains.UpdateDomainHomeNetwork(WSUserName, WSPassword, domainId, networkId, networkName, networkDescription, isActive);
                if (res != null)
                {
                    network = res.ToApiObject();
                }

                return network;

            }) as NetworkResponseObject;

            return network;
        }

        public NetworkResponseObject RemoveDomainHomeNetwork(long domainId, string networkId)
        {
            NetworkResponseObject network = null;

            network = Execute(() =>
            {
                var res = Domains.RemoveDomainHomeNetwork(WSUserName, WSPassword, domainId, networkId);
                if (res != null)
                {
                    network = res.ToApiObject();
                }

                return network;

            }) as NetworkResponseObject;

            return network;
        }

        public List<HomeNetwork> GetDomainHomeNetworks(long domainId)
        {
            List<HomeNetwork> networks = null;

            networks = Execute(() =>
                {
                    var response = Domains.GetDomainHomeNetworks(WSUserName, WSPassword, domainId);
                    if (response != null)
                    {
                        networks = response.Where(network => network != null).Select(network => network.ToApiObject()).ToList();                         
                    }

                    return networks;
                }) as List<HomeNetwork>;

            return networks;
        }

        public DeviceResponseObject GetDeviceInfo(string id, bool isUDID)
        {
            DeviceResponseObject deviceInfo = null;

            deviceInfo = Execute(() =>
            {
                var response = Domains.GetDeviceInfo(WSUserName, WSPassword, id, isUDID);
                if (response != null)
                {
                    deviceInfo = response.ToApiObject();
                }

                return deviceInfo;
            }) as DeviceResponseObject;

            return deviceInfo;
        }

        public DomainResponseObject ChangeDomainMaster(int domainId, int currentMasterId, int newMasterId)
        {
            DomainResponseObject domainResponse = null;

            domainResponse = Execute(() =>
            {
                var response = Domains.ChangeDomainMaster(WSUserName, WSPassword, domainId, currentMasterId, newMasterId);
                if (response != null)
                {
                    domainResponse = response.ToApiObject();
                }

                return domainResponse;
            }) as DomainResponseObject;

            return domainResponse;
        }

        public DomainResponseObject ResetDomainFrequency(int domainId, int frequencyType)
        {
            DomainResponseObject domainResponse = null;

            domainResponse = Execute(() =>
            {
                var response = Domains.ResetDomainFrequency(WSUserName, WSPassword, domainId, frequencyType);
                if (response != null)
                {
                    domainResponse = response.ToApiObject();
                }

                return domainResponse;
            }) as DomainResponseObject;

            return domainResponse;
        }

        public List<string> GetDomainUsersList(int domain_id)
        {
            List<string> users_ids = null;

            users_ids = Execute(() =>
            {
                var response = Domains.GetDomainUserList(WSUserName, WSPassword, domain_id);
                if (response != null)
                    users_ids = response.ToList();

                return users_ids;
            }) as List<string>;

            return users_ids;
        }
    }
}
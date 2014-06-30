using System;
using System.Collections.Generic;
using System.Linq;
using log4net;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Extentions;
using TVPApiModule.Context;
//using System.Reflection;

namespace TVPApiModule.Services
{
    public class ApiDomainsService : BaseService
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiDomainsService));

        #region Old Code - Will be removed later
        //private TVPPro.SiteManager.TvinciPlatform.Domains.module m_Module;

        //private string m_wsUserName = string.Empty;
        //private string m_wsPassword = string.Empty;

        //private int m_groupID;
        //private PlatformType m_platform;
        #endregion

        #endregion

        #region Ctor

        public ApiDomainsService(int groupID, PlatformType platform)
        {
            //m_Module = new TVPPro.SiteManager.TvinciPlatform.Domains.module();
            //m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.URL;
            //m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultUser;
            //m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultPassword;

            //m_groupID = groupID;
            //m_platform = platform;
        }

        public ApiDomainsService()
        { }

        #endregion

        #region Properties

        protected TVPPro.SiteManager.TvinciPlatform.Domains.module Domains
        {
            get
            {
                return (m_Module as TVPPro.SiteManager.TvinciPlatform.Domains.module);
            }
        }

        #endregion

        //#region Public Static Functions

        //public static ApiDomainsService Instance(int groupId, PlatformType platform)
        //{
        //    return BaseService.Instance(groupId, platform, eService.DomainsService) as ApiDomainsService;
        //}

        //#endregion

        public TVPApiModule.Objects.Responses.DomainResponseObject AddUserToDomain(int domainID, string masterSiteGuid, int AddedUserGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.AddUserToDomain(m_wsUserName, m_wsPassword, domainID, AddedUserGuid, int.Parse(masterSiteGuid), false);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject RemoveUserFromDomain(int iDomainID, string userGuidToRemove)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.RemoveUserFromDomain(m_wsUserName, m_wsPassword, iDomainID, userGuidToRemove);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject AddDeviceToDomain(int iDomainID, string sUDID, string sDeviceName, int iDeviceBrandID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            domain = Execute(() =>
            {
                var res = Domains.AddDeviceToDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
                if (res != null)
                    domain = res.ToApiObject();

                return domain;
            }) as DomainResponseObject;

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject RemoveDeviceToDomain(int iDomainID, string sUDID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.RemoveDeviceFromDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject ChangeDeviceDomainStatus(int iDomainID, string sUDID, bool bActive)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.ChangeDeviceDomainStatus(m_wsUserName, m_wsPassword, iDomainID, sUDID, bActive);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as DomainResponseObject;

            return domain;
        }

        public TVPApiModule.Objects.Responses.Domain GetDomainInfo(int iDomainID)
        {
            TVPApiModule.Objects.Responses.Domain domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.GetDomainInfo(m_wsUserName, m_wsPassword, iDomainID);
                    if (res != null)
                        domain = res.ToApiObject();

                    return domain;
                }) as TVPApiModule.Objects.Responses.Domain;

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject SetDomainInfo(int iDomainID, string sDomainName, string sDomainDescription)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            domain = Execute(() =>
                {
                    var res = Domains.SetDomainInfo(m_wsUserName, m_wsPassword, iDomainID, sDomainName, sDomainDescription);
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
                    var response = Domains.GetDeviceDomains(m_wsUserName, m_wsPassword, udid);

                    if (response != null)
                    {
                        IEnumerable<TVPApiModule.Objects.Responses.Domain> domains = response.Where(d => d != null).Select(d => d.ToApiObject());

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
                    return Domains.GetPINForDevice(m_wsUserName, m_wsPassword, udid, devBrandID);
                }) as string;

            return pin;
        }

        public TVPApiModule.Objects.Responses.DeviceResponseObject RegisterDeviceByPIN(string udid, int domainID, string pin)
        {
            TVPApiModule.Objects.Responses.DeviceResponseObject device = null;

            device = Execute(() =>
                {
                    var res = Domains.RegisterDeviceToDomainWithPIN(m_wsUserName, m_wsPassword, pin, domainID, string.Empty);
                    if (res != null)
                        device = res.ToApiObject();

                    return device;
                }) as DeviceResponseObject;

            return device;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject ResetDomain(int domainID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.ResetDomain(m_wsUserName, m_wsPassword, domainID);
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
                    return Domains.SetDeviceInfo(m_wsUserName, m_wsPassword, udid, deviceName);                    
                }));
            return response;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject AddDomain(string domainName, string domainDesc, int masterGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.AddDomain(m_wsUserName, m_wsPassword, domainName, domainDesc, masterGuid);
                    if (res != null)
                        response = res.ToApiObject();

                    return response;
                }) as DomainResponseObject;

            return response;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject AddDomainWithCoGuid(string domainName, string domainDesc, int masterGuid, string CoGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.AddDomainWithCoGuid(m_wsUserName, m_wsPassword, domainName, domainDesc, masterGuid, CoGuid);
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

        public TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(string coGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;

            response = Execute(() =>
                {
                    var res = Domains.GetDomainByCoGuid(m_wsUserName, m_wsPassword, coGuid);
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
                    domainID = Domains.GetDomainIDByCoGuid(m_wsUserName, m_wsPassword, coGuid);
                    return domainID;
                }));

            return domainID;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject SubmitAddUserToDomainRequest(string siteGuid, string masterUsername)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject res = null;

            res = Execute(() =>
                {
                    var response = Domains.SubmitAddUserToDomainRequest(m_wsUserName, m_wsPassword, int.Parse(siteGuid), masterUsername);
                        if (response != null)
                            res = response.ToApiObject();
                    
                    return res;
                }) as DomainResponseObject;

            return res;
        }

        public TVPApiModule.Objects.Responses.DomainResponseStatus RemoveDomain(int domainID)
        {
            TVPApiModule.Objects.Responses.DomainResponseStatus response = TVPApiModule.Objects.Responses.DomainResponseStatus.UnKnown;

            response = (TVPApiModule.Objects.Responses.DomainResponseStatus)Enum.Parse(typeof(TVPApiModule.Objects.Responses.DomainResponseStatus), Execute(() =>
                {
                    response = (TVPApiModule.Objects.Responses.DomainResponseStatus)Domains.RemoveDomain(m_wsUserName, m_wsPassword, domainID);
                    
                    return response;
                }).ToString());

            return response;
        }

        public List<int> GetDomainIDsByOperatorCoGuid(string operatorCoGuid)
        {
            List<int> retVal = null;

            retVal = Execute(() =>
                {
                    var response = Domains.GetDomainIDsByOperatorCoGuid(m_wsUserName, m_wsPassword, operatorCoGuid);
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
                return Domains.SetDomainRestriction(m_wsUserName, m_wsPassword, domainId, restriction);                
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
                        var res = Domains.SubmitAddDeviceToDomainRequest(m_wsUserName, m_wsPassword, domainId, siteGuid, udid, deviceName, brandId);
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
                var res = Domains.AddHomeNetworkToDomain(m_wsUserName, m_wsPassword, domainId, networkId, networkName, networkDescription);
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
                var res = Domains.UpdateDomainHomeNetwork(m_wsUserName, m_wsPassword, domainId, networkId, networkName, networkDescription, isActive);
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
                var res = Domains.RemoveDomainHomeNetwork(m_wsUserName, m_wsPassword, domainId, networkId);
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
                    var response = Domains.GetDomainHomeNetworks(m_wsUserName, m_wsPassword, domainId);
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
                var response = Domains.GetDeviceInfo(m_wsUserName, m_wsPassword, id, isUDID);
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
                var response = Domains.ChangeDomainMaster(m_wsUserName, m_wsPassword, domainId, currentMasterId, newMasterId);
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
                var response = Domains.ResetDomainFrequency(m_wsUserName, m_wsPassword, domainId, frequencyType);
                if (response != null)
                {
                    domainResponse = response.ToApiObject();
                }

                return domainResponse;
            }) as DomainResponseObject;

            return domainResponse;
        }
    }
}

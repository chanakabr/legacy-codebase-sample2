using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPApiModule.Services
{
    public class ApiDomainsService
    {
        #region Variables
        private static ILog logger = LogManager.GetLogger(typeof(ApiDomainsService));

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
        }

        [Serializable]
        public enum eDeviceRegistrationStatus { Success = 0, Invalid = 1, Error = 2 }

        [Serializable]
        public struct DeviceRegistration
        {
            public string UDID;
            public eDeviceRegistrationStatus RegStatus;
        }
        #endregion

        #region C'tor
        public ApiDomainsService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Domains.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }
        #endregion C'tor

        #region Public methods

        public DomainResponseObject AddUserToDomain(int iDomainID, string sSiteGuid, bool bMaster)
        {
            DomainResponseObject domain = null;
            try
            {
                domain = m_Module.AddUserToDomain(m_wsUserName, m_wsPassword, iDomainID, sSiteGuid, bMaster);             
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddUserToDomain, Error Message: {0} Parameters: sSiteGuid: {1}, bMaster: {2}", ex.Message, sSiteGuid, bMaster);
            }

            return domain;
        }

        public Domain RemoveUserFromDomain(int iDomainID, string sSiteGuid)
        {
            Domain domain = null;

            try
            {
                DomainResponseObject res = m_Module.RemoveUserFromDomain(m_wsUserName, m_wsPassword, iDomainID, sSiteGuid);

                if (res.m_oDomainResponseStatus == DomainResponseStatus.OK)
                    domain = res.m_oDomain;                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RemoveUserFromDomain, Error Message: {0} Parameters: iDomainID: {1}, sSiteGUID: {2}", ex.Message, iDomainID, sSiteGuid);
            }

            return domain;
        }

        public DomainResponseObject AddDeviceToDomain(int iDomainID, string sUDID, string sDeviceName, int iDeviceBrandID)
        {
            DomainResponseObject domain = null;
 
            try
            {
                domain = m_Module.AddDeviceToDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID, sDeviceName, iDeviceBrandID);                
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDeviceToDomain, Error Message: {0} Parameters: iDomainID: {1}, sUDID: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
            }

            return domain;
        }

        public DomainResponseObject RemoveDeviceToDomain(int iDomainID, string sUDID)
        {
            DomainResponseObject domain = null;

            try
            {
                domain = m_Module.RemoveDeviceFromDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID);
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
                domain = m_Module.ChangeDeviceDomainStatus(m_wsUserName, m_wsPassword, iDomainID, sUDID, bActive);                
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
                domain = m_Module.GetDomainInfo(m_wsUserName, m_wsPassword, iDomainID);
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
                domain = m_Module.SetDomainInfo(m_wsUserName, m_wsPassword, iDomainID, sDomainName, sDomainDescription);
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
                domains = m_Module.GetDeviceDomains(m_wsUserName, m_wsPassword, udid);
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
                pin = m_Module.GetPINForDevice(m_wsUserName, m_wsPassword, udid, devBrandID);
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
                device = m_Module.RegisterDeviceToDomainWithPIN(m_wsUserName, m_wsPassword, pin, domainID, string.Empty);
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
                response = m_Module.ResetDomain(m_wsUserName, m_wsPassword, domainID);
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
                response = m_Module.SetDeviceInfo(m_wsUserName, m_wsPassword, udid, deviceName);
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
                response = m_Module.AddDomain(m_wsUserName, m_wsPassword, domainName, domainDesc, masterGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDomain, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, masterGuid);
            }

            return response;
        }

        #endregion          
   
    }
}

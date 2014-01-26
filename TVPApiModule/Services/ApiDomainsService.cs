using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPApiModule.Objects.Responses;
using TVPApiModule.Extentions;

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

        public ApiDomainsService(int groupID, PlatformType platform)
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Domains.module();
            m_Module.Url = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.URL;
            m_wsUserName = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultUser;
            m_wsPassword = ConfigManager.GetInstance().GetConfig(groupID, platform).PlatformServicesConfiguration.Data.DomainsService.DefaultPassword;

            m_groupID = groupID;
            m_platform = platform;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject AddUserToDomain(int domainID, int masterSiteGuid, int AddedUserGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;
            try
            {
                var res = m_Module.AddUserToDomain(m_wsUserName, m_wsPassword, domainID, AddedUserGuid, masterSiteGuid, false);
                if (res != null)
                    domain = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddUserToDomain, Error Message: {0} Parameters: AddedUserGuid: {1}, masterSiteGuid: {2}", ex.Message, AddedUserGuid, masterSiteGuid);
            }

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject RemoveUserFromDomain(int iDomainID, string userGuidToRemove)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            try
            {
                var res = m_Module.RemoveUserFromDomain(m_wsUserName, m_wsPassword, iDomainID, userGuidToRemove);
                if (res != null)
                    domain = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RemoveUserFromDomain, Error Message: {0} Parameters: iDomainID: {1}, userGuidToRemove: {2}", ex.Message, iDomainID, userGuidToRemove);
            }

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject AddDeviceToDomain(int iDomainID, string sUDID, string sDeviceName, int iDeviceBrandID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            try
            {
                var res = m_Module.AddDeviceToDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
                if (res != null)
                    domain = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDeviceToDomain, Error Message: {0} Parameters: iDomainID: {1}, sUDID: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, iDomainID, sUDID, sDeviceName, iDeviceBrandID);
            }

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject RemoveDeviceToDomain(int iDomainID, string sUDID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            try
            {
                var res = m_Module.RemoveDeviceFromDomain(m_wsUserName, m_wsPassword, iDomainID, sUDID);
                if (res != null)
                    domain = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDeviceToDomain, Error Message: {0} Parameters: iDomainID: {1}, sUDID: {2}, sDeviceName: {3}, iDeviceBrandID: {4}", ex.Message, iDomainID, sUDID);
            }

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject ChangeDeviceDomainStatus(int iDomainID, string sUDID, bool bActive)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            try
            {
                var res = m_Module.ChangeDeviceDomainStatus(m_wsUserName, m_wsPassword, iDomainID, sUDID, bActive);
                if (res != null)
                    domain = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : ChangeDeviceDomainStatus, Error Message: {0} Parameters: iDomainID: {1}, bActive: {2}", ex.Message, iDomainID, sUDID, bActive);
            }

            return domain;
        }

        public TVPApiModule.Objects.Responses.Domain GetDomainInfo(int iDomainID)
        {
            TVPApiModule.Objects.Responses.Domain domain = null;

            try
            {
                var res = m_Module.GetDomainInfo(m_wsUserName, m_wsPassword, iDomainID);
                if (res != null)
                    domain = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainInfo, Error Message: {0} Parameters: iDomainID: {1}", ex.Message, iDomainID);
            }

            return domain;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject SetDomainInfo(int iDomainID, string sDomainName, string sDomainDescription)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject domain = null;

            try
            {
                var res = m_Module.SetDomainInfo(m_wsUserName, m_wsPassword, iDomainID, sDomainName, sDomainDescription);
                if (res != null)
                    domain = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SetDomainInfo, Error Message: {0} Parameters: iDomainID: {1}, sDomainName: {2}, sDomainDescription: {3}", ex.Message, iDomainID, sDomainName, sDomainDescription);
            }

            return domain;
        }

        public IEnumerable<TVPApiModule.Objects.Responses.Domain> GetDeviceDomains(string udid)
        {
            IEnumerable<TVPApiModule.Objects.Responses.Domain> domains = null;

            try
            {
                var response = m_Module.GetDeviceDomains(m_wsUserName, m_wsPassword, udid);
                if (response != null)
                    domains = response.Where(d => d != null).Select(d => d.ToApiObject());
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

        public TVPApiModule.Objects.Responses.DeviceResponseObject RegisterDeviceByPIN(string udid, int domainID, string pin)
        {
            TVPApiModule.Objects.Responses.DeviceResponseObject device = null;
            try
            {
                var res = m_Module.RegisterDeviceToDomainWithPIN(m_wsUserName, m_wsPassword, pin, domainID, string.Empty);
                if (res != null)
                    device = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : RegisterDeviceByPIN, Error Message: {0} Parameters: udid: {1}, pin: {2}", ex.Message, udid, pin);
            }

            return device;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject ResetDomain(int domainID)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;
            try
            {
                var res = m_Module.ResetDomain(m_wsUserName, m_wsPassword, domainID);
                if (res != null)
                    response = res.ToApiObject();
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

        public TVPApiModule.Objects.Responses.DomainResponseObject AddDomain(string domainName, string domainDesc, int masterGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;
            try
            {
                var res = m_Module.AddDomain(m_wsUserName, m_wsPassword, domainName, domainDesc, masterGuid);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDomain, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, masterGuid);
            }

            return response;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject AddDomainWithCoGuid(string domainName, string domainDesc, int masterGuid, string CoGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;
            try
            {
                var res = m_Module.AddDomainWithCoGuid(m_wsUserName, m_wsPassword, domainName, domainDesc, masterGuid, CoGuid);
                if (res != null)
                    response = res.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : AddDomainWithCoGuid, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, masterGuid);
            }
            return response;
        }

        public string GetDomainCoGuid(int nDomainID)
        {
            string resp = string.Empty;
            try
            {
                //resp = m_Module.GetDomainCoGuid(m_wsUserName, m_wsPassword, nDomainID);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainCoGuid, Error Message: {0} Parameters: masterGuid: {1}", ex.Message, nDomainID);
            }
            return resp;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject GetDomainByCoGuid(string coGuid)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject response = null;
            try
            {
                var res = m_Module.GetDomainByCoGuid(m_wsUserName, m_wsPassword, coGuid);
                if (res != null)
                    response = res.ToApiObject();
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
                domainID = m_Module.GetDomainIDByCoGuid(m_wsUserName, m_wsPassword, coGuid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : GetDomainIDByCoGuid, Error Message: {0} Parameters: coGuid: {1}", ex.Message, coGuid);
            }
            return domainID;
        }

        public TVPApiModule.Objects.Responses.DomainResponseObject SubmitAddUserToDomainRequest(int userID, string masterUsername)
        {
            TVPApiModule.Objects.Responses.DomainResponseObject res = null;
            try
            {
                var response = m_Module.SubmitAddUserToDomainRequest(m_wsUserName, m_wsPassword, userID, masterUsername);
                if (response != null)
                    res = response.ToApiObject();
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error calling webservice protocol : SubmitAddUserToDomainRequest, Error Message: {0} Parameters: UserID: {1}", ex.Message, userID);
            }
            return res;
        }

        public TVPApiModule.Objects.Responses.DomainResponseStatus RemoveDomain(int domainID)
        {
            TVPApiModule.Objects.Responses.DomainResponseStatus response = TVPApiModule.Objects.Responses.DomainResponseStatus.UnKnown;

            try
            {
                response = (TVPApiModule.Objects.Responses.DomainResponseStatus)m_Module.RemoveDomain(m_wsUserName, m_wsPassword, domainID);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error in RemoveDomain, Error : {0} Parameters : Domain ID: {1}", e.Message, domainID);
            }

            return response;
        }

        public IEnumerable<int> GetDomainIDsByOperatorCoGuid(string operatorCoGuid)
        {
            IEnumerable<int> response = null;

            try
            {
                response = m_Module.GetDomainIDsByOperatorCoGuid(m_wsUserName, m_wsPassword, operatorCoGuid);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error in GetDomainIDsByOperatorCoGuid, Error : {0} Parameters : operatorCoGuid: {1}", e.Message, operatorCoGuid);
            }

            return response;
        }

    }
}

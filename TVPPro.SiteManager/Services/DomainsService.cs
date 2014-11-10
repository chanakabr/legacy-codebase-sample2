using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using TVPPro.Configuration.PlatformServices;
using TVPPro.SiteManager.TvinciPlatform.Domains;
using TVPPro.SiteManager.Manager;

namespace TVPPro.SiteManager.Services
{
    public class DomainsService
    {
        #region Fields
        private readonly ILog logger = LogManager.GetLogger(typeof(DomainsService));
        static object instanceLock = new object();

        private static DomainsService m_Instance;
        public static DomainsService Instance
        {
            get
            {
                if (m_Instance == null)
                {
                    lock (instanceLock)
                    {
                        m_Instance = new DomainsService();
                    }
                }

                return m_Instance;
            }
        }


        private string wsUserName;
        private string wsPassword;

        private TvinciPlatform.Domains.module m_Module;

        #endregion

        #region C'tor
        public DomainsService()
        {
            m_Module = new TVPPro.SiteManager.TvinciPlatform.Domains.module();
            m_Module.Url = PlatformServicesConfiguration.Instance.Data.DomainsService.URL;

            wsUserName = PlatformServicesConfiguration.Instance.Data.DomainsService.DefaultUser;
            wsPassword = PlatformServicesConfiguration.Instance.Data.DomainsService.DefaultPassword;
        }
        #endregion

        public Domain GetDomainInfo()
        {
            Domain domain = null;
            int did = 0;
            try
            {
                did = UsersService.Instance.GetDomainID();
                domain = m_Module.GetDomainInfo(wsUserName, wsPassword, did);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in GetDomainInfo, Error : {0} Parameters : Domain : {1}", ex.Message, did);
                return null;
            }

            return domain;
        }

        public Domain DeactivateDevice(string udid)
        {
            int did = 0;
            Domain domain = null;
            try
            {
                did = UsersService.Instance.GetDomainID();
                DomainResponseObject res = m_Module.ChangeDeviceDomainStatus(wsUserName, wsPassword, did, udid, false);

                if (res.m_oDomainResponseStatus == DomainResponseStatus.OK)
                    domain = res.m_oDomain;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in DeactivateDevice, Error : {0} Parameters : Domain : {1}, UDID: {2}", ex.Message, did, udid);
                return null;
            }

            return domain;
        }

        public Domain ActivateDevice(string udid)
        {
            int did = 0;
            Domain domain = null;
            try
            {
                did = UsersService.Instance.GetDomainID();
                DomainResponseObject res = m_Module.ChangeDeviceDomainStatus(wsUserName, wsPassword, did, udid, true);
                domain = res.m_oDomain;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in ActivateDevice, Error : {0} Parameters : Domain : {1}, UDID: {2}", ex.Message, did, udid);
                return domain;
            }

            return domain;
        }

        public DomainResponseStatus DeleteDevice(string udid)
        {
            int did = 0;
            DomainResponseStatus resCode = DomainResponseStatus.UnKnown;
            try
            {
                did = UsersService.Instance.GetDomainID();
                DomainResponseObject res = m_Module.RemoveDeviceFromDomain(wsUserName, wsPassword, did, udid);
                resCode = res.m_oDomainResponseStatus;
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in DeleteDevice, Error : {0} Parameters : Domain : {1}, UDID: {2}", ex.Message, did, udid);
                return resCode;
            }

            return resCode;
        }

        public DomainResponseObject DeleteDeviceDetailed(string udid)
        {
            int did = 0;
            DomainResponseObject response = null;
            try
            {
                did = UsersService.Instance.GetDomainID();
                response = m_Module.RemoveDeviceFromDomain(wsUserName, wsPassword, did, udid);
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in DeleteDevice, Error : {0} Parameters : Domain : {1}, UDID: {2}", ex.Message, did, udid);
                return response;
            }

            return response;
        }

        public bool CreateDomain()
        {
            DomainResponseObject domain = null;
            try
            {
                if (UsersService.Instance.GetDomainID() > 0)
                    return false;

                var domainName = string.Concat(UsersService.Instance.UserContext.UserResponse.m_user.m_oBasicData.m_sFirstName, "'s Domain'");

                domain = m_Module.AddDomain(wsUserName, wsPassword, domainName, string.Empty, int.Parse(UsersService.Instance.GetUserID()));
            }
            catch (Exception ex)
            {
                logger.ErrorFormat("Error occured in CreateDomain, Error : {0} Parameters : siteGuid : {1}", ex.Message, UsersService.Instance.GetUserID());
            }

            return domain != null && domain.m_oDomainResponseStatus == DomainResponseStatus.OK;
        }

        public DeviceResponseObject RegisterDeviceByPinCode(string pinCode)
        {
            int domainId = 0;
            DeviceResponseObject device = null;
            try
            {
                domainId = UsersService.Instance.GetDomainID();
                device = m_Module.RegisterDeviceToDomainWithPIN(wsUserName, wsPassword, pinCode, domainId, string.Empty);                
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occured in RegisterDeviceByPinCode, Error : {0} Parameters : Domain : {1}, PIN Code: {2}", e.Message, domainId, pinCode);
                return null;
            }

            return device;
        }

        public bool SetDeviceInfo(string udid, string deviceName)
        {
            bool bRes = false;            

            try
            {                
                bRes = m_Module.SetDeviceInfo(wsUserName, wsPassword, udid, deviceName);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occured in SetDeviceInfo, Error : {0} Parameters : Device UDID: {1}, Device Name {2}", e.Message, udid, deviceName);                
            }

            return bRes;
        }

        public DeviceResponseObject GetDeviceInfo(string udid)
        {
            DeviceResponseObject device = null;

            try
            {
                device = m_Module.GetDeviceInfo(wsUserName, wsPassword, udid, true);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occured in GetDeviceInfo, Error : {0} Parameters : Device UDID: {1}, IsUDID {2}", e.Message, udid, true);
            }

            return device;
        }

        public DomainResponseObject AddDeviceToDomain(string udid, string deviceName)
        {
            int domainId = 0;
            DomainResponseObject device = null;

            try
            {                
                domainId = UsersService.Instance.GetDomainID();
                device = m_Module.AddDeviceToDomain(wsUserName, wsPassword, domainId, udid, deviceName, 22);
            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occured in SetDeviceInfo, Error : {0} Parameters : Device UDID: {1}, Device Name {2}", e.Message, udid, deviceName);
            }

            return device;
        }
        
    }
}

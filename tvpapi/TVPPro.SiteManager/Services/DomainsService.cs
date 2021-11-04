using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.Configuration.PlatformServices;
using TVPPro.SiteManager.Manager;
using KLogMonitor;
using System.Reflection;
using Core.Users;

namespace TVPPro.SiteManager.Services
{
    public class DomainsService
    {
        #region Fields
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
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
        private int groupId;

        #endregion

        #region C'tor

        public DomainsService()
        {
            wsUserName = PlatformServicesConfiguration.Instance.Data.DomainsService.DefaultUser;
            wsPassword = PlatformServicesConfiguration.Instance.Data.DomainsService.DefaultPassword;
            groupId = Utils.GetGroupID(wsUserName, wsPassword);
        }
        #endregion

        public Domain GetDomainInfo()
        {
            Domain domain = null;
            int did = 0;
            try
            {
                did = UsersService.Instance.GetDomainID();
                var response = Core.Domains.Module.GetDomainInfo(groupId, did);
                if (response != null)
                {
                    domain = response.Domain;
                }
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
                var res = Core.Domains.Module.ChangeDeviceDomainStatus(groupId, did, udid, false);

                if (res != null && res.DomainResponse != null && res.DomainResponse.m_oDomainResponseStatus == DomainResponseStatus.OK)
                {
                    domain = res.DomainResponse.m_oDomain;
                }
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
                var res = Core.Domains.Module.ChangeDeviceDomainStatus(groupId, did, udid, true);
                if (res != null && res.DomainResponse != null)
                {
                    domain = res.DomainResponse.m_oDomain;
                }
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
                DomainStatusResponse res = Core.Domains.Module.RemoveDeviceFromDomain(groupId, did, udid);
                if (res != null && res.DomainResponse != null)
                {
                    resCode = res.DomainResponse.m_oDomainResponseStatus;
                }
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
                var res = Core.Domains.Module.RemoveDeviceFromDomain(groupId, did, udid);
                if (res != null)
                {
                    response = res.DomainResponse;
                }
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

                var res = Core.Domains.Module.AddDomain(groupId, domainName, string.Empty, int.Parse(UsersService.Instance.GetUserID()), null);

                if (res != null)
                {
                    domain = res.DomainResponse;
                }

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
                var deviceRes = Core.Domains.Module.RegisterDeviceToDomainWithPIN(groupId, pinCode, domainId, string.Empty);
                if (deviceRes != null)
                {
                    device = deviceRes.Device;
                }
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
                var res = Core.Domains.Module.SetDeviceInfo(groupId, udid, deviceName);
                if (res != null && res.Code == 0)
                {
                    bRes = true;
                }
                else
                {
                    bRes = false;
                }
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
                device = Core.Domains.Module.Instance.GetDeviceInfo(groupId, udid, true);
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
            DomainResponseObject domain = null;

            try
            {
                domainId = UsersService.Instance.GetDomainID();
                var res = Core.Domains.Module.AddDeviceToDomain(groupId, domainId, udid, deviceName, 22);
                if (res != null)
                {
                    domain = res.DomainResponse;
                }

            }
            catch (Exception e)
            {
                logger.ErrorFormat("Error occured in SetDeviceInfo, Error : {0} Parameters : Device UDID: {1}, Device Name {2}", e.Message, udid, deviceName);
            }

            return domain;
        }

    }
}

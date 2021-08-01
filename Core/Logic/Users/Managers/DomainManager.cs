using ApiObjects;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading;
using APILogic.Api.Managers;
using Core.Users;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiObjects.Response;

namespace ApiLogic.Users.Managers
{
    public interface IDomainManager
    {
        DomainResponseStatus AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string deviceName, int brandID, Domain domain, ref Device device, out bool bRemove);
    }

    public class DomainManager : IDomainManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<DomainManager> lazy = new Lazy<DomainManager>(() => new DomainManager(RolesPermissionsManager.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static DomainManager Instance => lazy.Value;

        private readonly IRolesPermissionsManager _rolesPermissionsManager;

        public DomainManager(IRolesPermissionsManager rolesPermissionsManager)
        {
            _rolesPermissionsManager = rolesPermissionsManager;
        }

        public DomainResponseStatus AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string deviceName, int brandID, Domain domain, ref Device device, out bool bRemove)
        {
            var eRetVal = DomainResponseStatus.UnKnown;
            bRemove = false;
            int isDevActive = 0;
            int status = 0;
            long tempDeviceID = 0;
            long nDbDomainDeviceID = 0;

            //BEO-4478
            if (domain.m_DomainStatus == DomainStatus.DomainSuspended && !_rolesPermissionsManager.AllowActionInSuspendedDomain(nGroupID, domain.m_masterGUIDs[0], false))
            {
                if (domain.roleId == 0 || (domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0
                                                          && !_rolesPermissionsManager.IsPermittedPermissionItem(domain.m_nGroupID, domain.m_masterGUIDs[0].ToString(), PermissionItems.HOUSEHOLDDEVICE_ADD.ToString())))
                {
                    eRetVal = DomainResponseStatus.DomainSuspended;
                    return eRetVal;
                }
            }

            int domainID = DomainDal.Instance.GetDeviceDomainData(nGroupID, sUDID, ref tempDeviceID, ref isDevActive, ref status, ref nDbDomainDeviceID);

            var skipHHQuantityValidation = false;
            // If the device is already contained in any domain
            if (domainID != 0)
            {
                // If the device is already contained in ANOTHER domain
                if (domainID != nDomainID)
                {
                    if (ValidateDeviceMobilityPolicy(nGroupID))
                    {
                        DeviceContainer _container = domain.GetDeviceContainerByFamilyId(device.m_deviceFamilyID);
                        DomainResponseStatus _responseStatus = domain.ValidateQuantity(sUDID, brandID, ref _container, ref device, true);

                        if (_responseStatus == DomainResponseStatus.ExceededLimit)
                        {
                            log.Error($"Failed to reassign device: {sUDID} from hh: {domainID} to hh: {nDomainID}, group: {nGroupID}, new HH ExceededLimit");
                            return _responseStatus;
                        }
                        else
                        {
                            skipHHQuantityValidation = true;
                        }
            
                        var delete = Core.Domains.Module.RemoveDeviceFromDomain(nGroupID, domainID, sUDID, true);
                        if (delete.Status.Code == (int)eResponseStatus.OK)
                        {
                            //Delete hh device from old hh
                            log.Debug($"Device: {sUDID} was removed from hh: {domainID} and will be added to hh: {nDomainID}");
                        }
                        else
                        {
                            log.Error($"Failed to reassign device: {sUDID} from hh: {domainID} to hh: {nDomainID}, group: {nGroupID}");
                            eRetVal = delete?.DomainResponse != null ? delete.DomainResponse.m_oDomainResponseStatus : DomainResponseStatus.Error;
                            return eRetVal;
                        }
                    }
                    else
                    {
                        eRetVal = DomainResponseStatus.DeviceExistsInOtherDomains;
                        return eRetVal;
                    }
                }
                // If the device is already contained in THIS domain
                else
                {
                    // Pending master approval
                    if (status == 3 && isDevActive == 3)
                    {
                        var domainDevice = new Core.Users.DomainDevice()
                        {
                            Id = nDbDomainDeviceID,
                            ActivataionStatus = DeviceState.Activated,
                            DeviceBrandId = brandID,
                            DeviceId = tempDeviceID,
                            DomainId = nDomainID,
                            Name = deviceName,
                            Udid = sUDID,
                            ActivatedOn = DateTime.UtcNow,
                            GroupId = domain.m_nGroupID,
                            DeviceFamilyId = device.m_deviceFamilyID,
                            ExternalId = device.ExternalId,
                            MacAddress = device.MacAddress,
                            Model = device.Model,
                            Manufacturer = device.Manufacturer,
                            ManufacturerId = device.ManufacturerId,
                            DynamicData = device.DynamicData
                        };

                        bool updated = domainDevice.Update();
                        if (updated)
                        {
                            eRetVal = DomainResponseStatus.OK;
                            bRemove = true;
                            device.m_domainID = nDomainID;
                            device.m_state = DeviceState.Activated;
                            device.Save(1, 1, domainDevice);
                            domain.GetDeviceList();

                            return eRetVal;
                        }
                    }

                    eRetVal = DomainResponseStatus.DeviceAlreadyExists;
                    return eRetVal;
                }
            }

            if (!skipHHQuantityValidation)
            {
                DeviceContainer container = domain.GetDeviceContainerByFamilyId(device.m_deviceFamilyID);

                //Check if exceeded limit for the device type
                DomainResponseStatus responseStatus = domain.ValidateQuantity(sUDID, brandID, ref container, ref device);

                if (responseStatus == DomainResponseStatus.ExceededLimit || responseStatus == DomainResponseStatus.DeviceTypeNotAllowed || responseStatus == DomainResponseStatus.DeviceAlreadyExists)
                {
                    eRetVal = responseStatus;
                    return eRetVal;
                }
            }

            int isActive = 0;
            long nDeviceID = 0;
            // Get row id from domains_devices
            int nDomainsDevicesID = DomainDal.DoesDeviceExistInDomain(domain.m_nDomainID, domain.m_nGroupID, sUDID, ref isActive, ref nDeviceID);

            //New Device Domain Connection
            if (nDomainsDevicesID == 0)
            {
                // Get row id from devices table (not udid)
                device.m_domainID = nDomainID;
                var deviceID = device.Save(1, 1, null, device.MacAddress, device.ExternalId, device.Model, device.ManufacturerId, device.Manufacturer, device.DynamicData);
                Core.Users.DomainDevice domainDevice = new Core.Users.DomainDevice()
                {
                    Id = nDbDomainDeviceID,
                    ActivataionStatus = DeviceState.Activated,
                    DeviceId = deviceID,
                    DomainId = domain.m_nDomainID,
                    DeviceBrandId = brandID,
                    ActivatedOn = DateTime.UtcNow,
                    Udid = sUDID,
                    GroupId = domain.m_nGroupID,
                    Name = deviceName,
                    DeviceFamilyId = device.m_deviceFamilyID,
                    ExternalId = device.ExternalId,
                    MacAddress = device.MacAddress,
                    Model = device.Model,
                    Manufacturer = device.Manufacturer,
                    ManufacturerId = device.ManufacturerId,
                    DynamicData = device.DynamicData
                };

                bool domainDeviceInsertSuccess = domainDevice.Insert();

                if (domainDeviceInsertSuccess && domainDevice.Id > 0)
                {
                    device.m_state = DeviceState.Activated;
                    domain.DeviceFamiliesMapping[device.m_deviceFamilyID].AddDeviceInstance(device);
                    domain.m_totalNumOfDevices++;

                    bRemove = true;
                    eRetVal = DomainResponseStatus.OK;
                }
                else
                {
                    eRetVal = DomainResponseStatus.Error;
                }
            }
            else
            {
                //Update device status if exists
                if (isActive != 1) // should be status != 1 ?
                {
                    var domainDevice = new Core.Users.DomainDevice()
                    {
                        Id = nDomainsDevicesID,
                        ActivataionStatus = DeviceState.Activated,
                        DeviceId = nDeviceID,
                        DomainId = domain.m_nDomainID,
                        DeviceBrandId = brandID,
                        ActivatedOn = DateTime.UtcNow,
                        Udid = sUDID,
                        GroupId = domain.m_nGroupID,
                        DeviceFamilyId = device.m_deviceFamilyID,
                        MacAddress = device.MacAddress,
                        Model = device.Model,
                        Manufacturer = device.Manufacturer,
                        ManufacturerId = device.ManufacturerId,
                        DynamicData = device.DynamicData
                    };

                    bool updated = domainDevice.Update();

                    if (updated)
                    {
                        bRemove = true;
                        eRetVal = DomainResponseStatus.OK;
                        device.m_domainID = nDomainID;
                        device.Save(1, 1, domainDevice);

                        // change the device in the container                      
                        domain.DeviceFamiliesMapping[device.m_deviceFamilyID].ChangeDeviceInstanceState(device.m_deviceUDID, DeviceState.Activated);
                    }
                }
                else
                {
                    eRetVal = DomainResponseStatus.DeviceAlreadyExists;
                }
            }

            return eRetVal;
        }

        private bool ValidateDeviceMobilityPolicy(int groupId)
        {
            
            var generalPartnerConfig = GeneralPartnerConfigManager.Instance.GetGeneralPartnerConfig(groupId);
            if (generalPartnerConfig != null)
            {
                return generalPartnerConfig.AllowDeviceMobility.HasValue && generalPartnerConfig.AllowDeviceMobility.Value;
            }

            return false;
        }
    }
}

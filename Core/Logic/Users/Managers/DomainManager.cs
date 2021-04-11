using ApiLogic.Api.Managers;
using ApiObjects;
using DAL;
using KLogMonitor;
using System;
using System.Reflection;
using System.Threading;
using APILogic.Api.Managers;
using Core.Users;

namespace ApiLogic.Users.Managers
{
    public interface IDomainManager
    {
        DomainResponseStatus AddDeviceToDomain(int nGroupID, int nDomainID, string sUDID, string deviceName, int brandID, Domain domain, ref Device device, out bool bRemove);
    }

    public class DomainManager : IDomainManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<DomainManager> lazy = new Lazy<DomainManager>(() =>
            new DomainManager(DomainDal.Instance,
                       PartnerConfigurationManager.Instance,
                       RolesPermissionsManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IDomainDal _repository;
        private readonly IPartnerConfigurationManager _partnerConfigurationManager;
        private readonly IRolesPermissionsManager _rolesPermissionsManager;

        public static DomainManager Instance { get { return lazy.Value; } }

        public DomainManager(IDomainDal repository,
                      IPartnerConfigurationManager partnerConfigurationManager,
                      IRolesPermissionsManager rolesPermissionsManager)
        {
            _repository = repository;
            _partnerConfigurationManager = partnerConfigurationManager;
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
            if (domain.m_DomainStatus == DomainStatus.DomainSuspended && !_partnerConfigurationManager.AllowSuspendedAction(nGroupID))
            {
                if (domain.roleId == 0 || (domain.m_masterGUIDs != null && domain.m_masterGUIDs.Count > 0
                                                          && !_rolesPermissionsManager.IsPermittedPermissionItem(domain.m_nGroupID, domain.m_masterGUIDs[0].ToString(), PermissionItems.HOUSEHOLDDEVICE_ADD.ToString())))
                {
                    eRetVal = DomainResponseStatus.DomainSuspended;
                    return eRetVal;
                }
            }

            int domainID = DomainDal.Instance.GetDeviceDomainData(nGroupID, sUDID, ref tempDeviceID, ref isDevActive, ref status, ref nDbDomainDeviceID);

            // If the device is already contained in any domain
            if (domainID != 0)
            {
                // If the device is already contained in ANOTHER domain
                if (domainID != nDomainID)
                {
                    eRetVal = DomainResponseStatus.DeviceExistsInOtherDomains;
                    return eRetVal;
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

            DeviceContainer container = domain.GetDeviceContainerByFamilyId(device.m_deviceFamilyID);

            //Check if exceeded limit for the device type
            DomainResponseStatus responseStatus = domain.ValidateQuantity(sUDID, brandID, ref container, ref device);

            if (responseStatus == DomainResponseStatus.ExceededLimit || responseStatus == DomainResponseStatus.DeviceTypeNotAllowed || responseStatus == DomainResponseStatus.DeviceAlreadyExists)
            {
                eRetVal = responseStatus;
                return eRetVal;
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
    }
}

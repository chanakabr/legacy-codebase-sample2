using ApiObjects.Response;
using System;
using System.Collections.Generic;
using Core.Users.Cache;

namespace Core.Users
{
    class TvinciDevice : BaseDevice
    {
        protected TvinciDevice()
        {
        }

        public TvinciDevice(int groupID)
            : base(groupID)
        {
        }

        public override string GetPINForDevice(int nGroupID, string sDeviceUDID, int nBrandID)
        {
            Device device = new Device(sDeviceUDID, nBrandID, nGroupID);
            device.Initialize(sDeviceUDID);
            return device.GetPINForDevice();
        }

        [Obsolete]
        public override ApiObjects.Response.Status SetDeviceInfo(int nGroupID, string sDeviceUDID, string sDeviceName)
        {
            ApiObjects.Response.Status status = null;
            Device device = new Device(sDeviceUDID, 0, nGroupID, sDeviceName);
            device.Initialize(sDeviceUDID);
            bool isSetSucceeded = device.SetDeviceInfo(sDeviceName);

            // in case set device Succeeded
            // domain should be remove from the cache 
            if (isSetSucceeded)
            {
                status = new ApiObjects.Response.Status((int)eResponseStatus.OK, "OK");
                Core.Users.BaseDomain baseDomain = null;
                Utils.GetBaseImpl(ref baseDomain, nGroupID);

                if (baseDomain != null)
                {
                    List<Domain> domains = baseDomain.GetDeviceDomains(sDeviceUDID);
                    if (domains != null && domains.Count > 0)
                    {
                        DomainsCache oDomainCache = DomainsCache.Instance();
                        foreach (var domain in domains)
                        {
                            oDomainCache.RemoveDomain(domain.m_nDomainID);

                        }
                    }
                }
            }
            else
            {
                if (device != null)
                {
                    status = Utils.ConvertDeviceStateToResponseObject(device.m_state);
                }
                else
                {
                    status = new ApiObjects.Response.Status((int)eResponseStatus.Error, "Error");
                }
            }

            return status;
        }

        public override DeviceResponseObject SetDevice(int nGroupID, string sDeviceUDID, string sDeviceName)
        {
            DeviceResponseObject ret = new DeviceResponseObject();
            Device device = new Device(sDeviceUDID, 0, nGroupID, sDeviceName);
            device.Initialize(sDeviceUDID);
            bool isSetSucceeded = device.SetDeviceInfo(sDeviceName);

            // in case set device Succeeded
            // domain should be remove from the cache 
            if (isSetSucceeded)
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.OK;
                ret.m_oDevice = device;

                Core.Users.BaseDomain baseDomain = null;
                Utils.GetBaseImpl(ref baseDomain, nGroupID);

                if (baseDomain != null)
                {
                    List<Domain> domains = baseDomain.GetDeviceDomains(sDeviceUDID);
                    if (domains != null && domains.Count > 0)
                    {
                        DomainsCache oDomainCache = DomainsCache.Instance();
                        foreach (var domain in domains)
                        {
                            oDomainCache.RemoveDomain(domain.m_nDomainID);

                        }
                    }
                }
            }
            else
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.Error;
                if (device != null)
                {
                    ret.m_oDevice = device;
                    if (device.m_state == DeviceState.NotExists)
                    {
                        ret.m_oDeviceResponseStatus = DeviceResponseStatus.DeviceNotExists;
                    }
                }
            }

            return ret;
        }

        public override DeviceResponseObject GetDeviceInfo(int nGroupID, string sID, bool bIsUDID)
        {
            DeviceResponseObject ret = new DeviceResponseObject();
            bool result = false;
            Device device = new Device(nGroupID);
            if (bIsUDID)
            {
                result = device.Initialize(sID);
            }
            else
            {
                int nID = 0;
                bool parseResult = int.TryParse(sID, out nID);
                if (parseResult)
                {
                    result = device.Initialize(nID);
                }
            }
            ret.m_oDevice = device;
            if (!result)
            {
                ret.m_oDeviceResponseStatus = DeviceResponseStatus.Error;
            }
            else
            {
                if (device.m_state == DeviceState.Error || device.m_state == DeviceState.UnActivated)
                {
                    ret.m_oDeviceResponseStatus = DeviceResponseStatus.Error;
                }
                else if (device.m_state == DeviceState.NotExists)
                {
                    ret.m_oDeviceResponseStatus = DeviceResponseStatus.DeviceNotExists;
                }
                else
                {
                    ret.m_oDeviceResponseStatus = DeviceResponseStatus.OK;
                }
            }
            return ret;
        }
    }
}

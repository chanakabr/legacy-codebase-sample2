using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TVPPro.SiteManager.Services;
using TVPPro.SiteManager.TvinciPlatform.Domains;

namespace TVPPro.SiteManager.Helper
{
    public class DeviceDNAHelper
    {
        public enum DeviceStatus
        {
            RegistrationAllowed,
            exceeded_max_devices_amount,
            RegisteredToAnotherUser,
            Activated,
            ActivationAllowed,
            RegistrationNotAllowed,
            AnonymousMode
        };

        public static string GetDeviceDNA()
        {
            string deviceDNA = string.Empty;
            SessionHelper.GetValueFromSession<string>("DeviceDNA", out deviceDNA);
            return deviceDNA;
        }

        public static void SetDeviceDNA(string deviceDNA)
        {
            if (!string.IsNullOrEmpty(deviceDNA))
            {
                SessionHelper.DeviceDNA = deviceDNA;
            }
        }


        //check if deviceDNA exists in the user domain 
        public static DeviceStatus checkByDeviceDNA()
        {
            if (string.IsNullOrEmpty(SessionHelper.DeviceDNA))
            {
                return DeviceStatus.AnonymousMode;
            }

            DeviceStatus deviceStatus = DeviceStatus.RegistrationNotAllowed;
            string deviceDNA = SessionHelper.DeviceDNA;
            TVPPro.SiteManager.TvinciPlatform.Domains.Device device = null;
            Domain userDomain = DomainsService.Instance.GetDomainInfo();
            int devicesCount = 0;

            if (userDomain != null && userDomain.m_deviceFamilies != null)
            {
                for (int i = 0; i < userDomain.m_deviceFamilies.Count(); i++)
                {
                    var deviceFamily = userDomain.m_deviceFamilies[i];
                    var deviceInstances = deviceFamily.DeviceInstances;
                    devicesCount += deviceInstances.Where(d => d.m_state.ToString().ToLower() == DeviceStatus.Activated.ToString().ToLower()).Count();
                    for (int j = 0; j < deviceInstances.Count(); j++)
                    {
                        var deviceInstance = deviceInstances[j];
                        if (deviceInstance.m_deviceUDID == deviceDNA)
                        {
                            device = deviceInstance;
                            break;
                        }
                    }
                }
            }

            SessionHelper.ActionAllowed = false;

            //check if the device is already registered device
            if (device != null)
            {
                // if the device is activated
                if (device.m_state == TVPPro.SiteManager.TvinciPlatform.Domains.DeviceState.Activated)
                {
                    deviceStatus = DeviceStatus.Activated;
                    SessionHelper.ActionAllowed = true;
                }
                else
                {
                    // if it is not show the activation popup
                    if (devicesCount < userDomain.m_nLimit)
                    {
                        deviceStatus = DeviceStatus.ActivationAllowed;
                    }
                    else
                    {
                        deviceStatus = DeviceStatus.exceeded_max_devices_amount;
                    }

                    SessionHelper.ActionAllowed = false;
                }
            }
            else
            {
                // device is not in the domain
                // First, we check if the number of active devices is lower then then limit
                if (devicesCount < userDomain.m_nLimit)
                {
                    deviceStatus = DeviceStatus.RegistrationAllowed;
                }
                else
                {
                    // if the number of active devices equals to the limit, show the max devices popup
                    deviceStatus = DeviceStatus.exceeded_max_devices_amount;
                }
            }

            return deviceStatus;
        }
    }
}

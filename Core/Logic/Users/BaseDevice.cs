using System.Collections.Generic;

namespace Core.Users
{
    public abstract class BaseDevice
    {
        protected int m_nGroupID;

        protected BaseDevice() { }
        public BaseDevice(int nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public abstract string GetPINForDevice(int nGroupID, string sDeviceUDID, int nBrandID);
        public abstract ApiObjects.Response.Status SetDeviceInfo(int nGroupID, string sDeviceUDID, string sDeviceName);
        public virtual DeviceResponseObject SetDevice(int nGroupID, string sDeviceUDID, string sDeviceName, string externalId, bool allowNullExternalId)
        {
            return SetDevice(nGroupID, sDeviceUDID, sDeviceName, "", externalId, null, allowNullExternalId, false, false);
        }
        public abstract DeviceResponseObject SetDevice(
            int nGroupID,
            string sDeviceUDID,
            string sDeviceName,
            string macAddress,
            string externalId,
            Dictionary<string, string> dynamicData,
            bool allowNullExternalId,
            bool allowNullMacAddress = false,
            bool allowNullDynamicData = false);

        public abstract DeviceResponseObject GetDeviceInfo(int nGroupID, string sID, bool bIsUDID);
    }
}

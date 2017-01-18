using DAL;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;

namespace Core.Users
{
    [Serializable]
    [JsonObject(Id = "Device")]
    public class Device : IEquatable<Device>
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string m_id;

        public string m_deviceUDID;

        public string m_deviceBrand;

        public string m_deviceFamily;

        public int m_deviceFamilyID;

        public int m_domainID;

        public string m_deviceName;

        private int m_groupID;

        public int m_deviceBrandID;

        public string m_pin;

        public DateTime m_activationDate;

        public DeviceState m_state;

        public string m_sStreamType;

        public string m_sProfile;

        public Device(string sUDID, int nDeviceBrandID, int nGroupID, string deviceName, int domainID)
        {
            int nFamilyID = 0;
            m_id = string.Empty;
            m_deviceUDID = sUDID;

            if (!string.IsNullOrEmpty(sUDID))
            {
                if (nDeviceBrandID > 0)
                {
                    m_deviceBrandID = nDeviceBrandID;
                    m_deviceFamily = GetDeviceFamily(nDeviceBrandID, ref nFamilyID);
                    m_deviceFamilyID = nFamilyID;
                }
                else
                {
                    int nBrandID = 0;
                    m_deviceFamilyID = DeviceDal.GetDeviceFamilyID(nGroupID, sUDID, ref nBrandID);
                    m_deviceBrandID = nBrandID;
                }
            }

            m_domainID = domainID;
            m_groupID = nGroupID;
            m_deviceName = deviceName;
            m_pin = string.Empty;
            m_activationDate = DateTime.UtcNow;
            m_state = DeviceState.UnKnown;
        }

        public Device(string sUDID, int nDeviceBrandID, int nGroupID, string sDeviceName, int nDomainID, int nDeviceID, int nDeviceFamilyID,
            string sDeviceFamilyName, string sPin, DateTime dtActivationDate, DeviceState eState)
        {
            this.m_deviceUDID = sUDID;
            this.m_deviceBrandID = nDeviceBrandID;
            this.m_groupID = nGroupID;
            this.m_deviceName = sDeviceName;
            this.m_domainID = nDomainID;
            this.m_id = nDeviceID + "";
            this.m_deviceFamilyID = nDeviceFamilyID;
            this.m_deviceFamily = sDeviceFamilyName;
            this.m_pin = sPin;
            this.m_activationDate = dtActivationDate;
            this.m_state = eState;

            if (nDeviceBrandID > 0)
            {
                PopulateDeviceStreamTypeAndProfile();
            }

        }

        private void PopulateDeviceStreamTypeAndProfile()
        {
            /*
            * 4.12.14. Used only by Vodafone. It is used when VodafoneConditionalAccess calculates NPVR Licensed Link against ALU.
            * When NPVR is extended for Harmonic, and other provider, find a way to abstract those fields, and save it in DB, rather in config.
            * 
            * 
            */
            m_sStreamType = Utils.GetTcmConfigValue(GetStreamTypeConfigKey(m_groupID, m_deviceBrandID));
            m_sProfile = Utils.GetTcmConfigValue(GetProfileConfigKey(m_groupID, m_deviceBrandID));

        }

        private string GetStreamTypeConfigKey(int groupID, int deviceBrandID)
        {
            return String.Concat("DEVICE_STREAM_TYPE_", groupID, "_", deviceBrandID);
        }

        private string GetProfileConfigKey(int groupID, int deviceBrandID)
        {
            return String.Concat("DEVICE_PROFILE_", groupID, "_", deviceBrandID);
        }

        public Device(string sUDID, int nDeviceBrandID, int nGroupID, string sDeviceName)
            : this(sUDID, nDeviceBrandID, nGroupID, sDeviceName, 0)
        {

        }

        public Device(string sUDID, int nDeviceBrandID, int nGroupID)
            : this(sUDID, nDeviceBrandID, nGroupID, string.Empty, 0)
        {

        }

        public Device(int nGroupID)
            : this(string.Empty, 0, nGroupID)
        {

        }

        public Device()
            : this(string.Empty, 0, 0)
        {
        }

        public bool Initialize(string sDeviceUDID)
        {
            bool result = InitDeviceInfo(sDeviceUDID, true);
            return result;
        }

        public bool Initialize(int nDeviceID)
        {
            bool result = InitDeviceInfo(nDeviceID.ToString(), false);
            return result;
        }

        public bool Initialize(int nDeviceID, int nDomainID)
        {
            m_id = nDeviceID.ToString();

            string sDbState = string.Empty;

            bool res = DeviceDal.InitDeviceInDb(nDeviceID, nDomainID,
                                                ref m_groupID, ref m_deviceUDID, ref m_deviceBrandID, ref m_deviceName, ref m_deviceFamilyID, ref m_pin, ref m_activationDate, ref sDbState);


            var stateDict = new Dictionary<string, DeviceState> {
                { "UnKnown", DeviceState.UnKnown },
                { "Error", DeviceState.Error },
                { "NotExists", DeviceState.NotExists },
                { "Pending", DeviceState.Pending }, 
                { "Activated", DeviceState.Activated },
                { "UnActivated", DeviceState.UnActivated }
            };

            m_state = stateDict[sDbState];

            return res;

        }

        public bool Initialize(string sDeviceUDID, int nDomainID)
        {

            int nID = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select id from devices with (nolock) where status=1";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sDeviceUDID);
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_groupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        nID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                    }
                    else
                    {
                        m_state = DeviceState.NotExists;
                    }
                }
                else
                {
                    m_state = DeviceState.Error;
                }
            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }

            if (nID > 0)
            {
                return (Initialize(nID, nDomainID));
            }

            return false;
        }

        public bool Initialize(string sUDID, string sDeviceName)
        {
            if (Initialize(sUDID))
            {
                m_deviceName = sDeviceName.Trim();
                return true;
            }

            return false;
        }

        private string GetDeviceFamily(int deviceBrand, ref int familyID)
        {
            return DeviceDal.Get_DeviceFamilyIDAndName(deviceBrand, ref familyID);
        }

        public int Save(int nIsActive, int nStatus = 1, int? nDeviceID = null)
        {
            int retVal = 0;

            bool deviceFound = (nDeviceID.HasValue && nDeviceID.Value > 0);
            if (!deviceFound)
            {
                retVal = DAL.DeviceDal.GetDeviceID(m_deviceUDID, m_groupID, m_deviceBrandID, m_deviceFamilyID, nStatus);
                deviceFound = retVal > 0;
            }
            else
            {
                retVal = nDeviceID.Value;
            }

            if (!deviceFound) // New Device
            {
                retVal = DAL.DeviceDal.InsertNewDevice(m_deviceUDID, m_deviceBrandID, m_deviceFamilyID, m_deviceName, m_groupID, nIsActive, nStatus, m_pin);
            }
            else // Update Device
            {
                ODBCWrapper.UpdateQuery updateQuery = null;
                try
                {
                    updateQuery = new ODBCWrapper.UpdateQuery("devices");
                    updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", m_deviceName);

                    if (nIsActive != -1)
                    {
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
                    }
                    if (nStatus != -1)
                    {
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                    }

                    updateQuery += "where";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", m_deviceUDID);
                    updateQuery += "and ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", m_deviceBrandID);
                    updateQuery += "and ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", m_deviceFamilyID);
                    updateQuery += "and ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_groupID);
                    updateQuery += "and ";
                    updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", retVal);
                    bool bUpdateRetVal = updateQuery.Execute();

                    if (!bUpdateRetVal)
                    {
                        retVal = 0;
                    }
                }
                finally
                {
                    if (updateQuery != null)
                    {
                        updateQuery.Finish();
                    }
                }
            }

            m_id = retVal.ToString();
            return retVal;
        }

        /// <summary>
        /// Generate a PIN code (Unique per account)
        /// Insert a new device to devices table with the new PIN code
        /// Return the new PIN
        /// </summary>
        /// <returns>New PIN</returns>
        public string GetPINForDevice()
        {
            int nDeviceID = 0;

            // Device already exists
            if (m_pin != string.Empty)
            {
                return m_pin;
            }
            // New Device
            else
            {
                string sNewDevicePIN = GenerateNewPIN();
                m_pin = sNewDevicePIN;

                nDeviceID = Save(0, 1); // Returns device ID, 0 otherwise

                if (nDeviceID != 0)
                {
                    return sNewDevicePIN;
                }
                else
                {
                    return string.Empty;
                }
            }
        }


        private string GenerateNewPIN()
        {
            string sNewPIN = string.Empty;

            bool flag = true;

            while (flag)
            {
                // Create new PIN
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    sNewPIN = Guid.NewGuid().ToString().Substring(0, 5); ;

                    //Search for new PIN in devices table - if found, regenerate, else, return new PIN
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                    selectQuery += "select * from devices with (nolock) where status=1 and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", m_groupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PIN", "=", sNewPIN);

                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        if (nCount == 0)
                        {
                            flag = false; // Found unique PIN
                        }
                    }

                }
                finally
                {
                    if (selectQuery != null)
                    {
                        selectQuery.Finish();
                    }
                }
            }

            return sNewPIN;
        }

        public bool SetDeviceInfo(string sDeviceName)
        {
            bool res = false;
            m_deviceName = sDeviceName;

            if (m_state >= DeviceState.Pending)
            {
                int nDeviceID = Save(-1); // Returns device ID, 0 otherwise
                if (nDeviceID != 0)
                {
                    res = true;
                }
                else
                {
                    res = false;
                }
            }

            return res;
        }

        public static int GetDeviceIDByUDID(string sUDID, int nGroupID)
        {
            return (int)DeviceDal.Get_IDInDevicesByDeviceUDID(sUDID, nGroupID);
        }

        private bool InitDeviceInfo(string sID, bool isUDID)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery1 = null;
            try
            {
                if (string.IsNullOrEmpty(sID))
                {
                    return false;
                }

                DataTable dtDeviceInfo = DeviceDal.Get_DeviceInfo(sID, isUDID, m_groupID);

                if (dtDeviceInfo == null)
                {
                    m_state = DeviceState.Error;
                    return false;
                }

                int count = dtDeviceInfo.Rows.Count;

                if (count == 0) // Not found
                {
                    m_state = DeviceState.NotExists;
                    return false;
                }

                // Device found

                DataRow dr = dtDeviceInfo.Rows[0];

                int nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(dr["id"]);
                m_id = nDeviceID.ToString();
                m_deviceUDID = ODBCWrapper.Utils.GetSafeStr(dr["device_id"]);
                m_deviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(dr["device_brand_id"]);
                m_deviceName = ODBCWrapper.Utils.GetSafeStr(dr["Name"]);
                m_groupID = ODBCWrapper.Utils.GetIntSafeVal(dr["group_id"]);
                m_deviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(dr["device_family_id"]);
                m_pin = ODBCWrapper.Utils.GetSafeStr(dr["pin"]);

                PopulateDeviceStreamTypeAndProfile();

                int nDeviceActive = ODBCWrapper.Utils.GetIntSafeVal(dr["is_active"]);
                if (nDeviceActive == 0)
                {
                    m_state = DeviceState.Pending;
                    return true;
                }


                selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery1 += "select domain_id, last_activation_date, is_active from domains_devices with (nolock) where status=1 and";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
                if (selectQuery1.Execute("query", true) != null)
                {
                    count = selectQuery1.Table("query").DefaultView.Count;
                    if (count > 0) // Device found
                    {
                        m_domainID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "domain_id", 0);
                        m_activationDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "last_activation_date", 0);
                        int nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "is_active", 0);
                        int nStatus = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "status", 0);

                        m_state = (nActive == 1) ? DeviceState.Activated :
                                                                ((nStatus == 3) ? DeviceState.Pending : DeviceState.UnActivated);
                    }
                    else
                    {
                        m_state = DeviceState.UnActivated;
                    }
                }
                else
                {
                    m_state = DeviceState.Error;
                }


                return true;
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Exception at InitDeviceInfo. Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" ID: ", sID));
                sb.Append(String.Concat(" IsUDID: ", isUDID.ToString().ToLower()));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
            }
            finally
            {
                if (selectQuery1 != null)
                {
                    selectQuery1.Finish();
                }
            }

            return false;

        }

        public bool IsActivated()
        {
            return m_state == DeviceState.Activated;
        }


        public bool Equals(Device other)
        {
            return m_deviceFamilyID == other.m_deviceFamilyID && m_deviceUDID.Equals(other.m_deviceUDID);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(String.Concat(" Device ID: ", m_id));
            sb.Append(String.Concat(" Device UDID: ", m_deviceUDID));
            sb.Append(String.Concat(" Device Brand ID: ", m_deviceBrandID));
            sb.Append(String.Concat(" Device Family ID: ", m_deviceFamilyID));
            sb.Append(String.Concat(" Group ID: ", m_groupID));
            sb.Append(String.Concat(" Domain ID: ", m_domainID));
            sb.Append(String.Concat(" State: ", m_state.ToString()));

            return sb.ToString();
        }
    }
}

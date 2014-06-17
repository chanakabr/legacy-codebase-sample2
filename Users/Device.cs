using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Users
{
    public class Device
    {
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

        public Device(string sUDID, int nDeviceBrandID, int nGroupID, string deviceName, int domainID)
        {
            m_id = string.Empty;
            m_deviceUDID = sUDID;
            m_deviceBrandID = nDeviceBrandID;

            int nFamilyID = 0;
            m_deviceFamily = GetDeviceFamily(nDeviceBrandID, ref nFamilyID);
            m_deviceFamilyID = nFamilyID;

            m_domainID = domainID;
            m_groupID = nGroupID;
            m_deviceName = deviceName;
            m_pin = string.Empty;
            m_activationDate = DateTime.Now;
            m_state = DeviceState.UnKnown;
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

            bool res = DAL.DeviceDal.InitDeviceInDb(nDeviceID, nDomainID,
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

            #region Commented
            
            //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

            // Search for device in devices table
            //selectQuery += "select * from devices where status=1";
            //selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDeviceID);
            //selectQuery += "and";
            //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_groupID);
            //if (selectQuery.Execute("query", true) != null)
            //{
            //    int count = selectQuery.Table("query").DefaultView.Count;
            //    if (count > 0) // Device found
            //    {
            //        m_deviceUDID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "device_id", 0);
            //        m_deviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_brand_id", 0);
            //        m_deviceName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Name", 0);
            //        m_groupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", 0);
            //        m_deviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_family_id", 0);
            //        m_pin = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "pin", 0);

            //        int nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "is_active", 0); 
            //        if (nActive == 0)
            //        {
            //            m_state = DeviceState.Pending;
            //        }
            //        else if (nActive == 1)
            //        {
            //            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
            //            selectQuery1 += " select * from domains_devices where status=1 and";
            //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
            //            selectQuery1 += " and ";
            //            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
            //            if (selectQuery1.Execute("query", true) != null)
            //            {
            //                count = selectQuery1.Table("query").DefaultView.Count;
            //                if (count > 0)
            //                {
            //                    m_domainID = nDomainID;
            //                    m_activationDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "last_activation_date", 0);
            //
            //                    nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "is_active", 0);
            //                    if (nActive == 1)
            //                    {
            //                        m_state = DeviceState.Activated;
            //                    }
            //                    else
            //                    {
            //                        m_state = DeviceState.UnActivated;
            //                    }
            //                }
            //                else // Device is not registered
            //                {
            //                    m_state = DeviceState.UnKnown;
            //                }
            //            }
            //            else // Error
            //            {
            //                m_state = DeviceState.Error;
            //            }
            //            selectQuery1.Finish();
            //            selectQuery1 = null;
            //        }
            //        else
            //        {
            //            m_state = DeviceState.UnKnown;
            //        }
                    
            //    }
            //    else // Device not found
            //    {

            //        m_state = DeviceState.NotExists;
            //    }
            //}
            //else
            //{
            //    m_state = DeviceState.Error;
            //}
            
            //selectQuery.Finish();
            //selectQuery = null;

            #endregion
        }

        public bool Initialize(string sDeviceUDID, int nDomainID)
        {

            int nID = 0; 
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
            selectQuery.Finish();
            selectQuery = null;

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
            string retVal = string.Empty;
            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select lud.id, lud.name from lu_DeviceFamily lud WITH (nolock), lu_DeviceBrands lub WITH (nolock) where lub.device_family_id = lud.id and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("lub.id", "=", deviceBrand);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0)
                    {
                        retVal = ODBCWrapper.Utils.GetSafeStr(selectQuery.Table("query").DefaultView[0].Row["name"]);
                        familyID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery.Table("query").DefaultView[0].Row["id"]);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch
            {

            }
            return retVal;
        }

        public int Save(int nIsActive, int nStatus = 1, int? nDeviceID = null)
        {
            int retVal = 0;

            bool deviceFound = (nDeviceID.HasValue && nDeviceID.Value > 0);
            if (!deviceFound)
            {
                retVal = DAL.DeviceDal.GetDeviceID(m_deviceUDID, m_groupID, m_deviceBrandID, m_deviceFamilyID, nStatus);
                deviceFound = retVal > 0;

                //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                //selectQuery += "select id from devices with (nolock) where ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", m_deviceUDID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", m_deviceBrandID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", m_deviceFamilyID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_groupID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                //if (selectQuery.Execute("query", true) != null)
                //{
                //    int deviceCount = selectQuery.Table("query").DefaultView.Count;
                //    if (deviceCount > 0)
                //    {
                //        retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                //        deviceFound = true;
                //    }
                //}
                //selectQuery.Finish();
                //selectQuery = null;
            }
            else
            {
                retVal = nDeviceID.Value; 
            }

            if (!deviceFound) // New Device
            {
                retVal = DAL.DeviceDal.InsertNewDevice(m_deviceUDID, m_deviceBrandID, m_deviceFamilyID, m_deviceName, m_groupID, nIsActive, nStatus, m_pin);

                #region OLD
                //ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("devices");
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", m_deviceUDID);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", m_deviceBrandID);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", m_deviceFamilyID);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", m_deviceName);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_groupID);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                //insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIN", "=", m_pin);
                //insertQuery.Execute();
                //insertQuery.Finish();
                //insertQuery = null;

                //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                //selectQuery = new ODBCWrapper.DataSetSelectQuery();
                //selectQuery += "select id from devices with (nolock) where ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", m_deviceUDID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", m_deviceBrandID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", m_deviceFamilyID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("Name", "=", m_deviceName);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_groupID);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
                //selectQuery += "and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                //if (selectQuery.Execute("query", true) != null)
                //{
                //    int count = selectQuery.Table("query").DefaultView.Count;
                //    if (count > 0)
                //    {
                //        retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
                //    }
                //}
                //selectQuery.Finish();
                //selectQuery = null;
                #endregion
            }
            else // Update Device
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("devices");
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
                //updateQuery += "and ";
                //updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                updateQuery += "and ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", retVal);
                bool bUpdateRetVal = updateQuery.Execute();
                updateQuery.Finish();
                updateQuery = null;

                if (!bUpdateRetVal)
                {
                    retVal = 0;
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
                sNewPIN = Guid.NewGuid().ToString().Substring(0, 5); ;

                //Search for new PIN in devices table - if found, regenerate, else, return new PIN
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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

                selectQuery.Finish();
                selectQuery = null;
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
            int retVal = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            selectQuery += "select id from devices with (nolock) where status=1 and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sUDID);
            selectQuery += "and";
            selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            if (selectQuery.Execute("query", true) != null)
            {
                int count = selectQuery.Table("query").DefaultView.Count;
                if (count > 0)
                {
                    retVal = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "id", 0);
                }
            }
            selectQuery.Finish();
            selectQuery = null;
            return retVal;
        }

        private bool InitDeviceInfo(string sID , bool isUDID)
        {
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

                int nDeviceID       = ODBCWrapper.Utils.GetIntSafeVal(dr["id"]);
                m_id                = nDeviceID.ToString();
                m_deviceUDID        = ODBCWrapper.Utils.GetSafeStr(dr["device_id"]);
                m_deviceBrandID     = ODBCWrapper.Utils.GetIntSafeVal(dr["device_brand_id"]);
                m_deviceName        = ODBCWrapper.Utils.GetSafeStr(dr["Name"]);
                m_groupID           = ODBCWrapper.Utils.GetIntSafeVal(dr["group_id"]);
                m_deviceFamilyID    = ODBCWrapper.Utils.GetIntSafeVal(dr["device_family_id"]);
                m_pin               = ODBCWrapper.Utils.GetSafeStr(dr["pin"]);

                int nDeviceActive   = ODBCWrapper.Utils.GetIntSafeVal(dr["is_active"]);
                if (nDeviceActive == 0)
                {
                    m_state = DeviceState.Pending;
                    return true;
                }


                ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1 += "select domain_id, last_activation_date, is_active from domains_devices with (nolock) where status=1 and";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
                if (selectQuery1.Execute("query", true) != null)
                {
                    count = selectQuery1.Table("query").DefaultView.Count;
                    if (count > 0) // Device found
                    {
                        m_domainID          = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "domain_id", 0);
                        m_activationDate    = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "last_activation_date", 0);
                        int nActive         = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "is_active", 0);
                        int nStatus         = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "status", 0);

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

                selectQuery1.Finish();
                selectQuery1 = null;


                return true;
            }
            catch
            {
            }

            return false;

        }

    }
}

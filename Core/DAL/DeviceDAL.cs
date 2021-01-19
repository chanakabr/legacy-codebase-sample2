using System;
using System.Data;
using Tvinci.Core.DAL;
using ODBCWrapper;
using System.Collections.Generic;
using ApiObjects;
using Newtonsoft.Json;

namespace DAL
{
    public class DeviceDal : BaseDal
    {
        public static bool InitDeviceInDb(int nDeviceID, int nDomainID,
                                    int nGroupID, ref string sDbDeviceUDID, ref int nDbDeviceBrandID, ref string sDbDeviceName, ref int nDbDeviceFamilyID, ref string sDbPin, ref DateTime dtDbActivationDate, ref string sDbState)
        {
            bool res = false;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            ODBCWrapper.DataSetSelectQuery selectQuery1 = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                // Search for device in devices table
                selectQuery += "select * from devices WITH (nolock) where status<>2";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDeviceID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0) // Device found
                    {
                        sDbDeviceUDID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "device_id", 0);
                        nDbDeviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_brand_id", 0);
                        sDbDeviceName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "Name", 0);
                        //nGroupID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "group_id", 0);
                        nDbDeviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_family_id", 0);
                        sDbPin = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "pin", 0);

                        int nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "is_active", 0);
                        if (nActive == 0)
                        {
                            sDbState = "Pending";
                        }
                        else if (nActive == 1)
                        {
                            selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                            selectQuery1.SetConnectionKey("USERS_CONNECTION_STRING");
                            selectQuery1 += " select * from domains_devices WITH (nolock) where status=1 and";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
                            selectQuery1 += " and ";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
                            selectQuery1 += " and ";
                            selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                            if (selectQuery1.Execute("query", true) != null)
                            {
                                count = selectQuery1.Table("query").DefaultView.Count;
                                if (count > 0)
                                {
                                    //m_domainID = nDomainID;
                                    dtDbActivationDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "last_activation_date", 0);

                                    nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "is_active", 0);

                                    sDbState = (nActive == 1) ? "Activated" : "UnActivated";

                                }
                                else // Device is not registered
                                {
                                    sDbState = "UnKnown";
                                }
                            }
                            else // Error
                            {
                                sDbState = "Error";
                            }

                        }
                        else
                        {
                            sDbState = "UnKnown";
                        }

                        return true;
                    }
                    else // Device not found
                    {

                        sDbState = "NotExists";
                    }
                }
                else
                {
                    sDbState = "Error";
                }


            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
                if (selectQuery1 != null)
                {
                    selectQuery1.Finish();
                }
            }


            return res;
        }

        // not used for now
        public static bool InitDeviceInfo(string sID, bool isUDID, int m_groupID,
            ref string m_state,
            ref int m_id,
            ref string m_deviceUDID,
            ref int m_deviceBrandID,
            ref string m_deviceName,
            ref int m_deviceFamilyID,
            ref string m_pin,
            ref string externalId,
            ref string macAddress,
            ref string model,
            ref long? manufacturerId,
            ref Dictionary<string, string> dynamicData,
            ref int m_domainID,
            ref DateTime m_activationDate)
        {
            ODBCWrapper.DataSetSelectQuery selectQuery1 = null;
            try
            {
                if (string.IsNullOrEmpty(sID))
                {
                    return false;
                }

                DataTable dtDeviceInfo = Get_DeviceInfo(sID, isUDID, m_groupID);

                if (dtDeviceInfo == null)
                {
                    m_state = "Error";
                    return false;
                }

                int count = dtDeviceInfo.Rows.Count;

                if (count == 0) // Not found
                {
                    m_state = "NotExists";
                    return false;
                }

                // Device found

                DataRow dr = dtDeviceInfo.Rows[0];
                m_id = ODBCWrapper.Utils.GetIntSafeVal(dr["id"]);
                m_deviceUDID = ODBCWrapper.Utils.GetSafeStr(dr["device_id"]);
                m_deviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(dr["device_brand_id"]);
                m_deviceName = ODBCWrapper.Utils.GetSafeStr(dr["Name"]);
                m_groupID = ODBCWrapper.Utils.GetIntSafeVal(dr["group_id"]);
                m_deviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(dr["device_family_id"]);
                m_pin = ODBCWrapper.Utils.GetSafeStr(dr["pin"]);
                externalId = ODBCWrapper.Utils.GetSafeStr(dr["external_id"]);
                macAddress = ODBCWrapper.Utils.GetSafeStr(dr["mac_address"]);
                model = ODBCWrapper.Utils.GetSafeStr(dr["model"]);
                manufacturerId = ODBCWrapper.Utils.GetNullableLong(dr, "manufacturer_id");
                dynamicData = DeserializeDynamicData(ODBCWrapper.Utils.GetSafeStr(dr["dynamic_data"]));

                //PopulateDeviceStreamTypeAndProfile();

                int nDeviceActive = ODBCWrapper.Utils.GetIntSafeVal(dr["is_active"]);
                if (nDeviceActive == 0)
                {
                    m_state = "Pending";
                    return true;
                }


                selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
                selectQuery1.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery1 += "select domain_id, last_activation_date, is_active,status from domains_devices with (nolock) where status=1 and";
                selectQuery1 += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", m_id);
                if (selectQuery1.Execute("query", true) != null)
                {
                    count = selectQuery1.Table("query").DefaultView.Count;
                    if (count > 0) // Device found
                    {
                        m_domainID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "domain_id", 0);
                        m_activationDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery1, "last_activation_date", 0);
                        int nActive = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "is_active", 0);
                        int nStatus = ODBCWrapper.Utils.GetIntSafeVal(selectQuery1, "status", 0);

                        m_state = (nActive == 1) ? "Activated" :
                                                                ((nStatus == 3) ? "Pending" : "UnActivated");
                    }
                    else
                    {
                        m_state = "UnActivated";
                    }
                }
                else
                {
                    m_state = "Error";
                }

                return true;
            }
            finally
            {
                if (selectQuery1 != null)
                {
                    selectQuery1.Finish();
                }
            }

        }        

        public static DataTable Get_DeviceInfo(string sID, bool isUDID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DeviceInfo");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@ID", sID);
            sp.AddParameter("@IsUDID", isUDID);
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables.Count > 0)
                return ds.Tables[0];
            return null;
        }

        public static int GetDeviceFamilyID(int nGroupID, string sUDID, ref int nDeviceBrandID)
        {
            int res = 0;
            StoredProcedure sp = new StoredProcedure("Get_DeviceFamilyID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DeviceUDID", sUDID);
            sp.AddParameter("@GroupID", nGroupID);

            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    res = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["device_family_id"]);
                    nDeviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["device_brand_id"]);
                }
            }
            return res;
        }

        public static bool UpdateDomainsDevicesIsActive(int nDomainDeviceID, int enableInt, bool bIsEnable)
        {
            bool res = false;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("domains_devices");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", enableInt);
                if (bIsEnable)
                {
                    updateQuery += ", last_activation_date = getdate()";
                }
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDomainDeviceID);

                res = updateQuery.Execute();
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }

            return res;
        }

        public static int GetDeviceIdByUDID(string sDeviceUDID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IDInDevicesByDeviceUDID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DeviceUDID", sDeviceUDID);
            sp.AddParameter("@GroupID", nGroupID);
            return sp.ExecuteReturnValue<int>();
        }
        
        public static string GetDeviceIdByExternalId(int nGroupID, string externalId)
        {
            if (string.IsNullOrEmpty(externalId))
                return string.Empty;

            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DeviceInfoByExternalId");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@ExternalID", externalId);

            DataSet ds = sp.ExecuteDataSet();
            var dtDeviceInfo = ds != null && ds.Tables.Count > 0 ? ds.Tables[0] : null;
            var exists = dtDeviceInfo?.Rows.Count > 0;
            if (exists)
            {
                DataRow dr = dtDeviceInfo.Rows[0];
                return ODBCWrapper.Utils.ExtractValue<string>(dr, "id");
            }

            return string.Empty;            
        }

        public static int GetDeviceID(string sDeviceUDID, int nGroupID, int? nDeviceBrandID = null, int? nDeviceFamilyID = null, int? nStatus = null)
        {
            int retVal = 0;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                selectQuery += "select id from devices with (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sDeviceUDID);

                if (nDeviceBrandID.HasValue)
                {
                    selectQuery += "and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", nDeviceBrandID);
                }
                if (nDeviceFamilyID.HasValue)
                {
                    selectQuery += "and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", nDeviceFamilyID);
                }
                //if (nGroupID.HasValue)
                //{
                selectQuery += "and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                //}
                if (nStatus.HasValue)
                {
                    selectQuery += "and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
                }

                if (selectQuery.Execute("query", true) != null)
                {
                    int deviceCount = selectQuery.Table("query").DefaultView.Count;
                    if (deviceCount > 0)
                    {
                        retVal = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
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

            return retVal;
        }

        public static int InsertNewDevice(
            string sDeviceUDID,
            int nDeviceBrandID,
            int nDeviceFamilyID,
            string sDeviceName,
            int nGroupID,
            int nIsActive,
            int nStatus,
            string sPin,
            string externalId,
            string macAddress = "",
            string model = "",
            long? manufacturerId = null,
            Dictionary<string, string> dynamicData = null)
        {
            StoredProcedure sp = new StoredProcedure("Insert_NewDevice");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DeviceUDID", sDeviceUDID);
            sp.AddParameter("@DeviceBrandID", nDeviceBrandID);
            sp.AddParameter("@DeviceFamilyID", nDeviceFamilyID);
            sp.AddParameter("@DeviceName", sDeviceName);
            sp.AddParameter("@GroupID", nGroupID);
            sp.AddParameter("@IsActive", nIsActive);
            sp.AddParameter("@Status", nStatus);
            sp.AddParameter("@Pin", sPin);
            sp.AddParameter("@CreateDate", DateTime.UtcNow);
            sp.AddParameter("@ExternalID", externalId);
            if (!string.IsNullOrEmpty(macAddress)) sp.AddParameter("@MacAddress", macAddress);
            if (!string.IsNullOrEmpty(model)) sp.AddParameter("@Model", model);
            if (manufacturerId.HasValue) sp.AddParameter("@Manufacturer_Id", manufacturerId);
            if (dynamicData != null) sp.AddParameter("@DynamicData", SerializeDynamicData(dynamicData));

            return sp.ExecuteReturnValue<int>();
        }

        public static bool UpdateDevice(
            int deviceId,
            string deviceUDID,
            int deviceBrandID,
            int deviceFamilyID,
            int groupID,
            string deviceName,
            int isActive,
            int status,
            string externalId,
            string macAddress,
            string model,
            long? manufacturerId,
            Dictionary<string, string> dynamicData,
            bool allowNullExternalId,
            bool allowNullMacAddress,
            bool allowNullDynamicData)
        {
            UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new UpdateQuery("devices");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += Parameter.NEW_PARAM("Name", "=", deviceName);

                if (isActive != -1)
                {
                    updateQuery += Parameter.NEW_PARAM("is_active", "=", isActive);
                }
                if (status != -1)
                {
                    updateQuery += Parameter.NEW_PARAM("status", "=", status);
                }

                if (!string.IsNullOrEmpty(externalId) || allowNullExternalId)
                {
                    updateQuery += Parameter.NEW_PARAM("external_Id", "=", externalId);
                }

                if (!string.IsNullOrEmpty(macAddress) || allowNullMacAddress)
                {
                    updateQuery += Parameter.NEW_PARAM("mac_address", "=", macAddress);
                }

                if (!string.IsNullOrEmpty(model)) updateQuery += Parameter.NEW_PARAM("model", "=", model);
                if (manufacturerId.HasValue) updateQuery += Parameter.NEW_PARAM("manufacturer_Id", "=", manufacturerId.Value);

                if (dynamicData != null || allowNullDynamicData)
                {
                    updateQuery += Parameter.NEW_PARAM("dynamic_data", "=", SerializeDynamicData(dynamicData));
                }

                updateQuery += "where";
                updateQuery += Parameter.NEW_PARAM("device_id", "=", deviceUDID);
                updateQuery += "and ";
                updateQuery += Parameter.NEW_PARAM("device_brand_id", "=", deviceBrandID);
                updateQuery += "and ";
                updateQuery += Parameter.NEW_PARAM("device_family_id", "=", deviceFamilyID);
                updateQuery += "and ";
                updateQuery += Parameter.NEW_PARAM("group_id", "=", groupID);
                updateQuery += "and ";
                updateQuery += Parameter.NEW_PARAM("id", "=", deviceId);
                bool updated = updateQuery.Execute();

                return updated;
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }
        }

        public static string Get_DeviceFamilyIDAndName(int nDeviceBrandID, ref int nDeviceFamilyID)
        {
            string res = string.Empty;
            StoredProcedure sp = new StoredProcedure("Get_DeviceFamilyIDAndName");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@DeviceBrandID", nDeviceBrandID);
            DataSet ds = sp.ExecuteDataSet();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    nDeviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["id"]);
                    res = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0]["name"]);
                }
            }

            return res;
        }

        public static string GenerateNewPIN(int groupId)
        {
            string sNewPIN = string.Empty;

            bool flag = true;

            while (flag)
            {
                // Create new PIN
                ODBCWrapper.DataSetSelectQuery selectQuery = null;
                try
                {
                    sNewPIN = Guid.NewGuid().ToString().Substring(0, 5);

                    //Search for new PIN in devices table - if found, regenerate, else, return new PIN
                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                    selectQuery += "select id from devices with (nolock) where status=1 and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", groupId);
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

        public static Dictionary<string, string> DeserializeDynamicData(string dynamicDataString)
        {
            return dynamicDataString == null
                    ? null
                    : JsonConvert.DeserializeObject<Dictionary<string, string>>(dynamicDataString);
        }

        private static string SerializeDynamicData(Dictionary<string, string> dynamicData)
        {
            return dynamicData == null
                ? null
                : JsonConvert.SerializeObject(dynamicData);
        }
    }
}

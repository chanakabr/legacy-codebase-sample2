using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;
using ODBCWrapper;


namespace DAL
{
    public class DeviceDal : BaseDal
    {
        private static void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }


        public static bool InitDeviceInDb(int nDeviceID, int nDomainID,
                                    ref int nGroupID, ref string sDbDeviceUDID, ref int nDbDeviceBrandID, ref string sDbDeviceName, ref int nDbDeviceFamilyID, ref string sDbPin, ref DateTime dtDbActivationDate, ref string sDbState)
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

        public static long Get_IDInDevicesByDeviceUDID(string sDeviceUDID, int nGroupID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_IDInDevicesByDeviceUDID");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@DeviceUDID", sDeviceUDID);
            sp.AddParameter("@GroupID", nGroupID);
            return sp.ExecuteReturnValue<long>();
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

        public static int InsertNewDevice(string sDeviceUDID, int nDeviceBrandID, int nDeviceFamilyID, string sDeviceName, int nGroupID, int nIsActive, int nStatus, string sPin)
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

            return sp.ExecuteReturnValue<int>();
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

    }
}

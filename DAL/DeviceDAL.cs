using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;


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

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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
                            ODBCWrapper.DataSetSelectQuery selectQuery1 = new ODBCWrapper.DataSetSelectQuery();
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

                            selectQuery1.Finish();
                            selectQuery1 = null;
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

                selectQuery.Finish();
                selectQuery = null;

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return res;
        }

        public static int GetDeviceFamilyID(int nGroupID, string sUDID, ref int nDeviceBrandID)
        {
            int nDbDeviceFamilyID = 0;
            nDeviceBrandID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("USERS_CONNECTION_STRING");

                // Search for device in devices table
                selectQuery += "select device_brand_id, device_family_id from devices WITH (nolock) where status=1";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sUDID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int count = selectQuery.Table("query").DefaultView.Count;
                    if (count > 0) // Device found
                    {
                        nDbDeviceFamilyID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_family_id", 0);
                        nDeviceBrandID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_brand_id", 0);
                    }

                    selectQuery.Finish();
                    selectQuery = null;
                }
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nDbDeviceFamilyID;
        }



        #region Commented

        //public static bool GetDeviceIdAndBrandByPin(string sPIN, int nGroupID, ref string sUDID, ref int nBrandID)
        //{
        //    bool res = false;

        //    try
        //    {
        //        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //        selectQuery += "select device_id, device_brand_id from devices where status=1 and";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PIN", "=", sPIN);
        //        selectQuery += "and";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        //        if (selectQuery.Execute("query", true) != null)
        //        {
        //            int nCount = selectQuery.Table("query").DefaultView.Count;
        //            if (nCount > 0)
        //            {
        //                sUDID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "device_id", 0); // selectQuery.Table("query").DefaultView[0].Row["device_id"].ToString();
        //                nBrandID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_brand_id", 0); // int.Parse(selectQuery.Table("query").DefaultView[0].Row["device_brand_id"].ToString());
        //            }
        //        }

        //        selectQuery.Finish();
        //        selectQuery = null;

        //        res = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleException(ex);
        //    }

        //    return res;
        //}

        //public static bool InsertToDomainsDevices(int nDeviceID, int nDomainID, int nGroupID, int nIsActive, int nStatus)
        //{
        //    bool res = false;

        //    try
        //    {
        //        ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("domains_devices");
        //        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", nDeviceID);
        //        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("domain_id", "=", nDomainID);
        //        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
        //        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
        //        insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);

        //        res = insertQuery.Execute();

        //        insertQuery.Finish();
        //        insertQuery = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleException(ex);
        //    }

        //    return res;

        //}

        //public static bool UpdateDomainsDevicesStatus(int nDomainsDevicesID, int nIsActive, int nStatus)
        //{
        //    bool res = false;

        //    try
        //    {
        //        ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("domains_devices");
        //        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
        //        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
        //        updateQuery += " where ";
        //        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDomainsDevicesID);
        //        res = updateQuery.Execute();

        //        updateQuery.Finish();
        //        updateQuery = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleException(ex);
        //    }

        //    return res;
        //}

        //public static int DoesDeviceExistInDomain(int nDomainID, int nGroupID, string deviceUdid, ref int isActive, ref int nDeviceID)
        //{
        //    int nDeviceDomainID = 0;

        //    try
        //    {
        //        ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //        selectQuery += "select dd.id, dd.is_active, dd.device_id from domains_devices dd, devices d where ";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.DOMAIN_ID", "=", nDomainID);
        //        selectQuery += " and ";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.GROUP_ID", "=", nGroupID);
        //        selectQuery += "and";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.device_id", "=", deviceUdid);
        //        selectQuery += "and";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.status", "=", 1);
        //        selectQuery += " and ";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.group_id", "=", nGroupID);
        //        selectQuery += " and ";
        //        selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.STATUS", "=", 1);
        //        selectQuery += "and";
        //        selectQuery += "dd.DEVICE_ID=d.id";
        //        if (selectQuery.Execute("query", true) != null)
        //        {
        //            int nCount = selectQuery.Table("query").DefaultView.Count;
        //            if (nCount > 0)
        //            {
        //                nDeviceDomainID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["id"].ToString());
        //                isActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
        //                nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_id", 0);
        //            }
        //        }

        //        selectQuery.Finish();
        //        selectQuery = null;
        //    }
        //    catch (Exception ex)
        //    {
        //        HandleException(ex);
        //    }

        //    return nDeviceDomainID;
        //}

        //public static int GetDomainOfDevice(string deviceUdid, int nGroupID, ref int isActive, ref int nDeviceID)
        //{
        //    int nDeviceDomainID = 0;

        //    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
        //    selectQuery += "select dd.id, dd.is_active, dd.device_id, dd.domain_id from domains_devices dd, devices d where ";

        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.GROUP_ID", "=", nGroupID);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.device_id", "=", deviceUdid);
        //    selectQuery += "and";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.status", "=", 1);
        //    selectQuery += " and ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("d.group_id", "=", nGroupID);
        //    selectQuery += " and ";
        //    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("dd.STATUS", "=", 1);
        //    selectQuery += "and";
        //    selectQuery += "dd.DEVICE_ID=d.id";
        //    if (selectQuery.Execute("query", true) != null)
        //    {
        //        int nCount = selectQuery.Table("query").DefaultView.Count;
        //        if (nCount > 0)
        //        {
        //            nDeviceDomainID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["domain_id"].ToString());

        //            isActive = int.Parse(selectQuery.Table("query").DefaultView[0].Row["is_active"].ToString());
        //            nDeviceID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_id", 0);
        //        }
        //    }

        //    selectQuery.Finish();
        //    selectQuery = null;

        //    return nDeviceDomainID;
        //}

        #endregion


        public static bool UpdateDomainsDevicesIsActive(int nDomainDeviceID, int enableInt, bool bIsEnable)
        {
            bool res = false;

            try
            {
                ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("domains_devices");
                updateQuery.SetConnectionKey("USERS_CONNECTION_STRING");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", enableInt);
                if (bIsEnable)
                {
                    updateQuery += ", last_activation_date = getdate()";
                }
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", nDomainDeviceID);

                res = updateQuery.Execute();

                updateQuery.Finish();
                updateQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
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

            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
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

            selectQuery.Finish();
            selectQuery = null;

            return retVal;
        }

        public static int InsertNewDevice(string sDeviceUDID, int nDeviceBrandID, int nDeviceFamilyID, string sDeviceName, int nGroupID, int nIsActive, int nStatus, string sPin)
        {
            int retVal = 0;

            ODBCWrapper.InsertQuery insertQuery = new ODBCWrapper.InsertQuery("devices");
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_id", "=", sDeviceUDID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_brand_id", "=", nDeviceBrandID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("device_family_id", "=", nDeviceFamilyID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("name", "=", sDeviceName);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("is_active", "=", nIsActive);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("status", "=", nStatus);
            insertQuery += ODBCWrapper.Parameter.NEW_PARAM("PIN", "=", sPin);
            insertQuery.Execute();
            insertQuery.Finish();
            insertQuery = null;

            retVal = GetDeviceID(sDeviceUDID, nGroupID, nDeviceBrandID, nDeviceFamilyID, nStatus);

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

            return retVal;
        }
    }
}

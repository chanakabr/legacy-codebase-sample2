using System;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Collections.Generic;
using TVPApi.External;
using System.Configuration;
using TVPApiModule.Services;
using TVPApi;
using TVPPro.SiteManager.Helper;
using KLogMonitor;
using System.Reflection;
using Core.Users;

namespace TVPApiServices
{
    [WebService(Namespace = "http://tvpapi.tvinci.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    [System.Web.Script.Services.ScriptService]
    public class ExtService : System.Web.Services.WebService
    {
        private static readonly KLogger logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public List<DeviceInfo> GetAccountDevices(string sUsername, string sPassword, int sAccountID)
        {
            List<DeviceInfo> retDevices = new List<DeviceInfo>();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAccountDevices", sUsername, sPassword, SiteHelper.GetClientIP());
            if (groupId != 0)
            {
                Domain domain = null;
                try
                {
                    //TODO: Change db query to domains method
                    string sAccUuid = string.Empty;
                    TVPApi.ODBCWrapper.DataSetSelectQuery selectQuery = new TVPApi.ODBCWrapper.DataSetSelectQuery(ConnectionHelper.GetTvinciConnectionString());

                    selectQuery += "Select [Data_Value] from [Users].[dbo].[users_dynamic_data] where USER_ID in ";
                    selectQuery += "(SELECT top 1 [USER_ID]";
                    selectQuery += "FROM [Users].[dbo].[users_dynamic_data] where ";
                    selectQuery += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("[DATA_VALUE]", "=", sAccountID.ToString());
                    selectQuery += ") and [DATA_TYPE] = 'AccountUuid'";

                    if (selectQuery.Execute("query", true) != null)
                    {
                        Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                        logger.DebugFormat(selectQuery.Table("query").DefaultView.Table.Columns[0].ColumnName);

                        if (nCount > 0)
                        {
                            sAccUuid = selectQuery.Table("query").DefaultView[0].Row["Data_Value"].ToString();
                            logger.DebugFormat(sAccUuid);
                        }
                    }
                    selectQuery.Finish();
                    selectQuery = null;

                    int iDomainID = new ApiDomainsService(groupId, PlatformType.iPad).GetDomainIDByCoGuid(sAccUuid);
                    domain = new ApiDomainsService(groupId, PlatformType.iPad).GetDomainInfo(iDomainID);
                }
                catch (Exception ex) { logger.Error("", ex); }
                if (domain != null && domain.m_deviceFamilies.Count > 0)
                {
                    foreach (DeviceContainer dc in domain.m_deviceFamilies)
                    {
                        foreach (Device device in dc.DeviceInstances)
                        {
                            DeviceInfo deviceInfo = new DeviceInfo();
                            deviceInfo.Name = device.m_deviceName;
                            deviceInfo.Type = dc.m_deviceFamilyName;
                            deviceInfo.UDID = device.m_deviceUDID;
                            deviceInfo.Active = !(device.m_state != DeviceState.Activated && device.m_state == DeviceState.UnActivated);
                            retDevices.Add(deviceInfo);
                        }
                    }
                }
            }
            return retDevices;
        }

        public ResponseStatus ChangeDeviceInfo(string sUsername, string sPassword, int sAccountID, string sUDID, string sDeviceName, bool bIsActive)
        {
            ResponseStatus retStat = new ResponseStatus();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "GetAccountDevices", sUsername, sPassword, SiteHelper.GetClientIP());
            if (groupId != 0)
            {
                bool bDeviceName = false;
                if (!string.IsNullOrEmpty(sDeviceName))
                    bDeviceName = new ApiDomainsService(groupId, PlatformType.iPad).SetDeviceInfo(sUDID, sDeviceName);

                Domain[] domain = new ApiDomainsService(groupId, PlatformType.iPad).GetDeviceDomains(sUDID);

                DomainResponseObject res = new DomainResponseObject();
                if (domain != null && domain.Length > 0)
                {
                    res = new ApiDomainsService(groupId, PlatformType.iPad).ChangeDeviceDomainStatus(domain[0].m_nDomainID, sUDID, bIsActive);

                    retStat.Code = res.m_oDomainResponseStatus.ToString();
                    retStat.Description = res.m_oDomainResponseStatus.ToString();
                }
                else
                {
                    retStat.Code = "700";
                    retStat.Description = "Device DNA or AccountID does not exsist";
                }
            }
            else
            {
                retStat.Code = "401";
                retStat.Description = "No permission";
            }

            return retStat;
        }

        public ResponseStatus RemoveDeviceFromAccount(string sUsername, string sPassword, string sUDID)
        {
            ResponseStatus retStat = new ResponseStatus();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "RemoveDeviceFromAccount", sUsername, sPassword, SiteHelper.GetClientIP());
            if (groupId != 0)
            {
                Domain[] domain = new ApiDomainsService(groupId, PlatformType.iPad).GetDeviceDomains(sUDID);

                DomainResponseObject res = new DomainResponseObject();
                if (domain != null && domain.Length > 0)
                {
                    res = new ApiDomainsService(groupId, PlatformType.iPad).RemoveDeviceToDomain(domain[0].m_nDomainID, sUDID);
                    retStat.Code = (res.m_oDomainResponseStatus.ToString().ToLower().Equals("ok") ? "0" : res.m_oDomainResponseStatus.ToString());
                    retStat.Description = res.m_oDomainResponseStatus.ToString();
                }
                else
                {
                    retStat.Code = "700";
                    retStat.Description = "Device DNA does not exsist";
                }
            }
            else
            {
                retStat.Code = "401";
                retStat.Description = "No permission";
            }

            return retStat;
        }

        public ResponseStatus RemoveAccount(string sUsername, string sPassword, int sAccountID)
        {
            ResponseStatus retStat = new ResponseStatus();

            int groupId = ConnectionHelper.GetGroupID("tvpapi", "RemoveAccount", sUsername, sPassword, SiteHelper.GetClientIP());
            if (groupId != 0)
            {
                //TODO: Change db query to domains method
                string sAccUuid = string.Empty;
                TVPApi.ODBCWrapper.DataSetSelectQuery selectQuery = new TVPApi.ODBCWrapper.DataSetSelectQuery(ConnectionHelper.GetTvinciConnectionString());

                selectQuery += "Select [Data_Value] from [Users].[dbo].[users_dynamic_data] where USER_ID in ";
                selectQuery += "(SELECT top 1 [USER_ID]";
                selectQuery += "FROM [Users].[dbo].[users_dynamic_data] where ";
                selectQuery += TVPApi.ODBCWrapper.Parameter.NEW_PARAM("[DATA_VALUE]", "=", sAccountID.ToString());
                selectQuery += ") and [DATA_TYPE] = 'AccountUuid'";

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    logger.DebugFormat(selectQuery.Table("query").DefaultView.Table.Columns[0].ColumnName);

                    if (nCount > 0)
                    {
                        sAccUuid = selectQuery.Table("query").DefaultView[0].Row["Data_Value"].ToString();
                        logger.DebugFormat(sAccUuid);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;

                int iDomainID = new ApiDomainsService(groupId, PlatformType.iPad).GetDomainIDByCoGuid(sAccUuid);
                if (iDomainID == 0)
                {
                    retStat.Code = "700";
                    retStat.Description = "Account does not exist";
                }
                else
                {

                    DomainResponseStatus ret = new ApiDomainsService(groupId, PlatformType.iPad).RemoveDomain(iDomainID);
                    if (ret != DomainResponseStatus.OK)
                    {
                        retStat.Code = "750";
                        retStat.Description = ret.ToString();
                    }
                    else
                    {
                        retStat.Code = "0";
                        retStat.Description = ret.ToString();
                    }
                }
            }
            else
            {
                retStat.Code = "401";
                retStat.Description = "No permission";
            }

            return retStat;
        }

    }
}


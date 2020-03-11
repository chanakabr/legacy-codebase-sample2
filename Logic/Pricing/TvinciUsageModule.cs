using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;

namespace Core.Pricing
{
    public class TvinciUsageModule : BaseUsageModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public TvinciUsageModule(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public override UsageModule[] GetUsageModuleList()
        {
            UsageModule[] ret = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select id from usage_modules with (nolock) where is_active=1 and status=1 and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        ret = new UsageModule[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        ret[i] = GetUsageModuleData(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUsageModuleList. ");
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }

            return ret;
        }

        public override UsageModule GetUsageModuleData(string sUsageModuleCode)
        {
            Int32 nUsageModuleID = 0;

            if (!Int32.TryParse(sUsageModuleCode, out nUsageModuleID) || nUsageModuleID < 1)
            {
                return null;
            }

            UsageModule tmp = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");
                selectQuery += "select * from usage_modules with (nolock) where is_active=1 and status=1 and ";
                selectQuery += " group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ID", "=", nUsageModuleID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        DataRow drSource = selectQuery.Table("query").Rows[0];

                        tmp = new UsageModule();
                        Int32 nViewLifeCycleInMin = int.Parse(drSource["VIEW_LIFE_CYCLE_MIN"].ToString());
                        Int32 nFullLifeCycleInMin = int.Parse(drSource["FULL_LIFE_CYCLE_MIN"].ToString());
                        Int32 nMaxViewsNumber = int.Parse(drSource["MAX_VIEWS_NUMBER"].ToString());
                        Int32 nID = int.Parse(drSource["ID"].ToString());
                        string sVirtualName = drSource["NAME"].ToString();

                        //new paramter supported multi usage module
                        Int32 m_ext_discount_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ext_discount_id", 0);
                        Int32 m_internal_discount_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "internal_discount_id", 0);
                        Int32 m_pricing_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "pricing_id", 0);
                        Int32 m_coupon_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "coupon_id", 0);
                        Int32 m_type = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "type", 0);
                        Int32 m_subscription_only = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "subscription_only", 0);
                        Int32 m_is_renew = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "is_renew", 0);
                        Int32 m_num_of_rec_periods = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "num_of_rec_periods", 0);
                        Int32 m_device_limit_id = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "device_limit_id", 0);

                        //cancel window
                        string sWaiver = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "WAIVER", 0);
                        bool bWaiver = sWaiver.ToLower() == "true";
                        int nWaiverPeriod = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "WAIVER_PERIOD", 0);

                        // Offline playback
                        bool bIsOfflinePlayback = ODBCWrapper.Utils.ExtractBoolean(drSource, "OFFLINE_PLAYBACK");

                        tmp.Initialize(nMaxViewsNumber, nViewLifeCycleInMin, nFullLifeCycleInMin, nID, sVirtualName, m_ext_discount_id, m_internal_discount_id,
                            m_pricing_id, m_coupon_id, m_type, m_subscription_only, m_is_renew, m_num_of_rec_periods, m_device_limit_id, bWaiver, nWaiverPeriod, bIsOfflinePlayback);
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUsageModuleData. ");
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" UMC: ", sUsageModuleCode));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }
            return tmp;
        }

        public override UsageModule GetOfflineUsageModuleData()
        {
            string sUsageModuleID = string.Empty;
            UsageModule tmp = null;
            ODBCWrapper.DataSetSelectQuery selectusagemoduleidQuery = null;
            try
            {
                selectusagemoduleidQuery = new ODBCWrapper.DataSetSelectQuery();

                selectusagemoduleidQuery += "SELECT top 1 USAGE_MODULE_CODE";
                selectusagemoduleidQuery += "FROM Pricing.dbo.groups_parameters with (nolock) ";
                selectusagemoduleidQuery += " where ";
                selectusagemoduleidQuery += " GROUP_ID " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectusagemoduleidQuery.Execute("query", true) != null)
                {
                    Int32 nCountoffline = selectusagemoduleidQuery.Table("query").DefaultView.Count;
                    if (nCountoffline > 0)
                    {
                        sUsageModuleID = selectusagemoduleidQuery.Table("query").DefaultView[0].Row["USAGE_MODULE_CODE"].ToString();
                    }
                }


                tmp = GetUsageModuleData(sUsageModuleID);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetOfflineUsageModuleData. ");
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }
            finally
            {
                if (selectusagemoduleidQuery != null)
                {
                    selectusagemoduleidQuery.Finish();
                }
            }

            return tmp;
        }

        public override UsageModule[] GetSubscriptionUsageModuleList(string nSubscitionnSubscriptionCode)
        {
            UsageModule[] ret = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("pricing_connection");
                selectQuery += "select sum.usage_module_id from subscriptions_usage_modules sum with (nolock) ";
                selectQuery += " inner join usage_modules um with (nolock) on um.id=sum.usage_module_id and um.is_active=1 and um.status=1 ";
                selectQuery += " where sum.is_active=1 and sum.status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("sum.subscription_id", "=", nSubscitionnSubscriptionCode);
                selectQuery += " and ";
                selectQuery += " sum.group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                selectQuery += " Order by sum.order_num";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        ret = new UsageModule[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        ret[i] = GetUsageModuleData(selectQuery.Table("query").DefaultView[i].Row["usage_module_id"].ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetSubscriptionUsageModuleList. ");
                sb.Append(String.Concat(" Group ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Sub Code: ", nSubscitionnSubscriptionCode));
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            finally
            {
                if (selectQuery != null)
                {
                    selectQuery.Finish();
                }
            }

            return ret;
        }
    }
}

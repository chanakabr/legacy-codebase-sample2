using DAL;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using KLogMonitor;
using System.Reflection;
using ApiObjects;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BasePreviewModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        protected int m_nGroupID;
        protected BasePreviewModule() { }
        protected BasePreviewModule(int nGroupID)
        {
            this.m_nGroupID = nGroupID;
        }

        public virtual PreviewModule GetPreviewModuleByID(long lPreviewModuleID)
        {
            PreviewModule res = null;
            DataTable dt = PricingDAL.Get_PreviewModuleData(m_nGroupID, lPreviewModuleID);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new PreviewModule();
                res.m_nID = lPreviewModuleID;
                res.m_sName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "Name");

                if (dt.Rows[0]["FULL_LIFE_CYCLE_ID"] != DBNull.Value && dt.Rows[0]["FULL_LIFE_CYCLE_ID"] != null)
                    res.m_tsFullLifeCycle = Int32.Parse(dt.Rows[0]["FULL_LIFE_CYCLE_ID"].ToString());

                if (dt.Rows[0]["NON_RENEWING_PERIOD_ID"] != DBNull.Value && dt.Rows[0]["NON_RENEWING_PERIOD_ID"] != null)
                    res.m_tsNonRenewPeriod = Int32.Parse(dt.Rows[0]["NON_RENEWING_PERIOD_ID"].ToString());
            }

            return res;
        }

        public virtual PreviewModule[] GetPreviewModulesArrayByGroupID(int nGroupID)
        {
            PreviewModule[] res = null;
            DataTable dt = PricingDAL.Get_PreviewModulesByGroupID(nGroupID, true, true);
            if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
            {
                res = new PreviewModule[dt.Rows.Count];
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    long lID = 0;
                    string sName = string.Empty;
                    int nFullLifeCycle = 0;
                    int nNonRenewingPeriod = 0;
                    sName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[i], "NAME");
                    if (dt.Rows[i]["ID"] != DBNull.Value && dt.Rows[i]["ID"] != null)
                        Int64.TryParse(dt.Rows[i]["ID"].ToString(), out lID);
                    if (dt.Rows[i]["FULL_LIFE_CYCLE_ID"] != DBNull.Value && dt.Rows[i]["FULL_LIFE_CYCLE_ID"] != null)
                        Int32.TryParse(dt.Rows[i]["FULL_LIFE_CYCLE_ID"].ToString(), out nFullLifeCycle);
                    if (dt.Rows[i]["NON_RENEWING_PERIOD_ID"] != DBNull.Value && dt.Rows[i]["NON_RENEWING_PERIOD_ID"] != null)
                        Int32.TryParse(dt.Rows[i]["NON_RENEWING_PERIOD_ID"].ToString(), out nNonRenewingPeriod);

                    res[i] = new PreviewModule(lID, sName, nFullLifeCycle, nNonRenewingPeriod);
                }
            }
            else
            {
                res = new PreviewModule[0];
            }

            return res;

        }

        public virtual UsageModule GetUsageModule(int nGroupID, string sAssetCode, eTransactionType transactionType)
        {
            UsageModule oUsageModule = null;
            DataTable dt = null;
            try
            {
                switch (transactionType)
                {
                    case eTransactionType.PPV:
                        dt = PricingDAL.GetUsageModulePPV(sAssetCode);
                        break;
                    case eTransactionType.Subscription:
                        dt = PricingDAL.GetUsageModuleSubscription(sAssetCode);
                        break;
                    case eTransactionType.Collection:
                        dt = PricingDAL.GetUsageModuleCollection(sAssetCode);
                        break;
                    default:
                        return null;
                }
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    oUsageModule = new UsageModule();
                    string sWaiver = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "WAIVER");
                    bool bWaiver = sWaiver.ToLower() == "true" ? true : false;
                    int nWaiverPeriod = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "WAIVER_PERIOD");
                    int nObjectID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "id");
                    string sVirtualName = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "NAME");
                    int nMaxNumberOfViews = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "VIEW_LIFE_CYCLE_MIN");
                    int tsViewLifeCycle = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "FULL_LIFE_CYCLE_MIN");
                    int tsMaxUsageModuleLifeCycle = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "MAX_VIEWS_NUMBER"); ;
                    int ext_discount_id = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "ext_discount_id");
                    int internal_discount_id = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "internal_discount_id");
                    int pricing_id = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "pricing_id");
                    int coupon_id = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "coupon_id");
                    int type = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "type");
                    int subscription_only = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "subscription_only");
                    int s_renew = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "is_renew");
                    int num_of_rec_periods = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "num_of_rec_periods");
                    int device_limit_id = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "device_limit_id");
                    bool bIsOfflinePlayBack = ODBCWrapper.Utils.ExtractBoolean(dt.Rows[0], "OFFLINE_PLAYBACK");

                    oUsageModule.Initialize(nMaxNumberOfViews, tsViewLifeCycle, tsMaxUsageModuleLifeCycle, nObjectID, sVirtualName, bWaiver, nWaiverPeriod, bIsOfflinePlayBack);
                }

                return oUsageModule;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetUsageModule. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Group ID: ", nGroupID));
                sb.Append(String.Concat(" Asset Cd: ", sAssetCode));
                sb.Append(String.Concat(" Trans Type: ", transactionType.ToString()));
                sb.Append(String.Concat(String.Concat(" ST: ", ex.StackTrace)));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                return null;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Xml;
using KLogMonitor;
using System.Reflection;

namespace Financial
{
    public class TvinciFinancialCalculator : TvinciFinancialCalculatorBase
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        //Hashtable m_hWeightIDToWeightVal;
        //Hashtable m_hMediaFileToPPVM;
        //Hashtable m_hPPVMToPrice;

        public TvinciFinancialCalculator(Int32 nGroupID, DateTime dStart, DateTime dEnd)
            : base(nGroupID, dStart, dEnd)
        {
        }


        /// New CalculateSubscriptions :
        /// with 100% coupons discounts
        public override void CalculateSubscriptions()
        {
            try
            {
                Hashtable hSubAmount = new Hashtable();
                Hashtable hSubWithCoupon = new Hashtable();
                Hashtable hSubUsesToSubPurchase = new Hashtable();
                Hashtable hMediaToWeight = new Hashtable();

                int nCount = 0;

                #region Get List of collections
                List<string> lCollections = new List<string>();
                Int32 nNumOfCollections = 0;
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select distinct subscription_code from subscriptions_purchases (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", ">", 0);
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    nNumOfCollections = selectQuery.Table("query").DefaultView.Count;

                    for (int i = 0; i < nNumOfCollections; i++)
                    {
                        lCollections.Add(Utils.GetStrSafeVal(ref selectQuery, "subscription_code", i));
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                #endregion

                #region  Get all Subscriptions with 100% discount
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select distinct subscription_code from subscriptions_purchases (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("collection_metadata", "=", 0);
                selectQuery += " and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("price", "=", 0);
                selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        int nSubID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "subscription_code", i);
                        if (!hSubWithCoupon.Contains(nSubID))
                        {
                            hSubWithCoupon.Add(nSubID, nSubID);
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                #endregion

                #region Get all income for each subscription
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select subscription_code, sum(price) as total, currency_code from billing_transactions (nolock) where is_active=1 and status=1 ";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("media_file_id", "=", 0);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_CODE", "<>", string.Empty);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_method", "<>", 7);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("billing_status", "=", 0);
                if (nNumOfCollections > 0)
                {
                    selectQuery += "and SUBSCRIPTION_CODE not in (" + string.Join(",", lCollections.ToArray()) + ")";
                }
                selectQuery += "group by subscription_code, currency_code";
                if (selectQuery.Execute("query", true) != null)
                {
                    nCount = selectQuery.Table("query").DefaultView.Count;
                    for (int i = 0; i < nCount; i++)
                    {
                        int nSubID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "subscription_code", i);
                        double dAmount = ODBCWrapper.Utils.GetDoubleSafeVal(selectQuery, "total", i);
                        if (!hSubAmount.Contains(nSubID))
                        {
                            hSubAmount.Add(nSubID, dAmount);
                        }
                        else
                        {
                            double dSum = (double)hSubAmount[nSubID];
                            hSubAmount[nSubID] = dSum + dAmount;
                        }
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
                #endregion

                #region Add Subscriptions with coupon to total hashtable
                foreach (DictionaryEntry de in hSubWithCoupon)
                {
                    int nSubID = (int)de.Key;

                    if (!hSubAmount.Contains(nSubID))
                    {
                        hSubAmount.Add(nSubID, 0.0);
                    }
                }
                #endregion

                #region Calculate Subscription revenues
                foreach (DictionaryEntry de in hSubAmount)
                {
                    FinancialPurchaseObject fpo = new FinancialPurchaseObject();

                    fpo.m_nRelSub = (int)de.Key;
                    fpo.m_dDiscountPrice = (double)de.Value;
                    fpo.m_dCataloguePrice = fpo.m_dDiscountPrice;

                    //Filmo only
                    fpo.m_sCurrencyCD = "EUR";
                    fpo.m_nCurrencyID = 4;

                    fpo.m_dDate = m_dStartDate;

                    fpo.m_eItemType = ItemType.SUBSCRIPTION;
                    fpo.m_eRelatedTo = RelatedTo.SUBSCRIPTION;

                    //Get all users who got the sub as gift 
                    List<string> lGiftUsersList = GetGiftUserListForSub(fpo.m_nRelSub);

                    selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select * from subscriptions_uses (nolock) where is_active=1 and status=1 and ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", ">=", m_dStartDate.ToString("yyyy-MM-dd HH:mm"));
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("create_date", "<", m_dEndDate.ToString("yyyy-MM-dd HH:mm"));
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", m_nGroupID);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("subscription_code", "=", fpo.m_nRelSub);
                    selectQuery += "and";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("is_credit_downloaded", "=", 1);
                    if (lGiftUsersList.Count > 0)
                    {
                        selectQuery += "and site_user_guid not in (" + string.Join(",", lGiftUsersList.ToArray()) + ")";
                    }
                    selectQuery += "order by create_date";
                    selectQuery.SetConnectionKey("CA_CONNECTION_STRING");
                    if (selectQuery.Execute("query", true) != null)
                    {
                        nCount = selectQuery.Table("query").DefaultView.Count;
                        FinancialMedia[] fms = new FinancialMedia[nCount];

                        double dTotalAdjustedPlays = 0;

                        for (int x = 0; x < nCount; x++)
                        {
                            fms[x] = new FinancialMedia();
                            fms[x].m_nMediaFileID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "media_file_id", x);
                            fms[x].m_nMediaFileOwner = GetContractFamilyID(fms[x].m_nMediaFileID);
                            fms[x].m_sCountryName = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "country_code", x);
                            fms[x].m_sSiteUserGUID = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "site_user_guid", x);
                            if (hSubWithCoupon.Contains(fpo.m_nRelSub))
                            {
                                fms[x].m_nCouponCode = GetCouponCodeForUserWithSub(hSubUsesToSubPurchase, fms[x].m_sSiteUserGUID, fpo.m_nRelSub);
                            }
                            fms[x].m_dDate = ODBCWrapper.Utils.GetDateSafeVal(selectQuery, "create_date", x);

                            Int32 nFWID = 0;
                            if (hMediaToWeight.Contains(fms[x].m_nMediaFileID) == false)
                            {
                                Object oWeight = ODBCWrapper.Utils.GetTableSingleVal("media_files", "financial_weight_id", fms[x].m_nMediaFileID, "MAIN_CONNECTION_STRING");
                                if (oWeight != null && oWeight != DBNull.Value)
                                {
                                    nFWID = int.Parse(oWeight.ToString());
                                }
                                hMediaToWeight.Add(fms[x].m_nMediaFileID, nFWID);
                            }
                            else
                            {
                                nFWID = (Int32)hMediaToWeight[fms[x].m_nMediaFileID];
                            }

                            if (!m_hWeightIDToWeightVal.Contains(nFWID))
                            {
                                m_hWeightIDToWeightVal[nFWID] = 1;
                                Object oWeight = ODBCWrapper.Utils.GetTableSingleVal("fr_financial_weights", "weight", nFWID, "MAIN_CONNECTION_STRING");
                                if (oWeight != null && oWeight != DBNull.Value)
                                {
                                    m_hWeightIDToWeightVal[nFWID] = double.Parse(oWeight.ToString());
                                }
                            }

                            fms[x].m_dMediaFileWeight = (double)m_hWeightIDToWeightVal[nFWID];
                            dTotalAdjustedPlays += fms[x].m_dMediaFileWeight;
                        }
                        CalcSubscription(fms, fpo, dTotalAdjustedPlays, fpo.m_dDiscountPrice);
                    }
                    selectQuery.Finish();
                    selectQuery = null;
                }
                #endregion
            }

            catch (Exception ex)
            {
                log.Error("CalculateSubscription - groupId: " + m_nGroupID.ToString() + " exception: " + ex.Message, ex);
            }
        }
    }
}

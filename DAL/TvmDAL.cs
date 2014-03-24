using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace DAL
{
    public class TvmDAL
    {
               
        public static int GetSubscriptionsNotifierImpl(int nGroupID, int nModuleID)
        {
            int nImplID = 0;

            try
            {                                              
                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_SubNotifierImplementation");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", nGroupID);
                sp.AddParameter("@ModuleID", nModuleID);

                DataSet ds = sp.ExecuteDataSet();

                if ((ds != null) && (ds.Tables[0] != null) && (ds.Tables[0].DefaultView.Count > 0))
                {
                    DataTable dt = ds.Tables[0];

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return nImplID;
                    }

                    nImplID = ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0], "IMPLEMENTATION_ID");
                }

                #region Old
                //ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();

                //selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

                //selectQuery += "SELECT IMPLEMENTATION_ID FROM GROUPS_NOTIFIERS_IMPLEMENTATIONS GNI WITH (NOLOCK), GROUPS G WITH (NOLOCK) WHERE GNI.IS_ACTIVE=1 AND GNI.STATUS=1 AND GNI.GROUP_ID = G.PARENT_GROUP_ID AND ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("G.ID", "=", nGroupID);
                //selectQuery += "AND";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GNI.MODULE_ID", "=", nModuleID);

                //selectQuery += "select * from groups_notifiers_implementations with (nolock) where is_active=1 and status=1 and ";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                //selectQuery += "and";
                //selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", nModuleID);
                //if (selectQuery.Execute("query", true) != null)
                //{
                //    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                //    if (nCount > 0)
                //    {
                //        nImplID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["IMPLEMENTATION_ID"].ToString());
                //    }
                //}

                //selectQuery.Finish();
                //selectQuery = null;
                #endregion
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nImplID;
        }

        public static Dictionary<string, string> GetSubscriptionInfo(int nGroupID, string sSubscriptionID)
        {
            Dictionary<string, string> prodDict = new Dictionary<string, string>();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");

                selectQuery += "SELECT S.ID, S.COGUID, SN.DESCRIPTION AS 'TITLE', SD.DESCRIPTION, S.IS_ACTIVE, COALESCE(PC.PRICE, 0) AS 'PRICE', LC.CODE3, S.START_DATE, S.END_DATE " + 
                                "FROM SUBSCRIPTIONS S WITH (NOLOCK) " +
                                "LEFT JOIN SUBSCRIPTION_NAMES SN WITH (NOLOCK) ON S.ID = SN.SUBSCRIPTION_ID " +
                                "LEFT JOIN SUBSCRIPTION_DESCRIPTIONS SD WITH (NOLOCK) ON S.ID = SD.SUBSCRIPTION_ID " +
                                "LEFT JOIN PRICE_CODES PC WITH (NOLOCK) ON S.SUB_PRICE_CODE = PC.ID " +
                                "LEFT JOIN LU_CURRENCY LC WITH (NOLOCK) ON PC.CURRENCY_CD = LC.ID " +
                                "WHERE S.STATUS = 1 AND ";

                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("S.GROUP_ID", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("S.ID", "=", sSubscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        prodDict["InternalProductID"]   = selectQuery.Table("query").DefaultView[0].Row["ID"].ToString();
                        prodDict["ExternalProductID"]   = selectQuery.Table("query").DefaultView[0].Row["COGUID"].ToString();
                        prodDict["Title"]               = selectQuery.Table("query").DefaultView[0].Row["TITLE"].ToString();
                        prodDict["Description"]         = selectQuery.Table("query").DefaultView[0].Row["DESCRIPTION"].ToString();
                        prodDict["Status"]              = selectQuery.Table("query").DefaultView[0].Row["IS_ACTIVE"].ToString();

                        prodDict["PriceB2C"] = "0.0";
                        if (selectQuery.Table("query").DefaultView[0].Row["PRICE"] != null)
                        {
                            prodDict["PriceB2C"] = selectQuery.Table("query").DefaultView[0].Row["PRICE"].ToString();
                        }

                        prodDict["StartDate"] = new DateTime(2000, 1, 1).ToString();
                        if (selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["START_DATE"] != DBNull.Value)
                        {
                            prodDict["StartDate"] = selectQuery.Table("query").DefaultView[0].Row["START_DATE"].ToString();
                        }

                        prodDict["EndDate"] = new DateTime(2099, 1, 1).ToString();
                        if (selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["END_DATE"] != DBNull.Value)
                        {
                            prodDict["EndDate"] = selectQuery.Table("query").DefaultView[0].Row["END_DATE"].ToString();
                        }
                    }

                }

                selectQuery.Finish();
                selectQuery = null;                
            }
            catch (Exception)
            {
            }

            return prodDict;
        }


        private static void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }



        public static List<string> GetSubscriptionOperatorCoGuids(int nGroupID, string sSubscriptionID)
        {
            List<string> lOperatorCoGuids = new List<string>();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");

                selectQuery += "SELECT GO.CLIENT_ID FROM SUBSCRIPTION_OPERATORS SO WITH (NOLOCK) JOIN TVINCI..GROUPS_OPERATORS GO WITH (NOLOCK) ON GO.ID = SO.OPERATOR_ID " +
                                "WHERE SO.IS_ACTIVE = 1 AND SO.STATUS = 1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SO.GROUP_ID", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SO.SUBSCRIPTION_ID", "=", sSubscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        for (int i = 0; i < nCount; i++)
			            {
                            string operatorCoGuid = selectQuery.Table("query").DefaultView[i].Row["CLIENT_ID"].ToString();
                            lOperatorCoGuids.Add(operatorCoGuid);
			            }
                    }

                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lOperatorCoGuids.Distinct().ToList();
        }

        public static List<int> GetSubscriptionChannelIDs(int nGroupID, string sSubscriptionID)
        {
            List<int> lChannelIDs = new List<int>();

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("PRICING_CONNECTION");

                selectQuery += "SELECT CHANNEL_ID FROM SUBSCRIPTIONS_CHANNELS WITH (NOLOCK) WHERE IS_ACTIVE = 1 AND STATUS = 1 AND ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "AND";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("SUBSCRIPTION_ID", "=", sSubscriptionID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        for (int i = 0; i < nCount; i++)
                        {
                            int channelID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["CHANNEL_ID"].ToString());
                            lChannelIDs.Add(channelID);
                        }
                    }

                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lChannelIDs.Distinct().ToList();

        }

        public static Dictionary<string, string> GetVirtualPackageInfo(int nGroupID, string sMediaID)
        {
            Dictionary<string, string> dPackage = new Dictionary<string, string>();

            dPackage["InternalProductID"] = null;
            dPackage["ExternalProductID"] = null;
            dPackage["OperatorID"] = null;
            dPackage["Title"] = null;
            dPackage["Price"] = null;
            dPackage["Description"] = null;
            dPackage["StartDate"] = null;
            dPackage["EndDate"] = null;
            dPackage["ImageUrl"] = null;


            try
            {

                ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_PackageDetails");
                sp.SetConnectionKey("MAIN_CONNECTION_STRING");
                sp.AddParameter("@GroupID", nGroupID);
                sp.AddParameter("@MediaID", sMediaID);

                DataSet ds = sp.ExecuteDataSet();

                if ((ds != null) && (ds.Tables[0] != null) && (ds.Tables[0].DefaultView.Count > 0))
                {
                    DataTable dt = ds.Tables[0];

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return dPackage;
                    }

                    dPackage["InternalProductID"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "ID");     //ODBCWrapper.Utils.GetIntSafeVal(dt.Rows[0]["ID"]);

                    dPackage["ExternalProductID"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "COGUID");

                    dPackage["OperatorID"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "OPERATOR_ID");

                    dPackage["Title"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "TITLE");

                    dPackage["Price"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "PRICE");

                    dPackage["Description"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "DESCRIPTION");

                    dPackage["StartDate"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "START_DATE");

                    dPackage["EndDate"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "END_DATE");

                    dt = ds.Tables[1];

                    if (dt == null || dt.Rows.Count == 0)
                    {
                        return dPackage;
                    }

                    dPackage["ImageUrl"] = ODBCWrapper.Utils.GetSafeStr(dt.Rows[0], "PIC_URL");

                    dt = null;
                }

                ds = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return dPackage;
        }
    }
    
}

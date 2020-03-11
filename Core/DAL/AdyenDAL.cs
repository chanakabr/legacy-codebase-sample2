using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;

namespace DAL
{
    public class AdyenDAL
    { 
        public static DataTable Get_WSPaymentUrl(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spWSPaymentUrl = new ODBCWrapper.StoredProcedure("Get_WSPaymentUrl");
            spWSPaymentUrl.SetConnectionKey("BILLING_CONNECTION_STRING");
            spWSPaymentUrl.AddParameter("@GroupID", nGroupID);

            DataSet ds = spWSPaymentUrl.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_WSRecurringUrl(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spWSRecurringUrl = new ODBCWrapper.StoredProcedure("Get_WSRecurringUrl");
            spWSRecurringUrl.SetConnectionKey("BILLING_CONNECTION_STRING");
            spWSRecurringUrl.AddParameter("@GroupID", nGroupID);

            DataSet ds = spWSRecurringUrl.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_WSCredentials(int nGroupID, int? nPurchaseType)
        {
            ODBCWrapper.StoredProcedure spWSCredentials = new ODBCWrapper.StoredProcedure("Get_WSCredentials");
            spWSCredentials.SetConnectionKey("BILLING_CONNECTION_STRING");
            spWSCredentials.AddParameter("@GroupID", nGroupID);
            spWSCredentials.AddParameter("@PurchaseType", nPurchaseType);

            DataSet ds = spWSCredentials.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable Get_AccountCredentials(string merchant)
        {
            ODBCWrapper.StoredProcedure spAccountCredentials = new ODBCWrapper.StoredProcedure("Get_AccountCredentials");
            spAccountCredentials.SetConnectionKey("BILLING_CONNECTION_STRING");
            spAccountCredentials.AddParameter("@Merchant", merchant);

            DataSet ds = spAccountCredentials.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }  
    }
}

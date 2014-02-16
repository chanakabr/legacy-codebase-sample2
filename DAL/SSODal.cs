using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;

namespace DAL
{
    public static class SSODal
    {


        public static DataSet Get_UserOperator(string sCoGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_UserOperator");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@CoGuid", sCoGuid);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds;
            return null;
        }

        public static int Create_UserOperator(string sCoGuid, int nOperatorID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Create_UserOperator");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@CoGuid", sCoGuid);           
            sp.AddParameter("@OperatorId", nOperatorID);

            int rowCount = sp.ExecuteReturnValue<int>();
            return rowCount;            
        }

        public static DataTable Get_ProviderDetails(string entityID, int? nProviderID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_ProviderDetails");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@EntityID", entityID);
            sp.AddParameter("@ProviderID", nProviderID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }      

        public static DataTable IsUserExsits(string sCoGuid, int nOperatorID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("IsUserExsits");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@coGuid", sCoGuid);
            sp.AddParameter("@operatorId", nOperatorID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static int Update_UserOperator( string coGuid, int operatorId, string siteGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Update_UserOperator");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");           
            sp.AddParameter("@coGuid", coGuid);
            sp.AddParameter("@operatorId", operatorId);
            sp.AddParameter("@siteGuid", siteGuid);

            int rowCount = sp.ExecuteReturnValue<int>();
            return rowCount;
        }

        public static DataTable Get_DomainIDByUser(string sSiteGuid)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("Get_DomainIDByUser");
            sp.SetConnectionKey("USERS_CONNECTION_STRING");
            sp.AddParameter("@userID", sSiteGuid);
            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetScopeRedirectUrl(int nGroupID, string sScope, int nOperatorID)
        {
            ODBCWrapper.StoredProcedure sp = new ODBCWrapper.StoredProcedure("GetScopeRedirectUrl");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupID", nGroupID);
             sp.AddParameter("@Scope", sScope);
            sp.AddParameter("@OperatorID", nOperatorID);

            DataSet ds = sp.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

       
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Configuration;

namespace DAL
{
    public class ImporterImpDAL
    {       

        public static DataTable Get_LuceneUrl(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spLuceneUr = new ODBCWrapper.StoredProcedure("Get_LuceneUrl");
            spLuceneUr.SetConnectionKey("MAIN_CONNECTION_STRING");
            spLuceneUr.AddParameter("@GroupID", nGroupID);

            DataSet ds = spLuceneUr.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable StartCategoriesTransaction(int nRootID)
        {
            ODBCWrapper.StoredProcedure spGet_Operators_Info = new ODBCWrapper.StoredProcedure("sp_FixYesCategoryTree");
            spGet_Operators_Info.SetConnectionKey("CONNECTION_STRING");
            spGet_Operators_Info.AddParameter("@VodRoot", nRootID);

            spGet_Operators_Info.ExecuteDataSet();

            return null;
        }  

        public static DataTable Get_CatalogUrl(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spCatalogURL = new ODBCWrapper.StoredProcedure("Get_CatalogUrl");
            spCatalogURL.SetConnectionKey("MAIN_CONNECTION_STRING");
            spCatalogURL.AddParameter("@GroupID", nGroupID);

            DataSet ds = spCatalogURL.ExecuteDataSet();

            if (ds != null)
                return ds.Tables[0];
            return null;
        }

        public static DataTable GetMediasByPPVModuleID (int groupID, int ppvModuleID)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetMediasByPPVModuleID = new ODBCWrapper.StoredProcedure("Get_MediasByPPVModuleID");
            spGetMediasByPPVModuleID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetMediasByPPVModuleID.AddParameter("@GroupID", groupID);
            spGetMediasByPPVModuleID.AddParameter("@PpvModuleID", ppvModuleID);

            dt = spGetMediasByPPVModuleID.Execute();
            
            return dt;
        }
           
    }
}
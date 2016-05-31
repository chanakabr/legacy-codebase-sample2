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

        public static DataTable GetMediasByPPVModuleID (int groupID, int ppvModuleID, int mediaIDMin)
        {
            DataTable dt = null;
            ODBCWrapper.StoredProcedure spGetMediasByPPVModuleID = new ODBCWrapper.StoredProcedure("Get_MediasByPPVModuleID");
            spGetMediasByPPVModuleID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spGetMediasByPPVModuleID.AddParameter("@GroupID", groupID);
            spGetMediasByPPVModuleID.AddParameter("@PpvModuleID", ppvModuleID);
            spGetMediasByPPVModuleID.AddParameter("@MediaIDMin", mediaIDMin);

            DataSet ds = spGetMediasByPPVModuleID.ExecuteDataSet();

            if (ds != null)
            {
                dt = ds.Tables[0];
            }            
            
            return dt;
        }

        public static string Get_CatalogUrlByParameters(int groupId, ApiObjects.eObjectType? objectType, ApiObjects.eAction? action)
        {
            string url = string.Empty;
            
            ODBCWrapper.StoredProcedure spCatalogURL = new ODBCWrapper.StoredProcedure("Get_CatalogUrlByParameters");
            spCatalogURL.SetConnectionKey("MAIN_CONNECTION_STRING");
            spCatalogURL.AddParameter("@GroupID", groupId);

            if (objectType != null && objectType.HasValue)
            {
                spCatalogURL.AddParameter("@ObjectType", (int)objectType);
            }
            else
            {
                spCatalogURL.AddParameter("@ObjectType", int.MinValue);
            }

            if (action != null && action.HasValue)
            {
                spCatalogURL.AddParameter("@Action", (int)action);
            }
            else
            {
                spCatalogURL.AddParameter("@Action", int.MinValue);
            }

            DataSet dataSet = spCatalogURL.ExecuteDataSet();

            if (dataSet != null && dataSet.Tables != null && dataSet.Tables.Count > 0 && dataSet.Tables[0].Rows.Count > 0)
            {
                object objectUrl = dataSet.Tables[0].Rows[0]["Catalog_URL"];

                if (objectUrl != null && objectUrl != DBNull.Value)
                {
                    url = Convert.ToString(objectUrl);
                }
            }

            return url;
        }
    }
}
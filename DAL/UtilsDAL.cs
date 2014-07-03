using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Tvinci.Core.DAL;


namespace DAL
{
    public class UtilsDal : BaseDal
    {
        private const string SP_GET_OPERATOR_GROUP_ID = "sp_GetGroupIDByOperatorID";


        private static void HandleException(Exception ex)
        {
            //throw new NotImplementedException();
        }


        public static DataRow GetModuleImpementationID(int nGroupID, int moduleID)
        {
            DataRow ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select IMPLEMENTATION_ID from groups_modules_implementations WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_ID", "=", moduleID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        ret = selectQuery.Table("query").DefaultView[0].Row;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        public static DataRow GetEncrypterData(int nGroupID)
        {
            DataRow ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select ENCRYPTER_IMPLEMENTATION from groups_parameters WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("GROUP_ID", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        ret = selectQuery.Table("query").DefaultView[0].Row;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        public static int GetCountryIDFromIP(long nIPVal)
        {
            int nCountryID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("main_connection");
                selectQuery += "select top 1 COUNTRY_ID from ip_to_country WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "<=", nIPVal);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", ">=", nIPVal);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nCountryID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["COUNTRY_ID"].ToString().ToLower());
                        //nID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["ID"].ToString().ToLower());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nCountryID;
        }

        public static List<int> GetStatesByCountry(int nCountryID)
        {
            List<int> lStateIDs = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select ID from states WITH (nolock) where ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("country_id", "=", nCountryID);
                selectQuery += " order by STATE_NAME";
                selectQuery.SetCachedSec(2678400);

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        lStateIDs = new List<int>();
                    }
                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        lStateIDs.Add(nID);
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lStateIDs;
        }

        public static List<int> GetAllCountries()
        {
            List<int> ret = null;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += " select ID from countries WITH (nolock) order by COUNTRY_NAME";
                selectQuery.SetCachedSec(2678400);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        ret = new List<int>();
                    }

                    for (int i = 0; i < nCount; i++)
                    {
                        Int32 nID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["ID"].ToString());
                        ret.Add(nID);
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return ret;
        }

        public static int GetGroupID(string sUN, string sPass, string sModuleName, string sIP, string sWSName)
        {
            int nGroupID = 0;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select group_id from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("PASSWORD", "=", sPass);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", sModuleName);
                selectQuery += "or";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", "00000");
                selectQuery += ")";
                selectQuery += "order by MODULE_NAME desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nGroupID;
        }

        public static string GetSecretCode(string sWSName, string sModuleName, string sUN, ref int nGroupID)
        {
            string sSecret = string.Empty;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select group_id,secret_code from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("USERNAME", "=", sUN);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", sModuleName);
                selectQuery += "or";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", "00000");
                selectQuery += ") ";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "and ALLOW_CLIENT_SIDE=1";

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        if (selectQuery.Table("query").DefaultView[0].Row["secret_code"] != null &&
                            selectQuery.Table("query").DefaultView[0].Row["secret_code"] != DBNull.Value)
                        {
                            sSecret = selectQuery.Table("query").DefaultView[0].Row["secret_code"].ToString();
                        }

                        nGroupID = int.Parse(selectQuery.Table("query").DefaultView[0].Row["group_id"].ToString());
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return sSecret;
        }

        public static bool GetWSUNPass(int nGroupID, string sIP, string sWSFunctionName, string sWSName, ref string sWSUN, ref string sWSPassword)
        {
            bool res = false;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "select USERNAME,PASSWORD from groups_modules_ips WITH (nolock) where is_active=1 and status=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP", "=", sIP);
                selectQuery += "and (";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", sWSFunctionName);
                selectQuery += "or";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("MODULE_NAME", "=", "00000");
                selectQuery += ") and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("WS_NAME", "=", sWSName);
                selectQuery += "order by MODULE_NAME desc";
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sWSUN = selectQuery.Table("querY").DefaultView[0].Row["USERNAME"].ToString();
                        sWSPassword = selectQuery.Table("querY").DefaultView[0].Row["PASSWORD"].ToString();
                        res = true;
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
                res = false;
            }

            return res;
        }

        public static string GetIP2CountryCode(long nIPVal)
        {
            string sCountry = string.Empty;

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");
                selectQuery += "select top 1 c.country_name , ipc.COUNTRY_ID,ipc.ID from ip_to_country ipc WITH (nolock), countries c WITH (nolock) where c.id = ipc.country_id and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_FROM", "<=", nIPVal);
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("IP_TO", ">=", nIPVal);
                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        sCountry = selectQuery.Table("query").DefaultView[0].Row["country_name"].ToString();
                    }
                }
                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return sCountry;
        }

        public static List<int> GetAllRelatedGroups(int nGroupID)
        {
            List<int> lGroupIDs = new List<int>();
            lGroupIDs.Add(nGroupID);

            try
            {
                ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery.SetConnectionKey("MAIN_CONNECTION_STRING");

                selectQuery += "select id from groups WITH (nolock) where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("parent_group_id", "=", nGroupID);
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;

                    for (int i = 0; i < nCount; i++)
                    {
                        int nChildGroupID = int.Parse(selectQuery.Table("query").DefaultView[i].Row["id"].ToString());

                        //lGroupIDs.Add(nChildGroupID);
                        lGroupIDs.AddRange(GetAllRelatedGroups(nChildGroupID));
                    }
                }

                selectQuery.Finish();
                selectQuery = null;
            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return lGroupIDs;
        }


        public static int GetOperatorGroupID(int nGroupID, string sOperatorCoGuid, ref int nOperatorID)
        {
            int nOperatorGroupID = 0;

            try
            {
                ODBCWrapper.StoredProcedure spGetOperatorGroupID = new ODBCWrapper.StoredProcedure(SP_GET_OPERATOR_GROUP_ID);
                spGetOperatorGroupID.SetConnectionKey("MAIN_CONNECTION_STRING");

                spGetOperatorGroupID.AddParameter("@parentGroupID", nGroupID);
                spGetOperatorGroupID.AddParameter("@operatorID", sOperatorCoGuid);


                DataSet ds = spGetOperatorGroupID.ExecuteDataSet();


                if ((ds == null) || (ds.Tables.Count == 0) || (ds.Tables[0].DefaultView.Count == 0))
                {
                    return nOperatorGroupID;
                }

                int nCount = ds.Tables[0].DefaultView.Count;
                
                if (nCount > 0)
                {
                    nOperatorID = int.Parse(ds.Tables[0].DefaultView[0].Row["ID"].ToString());
                    nOperatorGroupID = int.Parse(ds.Tables[0].DefaultView[0].Row["SUB_GROUP_ID"].ToString());
                }

            }
            catch (Exception ex)
            {
                HandleException(ex);
            }

            return nOperatorGroupID;

        }

        public static int GetParentGroupID(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spParentGroupID = new ODBCWrapper.StoredProcedure("GetParentGroupID");
            spParentGroupID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spParentGroupID.AddParameter("@GroupID", nGroupID);

            int result = spParentGroupID.ExecuteReturnValue<int>();
            return result;
        }

        public static int GetLangGroupID(int nGroupID)
        {
            ODBCWrapper.StoredProcedure spParentGroupID = new ODBCWrapper.StoredProcedure("GetLangGroupID");
            spParentGroupID.SetConnectionKey("MAIN_CONNECTION_STRING");
            spParentGroupID.AddParameter("@GroupID", nGroupID);

            int result = spParentGroupID.ExecuteReturnValue<int>();
            return result;
        }

        public static string getUserMediaMarkDocKey(int nSiteUserGuid, int nMediaID)
        {
            return string.Format("u{0}_m{1}", nSiteUserGuid, nMediaID);
        }

        public static string getDomainMediaMarksDocKey(int nDomainID)
        {
            return string.Format("d{0}", nDomainID);
        } 
    }
}

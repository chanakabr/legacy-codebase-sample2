using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using KLogMonitor;
using System.Reflection;
using DAL;

namespace Core.Pricing
{
    [Serializable]
    public abstract class BasePPVModule
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected static readonly string BASE_PPV_MODULE_LOG_FILE = "BasePPVModule";

        protected Int32 m_nGroupID;

        protected BasePPVModule() { }
        protected BasePPVModule(Int32 nGroupID)
        {
            m_nGroupID = nGroupID;
        }

        public int GroupID
        {
            get
            {
                return m_nGroupID;
            }
            protected set
            {
                m_nGroupID = value;
            }
        }

        public abstract PPVModule GetPPVModuleData(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract PPVModuleDataResponse GetPPVModuleDataResponse(string sPPVModuleCode, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract PPVModule[] GetPPVModulesData(string[] sPPVModuleCodes, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract PPVModule[] GetPPVModuleList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract PPVModule[] GetPPVModuleShrinkList(string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME);
        public abstract PPVModule[] GetPPVModulesDataByProductCodes(List<string> productCodes);

        public virtual PPVModuleContainer[] GetPPVModuleListForAdmin(Int32 nMediaFileID, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            PPVModuleContainer[] ret = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = null;
            try
            {
                selectQuery = new ODBCWrapper.DataSetSelectQuery();
                selectQuery += "SELECT ppm.ID, isnull(ppmf.ID, 0) AS mf_id , ppmf.start_date as start_date, ppmf.end_date as end_date " +
                                "FROM ppv_modules ppm (nolock) LEFT JOIN ppv_modules_media_files ppmf (nolock) ON ppmf.PPV_MODULE_ID = ppm.ID and ppmf.status=1 and ppmf.is_active=1 ";
                selectQuery += "and";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("ppmf.MEDIA_FILE_ID", "=", nMediaFileID);
                selectQuery += " WHERE (ppm.IS_ACTIVE = 1) AND (ppm.STATUS = 1) AND ";
                selectQuery += " ppm.group_id " + TVinciShared.PageUtils.GetFullChildGroupsStr(m_nGroupID, "MAIN_CONNECTION_STRING");
                if (selectQuery.Execute("query", true) != null)
                {
                    int nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                        ret = new PPVModuleContainer[nCount];
                    for (int i = 0; i < nCount; i++)
                    {
                        int nPPVID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "ID", i);
                        PPVModule p = GetPPVModuleData(nPPVID.ToString(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        DateTime? dStartDate = null;
                        DateTime? dEndDate = null;

                        object oSD = selectQuery.Table("query").DefaultView[i].Row["start_date"];
                        object oED = selectQuery.Table("query").DefaultView[i].Row["end_date"];

                        if (oSD != null && oSD != DBNull.Value)
                        {
                            dStartDate = (DateTime)oSD;
                        }

                        if (oED != null && oED != DBNull.Value)
                        {
                            dEndDate = (DateTime)oED;
                        }

                        int nMFID = ODBCWrapper.Utils.GetIntSafeVal(selectQuery, "mf_id", i);
                        bool bIsBelong = (nMFID > 0);

                        PPVModuleContainer tmp = new PPVModuleContainer();
                        tmp.Initialize(p, bIsBelong, dStartDate, dEndDate);

                        ret[i] = tmp;
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleListForAdmin. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" G ID: ", GroupID));
                sb.Append(String.Concat(" MF ID: ", nMediaFileID));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
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

        public virtual MediaFilePPVModule[] GetPPVModuleListForMediaFiles(Int32[] nMediaFileIDs, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            MediaFilePPVModule[] ret = null;
            PPVModule[] tmp = null;
            try
            {
                if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
                {
                    ret = new MediaFilePPVModule[nMediaFileIDs.Length];

                    DataTable dt = PricingDAL.Get_PPVModuleListForMediaFiles(m_nGroupID, nMediaFileIDs.ToList<int>());
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        for (int j = 0; j < nMediaFileIDs.Length; j++)
                        {
                            DataRow[] dataRows = dt.Select("mfid = " + nMediaFileIDs[j]); // get all related rows per mediaFileID
                            int i = 0;
                            int nCount = dataRows.Count();
                            if (nCount > 0)
                                tmp = new PPVModule[nCount];
                            string sProductCode = string.Empty;
                            foreach (DataRow row in dataRows)
                            {
                                if (i == 0)
                                    sProductCode = ODBCWrapper.Utils.GetSafeStr(row["Product_Code"]);
                                Int32 nPPVID = ODBCWrapper.Utils.GetIntSafeVal(row["ppmid"]);
                                PPVModule p = GetPPVModuleData(nPPVID.ToString(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                tmp[i] = p;
                                i++;
                            }
                            ret[j] = new MediaFilePPVModule();
                            ret[j].Initialize(tmp, nMediaFileIDs[j], sProductCode);
                            tmp = null;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < nMediaFileIDs.Length; k++)
                        {
                            ret[k] = new MediaFilePPVModule();
                            ret[k].Initialize(tmp, nMediaFileIDs[k], string.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleListForMediaFiles. ");
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Name: ", sDEVICE_NAME));
                if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
                {
                    sb.Append(String.Concat(" MF IDs: "));
                    for (int i = 0; i < nMediaFileIDs.Length; i++)
                    {
                        sb.Append(String.Concat(nMediaFileIDs[i], "; "));
                    }
                }
                else
                {
                    sb.Append("MF IDs are null or empty. ");
                }
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;

            }
            return ret;
        }

        public virtual MediaFilePPVContainer[] GetPPVModuleListForMediaFilesWithExpiry(Int32[] nMediaFileIDs, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            MediaFilePPVContainer[] ret = null;
            PPVModuleWithExpiry[] tmp = null;
            try
            {
                if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
                {
                    ret = new MediaFilePPVContainer[nMediaFileIDs.Length];

                    DataTable dt = PricingDAL.Get_PPVModuleListForMediaFilesWithExpired(m_nGroupID, nMediaFileIDs.ToList<int>());
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        for (int j = 0; j < nMediaFileIDs.Length; j++)
                        {
                            DataRow[] dataRows = dt.Select("mfid = " + nMediaFileIDs[j]); // get all related rows per mediaFileID

                            if (dataRows.Count() > 0)
                                tmp = new PPVModuleWithExpiry[dataRows.Count()];

                            string productCode = string.Empty;

                            DateTime startDate, endDate;
                            DataRow row;
                            for (int i = 0; i < dataRows.Count(); i++)
                            {
                                row = dataRows[i];

                                //if (i == 0)
                                //    sProductCode = ODBCWrapper.Utils.GetSafeStr(row["Product_Code"]);
                                Int32 nPPVID = ODBCWrapper.Utils.GetIntSafeVal(row["ppmid"]);
                                startDate = ((startDate = ODBCWrapper.Utils.GetDateSafeVal(row["PPMF_START_DATE"])) == ODBCWrapper.Utils.FICTIVE_DATE) ? DateTime.MinValue : startDate;
                                endDate = ((endDate = ODBCWrapper.Utils.GetDateSafeVal(row["PPMF_END_DATE"])) == ODBCWrapper.Utils.FICTIVE_DATE) ? DateTime.MaxValue : endDate;
                                PPVModule p = GetPPVModuleData(nPPVID.ToString(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                tmp[i] = new PPVModuleWithExpiry(p, startDate, endDate);

                            }
                            ret[j] = new MediaFilePPVContainer();
                            ret[j].Initialize(tmp, nMediaFileIDs[j], productCode);
                            tmp = null;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < nMediaFileIDs.Length; k++)
                        {
                            ret[k] = new MediaFilePPVContainer();
                            ret[k].Initialize(tmp, nMediaFileIDs[k], string.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleListForMediaFilesWithExpiry. ");
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
                {
                    sb.Append(" MF IDs: ");
                    for (int i = 0; i < nMediaFileIDs.Length; i++)
                    {
                        sb.Append(String.Concat(nMediaFileIDs[i], "; "));
                    }
                }
                else
                {
                    sb.Append("MF IDs is empty. ");
                }
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                throw;
            }


            return ret;
        }

        public virtual MediaFilePPVContainer[] Get_PPVModuleForMediaFiles(int[] nMediaFileIDs, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME)
        {
            MediaFilePPVContainer[] ret = null;
            PPVModuleWithExpiry[] tmp = null;
            try
            {
                if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
                {
                    ret = new MediaFilePPVContainer[nMediaFileIDs.Length];

                    DataTable dt = PricingDAL.Get_PPVModuleForMediaFiles(m_nGroupID, nMediaFileIDs.ToList<int>());
                    if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                    {
                        for (int j = 0; j < nMediaFileIDs.Length; j++)
                        {
                            DataRow[] dataRows = dt.Select("mfid = " + nMediaFileIDs[j]); // get all related rows per mediaFileID

                            if (dataRows.Count() > 0)
                                tmp = new PPVModuleWithExpiry[dataRows.Count()];

                            string sProductCode = string.Empty;

                            DateTime dtStartDate, dtEndDate;
                            DataRow row;
                            for (int i = 0; i < dataRows.Count(); i++)
                            {
                                row = dataRows[i];

                                //if (i == 0)
                                //    sProductCode = ODBCWrapper.Utils.GetSafeStr(row["Product_Code"]);
                                Int32 nPPVID = ODBCWrapper.Utils.GetIntSafeVal(row["ppmid"]);
                                dtStartDate = ((dtStartDate = ODBCWrapper.Utils.GetDateSafeVal(row["PPMF_START_DATE"])) == ODBCWrapper.Utils.FICTIVE_DATE) ? DateTime.MinValue : dtStartDate;
                                dtEndDate = ((dtEndDate = ODBCWrapper.Utils.GetDateSafeVal(row["PPMF_END_DATE"])) == ODBCWrapper.Utils.FICTIVE_DATE) ? DateTime.MaxValue : dtEndDate;
                                PPVModule p = GetPPVModuleData(nPPVID.ToString(), sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                                tmp[i] = new PPVModuleWithExpiry(p, dtStartDate, dtEndDate);

                            }
                            ret[j] = new MediaFilePPVContainer();
                            ret[j].Initialize(tmp, nMediaFileIDs[j], sProductCode);
                            tmp = null;
                        }
                    }
                    else
                    {
                        for (int k = 0; k < nMediaFileIDs.Length; k++)
                        {
                            ret[k] = new MediaFilePPVContainer();
                            ret[k].Initialize(tmp, nMediaFileIDs[k], string.Empty);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at GetPPVModuleListForMediaFilesWithExpiry. ");
                sb.Append(String.Concat(" G ID: ", m_nGroupID));
                sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                sb.Append(String.Concat(" Cntry Cd: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" D Nm: ", sDEVICE_NAME));
                if (nMediaFileIDs != null && nMediaFileIDs.Length > 0)
                {
                    sb.Append(" MF IDs: ");
                    for (int i = 0; i < nMediaFileIDs.Length; i++)
                    {
                        sb.Append(String.Concat(nMediaFileIDs[i], "; "));
                    }
                }
                else
                {
                    sb.Append("MF IDs is empty. ");
                }
                sb.Append(String.Concat(" Ex Type: ", ex.GetType().Name));
                sb.Append(String.Concat(" ST: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion
                throw;
            }


            return ret;
        }
    }
}

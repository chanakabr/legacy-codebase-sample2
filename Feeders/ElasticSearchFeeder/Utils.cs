using ApiObjects;
using ApiObjects.SearchObjects;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchFeeder
{
    public static class Utils
    {
        public static string GetWSURL(string key)
        {
            return TVinciShared.WS_Utils.GetTcmConfigValue(key);
        }

        public static long UnixTimeStampNow()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds, CultureInfo.CurrentCulture);
        }

        public static string GetEpgGroupAliasStr(int nGroupID)
        {
            return string.Format("{0}_epg", nGroupID);
        }

        public static string GetMediaGroupAliasStr(int nGroupID)
        {
            return nGroupID.ToString();
        }

        public static string GetNewEpgIndexStr(int nGroupID)
        {
            return string.Format("{0}_epg_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static string GetNewMediaIndexStr(int nGroupID)
        {
            return string.Format("{0}_{1}", nGroupID, DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
        }

        public static EpgCB GetEpgProgram(int nGroupID, int nEpgID)
        {
            EpgCB epg = new EpgCB();

            EpgBL.BaseEpgBL oEpgBL = EpgBL.Utils.GetInstance(nGroupID);
            try
            {
                ulong uEpgID = ulong.Parse(nEpgID.ToString());
                EPGChannelProgrammeObject oProg = oEpgBL.GetEpg(uEpgID);
                if (oProg != null)
                {
                    epg.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(oProg.EPG_CHANNEL_ID);
                    epg.EpgID = ODBCWrapper.Utils.GetUnsignedLongSafeVal(oProg.EPG_ID);
                    epg.GroupID = ODBCWrapper.Utils.GetIntSafeVal(oProg.GROUP_ID);
                    epg.isActive = (oProg.IS_ACTIVE == "true" ? true : false);
                    epg.Description = oProg.DESCRIPTION;
                    epg.Name = oProg.NAME;
                    if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(oProg.START_DATE)))
                    {
                        epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(oProg.START_DATE);
                    }
                     if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(oProg.END_DATE)))
                     {
                         epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(oProg.END_DATE);
                     }
                     
                    List<string> tempList;
                    foreach (EPGDictionary meta in oProg.EPG_Meta)
                    {
                        if (epg.Metas.TryGetValue(meta.Key, out tempList))
                        {
                            tempList.Add(meta.Value);
                            epg.Metas.Add(meta.Key, tempList);
                        }
                        else
                        {
                            tempList = new List<string>() { meta.Value };
                            epg.Metas.Add(meta.Key, tempList);
                        }
                    }


                    foreach (EPGDictionary tag in oProg.EPG_TAGS)
                    {
                        if (epg.Tags.TryGetValue(tag.Key, out tempList))
                        {
                            tempList.Add(tag.Value);
                            epg.Tags.Add(tag.Key, tempList);
                        }
                        else
                        {
                            tempList = new List<string>() { tag.Value };
                            epg.Tags.Add(tag.Key, tempList);
                        }
                    }                    

                }
                return epg;
            }
            catch (Exception ex)
            {
                //write to log???
                return null;
            }

            #region old code take details from DB
            /*            DataSet ds = Tvinci.Core.DAL.EpgDal.GetEpgProgramDetails(nGroupID, nEpgID);
            if (ds != null && ds.Tables != null)
            {
                if (ds.Tables[0] != null && ds.Tables[0].Rows != null && ds.Tables[0].Rows.Count > 0)
                {
                    //Basic Details
                    foreach (DataRow row in ds.Tables[0].Rows)
                    {
                        EpgCB epg = new EpgCB();
                        epg.ChannelID = ODBCWrapper.Utils.GetIntSafeVal(row["EPG_CHANNEL_ID"]);
                        epg.EpgID = ODBCWrapper.Utils.GetUnsignedLongSafeVal(row["ID"]);
                        epg.GroupID = ODBCWrapper.Utils.GetIntSafeVal(row["GROUP_ID"]);
                        epg.isActive = (ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) == 1) ? true : false;
                        epg.Description = ODBCWrapper.Utils.GetSafeStr(row["DESCRIPTION"]);
                        epg.Name = ODBCWrapper.Utils.GetSafeStr(row["NAME"]);
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["START_DATE"])))
                        {
                            epg.StartDate = ODBCWrapper.Utils.GetDateSafeVal(row["START_DATE"]);
                        }
                        if (!string.IsNullOrEmpty(ODBCWrapper.Utils.GetSafeStr(row["END_DATE"])))
                        {
                            epg.EndDate = ODBCWrapper.Utils.GetDateSafeVal(row["END_DATE"]);
                        }

                        //Metas
                        if (ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            List<string> tempList;
                            DataRow[] metas = ds.Tables[1].Select("program_id=" + epg.EpgID);
                            foreach (DataRow meta in metas)
                            {
                                string metaName = ODBCWrapper.Utils.GetSafeStr(meta["name"]);
                                string metaValue = ODBCWrapper.Utils.GetSafeStr(meta["value"]);

                                if (epg.Metas.TryGetValue(metaName, out tempList))
                                {
                                    tempList.Add(metaValue);
                                    epg.Tags.Add(metaName, tempList);
                                }
                                else
                                {
                                    tempList = new List<string>() { metaValue };
                                    epg.Metas.Add(metaName, tempList);
                                }
                            }
                        }
                        //Tags
                        if (ds.Tables.Count >= 4 && ds.Tables[3] != null && ds.Tables[3].Rows != null && ds.Tables[3].Rows.Count > 0)
                        {
                            List<string> tempList;
                            DataRow[] tags = ds.Tables[2].Select("program_id=" + epg.EpgID);
                            foreach (DataRow tag in tags)
                            {
                                string tagName = ODBCWrapper.Utils.GetSafeStr(tag["name"]);
                                string tagValue = ODBCWrapper.Utils.GetSafeStr(tag["value"]);
                                if (epg.Tags.TryGetValue(tagName, out tempList))
                                {
                                    tempList.Add(tagValue);
                                    epg.Tags.Add(tagName, tempList);
                                }
                                else
                                {
                                    tempList = new List<string>() { tagValue };
                                    epg.Tags.Add(tagName, tempList);
                                }
                            }
                        }

                        res = epg;
                    }
                }
            }
            
            return res;
 * */
            #endregion
        }

        public static string GetPermittedWatchRules(int nGroupId)
        {
            DataTable permittedWathRulesDt = Tvinci.Core.DAL.CatalogDAL.GetPermittedWatchRulesByGroupId(nGroupId);
            List<string> lWatchRulesIds = null;
            if (permittedWathRulesDt != null && permittedWathRulesDt.Rows.Count > 0)
            {
                lWatchRulesIds = new List<string>();
                foreach (DataRow permittedWatchRuleRow in permittedWathRulesDt.Rows)
                {
                    lWatchRulesIds.Add(ODBCWrapper.Utils.GetSafeStr(permittedWatchRuleRow["RuleID"]));
                }
            }

            string sRules = string.Empty;

            if (lWatchRulesIds != null && lWatchRulesIds.Count > 0)
            {
                sRules = string.Join(" ", lWatchRulesIds);
            }

            return sRules;
        }
    }

    public enum eESFeederType
    {
        MEDIA,
        EPG
    }
}

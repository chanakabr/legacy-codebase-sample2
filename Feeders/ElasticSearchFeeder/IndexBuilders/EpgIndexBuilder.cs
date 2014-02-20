using ApiObjects;
using Catalog;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElasticSearchFeeder.IndexBuilders
{
    public class EpgIndexBuilder : AbstractIndexBuilder
    {
        protected ElasticSearchApi m_oESApi;
        protected int m_nGroupID;
        protected ESSerializer m_oESSerializer;

        public EpgIndexBuilder(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESApi = new ElasticSearchApi();
            m_oESSerializer = new ESSerializer();
        }

        public override async Task<bool> BuildIndex()
        {
            bool bRes = false;
            GroupsCache.Instance.RemoveGroup(m_nGroupID);
            Group oGroup = GroupsCache.Instance.GetGroup(m_nGroupID);

            if (oGroup == null)
                return bRes;

            DateTime tempDate = dStartDate;

            string sGroupAlias = Utils.GetEpgGroupAliasStr(m_nGroupID);
            string sNewIndex = Utils.GetNewEpgIndexStr(m_nGroupID);

            bRes = m_oESApi.BuildIndex(sNewIndex, 0, 0);

            if (!bRes)
            {
                Logger.Logger.Log("Error", string.Format("Failed creating index for index:{0}", sNewIndex), "ElasticSearch");
                return bRes; ;
            }


            string sMapping = m_oESSerializer.CreateEpgMapping(oGroup.m_oEpgGroupSettings.m_lMetasName, oGroup.m_oEpgGroupSettings.m_lTagsName);
            bRes = m_oESApi.InsertMapping(sNewIndex, EPG, sMapping.ToString());

            if (!bRes)
            {
                Logger.Logger.Log("Error", string.Format("Failed creating EPG mapping for index:{0}; mapping:{1}", sNewIndex, sMapping), "ElasticSearch");
                return bRes;
            }

            while (tempDate <= dEndDate)
            {
                await PopulateEpgIndex(sNewIndex, EPG, tempDate);
                tempDate = tempDate.AddDays(1);
            }

            if (bSwitchIndex)
            {
                List<string> lOldIndices = m_oESApi.GetAliases(sGroupAlias);

                bRes = await Task<bool>.Factory.StartNew(() => m_oESApi.SwitchIndex(sNewIndex, sGroupAlias, lOldIndices, null));

                if (bRes && lOldIndices.Count > 0)
                {
                    await Task.Factory.StartNew(() => m_oESApi.DeleteIndices(lOldIndices));
                }
            }
            return bRes;
        }

        protected async Task PopulateEpgIndex(string sIndex, string sType, DateTime dDate)
        {
            Dictionary<ulong, EpgCB> programs = await GetEpgPrograms(m_nGroupID, dDate, 0);

            List<KeyValuePair<ulong, string>> lEpgObject = new List<KeyValuePair<ulong, string>>();
            foreach (ulong epgID in programs.Keys)
            {
                EpgCB oEpg = programs[epgID];

                if (oEpg != null)
                {
                    string sEpgObj = m_oESSerializer.SerializeEpgObject(oEpg);
                    lEpgObject.Add(new KeyValuePair<ulong, string>(oEpg.EpgID, sEpgObj));
                }

                if (lEpgObject.Count >= 50)
                {
                    m_oESApi.CreateBulkIndexRequest(sIndex, sType, lEpgObject);

                    lEpgObject = new List<KeyValuePair<ulong, string>>();
                }
            }

            if (lEpgObject.Count > 0)
            {

                m_oESApi.CreateBulkIndexRequest(sIndex, sType, lEpgObject);
            }
        }

        protected async Task<Dictionary<ulong, EpgCB>> GetEpgPrograms(int nGroupID, DateTime? dDateTime, int nEpgID)
        {
            Dictionary<ulong, EpgCB> epgs = new Dictionary<ulong, EpgCB>();

            DataSet ds = await Task.Factory.StartNew(() => Tvinci.Core.DAL.EpgDal.Get_EpgPrograms(m_nGroupID, dDateTime, nEpgID));

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
                        if (ds.Tables.Count >= 2 && ds.Tables[1] != null && ds.Tables[1].Rows != null && ds.Tables[1].Rows.Count > 0)
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
                        if (ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
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

                        epgs.Add(epg.EpgID, epg);
                    }
                }
            }

            return epgs;
        }
    }
}

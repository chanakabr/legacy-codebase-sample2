using ApiObjects;
using Catalog;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EpgBL;

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

            bRes = m_oESApi.BuildIndex(sNewIndex, 0, 0, null, null);

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

            //Get All programs by group_id + date from CB
            TvinciEpgBL oEpgBL = new TvinciEpgBL(nGroupID);
            List<EpgCB> lEpgCB = await Task.Factory.StartNew(() => oEpgBL.GetGroupEpgs(0, 0, dDateTime, dDateTime.Value.AddDays(1)));

            if (lEpgCB != null && lEpgCB.Count > 0)
            {
                foreach (EpgCB epg in lEpgCB)
                {
                    epgs.Add(epg.EpgID, epg);
                }
            }

            return epgs;
        }
    }
}

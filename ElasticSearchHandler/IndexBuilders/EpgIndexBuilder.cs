using ApiObjects;
using Catalog;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GroupsCacheManager;

namespace ElasticSearchHandler.IndexBuilders
{
    public class EpgIndexBuilder : IIndexBuilder
    {
         private static readonly string EPG = "epg";

        private int m_nGroupID;
        private ESSerializer m_oESSerializer;
        private ElasticSearchApi m_oESApi;
        private Group m_oGroup;

        public bool SwitchIndexAlias { get; set; }
        public bool DeleteOldIndices { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        public EpgIndexBuilder(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESApi = new ElasticSearchApi();
            m_oESSerializer = new ESSerializer();
        }

        public bool Build()
        {
            bool bSuccess = false;

            Logger.Logger.Log("Info", string.Concat("Starting epg index build for group ", m_nGroupID), "ESBuildHandler");

            if (m_nGroupID == 0)
            {
                bSuccess = true;
                return bSuccess;
            }

            GroupManager groupManager = new GroupManager();
            m_oGroup = groupManager.GetGroup(m_nGroupID);


            if (m_oGroup == null)
            {
                Logger.Logger.Log("Error", "Could not load group in epg index builder", "ESBuildHandler");
                return bSuccess;
            }

            string sNewIndex;
            bSuccess = CreateIndex(out sNewIndex);

            if (!bSuccess)
                return bSuccess;
            
            bSuccess = CreateMapping(ref sNewIndex);
            if (!bSuccess)
                return bSuccess;

            IndexPrograms(ref sNewIndex);

            if (SwitchIndexAlias)
                bSuccess = SwitchIndices(ref sNewIndex);

            
            return bSuccess;
        }

        private void IndexPrograms(ref string sIndex)
        {
            DateTime tempDate = StartDate.Value;

            while (tempDate <= EndDate)
            {

                Dictionary<ulong, EpgCB> programs = ElasticsearchTasksCommon.Utils.GetEpgPrograms(m_nGroupID, tempDate, 0);

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
                        m_oESApi.CreateBulkIndexRequest(sIndex, EPG, lEpgObject);

                        lEpgObject = new List<KeyValuePair<ulong, string>>();
                    }
                }

                if (lEpgObject.Count > 0)
                {

                    m_oESApi.CreateBulkIndexRequest(sIndex, EPG, lEpgObject);
                }

                tempDate = tempDate.AddDays(1);
            }
        }

        private bool CreateIndex(out string sNewIndex)
        {
            sNewIndex = ElasticsearchTasksCommon.Utils.GetNewEpgIndexStr(m_nGroupID);
            bool bRes = m_oESApi.BuildIndex(sNewIndex, 0, 0, null, null);

            if (!bRes)
            {
                Logger.Logger.Log("Error", string.Format("Failed creating index for index:{0}", sNewIndex), "ESBuildHandler");
            }

            return bRes;
        }

        private bool CreateMapping(ref string sIndex)
        {
            bool bRes = false;
            string sMapping = m_oESSerializer.CreateEpgMapping(m_oGroup.m_oEpgGroupSettings.m_lMetasName, m_oGroup.m_oEpgGroupSettings.m_lTagsName);

            if (!string.IsNullOrEmpty(sMapping))
            {
                bRes = m_oESApi.InsertMapping(sIndex, EPG, sMapping.ToString());
            }

            if (!bRes)
            {
                Logger.Logger.Log("Error", string.Format("Failed creating EPG mapping for index:{0}; mapping:{1}", sIndex, sMapping), "ESBuildHandler");
            }

            return bRes;
        }

        private bool SwitchIndices(ref string sIndex)
        {
            string sAlias = ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(m_nGroupID);
            List<string> lOldIndices = m_oESApi.GetAliases(sAlias);

            bool bSwithcIndex = m_oESApi.SwitchIndex(sIndex, sAlias, lOldIndices);

            if (!bSwithcIndex)
            {
                Logger.Logger.Log("Info", string.Concat("Unable to switch from old to new index. id=", sIndex), "ESBuildHandler");
            }
            else if (DeleteOldIndices)
            {
                m_oESApi.DeleteIndices(lOldIndices);
            }

            return bSwithcIndex;
        }

    }
}

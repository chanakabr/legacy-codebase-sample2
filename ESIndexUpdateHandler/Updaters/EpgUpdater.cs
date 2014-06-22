using ApiObjects;
using ElasticSearch.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESIndexUpdateHandler.Updaters
{
    public class EpgUpdater : IUpdateable
    {
        public static readonly string EPG = "epg";

        private int m_nGroupID;
        private ElasticSearch.Common.ESSerializer m_oESSerializer;
        private ElasticSearch.Common.ElasticSearchApi m_oESApi;

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public EpgUpdater(int nGroupID)
        {
            m_nGroupID = nGroupID;
            m_oESSerializer = new ElasticSearch.Common.ESSerializer();
            m_oESApi = new ElasticSearch.Common.ElasticSearchApi();
        }

        public bool Start()
        {
            bool result = false;
            Logger.Logger.Log("Info", "Start Epg update", "ESUpdateHandler");
            if (IDs == null || IDs.Count == 0)
            {
                Logger.Logger.Log("Info", "Epg Id list empty", "ESUpdateHandler");
                result = true;

                return result;
            }

            if (!m_oESApi.IndexExists(ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(m_nGroupID)))
            {
                Logger.Logger.Log("Error", string.Format("Index of type EPG for group {0} does not exist", m_nGroupID), "ESUpdateHandler");
                return result;
            }

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                    result = DeleteEpg(IDs);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    result = UpdateEpg(IDs);
                    break;
                default:
                    result = true;
                    break;
            }

            return result;
        }

        private bool UpdateEpg(List<int> lEpgIDs)
        {
            bool bRes = false;

            try
            {

                Task<EpgCB>[] tPrograms = new Task<EpgCB>[lEpgIDs.Count];
                //open task factory and run GetEpgProgram on different threads
                //wait to finish
                //bulk insert
                for (int i = 0; i < lEpgIDs.Count; i++)
                {
                    tPrograms[i] = Task.Factory.StartNew<EpgCB>(
                        (index) =>
                        {
                            return ElasticsearchTasksCommon.Utils.GetEpgProgram(m_nGroupID, (int)index);
                        }, lEpgIDs[i]);
                }

                Task.WaitAll(tPrograms);

                List<EpgCB> lEpg = tPrograms.Select(t => t.Result).Where(t => t != null).ToList();

                if (lEpg != null & lEpg.Count > 0)
                {

                    List<ESBulkRequestObj<ulong>> lBulkObj = new List<ESBulkRequestObj<ulong>>();
                    string sSerializedEpg;
                    string sAlias = ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(m_nGroupID);

                    foreach (EpgCB epg in lEpg)
                    {
                        sSerializedEpg = m_oESSerializer.SerializeEpgObject(epg);
                        lBulkObj.Add(new ESBulkRequestObj<ulong>() { docID = epg.EpgID, index = sAlias, type = EPG, Operation = eOperation.index, document = sSerializedEpg });
                    }

                    m_oESApi.CreateBulkIndexRequest(lBulkObj);

                    bRes = true;
                }
            }
            catch { }

            return bRes;
        }

        private bool DeleteEpg(List<int> lEpgIDs)
        {
            bool bRes = false;

            if (lEpgIDs != null & lEpgIDs.Count > 0)
            {
                List<ESBulkRequestObj<int>> lBulkObj = new List<ESBulkRequestObj<int>>();
                string sAlias = ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(m_nGroupID);

                foreach (int epgId in lEpgIDs)
                {
                    lBulkObj.Add(new ESBulkRequestObj<int>() { docID = epgId, index = sAlias, type = EPG, Operation = eOperation.delete });
                }

                m_oESApi.CreateBulkIndexRequest(lBulkObj);

                bRes = true;
            }


            return bRes;
        }

        
    }
}

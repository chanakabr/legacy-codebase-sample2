using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using ApiObjects.MediaIndexingObjects;
using Catalog;
using System.Data;
using ApiObjects.SearchObjects;
using QueueWrapper;
using ElasticSearch.Searcher;
using ElasticSearch.Common;
using ApiObjects;
using ElasticSearchFeeder.IndexBuilders;

namespace ElasticSearchFeeder
{
    public class BaseESMedia : ElasticSearchBaseImplementor
    {
        protected const string MEDIA = "media";
        protected const string EPG = "epg";

        protected ESSerializer m_oESSerializer;
        protected ElasticSearchApi m_oESApi;
        protected DateTime m_dStartDate, m_dEndDate;
        public bool bSwitchIndex { get; set; }


        public BaseESMedia(int nGroupID, string sQueueName, bool bRebuildIndex)
            : base(nGroupID, sQueueName, bRebuildIndex)
        {
            m_oESSerializer = new ESSerializer();
            m_oESApi = new ElasticSearchApi();
        }

        public BaseESMedia(int nGroupID, string sQueueName, bool bRebuildIndex, DateTime dStartDate, DateTime dEndDate)
            : base(nGroupID, sQueueName, bRebuildIndex)
        {
            m_oESSerializer = new ESSerializer();
            m_oESApi = new ElasticSearchApi();
            m_dStartDate = dStartDate;
            m_dEndDate = dEndDate;
        }

        public override async void Update(eESFeederType eESFeeder)
        {
            try
            {
                //If requested, reload index
                if (m_bRebuildIndex || !checkIndexExists(eESFeeder))
                {
                    IndexBuilders.AbstractIndexBuilder oIndexBuilder = IndexBuilders.AbstractIndexBuilder.GetIndexBuilder(m_nGroupID, eESFeeder);
                    oIndexBuilder.dStartDate = m_dStartDate;
                    oIndexBuilder.dEndDate = m_dEndDate;
                    oIndexBuilder.bSwitchIndex = bSwitchIndex;

                    bool bReloadSuccess = await oIndexBuilder.BuildIndex();

                    if (!bReloadSuccess)
                    {
                        Logger.Logger.Log("Error", string.Format("Reloading index failed for group {0}", m_nGroupID), "ESFeeder");
                        return;
                    }
                }
                //Get message queue
                using (IQueueImpl oMessageQueue = QueueImplFactory.GetQueueImp(QueueWrapper.Enums.QueueType.RabbitQueue))
                {
                    if (oMessageQueue != null)
                    {
                        string sGroupID = m_nGroupID.ToString();
                        IndexingData oMessage = null;

                        //Do while queue is not empty
                        string sAckId;
                        bool bRetVal;
                        try
                        {
                            while ((oMessage = oMessageQueue.Dequeue<IndexingData>(sGroupID, out sAckId)) != null)
                            {

                                if (oMessage.Ids != null && oMessage.Ids.Count > 0)
                                {
                                    try
                                    {
                                        bRetVal = false;

                                        switch (oMessage.ObjectType)
                                        {
                                            case ApiObjects.eObjectType.Media:
                                                bRetVal = await MediaChanged(oMessage.Ids, oMessage.Action);
                                                break;
                                            case ApiObjects.eObjectType.Channel:
                                                bRetVal = ChannelChanged(oMessage.Ids, oMessage.Action);
                                                break;
                                            case ApiObjects.eObjectType.EPG:
                                                bRetVal = EpgChanged(oMessage.Ids, oMessage.Action);
                                                break;
                                            default:
                                                bRetVal = true;
                                                break;
                                        }
                                        if (bRetVal && !string.IsNullOrEmpty(sAckId))
                                        {
                                            oMessageQueue.Ack(m_sQueueName, sAckId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Logger.Log("Exception ", ex.Message, "ESFeeder");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Logger.Log("Exception on Dequeue", ex.Message, "ESFeeder");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Exception", ex.Message, "ESFeeder");
            }
        }

        private bool EpgChanged(List<int> lEpgIDs, ApiObjects.eAction eAction)
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
                            return Utils.GetEpgProgram(m_nGroupID, (int)index);
                        }, i);
                }

                Task.WaitAll(tPrograms);

                List<EpgCB> lEpg = tPrograms.Select(t => t.Result).Where(t => t != null).ToList();

                if (lEpg != null & lEpg.Count > 0)
                {
                    List<KeyValuePair<ulong, string>> lKvp = new List<KeyValuePair<ulong, string>>();
                    string sSerializedEpg;
                    foreach (EpgCB epg in lEpg)
                    {
                        sSerializedEpg = m_oESSerializer.SerializeEpgObject(epg);
                        lKvp.Add(new KeyValuePair<ulong, string>(epg.EpgID, sSerializedEpg));
                    }
                    string sAlias = Utils.GetEpgGroupAliasStr(m_nGroupID);
                    m_oESApi.CreateBulkIndexRequest(sAlias, EPG, lKvp);

                    bRes = true;
                }
            }
            catch { }

            return bRes;
        }

        private bool checkIndexExists(eESFeederType eFeeder)
        {
            bool bRes = false;

            switch (eFeeder)
            {
                case eESFeederType.MEDIA:
                    bRes = m_oESApi.IndexExists(Utils.GetMediaGroupAliasStr(m_nGroupID));
                    break;
                case eESFeederType.EPG:
                    bRes = m_oESApi.IndexExists(Utils.GetEpgGroupAliasStr(m_nGroupID));
                    break;
                default:
                    break;
            }

            return bRes;
        }

        private bool ChannelChanged(List<int> lChannelIDs, ApiObjects.eAction eAction)
        {
            bool bRes = false;
            switch (eAction)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                    bRes = Delete(lChannelIDs, ApiObjects.eObjectType.Channel);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    bRes = UpdateChannel(lChannelIDs);
                    break;
                default:
                    bRes = true;
                    break;
            }

            return bRes;
        }

        private async Task<bool> MediaChanged(List<int> lMediaIDs, ApiObjects.eAction eAction)
        {
            bool bRes = false;
            switch (eAction)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    bRes = await UpdateMedias(lMediaIDs);
                    break;
                case ApiObjects.eAction.Delete:
                    bRes = Delete(lMediaIDs, ApiObjects.eObjectType.Media);
                    break;
                default:
                    bRes = true;
                    break;
            }

            return bRes;
        }

        private bool Delete(List<int> lIDs, ApiObjects.eObjectType eObjectType)
        {
            bool bRes = false;

            if (lIDs == null || lIDs.Count == 0)
                return bRes;

            string sIndex = m_nGroupID.ToString();

            if (eObjectType == ApiObjects.eObjectType.Media)
            {
                foreach (int id in lIDs)
                {
                    bRes = m_oESApi.DeleteDoc(sIndex, MEDIA, id.ToString());
                }
            }
            else if (eObjectType == ApiObjects.eObjectType.Channel)
            {
                List<string> aliases = m_oESApi.GetAliases(sIndex);
                if (aliases != null && aliases.Count > 0)
                {
                    foreach (int nChannelID in lIDs)
                    {
                        foreach (string index in aliases)
                        {
                            bRes = m_oESApi.DeleteDoc("_percolator", index, nChannelID.ToString());
                        }
                    }
                }
            }

            return bRes;
        }

        private bool UpdateChannel(List<int> lChannelIds)
        {
            bool bRes = false;
            Group oGroup = GroupsCache.Instance.GetGroup(m_nGroupID);
            if (oGroup == null || oGroup.m_oGroupChannels == null)
                return bRes;

            List<string> aliases = m_oESApi.GetAliases(m_nGroupID.ToString());

            Channel oChannel;
            MediaSearchObj oSearchObj;
            ESMediaQueryBuilder oQueryParser;
            string sQueryStr;

            if (aliases != null && aliases.Count > 0)
            {

                foreach (int nChannelID in lChannelIds)
                {
                    oChannel = ChannelRepository.GetChannel(nChannelID, oGroup);
                    if (oChannel != null && oChannel.m_nIsActive == 1)
                    {
                        oQueryParser = new ESMediaQueryBuilder() { QueryType = eQueryType.EXACT, m_nGroupID = oChannel.m_nGroupID };
                        oSearchObj = MediaIndexBuilder.BuildBaseChannelSearchObject(oChannel);
                        oQueryParser.oSearchObject = oSearchObj;
                        sQueryStr = oQueryParser.BuildSearchQueryString(false);

                        foreach (string sIndex in aliases)
                        {
                            bRes = m_oESApi.AddQueryToPercolator(sIndex, oChannel.m_nChannelID.ToString(), ref sQueryStr);
                        }
                    }
                }
            }

            return bRes;
        }

        private async Task<bool> UpdateMedias(List<int> lMediaIDs)
        {
            bool bRes = true;
            Group oGroup = GroupsCache.Instance.GetGroup(m_nGroupID);

            if (oGroup == null)
                return false;

            bool bTempRes;
            foreach (int nMediaID in lMediaIDs)
            {
                try
                {
                    //Create Media Object
                    Dictionary<int, Dictionary<int, Media>> dMedias = await IndexBuilders.MediaIndexBuilder.GetGroupMedias(m_nGroupID, nMediaID);

                    if (dMedias != null)
                    {
                        List<ESBulkRequestObj<int>> lBulkObj = new List<ESBulkRequestObj<int>>();

                        if (dMedias.ContainsKey(nMediaID))
                        {
                            foreach (int nLangID in dMedias[nMediaID].Keys)
                            {
                                Media oMedia = dMedias[nMediaID][nLangID];

                                if (oMedia != null)
                                {
                                    string sMediaObj;

                                    sMediaObj = m_oESSerializer.SerializeMediaObject(oMedia);

                                    string sType = Utils.GetTanslationType(MEDIA, oGroup.GetLanguage(nLangID));
                                    if (!string.IsNullOrEmpty(sMediaObj))
                                    {

                                        bTempRes = m_oESApi.InsertRecord(m_nGroupID.ToString(), sType, oMedia.m_nMediaID.ToString(), sMediaObj);
                                        bRes &= bTempRes;
                                        if (!bTempRes)
                                        {
                                            Logger.Logger.Log("Error", string.Format("Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3}", m_nGroupID, sType, oMedia.m_nMediaID, sMediaObj), "ESFeeder");
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Logger.Log("Start update Media", "Exception " + ex.Message, "ESFeeder");
                }
            }

            return bRes;
        }
    }
}

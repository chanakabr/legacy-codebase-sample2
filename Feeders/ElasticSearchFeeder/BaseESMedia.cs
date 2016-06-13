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
using ElasticSearch.Common.DeleteResults;
using Catalog.Cache;
using GroupsCacheManager;
using KLogMonitor;
using System.Reflection;

namespace ElasticSearchFeeder
{
    public class BaseESMedia : ElasticSearchBaseImplementor
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                Catalog.Bootstrapper.Bootstrap();
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
                        log.ErrorFormat("Error- Reloading index failed for group {0}", m_nGroupID);
                        return;
                    }
                }
                //Get message queue
                log.Debug("Info - Initializing rabbit queue. ESFeeder");


                using (RabbitQueueSingleConnection oMessageQueue = new RabbitQueueSingleConnection(m_sQueueName, string.Empty))
                {
                    try
                    {
                        if (oMessageQueue.Start())
                        {
                            log.Debug("Info - Message queue initialized successfully, ESFeeder");

                            string sGroupID = m_nGroupID.ToString();
                            IndexingData oMessage = null;

                            string sAckId;
                            bool bRetVal;
                            log.DebugFormat("Info - Attempting to read messages for queue {0}", m_sQueueName);

                            while ((oMessage = oMessageQueue.Dequeue<IndexingData>(m_sQueueName, out sAckId)) != null)
                            {
                                if (oMessage.Ids != null && oMessage.Ids.Count > 0)
                                {
                                    log.DebugFormat("Info - received message. objectType={0}; group_id={1}; action={2}; ids=[{3}]", oMessage.ObjectType.ToString(), oMessage.GroupId, oMessage.Action.ToString(), string.Join(",", oMessage.Ids));
                                    try
                                    {
                                        bRetVal = false;

                                        switch (oMessage.ObjectType)
                                        {
                                            case ApiObjects.eObjectType.Media:
                                                bRetVal = await MediaChanged(oMessage.Ids.ConvertAll<int>(x => (int)x), oMessage.Action);
                                                break;
                                            case ApiObjects.eObjectType.Channel:
                                                bRetVal = ChannelChanged(oMessage.Ids.ConvertAll<int>(x => (int)x), oMessage.Action);
                                                break;
                                            case ApiObjects.eObjectType.EPG:
                                                bRetVal = EpgChanged(oMessage.Ids.ConvertAll<int>(x => (int)x), oMessage.Action);
                                                break;
                                            default:
                                                bRetVal = true;
                                                break;
                                        }
                                        if (bRetVal && !string.IsNullOrEmpty(sAckId))
                                        {
                                            log.DebugFormat("Info - Message handled successfully. Sending ack to queue {0} ack_id={1}", m_sQueueName, sAckId);
                                            bool bAckSuccess = oMessageQueue.Ack(m_sQueueName, sAckId);
                                            log.DebugFormat("Info - Ack result from queue is {0}, for ack_id={1}", bAckSuccess, sAckId);
                                        }
                                        else
                                        {
                                            log.ErrorFormat("Error - Message handled with errors. asset_id=[{0}]; message ack_id={1}", string.Join(",", oMessage.Ids), sAckId);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        log.ErrorFormat("Error - Caught exception when performing action on message. ex={0}", ex);
                                    }
                                }
                                else
                                {
                                    log.Error("Error - Received message without ids. ESFeeder");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Exception on Dequeue - ex={0}", ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exception - ESFeeder", ex);
            }
        }

        private bool EpgChanged(List<int> lEpgIDs, ApiObjects.eAction eAction)
        {
            bool bRes = false;

            try
            {
                // get all languages per group
                Group oGroup = GroupsCache.Instance().GetGroup(m_nGroupID);

                if (oGroup == null)
                    return false;

                log.DebugFormat("EpgChanged Action:{0}", eAction.ToString());

                if (eAction == ApiObjects.eAction.Delete)
                {
                    string alias = Utils.GetEpgGroupAliasStr(m_nGroupID);
                    int success = 0;
                    foreach (int epgId in lEpgIDs)
                    {
                        var res = m_oESApi.DeleteDoc(alias, EPG, epgId.ToString());
                        if (res.Ok)
                        {
                            success++;
                        }
                    }

                    log.DebugFormat("DeleteDoc Total:{0} success:{1} - {2}%", lEpgIDs.Count, success, (success * 100) / lEpgIDs.Count);

                    return true;
                }

                List<LanguageObj> lLanguage = oGroup.GetLangauges(); // dictionary contains all language ids and its  code (string)
                List<string> languages = lLanguage.Select(p => p.Code.ToLower()).ToList<string>();
                Dictionary<int, List<EpgCB>> dPrograms = new Dictionary<int, List<EpgCB>>();

                Task<List<EpgCB>>[] tPrograms = new Task<List<EpgCB>>[lEpgIDs.Count];

                //open task factory and run GetEpgProgram on different threads
                //wait to finish
                //bulk insert                

                for (int i = 0; i < lEpgIDs.Count; i++)
                {
                    tPrograms[i] = Task.Factory.StartNew<List<EpgCB>>(
                        (index) =>
                        {
                            return Utils.GetEpgProgram(m_nGroupID, (int)index, languages);
                        }, lEpgIDs[i]);
                }

                Task.WaitAll(tPrograms);

                List<EpgCB> lEpg = tPrograms.SelectMany(t => t.Result).Where(t => t != null).ToList();
                // create dictionary by languages                 
                foreach (LanguageObj lang in lLanguage)
                {
                    List<EpgCB> tempEpgs = lEpg.Where(x => x.Language.ToLower() == lang.Code.ToLower() || (lang.IsDefault && string.IsNullOrEmpty(x.Language))).ToList();

                    if (tempEpgs != null && tempEpgs.Count > 0)
                    {

                        List<KeyValuePair<ulong, string>> lKvp = new List<KeyValuePair<ulong, string>>();
                        string sSerializedEpg;
                        foreach (EpgCB epg in tempEpgs)
                        {
                            sSerializedEpg = m_oESSerializer.SerializeEpgObject(epg);
                            lKvp.Add(new KeyValuePair<ulong, string>(epg.EpgID, sSerializedEpg));
                        }
                        string sAlias = Utils.GetEpgGroupAliasStr(m_nGroupID);

                        string sType = Utils.GetTanslationType(EPG, oGroup.GetLanguage(lang.ID));

                        m_oESApi.CreateBulkIndexRequest(sAlias, sType, lKvp);

                        bRes = true;
                    }
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
            bool bRes = true;

            if (lIDs == null || lIDs.Count == 0)
                return bRes;

            string sIndex = m_nGroupID.ToString();
            ESDeleteResult deleteResult;

            if (eObjectType == ApiObjects.eObjectType.Media)
            {
                foreach (int id in lIDs)
                {
                    deleteResult = m_oESApi.DeleteDoc(sIndex, MEDIA, id.ToString());

                    if (!deleteResult.Ok)
                        bRes = false;
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
                            deleteResult = m_oESApi.DeleteDoc("_percolator", index, nChannelID.ToString());

                            if (!deleteResult.Ok)
                                bRes = false;
                        }
                    }
                }
            }

            return bRes;
        }

        private bool UpdateChannel(List<int> lChannelIds)
        {
            bool bRes = false;
            GroupManager groupManager = new GroupManager();
            bool bres = groupManager.RemoveGroup(m_nGroupID);
            Group oGroup = groupManager.GetGroup(m_nGroupID);

            if (oGroup == null)
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
            Group oGroup = GroupsCache.Instance().GetGroup(m_nGroupID);

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
                                            log.ErrorFormat("Error - Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3}", m_nGroupID, sType, oMedia.m_nMediaID, sMediaObj);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error("Start update Media. ESFeeder", ex);
                }
            }

            return bRes;
        }
    }
}

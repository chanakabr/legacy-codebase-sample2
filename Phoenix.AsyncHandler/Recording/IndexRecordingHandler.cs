using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using ApiObjects;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.ConditionalAccess;
using GroupsCacheManager;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.IndexRecording;
using Phx.Lib.Log;

namespace Phoenix.AsyncHandler.Recording
{
    public class IndexRecordingHandler : IHandler<IndexRecording>
    {
        private readonly ILogger<IndexRecordingHandler> _logger;
        private const int CHUNK_EPG_SIZE = 5;
        private IIndexManager indexManager;
        public IndexRecordingHandler(ILogger<IndexRecordingHandler> logger)
        {
            _logger = logger;
        }
        
        public HandleResult Handle(ConsumeResult<string, IndexRecording> consumeResult)
        {
            if (!consumeResult.Result.Message.Value.PartnerId.HasValue)
            {
                _logger.LogError("Wrong event body - must have partner id");
                return Result.Ok;
            }
            
            indexManager = IndexManagerFactory.Instance.GetIndexManager((int)consumeResult.Result.Message.Value.PartnerId.Value);
            if (consumeResult.Result.Message.Value.AssetIds == null || consumeResult.Result.Message.Value.AssetIds.Length == 0)
            {
                _logger.LogDebug("Recordings Id list empty");
                return Result.Ok;
            }
            
            
            Dictionary<long, long> epgToRecordingMapping =  new Dictionary<long, long>();
            HandleResult result = Result.Ok;
            List<long> recordingIds = consumeResult.Result.Message.Value.AssetIds.ToList();
            var programs = Core.Recordings.PaddedRecordingsManager.Instance.GetProgramsByProgramIds((int)consumeResult.Result.Message.Value.PartnerId.Value, recordingIds);

            //DAL.Recordings.RecordingsRepository.Instance.GetProgramsByEpgIds(this.groupId, recordingIds);

            foreach (var program in programs)
            {
                epgToRecordingMapping.Add(program.EpgId, program.Id);
            }
            
            //var recordingIds = IDs.Select(i => (long)i).ToList();
            List<long> epgIds = epgToRecordingMapping.Keys.Select(x => (long)x).ToList();
            ApiObjects.eAction action = (ApiObjects.eAction) Enum.Parse(typeof(ApiObjects.eAction),
                consumeResult.Result.Message.Value.Action);
            // Call to methods in EPG Updater with the EPG IDs we "collected"
            switch (action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                {
                    if (indexManager.DeleteProgram(epgIds.Select(id => id).ToList(), true))
                    {
                        result = Result.Ok;
                    }
                }
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                {
                    if (UpdateEpg((int) consumeResult.Result.Message.Value.PartnerId.Value, epgIds,
                        epgToRecordingMapping))
                    {
                        result = Result.Ok;
                    }
                    break;
                }
                default:
                    result = Result.Ok;
                    break;
            }

            return result;
        }
        
        private bool UpdateEpg(int partnerId, List<long> epgIds, Dictionary<long, long> epgToRecordingMapping)
        {
            bool result = true;
            //result &= Core.Catalog.CatalogManagement.IndexManager.UpsertEpg(groupId, id);
            try
            {
                bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(partnerId);
                CatalogGroupCache catalogGroupCache = null;
                Group group = null;
                if (doesGroupUsesTemplates)
                {
                    if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(partnerId, out catalogGroupCache))
                    {
                        _logger.LogError($"failed to get catalogGroupCache for groupId: {partnerId} when calling UpdateEpg");
                        return false;
                    }
                }
                else
                {
                    group = GroupsCache.Instance().GetGroup(partnerId);
                    if (group == null)
                    {
                        _logger.LogError($"Couldn't get group {partnerId}");
                        return false;
                    }
                }

                // dictionary contains all language ids and its  code (string)
                List<LanguageObj> languages = doesGroupUsesTemplates ? catalogGroupCache.LanguageMapById.Values.ToList() : group.GetLangauges();
                List<string> languageCodes = new List<string>();

                if (languages != null)
                {
                    languageCodes = languages.Select(p => p.Code.ToLower()).ToList<string>();
                }
                else
                {
                    // return false; // perhaps?
                    _logger.LogDebug($"Warning - Group {partnerId} has no languages defined.");
                }

                List<EpgCB> epgObjects = new List<EpgCB>();

                if (epgIds.Count == 1)
                {
                    epgObjects = GetEpgPrograms(partnerId, epgIds[0], languageCodes);
                }
                else
                {
                    //open task factory and run GetEpgProgram on different threads
                    //wait to finish
                    //bulk insert
                    // It's absolutely useless to run 100+ epgs retrieval tasks -> due to context switching we can't retrieve them in reasonable amount of time. 
                    var cd = new LogContextData();
                    var epgBL = EpgBL.Utils.GetInstance(partnerId);
                    foreach (var chunk in BaseConditionalAccess.Chunkify(epgIds, CHUNK_EPG_SIZE))
                    {
                        var tasks = chunk.Select(ch => Task.Run(() =>
                        {
                            cd.Load();
                            return GetEpgPrograms(partnerId, ch, languageCodes, epgBL);
                        })).ToArray();

                        Task.WaitAll(tasks);

                        epgObjects.AddRange(tasks.Where(t => t.Result != null).SelectMany(t => t.Result));
                    }
                }

                if (epgObjects != null)
                {
                    if (epgObjects.Count == 0)
                    {
                        _logger.LogWarning($"Attention - when updating EPG, epg list is empty for IDs = {string.Join(",", epgIds)}");
                        result = true;
                    }
                    else
                    {
                        result = indexManager.UpdateEpgs(epgObjects, true, epgToRecordingMapping);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error - Update EPGs threw an exception. Exception={ex.Message};Stack={ex.StackTrace}", ex);
                return false;
            }

            return result;
        }
        
        private List<EpgCB> GetEpgPrograms(int groupId, long epgId, List<string> languages, EpgBL.BaseEpgBL epgBL = null)
        {
            List<EpgCB> results = new List<EpgCB>();

            // If no language was received - just get epg program by old method
            if (languages == null || languages.Count == 0)
            {
                EpgCB program = GetEpgProgram(groupId, epgId);

                results.Add(program);
            }
            else
            {
                try
                {
                    if (epgBL == null)
                    {
                        epgBL = EpgBL.Utils.GetInstance(groupId);
                    }

                    ulong uEpgID = (ulong)epgId;
                    results = epgBL.GetEpgCB(uEpgID, languages);
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error (GetEpgProgram) - epg:{epgId}, msg:{ex.Message}, st:{ex.StackTrace}", ex);
                }
            }

            return results;
        }
        
        public static EpgCB GetEpgProgram(int nGroupID, long nEpgID)
        {
            EpgCB res = null;

            DataSet ds = Tvinci.Core.DAL.EpgDal.GetEpgProgramDetails(nGroupID, nEpgID);

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
                        epg.IsActive = (ODBCWrapper.Utils.GetIntSafeVal(row["IS_ACTIVE"]) == 1) ? true : false;
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
                        epg.Crid = ODBCWrapper.Utils.GetSafeStr(row["crid"]);

                        //Metas
                        if (ds.Tables.Count >= 3 && ds.Tables[2] != null && ds.Tables[2].Rows != null && ds.Tables[2].Rows.Count > 0)
                        {
                            List<string> tempList;

                            foreach (DataRow meta in ds.Tables[2].Rows)
                            {
                                string metaName = ODBCWrapper.Utils.GetSafeStr(meta["name"]);
                                string metaValue = ODBCWrapper.Utils.GetSafeStr(meta["value"]);

                                if (epg.Metas.TryGetValue(metaName, out tempList))
                                {
                                    tempList.Add(metaValue);
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
                            foreach (DataRow tag in ds.Tables[3].Rows)
                            {
                                string tagName = ODBCWrapper.Utils.GetSafeStr(tag["TagTypeName"]);
                                string tagValue = ODBCWrapper.Utils.GetSafeStr(tag["TagValueName"]);
                                if (epg.Tags.TryGetValue(tagName, out tempList))
                                {
                                    tempList.Add(tagValue);
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
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiObjects;
using AutoMapper;
using AutoMapper.Configuration;
using Core.Catalog;
using EpgBL;
using Microsoft.Extensions.Logging;
using OTT.Lib.Kafka;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.Generated.Api.Events.Logical.RebuildRecordingsIndex;
using Phx.Lib.Appconfig;
using TVinciShared;

namespace Phoenix.AsyncHandler.Recording
{
    public class RebuildRecordingsIndexHandler : IHandler<RebuildRecordingsIndex>
    {
        private readonly ILogger<RebuildRecordingsIndex > _logger;
        private IIndexManager indexManager;
        protected Dictionary<long, long> epgToRecordingMapping = null;
        protected Dictionary<long, List<int>> linearChannelsRegionsMapping;
        protected int epgCbBulkSize = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.EpgPageSize.Value;

        public RebuildRecordingsIndexHandler(ILogger<RebuildRecordingsIndex> logger)
        {
            _logger = logger;
        }
        
        public HandleResult Handle(ConsumeResult<string, RebuildRecordingsIndex> consumeResult)
        {
            if (!consumeResult.Result.Message.Value.PartnerId.HasValue || !consumeResult.Result.Message.Value.SwitchIndexAlias.HasValue || !consumeResult.Result.Message.Value.DeleteOldIndices.HasValue)
            {
                _logger.LogError("Wrong event body - must have partner id, switchIndexAlias and deleteOldIndices");
                return Result.Ok;
            }
            
            int partnerId = (int)consumeResult.Result.Message.Value.PartnerId.Value;
            List<LanguageObj> languages = AutoMapper.Mapper.Map<List<LanguageObj>>(consumeResult.Result.Message.Value.Languages);
            linearChannelsRegionsMapping = RegionManager.Instance.GetLinearMediaRegions(partnerId);
            indexManager = IndexManagerFactory.Instance.GetIndexManager(partnerId);
            
            string newIndexName = consumeResult.Result.Message.Value.NewIndexName;

            if (newIndexName == null || newIndexName.IsEmpty())
            {
                newIndexName = indexManager.SetupEpgIndex(DateTime.UtcNow, true);
            }

            PopulateIndexPersonalizedRecording(newIndexName, languages, partnerId);
            indexManager.PublishEpgIndex(newIndexName, true, consumeResult.Result.Message.Value.SwitchIndexAlias.Value, consumeResult.Result.Message.Value.DeleteOldIndices.Value);
            
            return Result.Ok;
        }
        
        private void PopulateIndexPersonalizedRecording(string newIndexName, List<LanguageObj> languages, int groupId, int skip = 0)
        {
            _logger.LogDebug($"PopulateIndexPersonalizedRecording skip:{skip}");

            // Get information about relevant recordings
            epgToRecordingMapping = null;
            var programs = DAL.Recordings.RecordingsRepository.Instance.GetAllRecordedPrograms(groupId, 500, skip);

            if (programs?.Count > 0)
            {
                skip += programs.Count;

                epgToRecordingMapping = programs.ToDictionary(x => x.EpgId, x => x.Id);

                PopulateProgramsIndex(newIndexName, languages, groupId);

                PopulateIndexPersonalizedRecording(newIndexName, languages, groupId, skip);
            }
        }
        
        private void PopulateProgramsIndex(string newIndexName, List<LanguageObj> languages, int groupId)
        {
            List<string> epgIds = new List<string>();

            List<EpgCB> epgs = new List<EpgCB>();
            EpgBL.TvinciEpgBL epgBL = new TvinciEpgBL(groupId);

            foreach (var programId in epgToRecordingMapping.Keys)
            {
                // for main language
                epgIds.Add(programId.ToString());

                //Build list of keys with language
                foreach (var language in languages)
                {
                    string docID = string.Format("epg_{0}_lang_{1}", programId, language.Code.ToLower());
                    epgIds.Add(docID);
                }

                // Work in bulks so we don't chocke the Couchbase. every time get only a bulk of EPGs
                if (epgIds.Count >= epgCbBulkSize)
                {
                    // Get EPG objects
                    epgs.AddRange(epgBL.GetEpgs(epgIds, true));
                    epgIds.Clear();
                }
            }

            // Finish off what's left to get from CB
            if (epgIds.Count >= 0)
            {
                epgs.AddRange(epgBL.GetEpgs(epgIds, true));
            }

            Dictionary<ulong, Dictionary<string, EpgCB>> epgDictionary = BuildEpgsLanguageDictionary(epgs);

            indexManager.AddEPGsToIndex(newIndexName, true, epgDictionary, linearChannelsRegionsMapping, epgToRecordingMapping);
        }
        
        protected Dictionary<ulong, Dictionary<string, EpgCB>> BuildEpgsLanguageDictionary(List<EpgCB> epgs)
        {
            Dictionary<ulong, Dictionary<string, EpgCB>> epgDictionary = new Dictionary<ulong, Dictionary<string, EpgCB>>();

            foreach (var epg in epgs)
            {
                if (epg != null)
                {
                    if (!epgDictionary.ContainsKey(epg.EpgID))
                    {
                        epgDictionary.Add(epg.EpgID, new Dictionary<string, EpgCB>());
                    }
                    if (!epgDictionary[epg.EpgID].ContainsKey(epg.Language))
                    {
                        epgDictionary[epg.EpgID].Add(epg.Language, epg);
                    }
                }
                else
                {
                    _logger.LogError("Received null epg from TvinciEpgBL");
                }
            }

            return epgDictionary;
        }
    }
}
using ApiObjects;
using ApiObjects.TimeShiftedTv;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using EpgBL;
using Core.Catalog.CatalogManagement;
using Core.Catalog;
using GroupsCacheManager;

namespace ElasticSearchHandler.IndexBuilders
{
    public class RecordingIndexBuilderV2 : EpgIndexBuilderV2
    {
        #region Data Members

        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        protected Dictionary<long, long> epgToRecordingMapping = null;

        #endregion

        #region Ctor

        public RecordingIndexBuilderV2(int groupId)
            : base(groupId)
        {
            epgToRecordingMapping = new Dictionary<long, long>();
            shouldAddRouting = false;
        }

        #endregion

        #region Override Methods

        protected override string CreateNewIndex(int groupId, CatalogGroupCache catalogGroupCache, Group group, IEnumerable<LanguageObj> languages, LanguageObj defaultLanguage)
        {
            return _IndexManager.SetupEpgIndex(DateTime.UtcNow, isRecording: true);
        }

        protected override void PopulateIndex(string newIndexName, GroupsCacheManager.Group group)
        {
            bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            List<LanguageObj> languages = new List<LanguageObj>();
            if (doesGroupUsesTemplates)
            {
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out CatalogGroupCache catalogGroupCache))
                {
                    log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling PopulateEpgIndex", groupId);
                    return;
                }

                languages = catalogGroupCache.LanguageMapByCode.Values.ToList();
            }
            else
            {
                languages = group.GetLangauges();
            }

            var tstvs = Core.ConditionalAccess.Utils.GetTimeShiftedTvPartnerSettings(groupId);
            if (tstvs.PersonalizedRecordingEnable == true)
            {
                PopulateIndexPersonalizedRecording(newIndexName, languages);
            }
            else
            {
                PopulateIndexPaging(newIndexName, languages);
            }
        }

        private void PopulateIndexPaging(string newIndexName, List<LanguageObj> languages, long minId = 0)
        {
            log.Debug($"PopulateIndexPaging maxId:{minId}");

            long maxId = minId;
            List<int> statuses = new List<int>() { (int)RecordingInternalStatus.OK, (int)RecordingInternalStatus.Waiting,
            (int)RecordingInternalStatus.Canceled, (int)RecordingInternalStatus.Failed};

            // Get information about relevant recordings
            epgToRecordingMapping = DAL.RecordingsDAL.GetEpgToRecordingsMapByRecordingStatuses(this.groupId, statuses, minId);
            
            if (epgToRecordingMapping?.Count > 0)
            {
                PopulateProgramsIndex(newIndexName, languages);

                maxId = epgToRecordingMapping.Values.Max();

                if (maxId > minId)
                {
                    PopulateIndexPaging(newIndexName, languages, maxId);
                }
            }
        }

        private void PopulateIndexPersonalizedRecording(string newIndexName, List<LanguageObj> languages, int skip = 0)
        {
            log.Debug($"PopulateIndexPersonalizedRecording skip:{skip}");

            // Get information about relevant recordings
            epgToRecordingMapping = null;
            var programs = DAL.Recordings.RecordingsRepository.Instance.GetAllRecordedPrograms(this.groupId, 500, skip);

            if (programs?.Count > 0)
            {
                skip += programs.Count;

                epgToRecordingMapping = programs.ToDictionary(x => x.EpgId, x => x.Id);

                PopulateProgramsIndex(newIndexName, languages);

                PopulateIndexPersonalizedRecording(newIndexName, languages, skip);
            }
        }

        private void PopulateProgramsIndex(string newIndexName, List<LanguageObj> languages)
        {
            List<string> epgIds = new List<string>();

            List<EpgCB> epgs = new List<EpgCB>();
            EpgBL.TvinciEpgBL epgBL = new TvinciEpgBL(this.groupId);

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

            _IndexManager.AddEPGsToIndex(newIndexName, true, epgDictionary, linearChannelsRegionsMapping, epgToRecordingMapping);
        }

        protected override bool FinishUpEpgIndex(string newIndexName)
        {
            return _IndexManager.PublishEpgIndex(newIndexName, isRecording: true, this.SwitchIndexAlias, this.DeleteOldIndices);
        }

        #endregion
    }
}

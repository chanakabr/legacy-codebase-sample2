using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.SearchObjects;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using GroupsCacheManager;
using Phx.Lib.Log;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ElasticSearchHandler.IndexBuilders
{
    public class EpgIndexBuilderV2 : AbstractIndexBuilder
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members

        protected int epgCbBulkSize = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.EpgPageSize.Value;

        protected bool shouldAddRouting = true;
        protected Dictionary<long, List<int>> linearChannelsRegionsMapping;

        #endregion

        #region Ctor

        public EpgIndexBuilderV2(int groupID)
            : base(groupID)
        {
            linearChannelsRegionsMapping = new Dictionary<long, List<int>>();
        }

        #endregion

        public override bool BuildIndex()
        {
            LogContextData cd = new LogContextData();
            CatalogGroupCache catalogGroupCache;
            Group group;
            List<LanguageObj> languages;
            GroupManager groupManager;
            bool doesGroupUsesTemplates;
            LanguageObj defaultLanguage;
            try
            {
                GetGroupAndLanguages(out catalogGroupCache, out group, out languages, out groupManager, out doesGroupUsesTemplates, out defaultLanguage);
            }
            catch (Exception e)
            {
                log.Error("Erorr while getting groups and languages", e);
                return false;
            }

            // If request doesn't have start date, use [NOW - 7 days] as default
            if (!this.StartDate.HasValue)
            {
                this.StartDate = DateTime.UtcNow.Date.AddDays(-7);
            }

            // If request doesn't have end date, use [NOW + 7 days] as default
            if (!this.EndDate.HasValue)
            {
                this.EndDate = DateTime.UtcNow.Date.AddDays(7);
            }

            // Default size of epg cb bulk size
            epgCbBulkSize = epgCbBulkSize == 0 ? 1000 : epgCbBulkSize;

            string newIndexName = string.Empty;

            try
            {
                newIndexName = CreateNewIndex(groupId, catalogGroupCache, group, languages, defaultLanguage);
            }
            catch (Exception e)
            {
                log.Error("Error while building new index", e);
                return false;
            }

            log.DebugFormat("Start populating epg index = {0}", newIndexName);

            #region Get Linear Channels Regions

            linearChannelsRegionsMapping = RegionManager.Instance.GetLinearMediaRegions(groupId);

            #endregion

            PopulateIndex(newIndexName, group);

            #region Switch Index

            log.DebugFormat("Finished populating epg index = {0}", newIndexName);
            bool result = FinishUpEpgIndex(newIndexName);

            #endregion

            return result;
        }

        protected virtual bool FinishUpEpgIndex(string newIndexName)
        {
            return _IndexManager.PublishEpgIndex(newIndexName, isRecording: false, this.SwitchIndexAlias, this.DeleteOldIndices);
        }

        private void GetGroupAndLanguages(out CatalogGroupCache catalogGroupCache, out Group group, out List<LanguageObj> languages, out GroupManager groupManager, out bool doesGroupUsesTemplates, out LanguageObj defaultLanguage)
        {
            catalogGroupCache = null;
            group = null;
            languages = null;
            groupManager = new GroupManager();
            doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
            defaultLanguage = null;

            if (doesGroupUsesTemplates)
            {
                // TODO: verify that we need or not to invalidate the group cache before we get the group to get the latest
                if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                {
                    throw new Exception($"failed to get catalogGroupCache for groupId: {groupId} when calling BuildIndex");
                }

                languages = catalogGroupCache.LanguageMapById.Values.ToList();
                defaultLanguage = catalogGroupCache.GetDefaultLanguage();
            }
            else
            {
                groupManager.RemoveGroup(groupId);
                group = groupManager.GetGroup(groupId);

                if (group == null)
                {
                    throw new Exception($"failed to get group for groupId: {groupId} when calling BuildIndex");
                }

                languages = group.GetLangauges();
                defaultLanguage = group.GetGroupDefaultLanguage();
            }
        }


        #region Private and protected Methods

        protected virtual void PopulateIndex(string newIndexName, Group group)
        {
            DateTime tempDate = StartDate.Value;

            while (tempDate <= this.EndDate.Value)
            {
                PopulateEpgIndex(newIndexName, tempDate, group);
                tempDate = tempDate.AddDays(1);
            }
        }

        protected virtual string CreateNewIndex(int groupId, CatalogGroupCache catalogGroupCache, Group group, IEnumerable<LanguageObj> languages, LanguageObj defaultLanguage)
        {
            return _IndexManager.SetupEpgIndex(DateTime.UtcNow, false);
        }

        protected void PopulateEpgIndex(string index, DateTime date, Group group)
        {
            try
            {
                bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
                CatalogGroupCache catalogGroupCache = null;
                Dictionary<ulong, Dictionary<string, EpgCB>> programs = new Dictionary<ulong, Dictionary<string, EpgCB>>();
                if (doesGroupUsesTemplates)
                {
                    if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling PopulateEpgIndex", groupId);
                        return;
                    }          
                }

                // Get EPG objects from CB
                programs = GetEpgPrograms(groupId, date, epgCbBulkSize);

                if (programs != null && programs.Count > 0)
                {
                    log.DebugFormat($"found {programs.Count} epgs for day {date}");
                    _IndexManager.AddEPGsToIndex(index, false, programs,
                        linearChannelsRegionsMapping, null);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Failed when populating epg index. index = {0}, isRecording = {1}, date = {2}, message = {3}, st = {4}",
                    index, false, date, ex.Message, ex.StackTrace);

                throw ex;
            }
        }

        #endregion
    }
}

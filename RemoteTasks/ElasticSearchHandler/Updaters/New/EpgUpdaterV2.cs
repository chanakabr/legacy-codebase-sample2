using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using Phx.Lib.Appconfig;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using GroupsCacheManager;
using Phx.Lib.Log;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.SearchObjects;
using Core.Api;
using Core.Catalog.Response;
using Core.ConditionalAccess;
using Newtonsoft.Json;

namespace ElasticSearchHandler.Updaters
{
    public class EpgUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string EPG = "epg";

        #region Data Members

        protected readonly int groupId;
        protected EpgBL.BaseEpgBL epgBL;
        protected readonly int sizeOfBulk;
        protected readonly IIndexManager _indexManager;
        private List<long> _identifiers;
        private const int CHUNK_EPG_SIZE = 5;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }

        private List<long> Identifiers
        {
            get
            {
                _identifiers = _identifiers ?? IDs.Select(x => (long)x).ToList();
                return _identifiers;
            }
        }

        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public EpgUpdaterV2(int groupId)
        {
            this.groupId = groupId;
            _indexManager = IndexManagerFactory.Instance.GetIndexManager(groupId);

            epgBL = EpgBL.Utils.GetInstance(this.groupId);

            sizeOfBulk = ApplicationConfiguration.Current.ElasticSearchHandlerConfiguration.BulkSize.Value;

            if (sizeOfBulk == 0)
            {
                sizeOfBulk = 500;
            }

        }

        #endregion

        #region Interface methods

        public virtual bool Start()
        {
            bool result = false;
            log.Debug("Info - Start EPG update");
            if (IDs == null || IDs.Count == 0)
            {
                log.Debug("Info - EPG Id list empty");
                result = true;

                return result;
            }

            switch (Action)
            {
                case ApiObjects.eAction.Off:
                case ApiObjects.eAction.Delete:
                    result = DeleteEpg(Identifiers);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    {
                        // First we delete so we don't get this weird duplicate ID bug.
                        result = DeleteEpg(Identifiers);

                        // Only then we update normally
                        result &= UpdateEpg(Identifiers, UpdateEpgs);
                        break;
                    }
                case eAction.EpgRegionUpdate:
                {
                    log.Debug($"Start {nameof(eAction.EpgRegionUpdate)}.");
                    result = true;
                    foreach (var linearMediaId in IDs)
                    {
                        var epgIds = GetEpgIds(linearMediaId);
                        log.Debug($"Took {epgIds.Count} to update.");
                        if (epgIds.Count > 0)
                        {
                            result &= UpdateEpg(epgIds, epgCBs => UpdateEpgRegionsPartial(epgCBs, linearMediaId));
                        }
                    }

                    break;
                }
                default:
                    result = true;
                    break;
            }

            return result;
        }

        #endregion

        protected bool UpdateEpg(List<long> epgIds, Func<List<EpgCB>, bool> epgsUpdater)
        {
            bool result = true;
            //result &= Core.Catalog.CatalogManagement.IndexManager.UpsertEpg(groupId, id);
            try
            {
                bool doesGroupUsesTemplates = CatalogManager.Instance.DoesGroupUsesTemplates(groupId);
                CatalogGroupCache catalogGroupCache = null;
                Group group = null;
                if (doesGroupUsesTemplates)
                {
                    if (!CatalogManager.Instance.TryGetCatalogGroupCacheFromCache(groupId, out catalogGroupCache))
                    {
                        log.ErrorFormat("failed to get catalogGroupCache for groupId: {0} when calling UpdateEpg", groupId);
                        return false;
                    }
                }
                else
                {
                    group = GroupsCache.Instance().GetGroup(this.groupId);
                    if (group == null)
                    {
                        log.ErrorFormat("Couldn't get group {0}", this.groupId);
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
                    log.Debug("Warning - " + string.Format("Group {0} has no languages defined.", groupId));
                }

                List<EpgCB> epgObjects = new List<EpgCB>();

                if (epgIds.Count == 1)
                {
                    epgObjects = ElasticsearchTasksCommon.Utils.GetEpgPrograms(groupId, epgIds[0], languageCodes);
                }
                else
                {
                    //open task factory and run GetEpgProgram on different threads
                    //wait to finish
                    //bulk insert
                    // It's absolutely useless to run 100+ epgs retrieval tasks -> due to context switching we can't retrieve them in reasonable amount of time. 
                    var cd = new LogContextData();
                    var epgBL = EpgBL.Utils.GetInstance(groupId);
                    foreach (var chunk in BaseConditionalAccess.Chunkify(epgIds, CHUNK_EPG_SIZE))
                    {
                        var tasks = chunk.Select(ch => Task.Run(() =>
                        {
                            cd.Load();
                            return ElasticsearchTasksCommon.Utils.GetEpgPrograms(groupId, ch, languageCodes, epgBL);
                        })).ToArray();

                        Task.WaitAll(tasks);

                        epgObjects.AddRange(tasks.Where(t => t.Result != null).SelectMany(t => t.Result));
                    }
                }

                if (epgObjects != null)
                {
                    if (epgObjects.Count == 0)
                    {
                        log.WarnFormat("Attention - when updating EPG, epg list is empty for IDs = {0}",
                            string.Join(",", epgIds));
                        result = true;
                    }
                    else
                    {
                        result = epgsUpdater(epgObjects);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update EPGs threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }

            return result;
        }

        protected bool DeleteEpg(List<long> epgIDs, bool isRecording = false)
        {
            return _indexManager.DeleteProgram(epgIDs.Select(id => id).ToList(), isRecording);
            //return result;
        }

        protected virtual bool UpdateEpgs(List<EpgCB> epgObjects)
        {
            return _indexManager.UpdateEpgs(epgObjects, false);
        }
        
        private List<long> GetEpgIds(int linearMediaId)
        {
            var epgKsql = $"(and linear_media_id='{linearMediaId}')";
            var epgResult = api.SearchAssets(groupId, epgKsql, 0, 0, true, 0, false, string.Empty, string.Empty, string.Empty, 0, 0, true, true);
            return epgResult.Select(x => long.Parse(x.AssetId)).ToList();
        }

        private bool UpdateEpgRegionsPartial(IEnumerable<EpgCB> epgs, int linearMediaId)
        {
            var linearMediaRegions = RegionManager.Instance.GetLinearMediaRegions(groupId);
            if (linearMediaRegions != null && linearMediaRegions.ContainsKey(linearMediaId))
            {
                var epgPartialUpdateEsObjects = epgs.Select(e => new EpgPartialUpdate
                    {
                        EpgId = e.EpgID,
                        DocumentId = e.DocumentId,
                        Language = e.Language,
                        StartDate = e.StartDate,
                        EpgPartial = new EpgPartial
                        {
                            Regions = linearMediaRegions[linearMediaId].ToArray()
                        }
                    }).ToArray();

                return _indexManager.UpdateEpgsPartial(epgPartialUpdateEsObjects);
            }

            return true;
        }
    }
}

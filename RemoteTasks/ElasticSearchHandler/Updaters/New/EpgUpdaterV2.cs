using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Catalog;
using ConfigurationManager;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.GroupManagers;
using GroupsCacheManager;
using KLogMonitor;
using KlogMonitorHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using ApiObjects.SearchObjects;

namespace ElasticSearchHandler.Updaters
{
    public class EpgUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string EPG = "epg";

        #region Data Members

        protected int groupId;
        protected EpgBL.BaseEpgBL epgBL;
        protected int sizeOfBulk;
        protected IIndexManager _indexManager;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public EpgUpdaterV2(int groupId)
        {
            this.groupId = groupId;
            _indexManager = IndexManagerFactory.GetInstance(groupId);

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
                    result = DeleteEpg(IDs);
                    break;
                case ApiObjects.eAction.On:
                case ApiObjects.eAction.Update:
                    {
                        // First we delete so we don't get this weird duplicate ID bug.
                        result = DeleteEpg(IDs);

                        // Only then we update normally
                        result &= UpdateEpg(IDs);
                        break;
                    }
                default:
                    result = true;
                    break;
            }

            return result;
        }

        #endregion

        protected bool UpdateEpg(List<int> epgIds)
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
                    Task<List<EpgCB>>[] programsTasks = new Task<List<EpgCB>>[epgIds.Count];
                    ContextData cd = new ContextData();

                    //open task factory and run GetEpgProgram on different threads
                    //wait to finish
                    //bulk insert
                    for (int i = 0; i < epgIds.Count; i++)
                    {
                        programsTasks[i] = Task.Run<List<EpgCB>>(() =>
                        {
                            cd.Load();
                            return ElasticsearchTasksCommon.Utils.GetEpgPrograms(groupId, epgIds[i], languageCodes);
                        });
                    }

                    Task.WaitAll(programsTasks);

                    epgObjects = programsTasks.SelectMany(t => t.Result).Where(t => t != null).ToList();
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
                        result = this.UpdateEpgs(epgObjects);
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

        protected bool DeleteEpg(List<int> epgIDs)
        {
            return _indexManager.DeleteProgram(epgIDs.Select(id => (long)id).ToList(), null);
            //return result;
        }

        protected virtual bool UpdateEpgs(List<EpgCB> epgObjects)
        {
            return _indexManager.UpdateEpgs(epgObjects, false);
        }
    }
}

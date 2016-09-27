using ApiObjects;
using ElasticSearch.Common;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KLogMonitor;
using System.Reflection;
using Catalog;
using Catalog.Cache;
using ElasticSearch.Searcher;
using KlogMonitorHelper;

namespace ElasticSearchHandler.Updaters
{
    public class EpgUpdaterV2 : IElasticSearchUpdater
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly string EPG = "epg";
        public static readonly int DAYS = 30;

        #region Data Members

        protected int groupId;
        protected ElasticSearch.Common.ESSerializerV2 esSerializer;
        protected ElasticSearch.Common.ElasticSearchApi esApi;
        protected EpgBL.BaseEpgBL epgBL;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        public string ElasticSearchUrl
        {
            get
            {
                if (esApi != null)
                {
                    return esApi.baseUrl;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if (esApi != null)
                {
                    esApi.baseUrl = value;
                }
            }
        }

        #endregion

        #region Ctors

        public EpgUpdaterV2(int groupId)
        {
            this.groupId = groupId;
            esSerializer = new ElasticSearch.Common.ESSerializerV2();
            esApi = new ElasticSearch.Common.ElasticSearchApi();

            epgBL = EpgBL.Utils.GetInstance(this.groupId);
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

            if (!esApi.IndexExists(GetAlias()))
            {
                log.Error("Error - " + string.Format("Index of type EPG for group {0} does not exist", groupId));
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

        #endregion

        protected bool UpdateEpg(List<int> epgIds)
        {
            bool result = false;

            try
            {
                // get all languages per group
                Group group = GroupsCache.Instance().GetGroup(this.groupId);

                if (group == null)
                {
                    log.ErrorFormat("Couldn't get group {0}", this.groupId);
                    return false;
                }

                // dictionary contains all language ids and its  code (string)
                List<LanguageObj> languages = group.GetLangauges();
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

                Task<List<EpgCB>>[] programsTasks = new Task<List<EpgCB>>[epgIds.Count];
                ContextData cd = new ContextData();

                //open task factory and run GetEpgProgram on different threads
                //wait to finish
                //bulk insert
                for (int i = 0; i < epgIds.Count; i++)
                {
                    programsTasks[i] = Task.Factory.StartNew<List<EpgCB>>(
                        (epgId) =>
                        {
                            cd.Load();
                            return ElasticsearchTasksCommon.Utils.GetEpgPrograms(groupId, (int)epgId, languageCodes, epgBL);
                        }, epgIds[i]);
                }

                Task.WaitAll(programsTasks);

                List<EpgCB> epgObjects = programsTasks.SelectMany(t => t.Result).Where(t => t != null).ToList();

                // GetLinear Channel Values 
                ElasticSearchTaskUtils.GetLinearChannelValues(epgObjects, this.groupId);

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
                        // Temporarily - assume success
                        bool temporaryResult = true;

                        // Create dictionary by languages
                        foreach (LanguageObj language in languages)
                        {
                            // Filter programs to current language
                            List<EpgCB> currentLanguageEpgs = epgObjects.Where(epg =>
                                epg.Language.ToLower() == language.Code.ToLower() || (language.IsDefault && string.IsNullOrEmpty(epg.Language))).ToList();

                            if (currentLanguageEpgs != null && currentLanguageEpgs.Count > 0)
                            {
                                List<ESBulkRequestObj<ulong>> bulkRequests = new List<ESBulkRequestObj<ulong>>();
                                string alias = GetAlias();

                                // Create bulk request object for each program
                                foreach (EpgCB epg in currentLanguageEpgs)
                                {
                                    string suffix = null;

                                    if (!language.IsDefault)
                                    {
                                        suffix = language.Code;
                                    }

                                    string serializedEpg = SerializeEPG(epg, suffix);
                                    bulkRequests.Add(new ESBulkRequestObj<ulong>()
                                    {
                                        docID = GetDocumentId(epg),
                                        index = alias,
                                        type = ElasticSearchTaskUtils.GetTanslationType(GetDocumentType(), language),
                                        Operation = eOperation.index,
                                        document = serializedEpg,
                                        routing = epg.StartDate.ToUniversalTime().ToString("yyyyMMdd"),
                                    });
                                }

                                // send request to ES API
                                var invalidResults = esApi.CreateBulkRequest(bulkRequests);

                                if (invalidResults != null && invalidResults.Count > 0)
                                {
                                    foreach (var invalidResult in invalidResults)
                                    {
                                        log.Error("Error - " + string.Format(
                                            "Could not update EPG in ES. GroupID={0};Type={1};EPG_ID={2};error={3};",
                                            groupId, EPG, invalidResult.Key, invalidResult.Value));
                                    }

                                    result = false;
                                    temporaryResult = false;
                                }
                                else
                                {
                                    temporaryResult &= true;
                                }
                            }
                        }

                        result = temporaryResult;
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

        protected virtual ulong GetDocumentId(EpgCB epg)
        {
            return epg.EpgID;
        }

        protected virtual string GetDocumentType()
        {
            return EPG;
        }

        protected virtual ulong GetDocumentId(int epgId)
        {
            return (ulong)epgId;
        }

        protected virtual string SerializeEPG(EpgCB epg, string suffix = null)
        {
            return esSerializer.SerializeEpgObject(epg, suffix);
        }

        protected bool DeleteEpg(List<int> epgIDs)
        {
            bool result = false;

            if (epgIDs != null & epgIDs.Count > 0)
            {
                // get all languages per group
                Group group = GroupsCache.Instance().GetGroup(this.groupId);

                if (group == null)
                {
                    log.ErrorFormat("Couldn't get group {0}", this.groupId);
                    return false;
                }

                // dictionary contains all language ids and its  code (string)
                List<LanguageObj> languages = group.GetLangauges();

                string alias = GetAlias();

                ESTerms terms = new ESTerms(true)
                {
                    Key = "epg_id"
                };

                terms.Value.AddRange(epgIDs.Select(id => id.ToString()));

                ESQuery query = new ESQuery(terms);
                string queryString = query.ToString();

                foreach (var lang in languages)
                {
                    string type = ElasticSearchTaskUtils.GetTanslationType(GetDocumentType(), lang);
                    esApi.DeleteDocsByQuery(alias, type, ref queryString);
                }
                
                result = true;
            }

            return result;
        }

        protected virtual string GetAlias()
        {
            return ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(groupId);
        }
    }
}

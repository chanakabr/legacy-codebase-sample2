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

namespace ESIndexUpdateHandler.Updaters
{
    public class EpgUpdater : IUpdateable
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public static readonly string EPG = "epg";


        #region Data Members

        private int groupId;
        private ElasticSearch.Common.ESSerializer esSerializer;
        private ElasticSearch.Common.ElasticSearchApi esApi;

        #endregion

        #region Properties

        public List<int> IDs { get; set; }
        public ApiObjects.eAction Action { get; set; }

        #endregion

        #region Ctors

        public EpgUpdater(int groupId)
        {
            this.groupId = groupId;
            esSerializer = new ElasticSearch.Common.ESSerializer();
            esApi = new ElasticSearch.Common.ElasticSearchApi();
        }

        #endregion

        #region Interface methods

        public bool Start()
        {
            bool result = false;
            log.Debug("Info - Start EPG update");
            if (IDs == null || IDs.Count == 0)
            {
                log.Debug("Info - EPG Id list empty");
                result = true;

                return result;
            }

            if (!esApi.IndexExists(ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(groupId)))
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

        #region Private Methods

        private bool UpdateEpg(List<int> epgIds)
        {
            bool result = false;

            try
            {
                // get all languages per group
                Group group = GroupsCache.Instance().GetGroup(this.groupId);

                if (group == null)
                {
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

                //open task factory and run GetEpgProgram on different threads
                //wait to finish
                //bulk insert
                for (int i = 0; i < epgIds.Count; i++)
                {
                    programsTasks[i] = Task.Factory.StartNew<List<EpgCB>>(
                        (epgId) =>
                        {
                            return ElasticsearchTasksCommon.Utils.GetEpgPrograms(groupId, (int)epgId, languageCodes);
                        }, epgIds[i]);
                }

                Task.WaitAll(programsTasks);

                List<EpgCB> epgObjects = programsTasks.SelectMany(t => t.Result).Where(t => t != null).ToList();

                

                if (epgObjects != null & epgObjects.Count > 0)
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
                            string alias = ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(groupId);

                            // Create bulk request object for each program
                            foreach (EpgCB epg in epgObjects)
                            {
                                string serializedEpg = esSerializer.SerializeEpgObject(epg);
                                bulkRequests.Add(new ESBulkRequestObj<ulong>()
                                {
                                    docID = epg.EpgID,
                                    index = alias,
                                    type = EPG,
                                    Operation = eOperation.index,
                                    document = serializedEpg
                                });
                            }

                            // send request to ES API
                            var invalidResults = esApi.CreateBulkIndexRequest(bulkRequests);

                            if (invalidResults != null && invalidResults.Count > 0)
                            {
                                foreach (var invalidResult in invalidResults)
                                {
                                    log.Error("Error - " + string.Format("Could not update media in ES. GroupID={0};Type={1};MediaID={2};serializedObj={3};", groupId, EPG, invalidResult.docID, invalidResult.document));
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
            catch (Exception ex)
            {
                log.Error("Error - " + string.Format("Update EPGs threw an exception. Exception={0};Stack={1}", ex.Message, ex.StackTrace), ex);
                throw ex;
            }

            return result;
        }

        private bool DeleteEpg(List<int> epgIDs)
        {
            bool result = false;

            if (epgIDs != null & epgIDs.Count > 0)
            {
                List<ESBulkRequestObj<int>> bulkRequests = new List<ESBulkRequestObj<int>>();
                string alias = ElasticsearchTasksCommon.Utils.GetEpgGroupAliasStr(groupId);

                foreach (int epgId in epgIDs)
                {
                    bulkRequests.Add(new ESBulkRequestObj<int>()
                    {
                        docID = epgId,
                        index = alias,
                        type = EPG,
                        Operation = eOperation.delete
                    });
                }

                esApi.CreateBulkIndexRequest(bulkRequests);

                result = true;
            }

            return result;
        }


        #endregion

    }
}

using Phx.Lib.Appconfig;
using Core.GroupManagers;
using Phx.Lib.Log;
using Newtonsoft.Json;
using RemoteTasksCommon;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using ApiObjects;
using Core.Api;

namespace ElasticSearchHandler
{
    public class TaskHandler : ITaskHandler
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string HandleTask(string data)
        {
            string res = "failure";

            try
            {
                log.DebugFormat("starting elasticsearch request. data={0}", data);

                ElasticSearchRequest request = JsonConvert.DeserializeObject<ElasticSearchRequest>(data);

                // If the request is for a rebuild:
                if (request.Action == ApiObjects.eAction.Rebuild)
                {
                    #region Rebuild
                    var epgFeatureVersion = GroupSettingsManager.Instance.GetEpgFeatureVersion(request.GroupID);
                    if (request.Type == ApiObjects.eObjectType.EPG && epgFeatureVersion != EpgFeatureVersion.V1)
                    {
                        throw new Exception($"Rebuild index is required / supported for EPG Ingest V1 only, partner is set to:[{epgFeatureVersion}]");
                    }

                    Synchronizer.CouchbaseSynchronizer synchronizer = new Synchronizer.CouchbaseSynchronizer(0, 3600);
                    synchronizer.SynchronizedAct += synchronizer_SynchronizedAct;

                    Dictionary<string, object> parameters = new Dictionary<string, object>();
                    parameters.Add("request", request);

                    bool wasPerformed = false;
                    bool rebuildResult = false;
                    wasPerformed = synchronizer.SingleDoAction(
                        string.Format("rebuild_index_{0}_{1}", request.GroupID, request.Type.ToString().ToLower()),
                        out rebuildResult,
                        parameters);

                    if (!wasPerformed)
                    {
                        res = "Rebuild is already in process!";
                    }
                    else if (rebuildResult)
                    {
                        res = "success";
                        log.DebugFormat("Rebuilding {0} index for group id {1} has succeeded.", request.Type.ToString(), request.GroupID);
                    }
                    else
                    {
                        throw new Exception(string.Format("Rebuilding {0} index for group id {1} has failed.",
                            request.Type.ToString(), request.GroupID));
                    }

                    #endregion
                }
                else if (request.Action == ApiObjects.eAction.GeoUpdate)
                {
                    #region GeoUpdate
                    if (request.DocumentIDs != null && request.DocumentIDs.Count > 0)
                    {
                        if (Core.Api.Managers.AssetRuleManager.Instance.UpdateMedia(request.GroupID, request.DocumentIDs[0]))
                        {
                            res = "success";
                        }
                        else
                        {
                            throw new Exception(
                                string.Format("Performing {0} action on asset of type {1} with id: [{2}] did not finish successfully.",
                                request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocumentIDs)));
                        }
                    }
                    #endregion
                }
                else if (request.Action == ApiObjects.eAction.EpgRegionUpdate)
                {
                    var updater = Updaters.UpdaterFactory.CreateUpdater(request.GroupID, eObjectType.EPG); 

                    if (updater != null)
                    {
                        updater.Action = request.Action;
                        updater.IDs = request.DocumentIDs;

                        bool result = updater.Start();
                        
                        if (result)
                        {
                            res = "success";
                            log.DebugFormat("Successfully perform {0} action on asset of type {1} with id: [{2}].",
                                request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocumentIDs));
                        }
                        else
                        {
                            throw new Exception(
                                string.Format("Performing {0} action on asset of type {1} with id: [{2}] did not finish successfully.",
                                    request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocumentIDs)));
                        }
                    }
                }
                else
                {
                    #region Update
                    Updaters.IElasticSearchUpdater updater = Updaters.UpdaterFactory.CreateUpdater(request.GroupID, request.Type);

                    if (updater != null)
                    {
                        updater.Action = request.Action;
                        updater.IDs = request.DocumentIDs;

                        bool result = updater.Start();

                        if (result)
                        {
                            res = "success";
                            log.DebugFormat("Successfully perform {0} action on asset of type {1} with id: [{2}].",
                                request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocumentIDs));
                        }
                        else
                        {
                            throw new Exception(
                                string.Format("Performing {0} action on asset of type {1} with id: [{2}] did not finish successfully.",
                                request.Action.ToString(), request.Type.ToString(), string.Join(",", request.DocumentIDs)));
                        }
                    }
                    #endregion
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed performing elastic search request. data = {0} message = {1}, stack trace= {2}, target site = {3}",
                    data, ex.Message, ex.StackTrace, ex.TargetSite), ex);
                throw ex;
            }

            return res;
        }

        bool synchronizer_SynchronizedAct(Dictionary<string, object> parameters)
        {
            bool result = false;

            ElasticSearchRequest request = parameters["request"] as ElasticSearchRequest;
            IndexBuilders.AbstractIndexBuilder builder = IndexBuilders.IndexBuilderFactory.CreateIndexBuilder(request.GroupID, request.Type);

            if (builder != null)
            {
                if (request.SwitchIndexAlias.HasValue)
                {
                    builder.SwitchIndexAlias = request.SwitchIndexAlias.Value;
                }

                if (request.DeleteOldIndices.HasValue)
                {
                    builder.DeleteOldIndices = request.DeleteOldIndices.Value;
                }

                builder.StartDate = request.StartDate;
                builder.EndDate = request.EndDate;

                result = builder.BuildIndex();
            }

            return result;
        }
    }
}

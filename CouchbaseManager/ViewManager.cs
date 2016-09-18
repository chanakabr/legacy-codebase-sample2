using Couchbase;
using Couchbase.Core;
using Couchbase.Views;
using KLogMonitor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace CouchbaseManager
{
    public class ViewManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Data Members

        /// <summary>
        /// The design document that the View belongs to.
        /// </summary>
        public string designDoc;

        /// <summary>
        /// The View to query.
        /// </summary>
        public string viewName;

        /// <summary>
        ///  True will execute on the development dataset.
        /// </summary>
        public bool? isDevelopment;

        /// <summary>
        /// Return the documents in ascending by key order
        /// </summary>
        public bool? isAscending;

        /// <summary>
        /// Return the documents in descending by key order
        /// </summary>
        public bool? isDescending;

        /// <summary>
        /// Sets the base uri for the query if it's not set in the constructor.
        /// </summary>
        public Uri baseUri;

        /// <summary>
        /// Sets the name of the Couchbase Bucket.
        /// </summary>
        public string bucket;

        /// <summary>
        /// The number of seconds before the request will be terminated if it has not completed.
        /// </summary>
        public int? connectionTimeout;

        /// <summary>
        /// Stop returning records when the specified key is reached. Key must be specified as a JSON value.
        /// </summary>
        public object endKey;

        /// <summary>
        /// True to JSON encode the parameter.
        /// </summary>
        public bool? endKeyEncode;

        /// <summary>
        /// Stop returning records when the specified document ID is reached.
        /// </summary>
        public object endKeyDocId;

        /// <summary>
        /// Use the full cluster data set (development views only).
        /// </summary>
        public bool? fullSet;

        /// <summary>
        /// Group the results using the reduce function to a group or single row. 
        /// True to group using the reduce function into a single row
        /// </summary>
        public bool? group;

        /// <summary>
        /// Specify the group level to be used.
        /// </summary>
        public int? groupLevel;

        /// <summary>
        /// Specifies whether the specified end key should be included in the result
        /// </summary>
        public bool? inclusiveEnd;

        /// <summary>
        /// Return only documents that match the specified key. Key must be specified as a JSON value.
        /// </summary>
        public object key;

        /// <summary>
        /// True to JSON encode the parameter.
        /// </summary>
        public bool? keyEncode;

        /// <summary>
        /// Return only documents that match one of keys specified within the given array.
        /// Key must be specified as a JSON value. Sorting is not applied when using
        /// this option.
        /// </summary>
        public IEnumerable keys;

        /// <summary>
        /// True to JSON encode the parameter.
        /// </summary>
        public bool? keysEncode;

        /// <summary>
        /// Limit the number of the returned documents to the specified number.
        /// </summary>
        public int? limit;

        /// <summary>
        /// Sets the response in the event of an error
        /// </summary>
        public bool? onError;

        /// <summary>
        /// Use the reduction function
        /// </summary>
        public bool? reduce;

        /// <summary>
        ///  Skip this number of records before starting to return the results.
        /// </summary>
        public int? skip;

        /// <summary>
        /// Allow the results from a stale view to be used. The default is StaleState.Ok;
        /// for development work set to StaleState.False
        /// </summary>
        public ViewStaleState? staleState;

        /// <summary>
        /// Return records with a value equal to or greater than the specified key. Key
        /// must be specified as a JSON value.
        /// </summary>
        public object startKey;

        /// <summary>
        /// True to JSON encode the parameter.
        /// </summary>
        public bool? startKeyEncode;

        /// <summary>
        /// Return records starting with the specified document ID.
        /// </summary>
        public object startKeyDocId;

        /// <summary>
        /// The number of times the view request was retried if it fails before succeeding or giving up.
        /// </summary>
        public int? retryAttemps;

        /// <summary>
        /// When true, the generated url will contain 'https' and use port 18092
        /// </summary>
        public bool? useSsl;

        /// <summary>
        /// If some documents are missing, should everything fail or should we just get whoever exists?
        /// </summary>
        public bool allowPartialQuery;

        /// <summary>
        /// If view should look for the documents by the ID that is related to the key-value of the view
        /// </summary>
        public bool shouldLookupById;

        /// <summary>
        /// Should the lookup be to get strings and to deserialize them manually or not
        /// </summary>
        public bool asJson;

        #endregion

        #region Ctor

        public ViewManager(string designDoc, string viewName)
        {
            this.designDoc = designDoc;
            this.viewName = viewName;
        }

        #endregion

        #region Public Methods

        public List<T> Query<T>(IBucket bucket)
        {
            List<T> result = null;

            IViewQuery query = InitializeQuery(bucket);

            #region Perform query

            if (!shouldLookupById)
            {
                IViewResult<object> queryResult = null;

                string action = string.Format("Action: Query, ViewName: {0}, Bucket: {1}, RestURI: {2}", viewName, bucket.Name, query.RawUri().ToString());
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    queryResult = bucket.Query<object>(query);
                }

                // If something went wrong - log it and throw exception (if there is one)
                if (!queryResult.Success)
                {
                    log.ErrorFormat("Something went wrong when performing Couchbase query. bucket = {0}, view = {1}, message = {2}, error = {3}",
                        bucket.Name, viewName, queryResult.Message, queryResult.Error, queryResult.Exception);

                    if (queryResult.Exception != null)
                    {
                        throw queryResult.Exception;
                    }
                }
                else
                {
                    result = new List<T>();

                    // Get the documents based on the IDs that are taken from the view query result
                    var ids = queryResult.Rows.Select(row => row.Id).ToList();
                    IDictionary<string, IOperationResult<T>> getResults = null;

                    action = string.Format("Action: Get, bucket: {0}, keys: {1}", bucket.Name, string.Join(",", ids.ToArray()));
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                    {
                        getResults = bucket.Get<T>(ids);
                    }

                    // Run on all Get results
                    foreach (var getResult in getResults)
                    {
                        // If something went wrong - log it and throw exception (if there is one)
                        if (!getResult.Value.Success)
                        {
                            log.ErrorFormat("Error while getting value from view. Status code = {0}; Status = {1}", (int)getResult.Value.Status, getResult.Value.Status.ToString());

                            if (getResult.Value.Exception != null)
                            {
                                throw getResult.Value.Exception;
                            }
                        }
                        else
                        {
                            // If it is alright, add document to final list
                            result.Add(getResult.Value.Value);
                        }
                    }
                }
            }
            else
            {
                IViewResult<T> queryResult = null;

                string action = string.Format("Action: Query, ViewName: {0}, Bucket: {1}, RestURI: {2}", viewName, bucket.Name, query.RawUri().ToString());
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    queryResult = bucket.Query<T>(query);
                }

                // If something went wrong - log it and throw exception (if there is one)
                if (!queryResult.Success)
                {
                    log.ErrorFormat("Something went wrong when performing Couchbase query. bucket = {0}, view = {1}, message = {2}, error = {3}",
                        bucket.Name, viewName, queryResult.Message, queryResult.Error, queryResult.Exception);

                    if (queryResult.Exception != null)
                    {
                        throw queryResult.Exception;
                    }
                }
                else
                {
                    result = new List<T>();

                    result.AddRange(queryResult.Values);
                }
            }
            #endregion

            return result;
        }

        public List<KeyValuePair<object, T1>> QueryKeyValuePairs<T1>(IBucket bucket)
        {
            List<KeyValuePair<object, T1>> result = new List<KeyValuePair<object, T1>>();

            IViewQuery query = InitializeQuery(bucket);
            IViewResult<T1> queryResult = null;

            string action = string.Format("Action: Query, ViewName: {0}, Bucket: {1}, RestURI: {2}", viewName, bucket.Name, query.RawUri().ToString());
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
            {
                queryResult = bucket.Query<T1>(query);
            }

            // If something went wrong - log it and throw exception (if there is one)
            if (!queryResult.Success)
            {
                log.ErrorFormat("Something went wrong when performing Couchbase query. bucket = {0}, view = {1}, message = {2}, error = {3}",
                    bucket.Name, viewName, queryResult.Message, queryResult.Error, queryResult.Exception);

                if (queryResult.Exception != null)
                {
                    throw queryResult.Exception;
                }
            }
            else
            {
                foreach (var row in queryResult.Rows)
                {
                    result.Add(new KeyValuePair<object, T1>((object)row.Key, row.Value));
                }
            }

            return result;
        }

        public List<string> QueryIds(IBucket bucket)
        {
            List<string> result = new List<string>();

            IViewQuery query = InitializeQuery(bucket);
            IViewResult<object> queryResult = null;

            string action = string.Format("Action: Query, ViewName: {0}, Bucket: {1}, RestURI: {2}", viewName, bucket.Name, query.RawUri().ToString());
            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
            {
                queryResult = bucket.Query<object>(query);
            }

            // If something went wrong - log it and throw exception (if there is one)
            if (!queryResult.Success)
            {
                log.ErrorFormat("Something went wrong when performing Couchbase query. bucket = {0}, view = {1}, message = {2}, error = {3}",
                    bucket.Name, viewName, queryResult.Message, queryResult.Error, queryResult.Exception);

                if (queryResult.Exception != null)
                {
                    throw queryResult.Exception;
                }
            }
            else
            {
                foreach (var row in queryResult.Rows)
                {
                    result.Add(row.Id);
                }
            }

            return result;
        }

        public List<ViewRow<T>> QueryRows<T>(IBucket bucket)
        {
            List<ViewRow<T>> result = new List<ViewRow<T>>();

            IViewQuery query = InitializeQuery(bucket);

            if (shouldLookupById)
            {
                IViewResult<object> queryResult = null;

                string action = string.Format("Action: Query, ViewName: {0}, Bucket: {1}, RestURI: {2}", viewName, bucket.Name, query.RawUri().ToString());
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    queryResult = bucket.Query<object>(query);
                }

                // If something went wrong - log it and throw exception (if there is one)
                if (!queryResult.Success)
                {
                    log.ErrorFormat("Something went wrong when performing Couchbase query. bucket = {0}, view = {1}, message = {2}, error = {3}",
                        bucket.Name, viewName, queryResult.Message, queryResult.Error, queryResult.Exception);

                    if (queryResult.Exception != null)
                    {
                        throw queryResult.Exception;
                    }
                }
                else
                {
                    result = new List<ViewRow<T>>();

                    // Get the documents based on the IDs that are taken from the view query result
                    Dictionary<string, object> idsAndKeys = new Dictionary<string, object>();

                    foreach (var row in queryResult.Rows)
                    {
                        idsAndKeys.Add(row.Id, (object)row.Key);
                    }

                    IDictionary<string, IOperationResult<T>> getResults = null;

                    action = string.Format("Action: Get, bucket: {0}, keys: {1}", bucket.Name, string.Join(",", idsAndKeys.Keys.ToArray()));
                    using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                    {
                        getResults = bucket.Get<T>(idsAndKeys.Keys.ToList());
                    }

                    // Run on all Get results
                    foreach (var getResult in getResults)
                    {
                        // If something went wrong - log it and throw exception (if there is one)
                        if (!getResult.Value.Success)
                        {
                            log.ErrorFormat("Error while getting value from view. Status code = {0}; Status = {1}", (int)getResult.Value.Status, getResult.Value.Status.ToString());

                            if (getResult.Value.Exception != null)
                            {
                                throw getResult.Value.Exception;
                            }
                        }
                        else
                        {
                            // If it is alright, add document to final list
                            result.Add(
                                new ViewRow<T>()
                                {
                                    Id = getResult.Key,
                                    Key = idsAndKeys[getResult.Key],
                                    Value = getResult.Value.Value
                                });
                        }
                    }
                }
            }
            else
            {
                IViewResult<T> queryResult = null;

                string action = string.Format("Action: Query, ViewName: {0}, Bucket: {1}, RestURI: {2}", viewName, bucket.Name, query.RawUri().ToString());
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_COUCHBASE, null, action))
                {
                    queryResult = bucket.Query<T>(query);
                }

                // If something went wrong - log it and throw exception (if there is one)
                if (!queryResult.Success)
                {
                    log.ErrorFormat("Something went wrong when performing Couchbase query. bucket = {0}, view = {1}, message = {2}, error = {3}",
                        bucket.Name, viewName, queryResult.Message, queryResult.Error, queryResult.Exception);

                    if (queryResult.Exception != null)
                    {
                        throw queryResult.Exception;
                    }
                }
                else
                {
                    foreach (var row in queryResult.Rows)
                    {
                        result.Add(new ViewRow<T>()
                        {
                            Id = row.Id,
                            Key = (object)row.Key,
                            Value = row.Value
                        });
                    }
                }
            }

            return result;
        }

        #endregion

        #region Private Methods

        private IViewQuery InitializeQuery(IBucket bucket)
        {
            IViewQuery query = null;

            if (this.isDevelopment == null)
            {
                query = bucket.CreateQuery(this.designDoc, this.viewName);
            }
            else
            {
                query = bucket.CreateQuery(this.designDoc, this.viewName, this.isDevelopment.Value);
            }

            if (this.baseUri != null)
            {
                query = (IViewQuery)query.BaseUri(this.baseUri);
            }

            if (this.bucket != null)
            {
                query = query.Bucket(this.bucket);
            }

            if (this.connectionTimeout != null)
            {
                query = query.ConnectionTimeout(this.connectionTimeout.Value);
            }

            if (this.endKey != null)
            {
                if (this.endKeyEncode != null)
                {
                    query = query.EndKey(this.endKey, this.endKeyEncode.Value);
                }
                else
                {
                    query = query.EndKey(this.endKey);
                }
            }

            if (this.endKeyDocId != null)
            {
                query = query.EndKeyDocId(this.endKeyDocId);
            }

            if (this.fullSet != null && this.fullSet.Value)
            {
                query = query.FullSet();
            }

            if (this.group != null)
            {
                query = query.Group(this.group.Value);
            }

            if (this.groupLevel != null)
            {
                query = query.GroupLevel(this.groupLevel.Value);
            }

            if (this.inclusiveEnd != null)
            {
                query = query.InclusiveEnd(this.inclusiveEnd.Value);
            }

            if (this.isAscending != null && this.isAscending.Value)
            {
                query = query.Asc();
            }

            if (this.isDescending != null && this.isDescending.Value)
            {
                query = query.Desc();
            }

            if (this.key != null)
            {
                if (this.keyEncode != null)
                {
                    query = query.Key(this.key, this.keyEncode.Value);
                }
                else
                {
                    query = query.Key(this.key);
                }
            }

            if (this.keys != null)
            {
                if (this.keysEncode != null)
                {
                    query = query.Keys(this.keys, this.keysEncode.Value);
                }
                else
                {
                    query = query.Keys(this.keys);
                }
            }

            if (this.limit != null)
            {
                query = query.Limit(this.limit.Value);
            }

            if (this.onError != null)
            {
                query = query.OnError(this.onError.Value);
            }

            if (this.reduce != null)
            {
                query = query.Reduce(this.reduce.Value);
            }

            if (this.retryAttemps != null)
            {
                query.RetryAttempts = this.retryAttemps.Value;
            }

            if (this.skip != null)
            {
                query = query.Skip(this.skip.Value);
            }

            if (this.staleState != null)
            {
                StaleState queryStaleState = StaleState.None;

                switch (this.staleState.Value)
                {
                    case ViewStaleState.None:
                        {
                            queryStaleState = StaleState.None;
                            break;
                        }
                    case ViewStaleState.False:
                        {
                            queryStaleState = StaleState.False;
                            break;
                        }
                    case ViewStaleState.Ok:
                        {
                            queryStaleState = StaleState.Ok;
                            break;
                        }
                    case ViewStaleState.UpdateAfter:
                        {
                            queryStaleState = StaleState.UpdateAfter;
                            break;
                        }
                    default:
                        break;
                }

                query = query.Stale(queryStaleState);
            }

            if (this.startKey != null)
            {
                if (this.startKeyEncode != null)
                {
                    query = query.StartKey(this.startKey, this.startKeyEncode.Value);
                }
                else
                {
                    query = query.StartKey(this.startKey);
                }
            }

            if (this.startKeyDocId != null)
            {
                query = query.StartKeyDocId(this.startKeyDocId);
            }

            if (this.useSsl != null)
            {
                query.UseSsl = this.useSsl.Value;
            }
            return query;
        }

        #endregion
    }

    /// <summary>
    /// Allow the results from a stale view to be used
    /// </summary>
    public enum ViewStaleState
    {
        None = 0,
        False = 1,
        Ok = 2,
        UpdateAfter = 3,
    }

    public class ViewRow<T>
    {
        /// <summary>
        /// The identifier of the row
        /// </summary>
        public string Id
        {
            get;
            set;
        }

        /// <summary>
        /// The key emitted by the View Map function
        /// </summary>
        public object Key
        {
            get;
            set;
        }

        /// <summary>
        /// The value emitted by the View Map function or if a Reduce view, the value of the Reduce
        /// </summary>
        public T Value
        {
            get;
            set;
        }
    }
}

using Couchbase.Core;
using Couchbase.Views;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CouchbaseManager
{
    public class ViewManager
    {
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
        public string key;

        /// <summary>
        /// True to JSON encode the parameter.
        /// </summary>
        public bool? keyEncoded;

        /// <summary>
        /// Return only documents that match one of keys specified within the given array.
        /// Key must be specified as a JSON value. Sorting is not applied when using
        /// this option.
        /// </summary>
        public IEnumerable keys;

        /// <summary>
        /// True to JSON encode the parameter.
        /// </summary>
        public bool? keysEncoded;

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
        public StaleState? staleState;

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
        public bool? UseSsl;

        #endregion

        #region Ctor
        
        public ViewManager(string designDoc, string viewName)
        {
            this.designDoc = designDoc;
            this.viewName = viewName;
        }

        #endregion

        #region Public Method

        public List<T> Query<T>(IBucket bucket)
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

            if (this.startKey != null)
            {
                if (this.startKeyEncode != null)
                {
                    query = query.EndKey(this.startKey, this.startKeyEncode.Value);
                }
                else
                {
                    query = query.EndKey(this.startKey);
                }
            }
            var queryResult = bucket.Query<T>(query);

            return null;
        }

        #endregion
    }
}

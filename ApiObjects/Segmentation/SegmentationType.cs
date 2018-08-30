using ApiObjects.Response;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Segmentation
{
    public class SegmentationType : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        [JsonProperty()]
        public string Name;

        // Name in other languages other then default (when language="*")        
        [JsonProperty()]
        public List<LanguageContainer> NamesWithLanguages { get; set; }

        [JsonProperty()]
        public string Description;

        // Description in other languages other then default (when language="*")        
        [JsonProperty()]
        public List<LanguageContainer> DescriptionWithLanguages { get; set; }

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)] 
        public List<SegmentCondition> Conditions;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SegmentBaseValue Value;

        [JsonProperty()]
        public int Status;

        public Status ActionStatus;

        protected override bool DoInsert()
        {
            bool result = false;

            this.Status = 1;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            ulong versionMaximumId;

            int maximumId = couchbaseManager.GetWithVersion<int>(GetMaximumSegmentationTypeIdDocumentKey(), out versionMaximumId);
            int newId = maximumId + 1;

            bool setResult = false;
            
            for (int retryCount = 0; (retryCount < 3) && !setResult; retryCount++)
            {
                setResult = couchbaseManager.SetWithVersion<int>(GetMaximumSegmentationTypeIdDocumentKey(), newId, versionMaximumId, out versionMaximumId);

                if (!setResult)
                {
                    maximumId = couchbaseManager.GetWithVersion<int>(GetMaximumSegmentationTypeIdDocumentKey(), out versionMaximumId);
                    newId = maximumId + 1;
                }
            }

            if (!setResult)
            {
                log.ErrorFormat("Error updating maximum id of segmentation type.");
                return false;
            }

            this.Id = newId;

            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKey(this.GroupId);

            GroupSegmentationTypes groupSegmentationTypes = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);

            if (groupSegmentationTypes == null)
            {
                groupSegmentationTypes = new GroupSegmentationTypes();
            }

            if (groupSegmentationTypes.segmentationTypes == null)
            {
                groupSegmentationTypes.segmentationTypes = new List<long>();
            }

            groupSegmentationTypes.segmentationTypes.Add(newId);

            setResult = couchbaseManager.Set<GroupSegmentationTypes>(segmentationTypesKey, groupSegmentationTypes);

            if (!setResult)
            {
                log.ErrorFormat("Error updating list of segment types in group.");
                return false;
            }

            string newDocumentKey = GetSegmentationTypeDocumentKey(this.GroupId, this.Id);

            setResult = couchbaseManager.Set<SegmentationType>(newDocumentKey, this);

            if (!setResult)
            {
                log.ErrorFormat("Error adding new segment type to Couchbase.");
            }

            result = setResult;

            return result;
        }


        protected override bool DoUpdate()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKey(this.GroupId);

            GroupSegmentationTypes groupSegmentationTypes = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);

            if (groupSegmentationTypes == null || groupSegmentationTypes.segmentationTypes == null || !groupSegmentationTypes.segmentationTypes.Contains(this.Id))
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "Given Id does not exist for group");
                return false;
            }

            string updatedDocumentKey = GetSegmentationTypeDocumentKey(this.GroupId, this.Id);

            bool setResult = couchbaseManager.Set<SegmentationType>(updatedDocumentKey, this);

            if (!setResult)
            {
                log.ErrorFormat("Error updating existing segment type in Couchbase.");
            }

            result = setResult;

            return result;
        }

        protected override bool DoDelete()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKey(this.GroupId);

            GroupSegmentationTypes groupSegmentationTypes = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);

            if (groupSegmentationTypes == null || groupSegmentationTypes.segmentationTypes == null || !groupSegmentationTypes.segmentationTypes.Contains(this.Id))
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "Given Id does not exist for group");
                return false;
            }

            groupSegmentationTypes.segmentationTypes.Remove(this.Id);

            bool setResult = couchbaseManager.Set<GroupSegmentationTypes>(segmentationTypesKey, groupSegmentationTypes);

            if (!setResult)
            {
                log.ErrorFormat("Error removing segment type from group segmentation types document.");
            }

            string updatedDocumentKey = GetSegmentationTypeDocumentKey(this.GroupId, this.Id);

            this.Status = 2;

            setResult = couchbaseManager.Set<SegmentationType>(updatedDocumentKey, this);

            if (!setResult)
            {
                log.ErrorFormat("Error deleting existing segment type in Couchbase.");
            }

            result = setResult;

            return result;
        }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        public static string GetSegmentationTypeDocumentKey(int groupId, long id)
        {
            return string.Format("segment_type_{0}_{1}", groupId, id);
        }

        public static string GetMaximumSegmentationTypeIdDocumentKey()
        {
            return "maximum_segmentation_type_id";
        }

        public static string GetGroupSegmentationTypeDocumentKey(int groupId)
        {
            return string.Format("segmentation_types_{0}", groupId);
        }

        public static List<SegmentationType> List(int groupId, int pageIndex, int pageSize, out int totalCount)
        {
            List<SegmentationType> result = new List<SegmentationType>();
            totalCount = 0;
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKey(groupId);

            // get list of Ids of segmentaiton types in group
            GroupSegmentationTypes groupSegmentationTypes = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);

            if (groupSegmentationTypes == null || groupSegmentationTypes.segmentationTypes == null)
            {
                log.WarnFormat("Tried listing segmentation types of group without segmentation types. group id = {0}", groupId);
                return result;
            }

            totalCount = groupSegmentationTypes.segmentationTypes.Count;

            // get only Ids on current page
            var pagedIds = groupSegmentationTypes.segmentationTypes.Skip(pageIndex * pageSize).Take(pageSize);

            // transform Ids to document keys
            List<string> keys = pagedIds.Select(id => GetSegmentationTypeDocumentKey(groupId, id)).ToList();

            // get all segmentation types from CB
            var getResult = couchbaseManager.GetValues<SegmentationType>(keys, false, true);

            if (getResult == null)
            {
                throw new Exception("Failed getting list of segmentation types from Couchbase");
            }

            result = getResult.Values.ToList();

            return result;
        }
    }

    public class SegmentationTypesResponse
    {
        public List<SegmentationType> SegmentationTypes { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
        public int TotalCount { get; set; }
    }

    public class SegmentationTypeResponse
    {
        public SegmentationType SegmentationType { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
    }

    public class GroupSegmentationTypes
    {
        [JsonProperty(PropertyName ="segmentationTypes")]
        public List<long> segmentationTypes;
    }

    public enum ContentAction
    {
        watch_linear,
        watch_vod,
        catchup,
        npvr,
        favorite,
        recording,
        social_action
    }

    public enum ContentFieldType
    {
        meta,
        tag
    }

    public enum MonetizationType
    {
        ppv,
        subscription
    }

    public enum MathemticalOperatorType
    {
        count,
        sum,
        avg
    }
}

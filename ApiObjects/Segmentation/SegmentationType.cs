using ApiObjects.Response;
using ConfigurationManager;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using ODBCWrapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ApiObjects.Segmentation
{

    [JsonObject(ItemTypeNameHandling = TypeNameHandling.Auto)]
    public class SegmentationType : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly int USER_SEGMENT_TTL_HOURS = ApplicationConfiguration.UserSegmentTTL.IntValue;


        [JsonProperty()]
        public string Name;
        
        [JsonProperty()]
        public string Description;
        
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)] 
        public List<SegmentCondition> Conditions;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<SegmentAction> Actions;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public SegmentBaseValue Value;

        [JsonProperty()]
        public long CreateDate;

        [JsonProperty()]
        public bool AffectsContentOrdering;

        [JsonProperty()]
        public int Status;

        public Status ActionStatus;

        [JsonProperty()]
        public long Version;

        public GenericResponse<SegmentationType> ValidateForInsert()
        {
            var response = new GenericResponse<SegmentationType>();
            response.SetStatus(eResponseStatus.OK);

            if (Actions != null)
            {
                foreach (var action in Actions)
                {
                    var actionValidation = action.ValidateForInsert();
                    if (!actionValidation.IsOkStatusCode())
                    {
                        response.SetStatus(actionValidation);
                        break;
                    }
                }
            }

            return response;
        }

        protected override bool DoInsert()
        {
            bool result = false;

            this.Status = 1;
            this.Version = 1;
            this.CreateDate = (long)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            long newId = (long)couchbaseManager.Increment(GetSegmentationTypeSequenceDocument(), 1);

            if (newId == 0)
            {
                log.ErrorFormat("Error setting segmentation type id");
                return false;
            }

            this.Id = newId;

            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKey(this.GroupId);

            if (this.Value == null)
            {
                this.Value = new SegmentDummyValue();
            }

            if (this.Value != null)
            {
                bool addSegmentIdsResult = this.Value.AddSegmentsIds(this.Id);

                if (!addSegmentIdsResult)
                {
                    log.ErrorFormat("error setting segment Ids");
                    return false;
                }
            }
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

            bool setResult = couchbaseManager.Set<GroupSegmentationTypes>(segmentationTypesKey, groupSegmentationTypes);

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

        public GenericResponse<SegmentationType> ValidateForUpdate()
        {
            var response = new GenericResponse<SegmentationType>();
            response.SetStatus(eResponseStatus.OK);

            if (Actions != null)
            {
                foreach (var action in Actions)
                {
                    var actionValidation = action.ValidateForUpdate();
                    if (!actionValidation.IsOkStatusCode())
                    {
                        response.SetStatus(actionValidation);
                        break;
                    }
                }
            }

            return response;
        }

        protected override bool DoUpdate()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKey(this.GroupId);

            GroupSegmentationTypes groupSegmentationTypes = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);
            SegmentationType source = couchbaseManager.Get<SegmentationType>(GetSegmentationTypeDocumentKey(this.GroupId, this.Id), true);

            if (groupSegmentationTypes == null || groupSegmentationTypes.segmentationTypes == null || !groupSegmentationTypes.segmentationTypes.Contains(this.Id) ||
                source == null)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "Given Id does not exist for group");
                return false;
            }

            // copy create date from source
            this.CreateDate = source.CreateDate;

            if (this.Value != null)
            {
                bool updateSegmentIdsResult = this.Value.UpdateSegmentIds(source.Value, this.Id);

                if (!updateSegmentIdsResult)
                {
                    log.ErrorFormat("error setting segment Ids");
                    return false;
                }
            }
            // if user sent an empty value - and if source is a dummy value - then we will maintain it
            else if (source.Value is SegmentDummyValue)
            {
                this.Value = source.Value;
            }

            string updatedDocumentKey = GetSegmentationTypeDocumentKey(this.GroupId, this.Id);

            // copy and icnrease version of segmentation type
            this.Version = source.Version + 1;

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

            if (this.Value != null)
            {
                setResult = this.Value.DeleteSegmentIds();
            }

            if (!setResult)
            {
                log.ErrorFormat("Error deleting segments from segment type in Couchbase.");
            }

            result = setResult;

            return result;
        }

        public override CoreObject CoreClone()
        {
            return this.MemberwiseClone() as CoreObject;
        }
        
        public static string GetSegmentationTypeDocumentKey(int groupId, long id)
        {
            return string.Format("segment_type_{0}_{1}", groupId, id);
        }

        public static string GetSegmentationTypeSequenceDocument()
        {
            return "segmentation_type_sequence";
        }

        public static string GetSegmentSequenceDocument()
        {
            return "segment_sequence";
        }

        public static string GetGroupSegmentationTypeDocumentKey(int groupId)
        {
            return string.Format("segmentation_types_{0}", groupId);
        }

        public static List<SegmentationType> List(int groupId, List<long> ids, int pageIndex, int pageSize, out int totalCount)
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
            List<string> keys = null;

            if (ids?.Count > 0)
            {
                // ignore segmentation types that are not part of this group's segmentation types
                ids.RemoveAll(id => !groupSegmentationTypes.segmentationTypes.Exists(id2 => id == id2));                
                keys = ids.Select(id => GetSegmentationTypeDocumentKey(groupId, id)).ToList();
                totalCount = keys.Count;
            }
            else
            {
                // transform objects to document keys
                keys = groupSegmentationTypes.segmentationTypes.Select(id => GetSegmentationTypeDocumentKey(groupId, id)).ToList();
            }

            // get only Ids on current page
            var pagedKeys = keys.Skip(pageIndex * pageSize).Take(pageSize);

            // get all segmentation types from CB
            var segmentationTypes = couchbaseManager.GetValues<SegmentationType>(keys, true, true);


            DateTime now = DateTime.UtcNow;

            if (segmentationTypes == null)
            {
                throw new Exception("Failed getting list of segmentation types from Couchbase");
            }

            result = segmentationTypes.Values.ToList();

            return result;
        }

        public static List<SegmentationType> GetSegmentationTypesBySegmentIds(int groupId, IEnumerable<long> segmentIds)
        {
            List<SegmentationType> result = new List<SegmentationType>();
            HashSet<long> segmentationTypeIds = new HashSet<long>();

            foreach (var segmentId in segmentIds)
            {
                long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(segmentId);
                segmentationTypeIds.Add(segmentationTypeId);
            }

            List<string> keys = segmentationTypeIds.Select(id => GetSegmentationTypeDocumentKey(groupId, id)).ToList();

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            // get all segmentation types from CB
            var getResult = couchbaseManager.GetValues<SegmentationType>(keys, false, true);

            if (getResult == null)
            {
                throw new Exception("Failed getting list of segmentation types from Couchbase");
            }

            result = getResult.Values.ToList();

            return result;
        }

        private static bool UpdateAffectedUsers(int groupId, long segmentId, int affectedUsers)
        {
            bool result = false;

            int totalCount;
            long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(segmentId);

            var segmentationTypes = SegmentationType.List(groupId, new List<long>() { segmentationTypeId }, 0, 1000, out totalCount);

            SegmentationType segmentationType = null;

            if (segmentationTypes != null && segmentationTypes.Count > 0)
            {
                segmentationType = segmentationTypes.FirstOrDefault();
            }

            if (segmentationType != null)
            {
                var segmentDummyValue = (segmentationType.Value as SegmentDummyValue);

                if (segmentDummyValue != null)
                {
                    segmentDummyValue.AffectedUsers = affectedUsers;
                    segmentDummyValue.AffectedUsersTtl = DateTime.UtcNow.AddHours(USER_SEGMENT_TTL_HOURS);
                }
            }

            result = segmentationType.DoUpdate();

            return result;
        }

        public static bool UpdateSegmentsAffectedUsers(int groupId, Dictionary<long, int> data)
        {
            foreach (var item in data)
            {
                UpdateAffectedUsers(groupId, item.Key, item.Value);
            }

            return true;
        }
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
    
    public enum MonetizationType
    {
        ppv,
        subscription,
        boxset
    }

    public enum MathemticalOperatorType
    {
        count,
        sum,
        avg
    }

    public enum ContentConditionLengthType
    {
        minutes,
        percentage
    }
}

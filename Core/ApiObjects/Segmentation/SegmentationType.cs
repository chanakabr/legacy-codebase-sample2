using ApiObjects.Response;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using Phx.Lib.Log;
using Phx.Lib.Appconfig;
using Newtonsoft.Json;
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

        public static readonly int USER_SEGMENT_TTL_HOURS = ApplicationConfiguration.Current.UserSegmentTTL.Value;

        [JsonProperty()]
        public string Name;

        [JsonProperty()]
        public string Description;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<SegmentCondition> Conditions;

        [JsonProperty()]
        public eCutType ConditionsOperator;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public List<SegmentAction> Actions;

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.Auto)]
        public SegmentBaseValue Value;

        [JsonProperty()]
        public long CreateDate;

        [JsonProperty()]
        public long UpdateDate;

        [JsonProperty()]
        public long ExecuteDate;

        [JsonProperty()]
        public bool AffectsContentOrdering;

        [JsonProperty()]
        public int Status;

        public Status ActionStatus;

        [JsonProperty()]
        public long Version;

        [JsonProperty()]
        public long? AssetUserRuleId;

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

            long newId = (long)couchbaseManager.Increment(GetSegmentationTypeSequenceDocumentFromCb(), 1);

            if (newId == 0)
            {
                log.ErrorFormat("Error setting segmentation type id");
                return false;
            }

            this.Id = newId;

            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKeyForCb(this.GroupId);

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
            else
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupSegmentationTypesInvalidationKey(this.GroupId));

                if (this.Actions?.Count > 0)
                {
                    GroupSegmentationTypesWithActions.Update(this.GroupId, this.Id, true);
                }
            }

            string newDocumentKey = GetSegmentationTypeDocumentKeyForCb(this.GroupId, this.Id);

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

            GetSegmentationTypeFromCb(this.GroupId, this.Id, out Status status);

            if (!status.IsOkStatusCode())
            {
                response.SetStatus(status);
                return response;
            }

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

            SegmentationType source = GetSegmentationTypeFromCb(this.GroupId, this.Id, out Status status);

            if(!status.IsOkStatusCode())
            {
                this.ActionStatus = status;
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

            string updatedDocumentKey = GetSegmentationTypeDocumentKeyForCb(this.GroupId, this.Id);

            // copy and icnrease version of segmentation type
            this.Version = source.Version + 1;

            bool setResult = couchbaseManager.Set<SegmentationType>(updatedDocumentKey, this);

            if (!setResult)
            {
                log.ErrorFormat("Error updating existing segment type in Couchbase.");
            }
            else
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetSegmentationTypeInvalidationKey(this.GroupId, this.Id));
                GroupSegmentationTypesWithActions.Update(this.GroupId, this.Id, this.Actions?.Count > 0);
            }

            result = setResult;

            return result;
        }

        protected override bool DoDelete()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKeyForCb(this.GroupId);

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

            string updatedDocumentKey = GetSegmentationTypeDocumentKeyForCb(this.GroupId, this.Id);

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
            else
            {
                LayeredCache.Instance.SetInvalidationKey(LayeredCacheKeys.GetGroupSegmentationTypesInvalidationKey(this.GroupId));
                GroupSegmentationTypesWithActions.Update(this.GroupId, this.Id, false);
            }

            result = setResult;

            return result;
        }

        public override CoreObject CoreClone()
        {
            return this.MemberwiseClone() as CoreObject;
        }

        private static string GetSegmentationTypeDocumentKeyForCb(int groupId, long id)
        {
            return string.Format("segment_type_{0}_{1}", groupId, id);
        }

        private static string GetSegmentationTypeSequenceDocumentFromCb()
        {
            return "segmentation_type_sequence";
        }

        public static string GetSegmentSequenceDocumentFromCb()
        {
            return "segment_sequence";
        }

        private static string GetGroupSegmentationTypeDocumentKeyForCb(int groupId)
        {
            return string.Format("segmentation_types_{0}", groupId);
        }

        public static List<SegmentationType> ListFromCb(int groupId, List<long> ids, int pageIndex, int pageSize, out int totalCount)
        {
            List<SegmentationType> result = new List<SegmentationType>();
            totalCount = 0;

            // get list of Ids of segmentaiton types in group
            GroupSegmentationTypes groupSegmentationTypes = GetGroupSegmentationTypes(groupId);

            if (groupSegmentationTypes == null || groupSegmentationTypes.segmentationTypes == null)
            {
                log.WarnFormat("Tried listing segmentation types of group without segmentation types. group id = {0}", groupId);
                return result;
            }

            totalCount = groupSegmentationTypes.segmentationTypes.Count;

            if (totalCount == 0)
            {
                return result;
            }

            if (ids?.Count > 0)
            {
                // ignore segmentation types that are not part of this group's segmentation types
                ids.RemoveAll(id => !groupSegmentationTypes.segmentationTypes.Exists(id2 => id == id2));
                totalCount = ids.Count;
            }
            else
            {
                ids = groupSegmentationTypes.segmentationTypes;
            }

            // get only Ids on current page
            if (pageSize > 0)
            {
                ids = ids.Skip(pageIndex * pageSize).Take(pageSize).ToList();
            }

            if (ids == null || ids.Count == 0)
            {
                return result;
            }

            // get all segmentation types from Cache
            result = GetSegmentationTypes(groupId, ids);

            DateTime now = DateTime.UtcNow;

            if (result == null)
            {
                throw new Exception("Failed getting list of segmentation types from Couchbase");
            }

            return result;
        }

        public static List<SegmentationType> ListActionOfTypeFromCb<T>(int groupId, List<long> ids) where T : SegmentAction
        {
            List<SegmentationType> segmentations = new List<SegmentationType>();

            try
            {
                string actionKey = typeof(T).Name;

                var groupSegmentationTypes = GetSegmentationTypeIdsOfAction<T>(groupId, actionKey);

                if (groupSegmentationTypes != null && groupSegmentationTypes.segmentationTypes?.Count > 0)
                {
                    var intersact = ids.Intersect(groupSegmentationTypes.segmentationTypes).ToList();

                    if (intersact?.Count > 0)
                    {
                        segmentations = GetSegmentationTypes(groupId, intersact);
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed ListActionOfType for groupId: {groupId}, ex : {ex}");
            }

            return segmentations;
        }

        public static List<SegmentationType> GetSegmentationTypesBySegmentIdsFromCb(int groupId, IEnumerable<long> segmentIds)
        {
            List<SegmentationType> result = new List<SegmentationType>();
            HashSet<long> segmentationTypeIds = new HashSet<long>();

            foreach (var segmentId in segmentIds)
            {
                long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(segmentId);
                segmentationTypeIds.Add(segmentationTypeId);
            }

            List<string> keys = segmentationTypeIds.Select(id => GetSegmentationTypeDocumentKeyForCb(groupId, id)).ToList();

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

            var segmentationTypes = SegmentationType.ListFromCb(groupId, new List<long>() { segmentationTypeId }, 0, 1000, out totalCount);

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

        private static GroupSegmentationTypes GetGroupSegmentationTypes(int groupId)
        {
            GroupSegmentationTypes groupSegmentationTypes = null;

            try
            {
                string key = LayeredCacheKeys.GetGroupSegmentationTypesKey(groupId);
                List<string> segmentsInvalidationKey = new List<string>() { LayeredCacheKeys.GetGroupSegmentationTypesInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get<GroupSegmentationTypes>(key,
                                                          ref groupSegmentationTypes,
                                                          GetGroupSegmentationTypes,
                                                          new Dictionary<string, object>() { { "groupId", groupId } },
                                                          groupId,
                                                          LayeredCacheConfigNames.GET_GROUP_SEGMENTATION_TYPES,
                                                          segmentsInvalidationKey))
                {
                    log.ErrorFormat("Failed getting GetGroupSegmentationTypes from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetGroupSegmentationTypes for groupId: {0}", groupId), ex);
            }

            return groupSegmentationTypes;
        }

        private static Tuple<GroupSegmentationTypes, bool> GetGroupSegmentationTypes(Dictionary<string, object> funcParams)
        {
            GroupSegmentationTypes result = new GroupSegmentationTypes();

            try
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue)
                {
                    string segmentationTypesKey = GetGroupSegmentationTypeDocumentKeyForCb(groupId.Value);

                    CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
                    // get list of Ids of segmentaiton types in group
                    result = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetGroupSegmentationTypes failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<GroupSegmentationTypes, bool>(result, result != null);
        }

        private static List<SegmentationType> GetSegmentationTypesPaged(int groupId, List<long> segmentIds)
        {
            List<SegmentationType> resopnse = new List<SegmentationType>();

            try
            {
                Dictionary<string, string> keysToOriginalValueMap = new Dictionary<string, string>();
                Dictionary<string, List<string>> invalidationKeysMap = new Dictionary<string, List<string>>();

                foreach (long segmentId in segmentIds)
                {
                    string key = LayeredCacheKeys.GetSegmentationTypeKey(groupId, segmentId);
                    keysToOriginalValueMap.Add(key, GetSegmentationTypeDocumentKeyForCb(groupId, segmentId));
                    invalidationKeysMap.Add(key, new List<string>() { LayeredCacheKeys.GetSegmentationTypeInvalidationKey(groupId, segmentId) });
                }

                Dictionary<string, SegmentationType> segmentationTypes = null;

                // try to get full AssetUserRules from cache            
                if (LayeredCache.Instance.GetValues<SegmentationType>(keysToOriginalValueMap,
                                                                   ref segmentationTypes,
                                                                   GetSegmentationTypes,
                                                                   new Dictionary<string, object>() { { "segmentationTypeKeys", keysToOriginalValueMap.Values.ToList() },
                                                                    { "groupId",groupId }},
                                                                   groupId,
                                                                   LayeredCacheConfigNames.GET_SEGMENTATION_TYPE,
                                                                   invalidationKeysMap))
                {
                    if (segmentationTypes?.Count > 0)
                    {
                        resopnse = segmentationTypes.Values.ToList();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error($"GetSegmentationTypesPaged failed params : {string.Join(";", segmentIds)}, ex: {ex}");
            }

            return resopnse;
        }

        private static List<SegmentationType> GetSegmentationTypes(int groupId, List<long> segmentIds)
        {
            List<SegmentationType> resopnse = new List<SegmentationType>();

            try
            {
                int index = 0;
                int size = 30;

                var ids = segmentIds.Skip(index * size).Take(size).ToList();
                while (ids?.Count > 0)
                {

                    var seg = GetSegmentationTypesPaged(groupId, ids);
                    if (seg == null || seg.Count == 0)
                    {
                        break;
                    }

                    resopnse.AddRange(seg);
                    index++;
                    ids = segmentIds.Skip(index * size).Take(size).ToList();
                }
            }
            catch (Exception ex)
            {
                log.Error($"GetSegmentationTypes failed ex: {ex}");
            }

            return resopnse;
        }

        private static Tuple<Dictionary<string, SegmentationType>, bool> GetSegmentationTypes(Dictionary<string, object> funcParams)
        {
            bool res = false;
            Dictionary<string, SegmentationType> result = new Dictionary<string, SegmentationType>();

            try
            {
                if (funcParams != null && funcParams.ContainsKey("segmentationTypeKeys"))
                {
                    List<string> segmentationTypeKeys = funcParams["segmentationTypeKeys"] != null ? funcParams["segmentationTypeKeys"] as List<string> : null;
                    int? groupId = funcParams["groupId"] as int?;

                    if (segmentationTypeKeys?.Count > 0)
                    {
                        CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
                        var segmentationTypes = couchbaseManager.GetValues<SegmentationType>(segmentationTypeKeys, true, true);

                        string key = string.Empty;
                        foreach (var item in segmentationTypes)
                        {
                            key = LayeredCacheKeys.GetSegmentationTypeKey(groupId.Value, item.Value.Id);
                            result.Add(key, item.Value);
                        }

                        res = result.Count > 0;
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetSegmentationTypes failed params : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<Dictionary<string, SegmentationType>, bool>(result, res);
        }

        private static GroupSegmentationTypes GetSegmentationTypeIdsOfAction<T>(int groupId, string actionKey) where T : SegmentAction
        {
            GroupSegmentationTypes groupSegmentationTypes = null;

            try
            {
                string key = LayeredCacheKeys.GetGroupSegmentationTypeIdsOfActionKey(groupId, actionKey);
                List<string> segmentsInvalidationKey = new List<string>() { LayeredCacheKeys.GetGroupSegmentationTypeIdsOfActionInvalidationKey(groupId) };
                if (!LayeredCache.Instance.Get<GroupSegmentationTypes>(key,
                                                          ref groupSegmentationTypes,
                                                          GetSegmentationTypeIdsOfAction<T>,
                                                          new Dictionary<string, object>() { { "groupId", groupId }, { "actionKey", actionKey } },
                                                          groupId,
                                                          LayeredCacheConfigNames.GET_GROUP_SEGMENTATION_TYPES_OF_ACTION,
                                                          segmentsInvalidationKey))
                {
                    log.ErrorFormat("Failed getting GetSegmentationTypeIdsOfAction from LayeredCache, groupId: {0}, key: {1}", groupId, key);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetSegmentationTypeIdsOfAction for groupId: {0}", groupId), ex);
            }

            return groupSegmentationTypes;
        }

        private static Tuple<GroupSegmentationTypes, bool> GetSegmentationTypeIdsOfAction<T>(Dictionary<string, object> funcParams) where T : SegmentAction
        {
            GroupSegmentationTypes result = new GroupSegmentationTypes();

            try
            {
                int? groupId = funcParams["groupId"] as int?;

                if (groupId.HasValue)
                {
                    var segmentationTypeIds = GroupSegmentationTypesWithActions.Get(groupId.Value);

                    if (segmentationTypeIds != null && segmentationTypeIds.Count > 0)
                    {
                        var segmentationTypes = GetSegmentationTypes(groupId.Value, segmentationTypeIds.ToList());

                        if (segmentationTypes?.Count > 0)
                        {
                            result.segmentationTypes = segmentationTypes.Where(s => s.Actions != null && s.Actions.Any(y => y is T)).Select(z => z.Id).ToList();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetSegmentationTypeIdsOfAction failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }

            return new Tuple<GroupSegmentationTypes, bool>(result, result != null);
        }

        private static SegmentationType GetSegmentationTypeFromCb(int groupId, long segmentationTypeId, out Status status )
        {
            status = new Status() { Code = (int)eResponseStatus.OK };
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string segmentationTypesKey = GetGroupSegmentationTypeDocumentKeyForCb(groupId);

            GroupSegmentationTypes groupSegmentationTypes = couchbaseManager.Get<GroupSegmentationTypes>(segmentationTypesKey);
            SegmentationType source = couchbaseManager.Get<SegmentationType>(GetSegmentationTypeDocumentKeyForCb(groupId, segmentationTypeId), true);

            if (groupSegmentationTypes == null || groupSegmentationTypes.segmentationTypes == null || !groupSegmentationTypes.segmentationTypes.Contains(segmentationTypeId) ||
                source == null)
            {
                status = new Status((int)eResponseStatus.ObjectNotExist, "Given Id does not exist for group");                
            }

            return source;
        }

        public Status ValidateForDelete(int groupid, long segmentationTypeId)
        {
            GetSegmentationTypeFromCb(groupid, segmentationTypeId, out Status status);

            return status;
        }
    }
}
using ApiObjects.Base;
using ApiObjects.Response;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ApiObjects.Segmentation
{
    [JsonObject()]
    public class HouseholdSegment : CoreObject, ICrudHandeledObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        #region Members

        [JsonIgnore()]
        public long HouseholdId { get; set; }

        [JsonProperty()]
        public long SegmentId { get; set; }

        [JsonIgnore()]
        public string DocumentId { get; set; }

        [JsonIgnore()]
        public Status ActionStatus { get; set; }

        [JsonProperty()]
        public DateTime CreateDate { get; set; }

        [JsonProperty()]
        public DateTime UpdateDate { get; set; }

        #endregion

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            string householdSegmentsKey = GetHouseholdSegmentsKey(this.HouseholdId);

            HouseholdSegments householdSegments = couchbaseManager.Get<HouseholdSegments>(householdSegmentsKey);

            if (householdSegments == null)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "household has no segments");
                return false;
            }

            if (householdSegments.Segments == null)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "household has no segments");
                return false;
            }

            if (!householdSegments.Segments.ContainsKey(SegmentId))
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "household does not have given segment");
                return false;
            }

            householdSegments.Segments.Remove(SegmentId);

            result = couchbaseManager.Set<HouseholdSegments>(householdSegmentsKey, householdSegments);

            if (!result)
            {
                log.ErrorFormat("Error updating household segments.");
            }

            return result;
        }

        protected override bool DoInsert()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            string householdSegmentsKey = GetHouseholdSegmentsKey(this.HouseholdId);

            HouseholdSegments householdSegments = couchbaseManager.Get<HouseholdSegments>(householdSegmentsKey);

            if (householdSegments == null)
            {
                householdSegments = new HouseholdSegments()
                {
                    HouseholdId = this.HouseholdId
                };
            }

            if (householdSegments.Segments == null)
            {
                householdSegments.Segments = new Dictionary<long, HouseholdSegment>();
            }

            long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(this.SegmentId);

            var segmentationTypesList = SegmentationType.List(this.GroupId, new List<long>() { segmentationTypeId }, 0, 1000, out int totalCount);

            SegmentationType segmentationType = null;

            if (segmentationTypesList != null && segmentationTypesList.Count > 0)
            {
                segmentationType = segmentationTypesList.FirstOrDefault();
            }

            if (segmentationType == null)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "Segmentation type not found");
                return false;
            }

            if (!segmentationType.Value.HasSegmentId(SegmentId))
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "Given segment id does not exist for this segmentation type");
                return false;
            }

            if (householdSegments.Segments.ContainsKey(SegmentId))
            {
                if (householdSegments.Segments[SegmentId].UpdateDate != DateTime.MaxValue)
                {
                    householdSegments.Segments[SegmentId].UpdateDate = DateTime.UtcNow;
                }
            }
            else
            {
                this.CreateDate = DateTime.UtcNow;

                if (segmentationType.Actions != null && segmentationType.Actions.Any())
                {
                    this.UpdateDate = DateTime.MaxValue;
                }
                else
                {
                    this.UpdateDate = this.CreateDate;
                }

                householdSegments.Segments.Add(SegmentId, this);
            }

            // cleanup invalid and expired segments

            result = couchbaseManager.Set<HouseholdSegments>(householdSegmentsKey, householdSegments);

            if (!result)
            {
                log.ErrorFormat("Error updating household segments.");
                return false;
            }

            this.DocumentId = string.Format("household_{0}_{1}", this.HouseholdId, this.SegmentId);

            return result;
        }

        protected override bool DoUpdate()
        {
            return true;
        }
        
        public static List<HouseholdSegment> List(int groupId, long householdId, List<long> segmentsIds, int pageIndex, int pageSize, out int totalCount)
        {
            totalCount = 0;
            List<HouseholdSegment> result = new List<HouseholdSegment>();

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string key = GetHouseholdSegmentsKey(householdId);
            HouseholdSegments householdSegments = couchbaseManager.Get<HouseholdSegments>(key);

            if (householdSegments != null && householdSegments.Segments != null)
            {
                if (segmentsIds?.Count > 0)
                {
                    // should return only segments in this list
                    householdSegments.Segments = householdSegments.Segments.Where(x => segmentsIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                }

                if (pageSize == 0 && pageIndex == 0)
                {
                    result = householdSegments.Segments.Values.ToList();
                }
                else
                {
                    // get only segments on current page
                    result = householdSegments.Segments.Values.ToList().Skip(pageIndex * pageSize).Take(pageSize).ToList();
                }

                totalCount = householdSegments.Segments.Count;
            }

            return result;
        }

        public static List<SegmentationType> ListHouseholdSegmentationActionsOfType<T>(int groupId, long householdId)
        {
            List<SegmentationType> res = new List<SegmentationType>();

            var segmentation = List(groupId, householdId, null, 0, 0, out int totalCount);
            if (segmentation?.Count > 0)
            {
                var segmentsIds = segmentation.Select(s => s.SegmentId).ToList();
                var segmentations = SegmentationType.List(groupId, segmentsIds, 0, 1000, out totalCount);
                res = segmentations.Where(s => s.Actions != null && s.Actions.Any(y => y is T)).ToList();
            }

            return res;
        }

        private static string GetHouseholdSegmentsKey(long householdId)
        {
            return string.Format("household_segments_{0}", householdId);
        }

        private string GetHouseholdSegmentsSequenceDocument()
        {
            return "household_segment_sequence";
        }
    }

    public class HouseholdSegments
    {
        [JsonProperty()]
        public Dictionary<long, HouseholdSegment> Segments { get; set; }

        [JsonProperty()]
        public long HouseholdId { get; set; }

        public HouseholdSegments()
        {
            Segments = new Dictionary<long, HouseholdSegment>();
        }
    }
}
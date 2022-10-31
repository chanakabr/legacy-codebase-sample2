using ApiObjects.Base;
using ApiObjects.Response;
using CouchbaseManager;
using Phx.Lib.Log;
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

        [JsonProperty()]
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

            this.ActionStatus = IsSegmentExist(householdSegments);

            if (this.ActionStatus.Code != (int)eResponseStatus.OK)
            {
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

            var segmentationTypesList = SegmentationType.ListFromCb(this.GroupId, new List<long>() { segmentationTypeId }, 0, 0, out int totalCount);

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
        
        public static List<HouseholdSegment> ListFromCb(int groupId, long householdId, out int totalCount, List<long> segmentsIds = null)
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

                result = householdSegments.Segments.Values.ToList();             
                totalCount = householdSegments.Segments.Count;
            }

            return result;
        }

        private static string GetHouseholdSegmentsKey(long householdId)
        {
            return string.Format("household_segments_{0}", householdId);
        }

        public Status IsSegmentExist(HouseholdSegments householdSegments = null)
        {
            if (householdSegments == null)
            {
                CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

                string householdSegmentsKey = GetHouseholdSegmentsKey(this.HouseholdId);

                householdSegments = couchbaseManager.Get<HouseholdSegments>(householdSegmentsKey);
            }

            if (householdSegments == null)
            {
                return new Status((int)eResponseStatus.ObjectNotExist, "household has no segments");                
            }

            if (householdSegments.Segments == null)
            {
                return new Status((int)eResponseStatus.ObjectNotExist, "household has no segments");                
            }

            if (!householdSegments.Segments.ContainsKey(SegmentId))
            {
                return new Status((int)eResponseStatus.ObjectNotExist, "household does not have given segment");                
            }

            return new Status((int)eResponseStatus.OK);

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
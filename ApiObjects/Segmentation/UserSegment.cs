using ApiObjects.Response;
using CouchbaseManager;
using KLogMonitor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Segmentation
{
    public class UserSegment : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        
        public string UserId { get; set; }
        
        [JsonProperty()]
        public long SegmentId { get; set; }
        
        public string SegmentSystemName { get; set; }

        public string DocumentId { get; set; }

        public Status ActionStatus { get; set; }
        
        [JsonProperty()]
        public DateTime CreateDate { get; set; }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            string userSegmentsKey = GetUserSegmentsDocument(this.UserId);
            
            UserSegments userSegments = couchbaseManager.Get<UserSegments>(userSegmentsKey);

            if (userSegments == null)
            {
                userSegments = new UserSegments()
                {
                    UserId = this.UserId
                };
            }

            if (userSegments.Segments == null)
            {
                userSegments.Segments = new List<UserSegment>();
            }

            int totalCount;
            long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(this.SegmentId);

            var segmentationTypes = SegmentationType.List(this.GroupId, new List<long>() { segmentationTypeId }, 0, 1000, out totalCount);

            SegmentationType segmentationType = null;

            if (segmentationTypes != null && segmentationTypes.Count > 0)
            {
                segmentationType = segmentationTypes.FirstOrDefault();
            }

            if (segmentationType == null)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "Segmentation type not found");
                return false;
            }
            
            bool validSegmentId = validSegmentId = segmentationType.Value.HasSegmentId(SegmentId);

            if (!validSegmentId)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "Given segment id does not exist for this segmentation type");
                return false;
            }

            HashSet<long> alreadyContainedSegments = new HashSet<long>(userSegments.Segments.Select(o => o.SegmentId));
            
            if (!alreadyContainedSegments.Contains(this.SegmentId))
            {
                userSegments.Segments.Add(this);
            }

            long newId = (long)couchbaseManager.Increment(GetUserSegmentsSequenceDocument(), 1);

            if (newId == 0)
            {
                log.ErrorFormat("Error setting user segment id");
                return false;
            }

            this.Id = newId;
            this.CreateDate = DateTime.UtcNow;

            bool setResult = couchbaseManager.Set<UserSegments>(userSegmentsKey, userSegments);

            if (!setResult)
            {
                log.ErrorFormat("Error updating user segments.");
                return false;
            }

            this.DocumentId = string.Format("{0}_{1}", this.UserId, this.SegmentId);

            result = setResult;

            return result;
        }

        private static string GetCombinedString(long segmentationTypeId, long? segmentId)
        {
            if (segmentId.HasValue)
            {
                return string.Format("t_{0}_s_{1}", segmentationTypeId, segmentId);
            }
            else
            {
                return string.Format("t_{0}", segmentationTypeId);
            }
        }

        private static string GetUserSegmentsDocument(string userId)
        {
            return string.Format("user_segments_{0}", userId);
        }

        private string GetUserSegmentsSequenceDocument()
        {
            return "user_segment_sequence";
        }

        protected override bool DoDelete()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
  
            string userSegmentsKey = GetUserSegmentsDocument(this.UserId);

            UserSegments userSegments = couchbaseManager.Get<UserSegments>(userSegmentsKey);

            if (userSegments == null)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "User has no segments");
                return false;
            }

            if (userSegments.Segments == null)
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "User has no segments");
                return false;
            }

            HashSet<long> alreadyContainedSegments = new HashSet<long>(userSegments.Segments.Select(o => o.SegmentId));
            
            if (!alreadyContainedSegments.Contains(this.SegmentId))
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "User does not have given segment");
                return false;
            }

            userSegments.Segments.RemoveAll(userSegment => userSegment.SegmentId == SegmentId);

            bool setResult = couchbaseManager.Set<UserSegments>(userSegmentsKey, userSegments);

            if (!setResult)
            {
                log.ErrorFormat("Error updating user segments.");
                return false;
            }
            
            result = setResult;

            return result;
        }

        protected override bool DoUpdate()
        {
            return true;
        }

        public static List<UserSegment> List(int groupId, string userId, int pageIndex, int pageSize, out int totalCount)
        {
            List<UserSegment> result = new List<UserSegment>();
            totalCount = 0;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string userSegmentsKey = GetUserSegmentsDocument(userId);
            UserSegments userSegments = couchbaseManager.Get<UserSegments>(userSegmentsKey);

            if (userSegments != null && userSegments.Segments != null)
            {
                totalCount = userSegments.Segments.Count;
                
                if (pageSize == 0 && pageIndex == 0)
                {
                    result = userSegments.Segments;
                }
                else
                {
                    // get only segments on current page
                    result = userSegments.Segments.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                }
            }

            return result;
        }
    }

    public class UserSegments
    {
        [JsonProperty()]
        public List<UserSegment> Segments { get; set; }

        [JsonProperty()]
        public string UserId { get; set; }

        public UserSegments()
        {
            Segments = new List<UserSegment>();
        }
    }
}

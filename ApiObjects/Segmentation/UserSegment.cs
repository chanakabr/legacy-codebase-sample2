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
        public long SegmentationTypeId { get; set; }

        [JsonProperty()]
        public long SegmentId { get; set; }
        
        public string SegmentSystemName { get; set; }

        public string DocumentId { get; set; }

        public Status ActionStatus { get; set; }
        
        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            long newId = couchbaseManager.GetSequenceValue(GetUserSegmentatsSequenceDocument());

            if (newId == 0)
            {
                log.ErrorFormat("Error setting user segment id");
                return false;
            }

            this.Id = newId;

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

            HashSet<long> alreadyContainedSegments = new HashSet<long>(userSegments.Segments.Select(o => o.SegmentId));

            if (!alreadyContainedSegments.Contains(this.SegmentId))
            {
                userSegments.Segments.Add(this);
            }

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

        private static string GetUserSegmentsDocument(string userId)
        {
            return string.Format("user_segments_{0}", userId);
        }

        private string GetUserSegmentatsSequenceDocument()
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

                // get only segments on current page
                result = userSegments.Segments.Skip(pageIndex * pageSize).Take(pageSize).ToList();
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
    
    public class UserSegmentsResponse
    {
        public List<UserSegment> Segments { get; set; }
        public ApiObjects.Response.Status Status { get; set; }
        public int TotalCount { get; set; }
    }

    public class UserSegmentResponse
    {
        public UserSegment UserSegment { get; set; }

        public Status Status { get; set; }
    }
}

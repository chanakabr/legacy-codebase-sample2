using ApiObjects.Response;
using Phx.Lib.Appconfig;
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
    public class UserSegment : CoreObject
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static readonly int USER_SEGMENT_TTL_HOURS = ApplicationConfiguration.Current.UserSegmentTTL.Value;

        #region Members

        [JsonIgnore()]
        public string UserId { get; set; }

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

        #region CoreObject override methods

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        #region Insert, Update, Delete

        protected override bool DoInsert()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            string userSegmentsKey = GetUserSegmentsKey(this.UserId);

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
                userSegments.Segments = new Dictionary<long, UserSegment>();
            }

            long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(this.SegmentId);

            var segmentationTypesList = SegmentationType.ListFromCb(this.GroupId, new List<long>() { segmentationTypeId }, 0, 1000, out int totalCount);

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

            if (userSegments.Segments.ContainsKey(SegmentId))
            {
                if (userSegments.Segments[SegmentId].UpdateDate != DateTime.MaxValue)
                {
                    userSegments.Segments[SegmentId].UpdateDate = DateTime.UtcNow;
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

                userSegments.Segments.Add(SegmentId, this);
            }

            // cleanup invalid and expired segments
            List<long> segmentsToRemove = GetUserSegmentsToCleanup(GroupId, userSegments.Segments.Values.ToList());
            segmentsToRemove.ForEach(s => userSegments.Segments.Remove(s));

            ulong ttl = GetDocumentTTL();
            if (userSegments.Segments.Values.FirstOrDefault(x => x.UpdateDate == DateTime.MaxValue) != null)
            {
                ttl = 0;
            }

            result = couchbaseManager.Set<UserSegments>(userSegmentsKey, userSegments);

            if (!result)
            {
                log.ErrorFormat("Error updating user segments.");
                return false;
            }

            this.DocumentId = string.Format("{0}_{1}", this.UserId, this.SegmentId);

            return result;
        }

        protected override bool DoDelete()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            string userSegmentsKey = GetUserSegmentsKey(this.UserId);

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

            // cleanup invalid and expired segments
            List<long> segmentsToRemove = GetUserSegmentsToCleanup(GroupId, userSegments.Segments.Values.ToList());
            segmentsToRemove.ForEach(s => userSegments.Segments.Remove(s));

            if (!userSegments.Segments.ContainsKey(SegmentId))
            {
                this.ActionStatus = new Status((int)eResponseStatus.ObjectNotExist, "User does not have given segment");
                return false;
            }

            userSegments.Segments.Remove(SegmentId);

            result = couchbaseManager.Set<UserSegments>(userSegmentsKey, userSegments, GetDocumentTTL());

            if (!result)
            {
                log.ErrorFormat("Error updating user segments.");
            }

            return result;
        }

        protected override bool DoUpdate()
        {
            return true;
        }

        #endregion

        #endregion

        #region Public methods

        public static List<UserSegment> ListFromCb(int groupId, string userId, out int totalCount, List<long> segmentsIds = null)
        {
            List<UserSegment> result = new List<UserSegment>();
            totalCount = 0;           

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string userSegmentsKey = GetUserSegmentsKey(userId);
            UserSegments userSegments = couchbaseManager.Get<UserSegments>(userSegmentsKey);

            if (userSegments != null && userSegments.Segments != null)
            {
                List<long> segmentsToRemove = GetUserSegmentsToCleanup(groupId, userSegments.Segments.Values.ToList());

                if (segmentsToRemove.Count > 0)
                {
                    // remove all invalid user segments
                    segmentsToRemove.ForEach(s => userSegments.Segments.Remove(s));

                    try
                    {
                        couchbaseManager.Set(userSegmentsKey, userSegments, GetDocumentTTL());
                    }
                    catch (Exception ex)
                    {
                        log.ErrorFormat("Error when updating user segments object in Couchbase. user = {0}, ex = {1}", userId, ex);
                    }
                }

                if (segmentsIds?.Count > 0)
                {
                    // should return only segemtns in this list
                    userSegments.Segments = userSegments.Segments.Where(x => segmentsIds.Contains(x.Key)).ToDictionary(x => x.Key, x => x.Value);
                }

                result = userSegments.Segments.Values.ToList();

                totalCount = userSegments.Segments.Count;
            }

            return result;
        }

        public static List<UserSegment> ListAllFromCb(int groupId, string userId)
        {
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string userSegmentsKey = GetUserSegmentsKey(userId);
            UserSegments userSegments = couchbaseManager.Get<UserSegments>(userSegmentsKey);

            if (userSegments != null)
            {
                return userSegments.Segments.Values.ToList();
            }
            else
            {
                return new List<UserSegment>();
            }
        }

        public static List<UserSegments> ListAll(int partnerId, int pageIndex, int pageSize, out int totalCount)
        {
            totalCount = 0;
            var result = new List<UserSegments>();

            long totalNumOfResults = 0;
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            result = couchbaseManager.View<UserSegments>(new ViewManager("migration", "user_segments", pageSize)
            {
                skip = pageIndex * pageSize,
                limit = pageSize,
                staleState = ViewStaleState.False,
                allowPartialQuery = true,
                key = partnerId
            }, ref totalNumOfResults);


            totalCount = (int)totalNumOfResults;

            return result;
        }

        public static bool MultiInsert(int groupId, Dictionary<string, List<long>> usersSegments)
        {
            bool result = true;

            if (usersSegments == null || usersSegments.Count == 0)
            {
                return true;
            }

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);

            foreach (string userId in usersSegments.Keys)
            {
                string userSegmentsKey = GetUserSegmentsKey(userId);

                UserSegments userSegments = couchbaseManager.Get<UserSegments>(userSegmentsKey);

                if (userSegments == null)
                {
                    userSegments = new UserSegments()
                    {
                        UserId = userId
                    };
                }

                if (userSegments.Segments == null)
                {
                    userSegments.Segments = new Dictionary<long, UserSegment>();
                }

                int totalCount;

                foreach (var segmentId in usersSegments[userId])
                {
                    // segment is valid until proved othersise
                    bool validSegmentId = true;

                    /*
                    long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(segmentId);

                    var segmentationTypes = SegmentationType.List(groupId, new List<long>() { segmentationTypeId }, 0, 1000, out totalCount);

                    SegmentationType segmentationType = null;

                    if (segmentationTypes != null && segmentationTypes.Count > 0)
                    {
                        segmentationType = segmentationTypes.FirstOrDefault();
                    }

                    if (segmentationType == null)
                    {
                        log.WarnFormat("UserSegment..MultiInsert : for user {0} ignored invalid segment id {1}", userId, segmentId);
                        validSegmentId = false;
                    }

                    bool typeHasSegment = segmentationType.Value.HasSegmentId(segmentId);

                    if (!typeHasSegment)
                    {
                        log.WarnFormat("UserSegment..MultiInsert : for user {0} ignored invalid segment id {1}", userId, segmentId);
                        validSegmentId = false;
                    }

                    if (validSegmentId)
                    {
                        // just update update date if segment exists for user
                        if (userSegments.Segments.ContainsKey(segmentId))
                        {
                            userSegments.Segments[segmentId].UpdateDate = DateTime.UtcNow;
                        }
                        else
                        {
                            // otherwise create full object
                            userSegments.Segments.Add(segmentId, new UserSegment()
                            {
                                CreateDate = DateTime.UtcNow,
                                UpdateDate = DateTime.UtcNow,
                                GroupId = groupId,
                                UserId = userId,
                                SegmentId = segmentId
                            });
                        }
                    }
                    */

                    // just update update date if segment exists for user
                    if (userSegments.Segments.ContainsKey(segmentId))
                    {
                        userSegments.Segments[segmentId].UpdateDate = DateTime.UtcNow;
                    }
                    else
                    {
                        // otherwise create full object
                        userSegments.Segments.Add(segmentId, new UserSegment()
                        {
                            CreateDate = DateTime.UtcNow,
                            UpdateDate = DateTime.UtcNow,
                            GroupId = groupId,
                            UserId = userId,
                            SegmentId = segmentId
                        });
                    }
                }

                // cleanup invalid and expired segments
                //List<long> segmentsToRemove = GetUserSegmentsToCleanup(groupId, userSegments.Segments.Values.ToList());
                //segmentsToRemove.ForEach(s => userSegments.Segments.Remove(s));

                bool setResult = couchbaseManager.Set<UserSegments>(userSegmentsKey, userSegments, GetDocumentTTL());

                if (!setResult)
                {
                    log.ErrorFormat("Error updating user segments. userId:{0} ", userId);
                    continue;
                }
            }

            return result;
        }

        public static void Remove(string userId)
        {
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(eCouchbaseBucket.OTT_APPS);
            string userSegmentsKey = GetUserSegmentsKey(userId);
            if (!string.IsNullOrEmpty(userSegmentsKey))
            {
                UserSegments userSegments = couchbaseManager.Get<UserSegments>(userSegmentsKey);
                if (userSegments != null)
                {
                    if (!couchbaseManager.Remove(userSegmentsKey))
                    {
                        log.ErrorFormat("Error at UserSegments remove from cb, for user : {0} not exist ", userSegmentsKey);
                    }
                }
                else
                {
                    log.ErrorFormat("Error at UserSegments get document for user : {0}", userId);
                }
            }
            else
            {
                log.ErrorFormat("Error at UserSegments remove, userSegmentsKey for user : {0} not exist", userId);
            }
        }

        #endregion

        #region Private methods

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

        public static string GetUserSegmentsKey(string userId)
        {
            return string.Format("user_segments_{0}", userId);
        }

        private string GetUserSegmentsSequenceDocument()
        {
            return "user_segment_sequence";
        }

        public static List<long> GetUserSegmentsToCleanup(int groupId, List<UserSegment> userSegments)
        {
            List<long> segmentsToRemove = new List<long>();

            foreach (var userSegment in userSegments)
            {
                long segmentationTypeId = SegmentBaseValue.GetSegmentationTypeOfSegmentId(userSegment.SegmentId);

                // if we didn't find the type of this segment id - delete it
                if (segmentationTypeId == 0)
                {
                    segmentsToRemove.Add(userSegment.SegmentId);
                    continue;
                }

                int tempCount;

                // get the segmentation type of this segment id
                var segmentationTypes = SegmentationType.ListFromCb(groupId, new List<long>() { segmentationTypeId }, 0, 1, out tempCount);

                // if we didn't find the type of this segment id - delete it
                if (segmentationTypes == null || segmentationTypes.Count == 0)
                {
                    segmentsToRemove.Add(userSegment.SegmentId);
                    continue;
                }

                // if the type doesn't have a value at all - delete it
                if (segmentationTypes[0].Value == null)
                {
                    segmentsToRemove.Add(userSegment.SegmentId);
                    continue;
                }

                // if the segment id does not exists for this type - delete it
                if (!segmentationTypes[0].Value.HasSegmentId(userSegment.SegmentId))
                {
                    segmentsToRemove.Add(userSegment.SegmentId);
                    continue;
                }

                // if segment was added more than defined TTL ago - remove it
                if (userSegment.UpdateDate != DateTime.MaxValue && 
                    userSegment.UpdateDate.AddHours(USER_SEGMENT_TTL_HOURS) < DateTime.UtcNow)
                {
                    segmentsToRemove.Add(userSegment.SegmentId);
                    continue;
                }
            }

            return segmentsToRemove;
        }

        public static uint GetDocumentTTL()
        {
            return (uint)USER_SEGMENT_TTL_HOURS * 2 * 60 * 60;
        }

        #endregion
    }

    public class UserSegments
    {
        [JsonProperty()]
        public Dictionary<long, UserSegment> Segments { get; set; }

        [JsonProperty()]
        public string UserId { get; set; }

        public UserSegments()
        {
            Segments = new Dictionary<long, UserSegment>();
        }
    }
}

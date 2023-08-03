using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ApiObjects.Segmentation
{
    public class SegmentBaseValue
    {
        internal const string SegmentToSegmentationTypeDocumentKeyFormat = "segment_{0}_segmentation_type";

        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SegmentSource Source;

        #region Static Methods

        public static bool SetSegmentationTypeIdToSegmentId(long segmentId, long segmentationTypeId)
        {
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            return couchbaseManager.Set(string.Format(SegmentToSegmentationTypeDocumentKeyFormat, segmentId), segmentationTypeId);
        }

        public static long GetSegmentationTypeOfSegmentId(long segmentId)
        {
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            return couchbaseManager.Get<long>(string.Format(SegmentToSegmentationTypeDocumentKeyFormat, segmentId));
        }

        public static Dictionary<long, long> GetSegmentationTypeOfSegmentIds(IEnumerable<long> segmentIds)
        {
            Dictionary<long, long> result = new Dictionary<long, long>();
            Dictionary<string, long> keyToIds = new Dictionary<string, long>();
            keyToIds = segmentIds.Distinct().ToDictionary(segmentId => string.Format(SegmentToSegmentationTypeDocumentKeyFormat, segmentId));
            
            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);
            var resultDictionary = couchbaseManager.GetValues<long>(keyToIds.Keys.ToList(), true, true);

            if (resultDictionary == null || resultDictionary.Count == 0)
            {
                return result;
            }

            foreach (var item in resultDictionary)
            {
                result.Add(keyToIds[item.Key], item.Value);
            }

            return result;
        }

        #endregion

        #region Virtual Methods

        public virtual bool AddSegmentsIds(long segmentationTypeId)
        {
            return true;
        }

        public virtual bool UpdateSegmentIds(SegmentBaseValue source, long segmentationTypeId)
        {
            return true;
        }

        public virtual bool HasSegmentId(long segmentId)
        {
            return false;
        }

        internal virtual bool DeleteSegmentIds()
        {
            return true;
        }

        internal virtual SegmentBaseValue GetSegmentById(long segmentId)
        {
            return null;
        }

        public virtual long GetSegmentId()
        {
            return 0;
        }

        #endregion
    }

    public class SegmentDummyValue : SegmentBaseValue
    {
        [JsonProperty()]
        public long Id;

        [JsonProperty()]
        public int AffectedUsers;

        [JsonProperty()]
        public int AffectedHouseholds;

        [JsonProperty()]
        public DateTime AffectedUsersTtl;

        public override bool AddSegmentsIds(long segmentationTypeId)
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            this.Id = (long)couchbaseManager.Increment(SegmentationType.GetSegmentSequenceDocumentFromCb(), 1);

            if (this.Id == 0)
            {
                return false;
            }
            else
            {
                result = true;
            }

            result &= SetSegmentationTypeIdToSegmentId(this.Id, segmentationTypeId);

            return result;
        }

        public override bool HasSegmentId(long segmentId)
        {
            return this.Id == segmentId;
        }

        internal override bool DeleteSegmentIds()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            result = couchbaseManager.Remove(string.Format(SegmentToSegmentationTypeDocumentKeyFormat, this.Id));

            return result;
        }

        internal override SegmentBaseValue GetSegmentById(long segmentId)
        {
            return base.GetSegmentById(segmentId);
        }

        public override bool UpdateSegmentIds(SegmentBaseValue source, long segmentationTypeId)
        {
            if (source is SegmentDummyValue sdv)
            {
                this.Id = sdv.Id;
                return true;
            }

            return false;
        }

        public override long GetSegmentId()
        {
            return this.Id;
        }
    }
}
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

        internal virtual bool HasSegmentId(long segmentId)
        {
            return false;
        } 

        #endregion
    }

    public class SegmentDummyValue : SegmentBaseValue
    {
        [JsonProperty()]
        public long Id;

        public override bool AddSegmentsIds(long segmentationTypeId)
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);
            
            this.Id = (long)couchbaseManager.Increment(SegmentationType.GetSegmentSequenceDocument(), 1);

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

        internal override bool HasSegmentId(long segmentId)
        {
            return this.Id == segmentId;
        }
    }
}
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
    }
}
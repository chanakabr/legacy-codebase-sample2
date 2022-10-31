using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Segmentation
{
    public class SegmentRanges : SegmentBaseValue
    {
        [JsonProperty()]
        public List<SegmentRange> Ranges;

        public override bool AddSegmentsIds(long segmentationTypeId)
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            foreach (var segment in this.Ranges)
            {
                segment.Id = (long)couchbaseManager.Increment(SegmentationType.GetSegmentSequenceDocumentFromCb(), 1);

                if (segment.Id == 0)
                {
                    return false;
                }

                SetSegmentationTypeIdToSegmentId(segment.Id, segmentationTypeId);
            }

            result = true;

            return result;
        }

        public override bool UpdateSegmentIds(SegmentBaseValue source, long segmentationTypeId)
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            SegmentRanges sourceCasted = source as SegmentRanges;

            Dictionary<string, SegmentRange> sourceValues = new Dictionary<string, SegmentRange>();

            if (sourceCasted != null)
            {
                foreach (var sourceValue in sourceCasted.Ranges)
                {
                    sourceValues[sourceValue.SystematicName] = sourceValue;
                }
            }
            
            foreach (var destinationValue in this.Ranges)
            {
                if (sourceValues.ContainsKey(destinationValue.SystematicName))
                {
                    destinationValue.Id = sourceValues[destinationValue.SystematicName].Id;
                }
                else
                {
                    destinationValue.Id = (long)couchbaseManager.Increment(SegmentationType.GetSegmentSequenceDocumentFromCb(), 1);
                }

                if (destinationValue.Id == 0)
                {
                    return false;
                }
                
                SetSegmentationTypeIdToSegmentId(destinationValue.Id, segmentationTypeId);
            }

            result = true;

            return result;
        }

        public override bool HasSegmentId(long segmentId)
        {
            return this.Ranges != null && this.Ranges.Exists(range => range.Id == segmentId);
        }

        internal override bool DeleteSegmentIds()
        {
            bool result = false;

            if (this.Ranges == null)
            {
                result = true;
            }
            else
            {
                CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

                result = true;

                foreach (var range in this.Ranges)
                {
                    result &= couchbaseManager.Remove(string.Format(SegmentToSegmentationTypeDocumentKeyFormat, range.Id));
                }
            }

            return result;
        }
    }

    public class SegmentRange
    {
        [JsonProperty()]
        public long Id;

        [JsonProperty()]
        public string SystematicName;

        [JsonProperty()]
        public string Name;

        [JsonProperty()]
        public double? GreaterThanOrEquals;

        [JsonProperty()]
        public double? GreaterThan;

        [JsonProperty()]
        public double? LessThanOrEquals;

        [JsonProperty()]
        public double? LessThan;

        [JsonProperty()]
        public double? Equals;
    }
}

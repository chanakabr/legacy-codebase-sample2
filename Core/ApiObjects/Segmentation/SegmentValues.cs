using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.Segmentation
{
    public class SegmentValue
    {
        [JsonProperty()]
        public long Id;

        [JsonProperty()]
        public string SystematicName;

        [JsonProperty()]
        public string Name;
        
        [JsonProperty()]
        public string Value;
    }

    public class SegmentValues : SegmentBaseValue
    {
        [JsonProperty()]
        public List<SegmentValue> Values;

        public override bool AddSegmentsIds(long segmentationTypeId)
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            foreach (var segment in this.Values)
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

            SegmentValues sourceCasted = source as SegmentValues;
            Dictionary<string, SegmentValue> sourceValues = new Dictionary<string, SegmentValue>();

            if (sourceCasted != null)
            {
                foreach (var sourceValue in sourceCasted.Values)
                {
                    sourceValues[sourceValue.SystematicName] = sourceValue;
                }
            }

            foreach (var destinationValue in this.Values)
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
            return this.Values != null && this.Values.Exists(value => value.Id == segmentId);
        }

        internal override bool DeleteSegmentIds()
        {
            bool result = false;

            if (this.Values == null)
            {
                result = true;
            }
            else
            {
                CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

                result = true;

                foreach (var value in this.Values)
                {
                    result &= couchbaseManager.Remove(string.Format(SegmentToSegmentationTypeDocumentKeyFormat, value.Id));
                }
            }

            return result;
        }
    }
}

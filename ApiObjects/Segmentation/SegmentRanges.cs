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
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SegmentSource Source;
        
        [JsonProperty()]
        public List<SegmentRange> Ranges;


        public override bool AddSegmentsIds()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            foreach (var segment in this.Ranges)
            {
                segment.Id = couchbaseManager.GetSequenceValue(SegmentationType.GetSegmentSequenceDocument());

                if (segment.Id == 0)
                {
                    return false;
                }
            }

            result = true;

            return result;
        }

        public override bool UpdateSegmentIds(SegmentBaseValue source)
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            SegmentRanges sourceCasted = source as SegmentRanges;

            if (sourceCasted == null)
            {
                return false;
            }

            Dictionary<string, SegmentRange> sourceValues = new Dictionary<string, SegmentRange>();
            Dictionary<string, SegmentRange> destinationValues = new Dictionary<string, SegmentRange>();

            foreach (var sourceValue in sourceCasted.Ranges)
            {
                sourceValues[sourceValue.SystematicName] = sourceValue;
            }

            foreach (var destinationValue in this.Ranges)
            {
                if (sourceValues.ContainsKey(destinationValue.SystematicName))
                {
                    destinationValue.Id = sourceValues[destinationValue.SystematicName].Id;
                }
                else
                {
                    destinationValue.Id = couchbaseManager.GetSequenceValue(SegmentationType.GetSegmentSequenceDocument());
                }

                if (destinationValue.Id == 0)
                {
                    return false;
                }
            }

            result = true;

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
        public double GreaterThanOrEquals;

        [JsonProperty()]
        public double GreaterThan;

        [JsonProperty()]
        public double LessThanOrEquals;

        [JsonProperty()]
        public double LessThan;

        [JsonProperty()]
        public double Equals;
    }
}

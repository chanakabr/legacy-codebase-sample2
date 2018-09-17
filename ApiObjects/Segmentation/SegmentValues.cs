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

        // Name in other languages other then default (when language="*")        
        [JsonProperty()]
        public List<LanguageContainer> NamesWithLanguages { get; set; }

        [JsonProperty()]
        public string Value;

        [JsonProperty()]
        public int? Threshold;
    }

    public class SegmentValues : SegmentBaseValue
    {
        [JsonProperty(ItemTypeNameHandling = TypeNameHandling.All)]
        public SegmentSource Source;
        
        [JsonProperty()]
        public List<SegmentValue> Values;

        public override bool AddSegmentsIds()
        {
            bool result = false;

            CouchbaseManager.CouchbaseManager couchbaseManager = new CouchbaseManager.CouchbaseManager(CouchbaseManager.eCouchbaseBucket.OTT_APPS);

            foreach (var segment in this.Values)
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

            SegmentValues sourceCasted = source as SegmentValues;

            if (sourceCasted == null)
            {
                return false;
            }

            Dictionary<string, SegmentValue> sourceValues = new Dictionary<string, SegmentValue>();
            Dictionary<string, SegmentValue> destinationValues = new Dictionary<string, SegmentValue>();

            foreach (var sourceValue in sourceCasted.Values)
            {
                sourceValues[sourceValue.SystematicName] = sourceValue;
            }

            foreach (var destinationValue in this.Values)
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
}

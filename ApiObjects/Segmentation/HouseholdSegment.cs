using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace ApiObjects.Segmentation
{
    [JsonObject()]
    public class HouseholdSegment : CoreObject
    {
        [JsonProperty()]
        public long HouseholdSegmentId { get; set; }

        [JsonProperty()]
        public string HouseholdId { get; set; }

        [JsonProperty()]
        public List<long> BlockingSegmentIds { get; set; }

        public override CoreObject CoreClone()
        {
            throw new NotImplementedException();
        }

        protected override bool DoDelete()
        {
            throw new NotImplementedException();
        }

        protected override bool DoInsert()
        {
            throw new NotImplementedException();
        }

        protected override bool DoUpdate()
        {
            throw new NotImplementedException();
        }
    }
}

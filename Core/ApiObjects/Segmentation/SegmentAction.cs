using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using ApiObjects.Response;
using System.Linq;

namespace ApiObjects.Segmentation
{
    public class SegmentAction
    {
        public virtual Status ValidateForInsert()
        {
            return new Status(eResponseStatus.OK);
        }

        public virtual Status ValidateForUpdate()
        {
            return new Status(eResponseStatus.OK);
        }
    }

    public class SegmentAssetOrderAction : SegmentAction
    {
        [JsonProperty()]
        public string Name { get; set; }

        [JsonProperty()]
        public List<string> Values { get; set; }

        public override Status ValidateForInsert()
        {
            var status = new Status(eResponseStatus.OK);

            if (string.IsNullOrEmpty(Name))
            {
                status.Set(eResponseStatus.InvalidParameters, "missing name of segment order action");
            }

            if (Values == null)
            {
                status.Set(eResponseStatus.InvalidParameters, "missing values of segment order action");
            }
            else
            {
                if (string.IsNullOrEmpty(Values.FirstOrDefault()))
                {
                    status.Set(eResponseStatus.InvalidParameters, "value of segment order action can't be empty");
                }
            }

            return status;
        }

        public override Status ValidateForUpdate()
        {
            var status = new Status(eResponseStatus.OK);

            if (string.IsNullOrEmpty(Name))
            {
                status.Set(eResponseStatus.InvalidParameters, "missing name of segment order action");
            }

            if (Values == null)
            {
                status.Set(eResponseStatus.InvalidParameters, "missing values of segment order action");
            }
            else
            {
                if (string.IsNullOrEmpty(Values.FirstOrDefault()))
                {
                    status.Set(eResponseStatus.InvalidParameters, "value of segment order action can't be empty");
                }
            }

            return status;
        }
    }
}

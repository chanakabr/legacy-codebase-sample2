using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using ApiObjects.Response;
using System.Linq;

namespace ApiObjects.Segmentation
{
    public abstract class SegmentAction
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

    public abstract class SegmentActionObjectVirtualAsset : SegmentAction
    {
        [JsonProperty()]
        public string Ksql { get; set; }

        [JsonProperty()]
        public abstract ObjectVirtualAssetInfoType objectVirtualAssetInfoType { get; }

        public override Status ValidateForInsert()
        {
            var status = new Status(eResponseStatus.OK);

            if (string.IsNullOrEmpty(Ksql))
            {
                status.Set(eResponseStatus.InvalidParameters, "missing ksql");
            }

            return status;
        }

        public override Status ValidateForUpdate()
        {
            var status = new Status(eResponseStatus.OK);

            if (string.IsNullOrEmpty(Ksql))
            {
                status.Set(eResponseStatus.InvalidParameters, "missing ksql");
            }

            return status;
        }
    }

    public abstract class SegmentActionObjectVirtualFilterAsset : SegmentActionObjectVirtualAsset
    {
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


    public class SegmentAssetFilterSegmentAction : SegmentActionObjectVirtualFilterAsset
    {
        public override ObjectVirtualAssetInfoType objectVirtualAssetInfoType { get { return ObjectVirtualAssetInfoType.Segment; } }
    }

    public class SegmentAssetFilterSubscriptionAction : SegmentActionObjectVirtualFilterAsset
    {
        public override ObjectVirtualAssetInfoType objectVirtualAssetInfoType { get { return ObjectVirtualAssetInfoType.Subscription; } }
    }

    public abstract class SegmentActionObjectVirtualAssetBlockAction : SegmentActionObjectVirtualAsset
    {
    }

    public class SegmentBlockPlaybackSubscriptionAction : SegmentActionObjectVirtualAssetBlockAction
    {
        public override ObjectVirtualAssetInfoType objectVirtualAssetInfoType { get { return ObjectVirtualAssetInfoType.Subscription; } }
    }

    public class SegmentBlockCancelSubscriptionAction : SegmentActionObjectVirtualAssetBlockAction
    {
        public override ObjectVirtualAssetInfoType objectVirtualAssetInfoType { get { return ObjectVirtualAssetInfoType.Subscription; } }
    }

    public class SegmentBlockPurchaseSubscriptionAction : SegmentActionObjectVirtualAssetBlockAction
    {
        public override ObjectVirtualAssetInfoType objectVirtualAssetInfoType { get { return ObjectVirtualAssetInfoType.Subscription; } }
    }
}
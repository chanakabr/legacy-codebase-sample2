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
        public virtual bool AddSegmentsIds()
        {
            return true;
        }

        public virtual bool UpdateSegmentIds(SegmentBaseValue source)
        {
            return true;
        }

        internal virtual bool HasSegmentId(long segmentId)
        {
            return false;
        }
    }
}
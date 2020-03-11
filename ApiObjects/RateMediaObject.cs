using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects
{
    [Serializable]
    public class RateMediaObject
    {
        public Int32 nSum;
        public Int32 nCount;
        public double nAvg;

        public GenericWriteResponse oStatus;

        public RateMediaObject()
        {
            nSum = 0;
            nCount = 0;
            nAvg = 0.0;
            oStatus = new GenericWriteResponse();
        }

        public void Initialize(Int32 nTotalSum, Int32 nTotalCount, double dAvg)
        {
            nSum = nTotalSum;
            nCount = nTotalCount;
            nAvg = dAvg;
        }
    }
}

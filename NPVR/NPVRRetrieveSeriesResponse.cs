using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR
{
    public class NPVRRetrieveSeriesResponse
    {
        public string entityID;
        public bool isOK;
        public string msg;
        public int totalItems;
        public List<RecordedSeriesObject> results;


    }
}

using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Catalog.Response
{
    public class NPVRSeriesResponse : BaseResponse
    {
        public List<RecordedSeriesObject> recordedSeries;
        public int totalItems;
        public string entityID;
        public string msg;
    }
}

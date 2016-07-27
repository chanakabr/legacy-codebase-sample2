using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApiObjects.TimeShiftedTv
{
    public class RecordResult
    {
        public string RecordingId { get; set; } 
        public bool ActionSuccess { get; set; }     
        public List<RecordingLink> Links { get; set; }// not return form WS
        
        public int FailReason { get; set; }
        public string ProviderStatusCode { get; set; }
        public string ProviderStatusMessage { get; set; }
    }

    public class RecordingLink
    {
        public string DeviceType { get; set; }
        public int DeviceTypeBrand { get; set; }
        public string Url { get; set; }
    }
}

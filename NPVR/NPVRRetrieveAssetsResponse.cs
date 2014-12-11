using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NPVR
{
    public class NPVRRetrieveAssetsResponse
    {
        public string entityID;
        public int totalItems;
        public bool isOK;
        public string msg;
        public List<RecordedEPGChannelProgrammeObject> results;
    }
}

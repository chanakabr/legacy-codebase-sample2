using ApiObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Core.Catalog.Response
{
    public class NPVRRetrieveResponse : BaseResponse
    {
        public List<RecordedEPGChannelProgrammeObject> recordedProgrammes;

        public NPVRRetrieveResponse()
            : base()
        {
            this.recordedProgrammes = new List<RecordedEPGChannelProgrammeObject>();
        }
    }
}

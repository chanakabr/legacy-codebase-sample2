using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Catalog;

namespace WebAPI.ObjectsConvertor
{
    public class RecordingObj : BaseObject
    {
        public long recordingId;

        public ProgramObj program;
    }
}
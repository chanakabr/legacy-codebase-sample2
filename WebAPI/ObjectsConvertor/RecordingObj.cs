using Core.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebAPI.ObjectsConvertor
{
    public class RecordingObj : BaseObject
    {
        public long recordingId;

        public ProgramObj program;
    }

    public class ScheduledRecordingObj : RecordingObj
    {
        public WebAPI.Models.ConditionalAccess.KalturaRecordingType recordingType;
    }
}
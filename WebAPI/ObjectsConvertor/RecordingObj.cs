using Core.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Models.ConditionalAccess;

namespace WebAPI.ObjectsConvertor
{
    public class RecordingObj : BaseObject
    {
        public long RecordingId { get; set; }

        public KalturaRecordingType RecordingType { get; set; }

        public ProgramObj Program { get; set; }
    }

}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Catalog
{
    public class RecordingObj : BaseObject
    {
        public long RecordingId { get; set; }
        public ApiObjects.RecordingType? RecordingType { get; set; }
        public ProgramObj Program { get; set; }
        public bool IsMulti { get; set; }
    }
}

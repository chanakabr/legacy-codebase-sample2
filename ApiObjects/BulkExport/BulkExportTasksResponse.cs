using ApiObjects.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.BulkExport
{
    public class BulkExportTasksResponse
    {
        public List<BulkExportTask> Tasks{ get; set; }

        public Status Status { get; set; }
    }

    public class BulkExportTaskResponse
    {
        public BulkExportTask Task { get; set; }

        public Status Status { get; set; }
    }
}

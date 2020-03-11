using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiObjects.BulkExport
{
    public class BulkExportTask
    {
        public long Id { get; set; }

        public string ExternalKey { get; set; }

        public string Name { get; set; }

        public eBulkExportDataType DataType { get; set; }

        public string Filter { get; set; }

        public eBulkExportExportType ExportType { get; set; }

        public long Frequency { get; set; }

        public string Version { get; set; }

        public bool InProcess { get; set; }

        public DateTime? LastProcess { get; set; }

        public List<int> VodTypes { get; set; }

        public string NotificationUrl { get; set; }

        public bool IsActive { get; set; }

        public BulkExportTask()
        {
            VodTypes = new List<int>();
        }
    }
}

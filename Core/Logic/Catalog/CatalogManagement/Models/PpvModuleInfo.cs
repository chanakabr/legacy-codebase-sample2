using System;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class PpvModuleInfo
    {
        public long PpvModuleId { get; set; }
        public long FileTypeId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
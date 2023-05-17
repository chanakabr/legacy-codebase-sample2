using System.Collections.Generic;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class ChannelSearchOptionsResult
    {
        public bool ShouldSearchMedia { get; set; }

        public bool ShouldSearchEpg { get; set; }

        public bool ShouldUseSearchEndDate { get; set; }

        public IReadOnlyCollection<int> MediaTypes { get; set; }
    }
}
using System.Collections.Generic;
using ApiObjects.SearchObjects;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class ChannelSearchOptionsContext
    {
        public CatalogGroupCache CatalogGroupCache { get; set; }

        public IEnumerable<int> MediaTypes { get; set; }

        public BooleanPhraseNode InitialTree { get; set; }

        public bool ShouldUseSearchEndDate { get; set; }
    }
}
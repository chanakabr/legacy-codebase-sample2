using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class ChannelEsOrderingResult
    {
        public IReadOnlyCollection<IEsOrderByField> EsOrderByFields { get; set; }
        public OrderObj Order { get; set; }
        public IReadOnlyCollection<long> SpecificOrder { get; set; }
    }
}
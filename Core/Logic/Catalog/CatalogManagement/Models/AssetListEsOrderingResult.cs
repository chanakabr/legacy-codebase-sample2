using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ApiLogic.Catalog.CatalogManagement.Models
{
    public class AssetListEsOrderingResult
    {
        public IReadOnlyCollection<IEsOrderByField> EsOrderByFields { get; set; }
        public OrderObj Order { get; set; }
    }
}
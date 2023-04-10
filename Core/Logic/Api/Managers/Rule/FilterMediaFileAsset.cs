using System.Collections.Generic;
using ApiObjects.Catalog;

namespace ApiLogic.Api.Managers.Rule
{
    public class FilterMediaFileAsset
    {
        public long AssetId { get; set; }

        public IEnumerable<Metas> Metas { get; set; }

        public IEnumerable<Tags> Tags { get; set; }
    }
}
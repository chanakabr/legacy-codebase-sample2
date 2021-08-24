using System.Collections.Generic;
using ApiObjects;

namespace ApiLogic.Catalog.Tree
{
    public interface IFilterTreeResultProcessor
    {
        IndexesModel ProcessResults(eCutType operand, IEnumerable<IndexesModel> results);
    }
}
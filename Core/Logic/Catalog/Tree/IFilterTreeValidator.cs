using System.Collections.Generic;
using ApiObjects.SearchObjects;

namespace ApiLogic.Catalog.Tree
{
    public interface IFilterTreeValidator
    {
        IndexesModel ValidateTree(BooleanPhraseNode tree);
        
        IndexesModel ValidateTree(BooleanPhraseNode tree, IEnumerable<int> mediaTypes);
    }
}

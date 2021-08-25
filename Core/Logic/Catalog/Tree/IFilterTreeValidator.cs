using ApiObjects.SearchObjects;

namespace ApiLogic.Catalog.Tree
{
    public interface IFilterTreeValidator
    {
        IndexesModel ValidateTree(BooleanPhraseNode tree);
    }
}
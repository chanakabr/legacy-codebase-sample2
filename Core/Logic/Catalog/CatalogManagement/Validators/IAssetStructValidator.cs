using ApiObjects.Catalog;
using ApiObjects.Response;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Validators
{
    public interface IAssetStructValidator
    {
        Status ValidateBasicMetaIds(CatalogGroupCache catalogGroupCache, AssetStruct assetStruct, bool isProgramStruct);
        Status ValidateNoSystemNameDuplicationOnMetaIds(CatalogGroupCache catalogGroupCache, AssetStruct assetStruct);
    }
}
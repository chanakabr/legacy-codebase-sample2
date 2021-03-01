using System.Collections.Generic;
using ApiObjects;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Helpers
{
    public interface IEpgAssetMultilingualMutator
    {
        bool IsAllowedToFallback(int groupId, IDictionary<string, LanguageObj> languages);
        
        void PrepareEpgAsset(EpgAsset epgAsset, LanguageObj defaultLanguage, IDictionary<string, LanguageObj> languageMapByCode);
    }
}
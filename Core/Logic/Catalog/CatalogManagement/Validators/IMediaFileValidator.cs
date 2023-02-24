using System.Collections.Generic;
using Core.Catalog;

namespace ApiLogic.Catalog.CatalogManagement.Validators
{
    public interface IMediaFileValidator
    {
        IDictionary<string, IEnumerable<string>> GetValidatedDynamicData(MediaFileType mediaFileType, IDictionary<string, IEnumerable<string>> dynamicData);
    }
}
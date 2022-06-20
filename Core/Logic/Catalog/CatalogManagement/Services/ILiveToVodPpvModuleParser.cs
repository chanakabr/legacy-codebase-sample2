using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Models;
using Core.Catalog;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public interface ILiveToVodPpvModuleParser
    {
        IEnumerable<PpvModuleInfo> GetParsedPpv(ProgramAsset asset);
    }
}
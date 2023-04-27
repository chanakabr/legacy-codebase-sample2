using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.Tree;
using GroupsCacheManager;

namespace ApiLogic.Catalog.CatalogManagement.Services
{
    public class ChannelSearchOptionsService : IChannelSearchOptionsService
    {
        private static readonly Lazy<IChannelSearchOptionsService> LazyInstance = new Lazy<IChannelSearchOptionsService>(
            () => new ChannelSearchOptionsService(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IChannelSearchOptionsService Instance => LazyInstance.Value;

        public ChannelSearchOptionsResult ResolveKsqlChannelSearchOptions(ChannelSearchOptionsContext context)
        {
            var result = new ChannelSearchOptionsResult();
            var doesGroupUsesTemplates = context.CatalogGroupCache != null;
            var resultMediaTypes = context.MediaTypes?.ToList() ?? new List<int>();

            // if for some reason we are left with "0" in the list of media types (for example: "0, 424, 425"), let's ignore this 0.
            // In non-opc accounts,
            // this 0 probably came when TVM/CRUD decided this channel is for all types, but forgot to delete it when the others join in (424, 425 etc.).
            // in opc accounts it just means EPG
            // IN ANY CASE
            // we don't want to search for asset_type = 0, because the assets are not indexed with it! EPGs are just indexed in their index
            // and media will never have 0 media type

            var hasZeroMediaType = resultMediaTypes.Remove(0);
            var hasMinusTwentySixMediaType = resultMediaTypes.Remove(Channel.EPG_ASSET_TYPE);

            var filterTreeValidator = new FilterTreeValidator(
                new FilterTreeResultProcessor(), context.CatalogGroupCache?.GetProgramAssetStructId());
            var indexesModel = filterTreeValidator.ValidateTree(context.InitialTree, resultMediaTypes);
            // Special case - if no type was specified or "All" is contained, search all types
            if ((!doesGroupUsesTemplates && hasZeroMediaType && !resultMediaTypes.Any()) ||
                (!hasZeroMediaType && !resultMediaTypes.Any()))
            {
                result.ShouldSearchEpg = indexesModel?.ShouldSearchEpg ?? true;
                result.ShouldSearchMedia = indexesModel?.ShouldSearchMedia ?? true;
                result.ShouldUseSearchEndDate = context.ShouldUseSearchEndDate;
            }
            else
            {
                if (doesGroupUsesTemplates)
                {
                    var programAssetStructId = context.CatalogGroupCache.GetRealAssetStructId(0, out var hasProgramStructMediaType);
                    if (hasProgramStructMediaType)
                    {
                        hasProgramStructMediaType = resultMediaTypes.Remove((int)programAssetStructId);
                    }

                    // in OPC accounts, 0 media type means EPG
                    if (hasZeroMediaType || hasProgramStructMediaType)
                    {
                        result.ShouldSearchEpg = true;
                        result.ShouldUseSearchEndDate = context.ShouldUseSearchEndDate;
                    }

                    result.ShouldSearchEpg = indexesModel?.ShouldSearchEpg ?? result.ShouldSearchEpg;
                    result.ShouldSearchMedia = indexesModel?.ShouldSearchMedia ?? result.ShouldSearchMedia;
                }
                else
                {
                    // in non-OPC accounts, -26 media type means EPG
                    if (hasMinusTwentySixMediaType)
                    {
                        result.ShouldSearchEpg = true;
                        result.ShouldUseSearchEndDate = context.ShouldUseSearchEndDate;
                    }
                }
            }

            // If there are items left in media types after removing 0, we are searching for media
            if (resultMediaTypes.Count > 0)
            {
                result.ShouldSearchMedia = true;
            }

            result.MediaTypes = resultMediaTypes;

            return result;
        }
    }
}
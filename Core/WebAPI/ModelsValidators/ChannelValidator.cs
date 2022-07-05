using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class ChannelValidator
    {
        private static readonly int ORDERING_PARAMETERS_MAX_COUNT = 2;
        
        public static void ValidateForInsert(this KalturaChannel model)
        {
            if (string.IsNullOrEmpty(model.SystemName))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (model.Name?.Values == null || model.Name.Values.Count == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
            }

            model.Name.Validate("multilingualName");

            if (model.Description != null)
            {
                if (model.Description.Values == null || model.Description.Values.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
                }
                else
                {
                    model.Description.Validate("multilingualDescription");
                }
            }

            model.OrderBy?.Validate(model.GetType());

            if (!model.OrderingParameters.Any())
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "orderingParametersEqual");
            }

            if (model.OrderingParameters.Count > ORDERING_PARAMETERS_MAX_COUNT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "orderingParametersEqual", ORDERING_PARAMETERS_MAX_COUNT);
            }

            if (model.OrderingParameters.Count > 1 && model.GroupBy != null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "groupBy", "orderingParametersEqual");
            }
            
            switch (model)
            {
                case KalturaManualChannel c: c.ValidateForInsert(); break;
                case KalturaDynamicChannel _: break;
                case KalturaChannel _: break;
                default: throw new NotImplementedException($"ValidateForInsert for {model.objectType} is not implemented");
            }
        }
        
        public static void ValidateForUpdate(this KalturaChannel model)
        {
            if (model.SystemName != null && model.SystemName == string.Empty)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "systemName");
            }

            if (model.Name != null)
            {
                if (model.Name.Values == null || model.Name.Values.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "name");
                }
                else
                {
                    model.Name.Validate("multilingualName");
                }
            }

            if (model.Description != null)
            {
                if (model.Description.Values == null || model.Description.Values.Count == 0)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "description");
                }
                else
                {
                    model.Description.Validate("multilingualDescription", true, false);
                }
            }

            model.OrderBy?.Validate(model.GetType());

            if (model.OrderingParameters?.Count > ORDERING_PARAMETERS_MAX_COUNT)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MAX_ITEMS_CROSSED, "orderingParametersEqual", ORDERING_PARAMETERS_MAX_COUNT);
            }

            if (model.OrderingParameters?.Count > 1 && model.GroupBy != null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_CONFLICT_EACH_OTHER, "groupBy", "orderingParametersEqual");
            }
            
            switch (model)
            {
                case KalturaManualChannel c: c.ValidateForUpdate(); break;
                case KalturaDynamicChannel _: break;
                case KalturaChannel _: break;
                default: throw new NotImplementedException($"ValidateForInsert for {model.objectType} is not implemented");
            }
        }
    }

    public static class ManualChannelValidator
    {
        private static void ValidateMediaIds(this KalturaManualChannel model)
        {
            if (!string.IsNullOrEmpty(model.MediaIds))
            {
                string[] stringValues = model.MediaIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string stringValue in stringValues)
                {
                    long value;
                    if (!long.TryParse(stringValue, out value) || value < 1)
                    {
                        throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaManualChannel.mediaIds");
                    }
                }
            }
        }

        private static void ValidateAssets(this KalturaManualChannel model)
        {
            if (model.Assets != null)
            {
                List<string> ids = model.Assets.Where(x => x.Type == KalturaManualCollectionAssetType.media).Select(x => x.Id).ToList();
                var duplicates = ids.GroupBy(x => x).Count(t => t.Count() > 1);
                if (duplicates > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "assets");
                }

                ids = model.Assets.Where(x => x.Type == KalturaManualCollectionAssetType.epg).Select(x => x.Id).ToList();
                duplicates = ids.GroupBy(x => x).Count(t => t.Count() > 1);
                if (duplicates > 1)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENTS_VALUES_DUPLICATED, "assets");
                }
            }
        }

        public static void ValidateForInsert(this KalturaManualChannel model)
        {
            if (model.Assets != null && !string.IsNullOrEmpty(model.MediaIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaManualChannel.assets", "KalturaManualChannel.mediaIds");
            }

            model.ValidateMediaIds();
            model.ValidateAssets();
        }

        public static void ValidateForUpdate(this KalturaManualChannel model)
        {
            if (model.Assets != null && !string.IsNullOrEmpty(model.MediaIds))
            {
                throw new BadRequestException(BadRequestException.ARGUMENTS_CONFLICTS_EACH_OTHER, "KalturaManualChannel.assets", "KalturaManualChannel.mediaIds");
            }

            model.ValidateMediaIds();
            model.ValidateAssets();
        }
    }
}
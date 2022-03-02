using ApiLogic.Api.Managers;
using ApiObjects.Base;
using Core.Catalog.CatalogManagement;
using System;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.Upload;

namespace WebAPI.ModelsValidators
{
    public static class BulkUploadObjectDataValidator
    {
        public static void Validate(this KalturaBulkUploadObjectData model, int groupId)
        {
            switch (model)
            {
                case KalturaBulkUploadLiveAssetData c: c.Validate(groupId); break;
                case KalturaBulkUploadMediaAssetData c: c.Validate(); break;
                case KalturaBulkUploadProgramAssetData c: c.Validate(); break;
                case KalturaBulkUploadUdidDynamicListData c: c.Validate(groupId); break;
                default: throw new NotImplementedException($"Validate for {model.objectType} is not implemented");
            }
        }

        private static void Validate(this KalturaBulkUploadMediaAssetData model)
        {
            if (model.TypeId < 1)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_MIN_VALUE_CROSSED, "bulkUploadAssetData.typeId", 1);
            }
        }

        private static void Validate(this KalturaBulkUploadProgramAssetData model)
        {
            if (model.TypeId != 0)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "bulkUploadAssetData.typeId");
            }
        }

        private static void Validate(this KalturaBulkUploadLiveAssetData model, int groupId)
        {
            var linearMediaTypes = CatalogManager.Instance.GetLinearMediaTypes(groupId);
            if (linearMediaTypes.All(x => x.Id != model.TypeId))
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "bulkUploadAssetData.typeId");
            }
        }

        private static void Validate(this KalturaBulkUploadUdidDynamicListData model, int groupId)
        {
            var contextData = new ContextData(groupId);
            var dynamicListResponse = DynamicListManager.Instance.Get(contextData, model.DynamicListId);
            if (!dynamicListResponse.HasObject())
            {
                throw new ClientException(dynamicListResponse.Status);
            }

            if (dynamicListResponse.Object.Type != ApiObjects.DynamicListType.UDID)
            {
                throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "dynamicListId");
            }
        }
    }
}

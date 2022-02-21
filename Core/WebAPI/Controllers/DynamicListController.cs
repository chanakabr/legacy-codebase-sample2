using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.ModelsValidators;
using WebAPI.Utils;
using WebAPI.ObjectsConvertor.Extensions;

namespace WebAPI.Controllers
{
    [Service("dynamicList")]
    public class DynamicListController : IKalturaController
    {
        /// <summary>
        /// Add new KalturaDynamicList
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objectToAdd">KalturaDynamicList Object to add</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ExceededMaxCapacity)]
        static public KalturaDynamicList Add(KalturaDynamicList objectToAdd)
        {
            var contextData = KS.GetContextData();

            objectToAdd.ValidateForAdd();

            // call to manager and get response
            KalturaDynamicList response;
            switch (objectToAdd)
            {
                case KalturaUdidDynamicList c: response = AddUdidDynamicList(contextData, c); break;
                default: throw new NotImplementedException($"Add for {objectToAdd.objectType} is not implemented");
            }

            return response;
        }

        private static KalturaUdidDynamicList AddUdidDynamicList(ContextData contextData, KalturaUdidDynamicList udidDynamicList)
        {
            Func<UdidDynamicList, GenericResponse<UdidDynamicList>> addFunc = (UdidDynamicList coreObject) =>
                DynamicListManager.Instance.AddUdidDynamicList(contextData, coreObject);
            var result = ClientUtils.GetResponseFromWS(udidDynamicList, addFunc);
            return result;
        }

        /// <summary>
        /// Update existing KalturaDynamicList
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">id of KalturaDynamicList to update</param>
        /// <param name="objectToUpdate">KalturaDynamicList Object to update</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.DynamicListDoesNotExist)]
        static public KalturaDynamicList Update(long id, KalturaDynamicList objectToUpdate)
        {
            var contextData = KS.GetContextData();
            objectToUpdate.Id = id;

            // call to manager and get response
            KalturaDynamicList response;
            switch (objectToUpdate)
            {
                case KalturaUdidDynamicList c: response = UpdateUdidDynamicList(contextData, c); break;
                default: throw new NotImplementedException($"Update for {objectToUpdate.objectType} is not implemented");
            }

            return response;
        }

        private static KalturaUdidDynamicList UpdateUdidDynamicList(ContextData contextData, KalturaUdidDynamicList udidDynamicList)
        {
            Func<UdidDynamicList, GenericResponse<UdidDynamicList>> coreFunc = (UdidDynamicList objectToUpdate) =>
                DynamicListManager.Instance.UpdateUdidDynamicList(contextData, objectToUpdate);

            var result = ClientUtils.GetResponseFromWS(udidDynamicList, coreFunc);

            return result;
        }

        /// <summary>
        /// Delete existing DynamicList
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">DynamicList identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.DynamicListDoesNotExist)]
        static public void Delete(long id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => DynamicListManager.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        /// <summary>
        /// Returns the list of available DynamicList
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <param name="pager">Pager</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaDynamicListListResponse List(KalturaDynamicListFilter filter, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            filter.Validate();

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }
            var corePager = AutoMapper.Mapper.Map<CorePager>(pager);

            KalturaGenericListResponse<KalturaDynamicList> result;
            switch (filter)
            {
                case KalturaDynamicListIdInFilter f: result = ListByDynamicListIdInFilter(contextData, corePager, f); break;
                case KalturaUdidDynamicListSearchFilter f: result = ListByUdidDynamicListSearchFilter(contextData, corePager, f); break;
                default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
            }

            var response = new KalturaDynamicListListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            return response;
        }

        private static KalturaGenericListResponse<KalturaDynamicList> ListByDynamicListIdInFilter(ContextData contextData, CorePager pager, KalturaDynamicListIdInFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<DynamicListnIdInFilter>(filter);

            Func<GenericListResponse<DynamicList>> listFunc = () =>
                DynamicListManager.Instance.GetDynamicListsByIds(contextData, coreFilter);

            KalturaGenericListResponse<KalturaDynamicList> triggerCampaignResponse =
               ClientUtils.GetResponseListFromWS<KalturaDynamicList, DynamicList>(listFunc);

            var response = new KalturaGenericListResponse<KalturaDynamicList>();
            response.Objects.AddRange(triggerCampaignResponse.Objects);
            response.TotalCount = triggerCampaignResponse.TotalCount;
            return response;
        }

        private static KalturaGenericListResponse<KalturaDynamicList> ListByUdidDynamicListSearchFilter(ContextData contextData, CorePager pager, KalturaUdidDynamicListSearchFilter filter)
        {
            var coreFilter = AutoMapper.Mapper.Map<DynamicListSearchFilter>(filter);
            coreFilter.TypeEqual = DynamicListType.UDID;
            Func<GenericListResponse<DynamicList>> listFunc = () =>
                DynamicListManager.Instance.SearchDynamicLists(contextData, coreFilter, pager);

            KalturaGenericListResponse<KalturaDynamicList> triggerCampaignResponse =
               ClientUtils.GetResponseListFromWS<KalturaDynamicList, DynamicList>(listFunc);

            var response = new KalturaGenericListResponse<KalturaDynamicList>();
            response.Objects.AddRange(triggerCampaignResponse.Objects);
            response.TotalCount = triggerCampaignResponse.TotalCount;
            return response;
        }

        /// <summary>
        /// Add new bulk upload batch job Conversion profile id can be specified in the API.
        /// </summary>
        /// <param name="fileData">fileData</param>
        /// <param name="jobData">jobData</param>
        /// <param name="bulkUploadData">bulkUploadData</param>
        /// <returns></returns>
        [Action("addFromBulkUpload")]
        [ApiAuthorize]
        [Throws(eResponseStatus.FileDoesNotExists)]
        [Throws(eResponseStatus.FileAlreadyExists)]
        [Throws(eResponseStatus.ErrorSavingFile)]
        [Throws(eResponseStatus.FileIdNotInCorrectLength)]
        [Throws(eResponseStatus.InvalidFileType)]
        [Throws(eResponseStatus.EnqueueFailed)]
        [Throws(eResponseStatus.BulkUploadDoesNotExist)]
        [Throws(eResponseStatus.AccountEpgIngestVersionDoesNotSupportBulk)]
        [Throws(eResponseStatus.FileExceededMaxSize)]
        [Throws(eResponseStatus.FileExtensionNotSupported)]
        [Throws(eResponseStatus.FileMimeDifferentThanExpected)]
        [Throws(eResponseStatus.DynamicListDoesNotExist)]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        public static KalturaBulkUpload AddFromBulkUpload(KalturaOTTFile fileData, KalturaBulkUploadExcelJobData jobData, KalturaBulkUploadDynamicListData bulkUploadData)
        {
            KalturaBulkUpload bulkUpload = null;

            int groupId = KS.GetFromRequest().GroupId;
            long userId = Utils.Utils.GetUserIdFromKs();

            try
            {
                if (fileData == null || (fileData.File == null && string.IsNullOrEmpty(fileData.path)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "fileData");
                }

                if (jobData == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "jobData");
                }

                if (bulkUploadData == null)
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "bulkUploadData");
                }

                jobData.Validate(fileData);
                bulkUploadData.Validate(groupId);

                var dynamicListType = bulkUploadData.GetBulkUploadObjectType();

                bulkUpload =
                    ClientsManager.CatalogClient().AddBulkUpload(groupId, userId, dynamicListType, jobData, bulkUploadData, fileData);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return bulkUpload;
        }
    }
}
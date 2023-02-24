using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ApiObjects.MediaFiles;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using DAL;
using Microsoft.Extensions.Logging;
using Phx.Lib.Log;

namespace ApiLogic.Catalog.CatalogManagement.Managers
{
    public class MediaFileTypeDynamicDataManager : IMediaFileTypeDynamicDataManager
    {
        private static readonly Lazy<MediaFileTypeDynamicDataManager> Lazy =
            new Lazy<MediaFileTypeDynamicDataManager>(
                () => new MediaFileTypeDynamicDataManager(
                    MediaFileTypeDynamicDataDal.Instance, LayeredCache.Instance),
                LazyThreadSafetyMode.PublicationOnly);

        public static IMediaFileTypeDynamicDataManager Instance => Lazy.Value;

        private readonly IMediaFileTypeDynamicDataDal _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly ILogger _logger;

        public MediaFileTypeDynamicDataManager(IMediaFileTypeDynamicDataDal repository, ILayeredCache layeredCache)
            : this(repository, layeredCache, new KLogger(nameof(MediaFileTypeDynamicDataManager)))
        {
            _repository = repository;
        }

        public MediaFileTypeDynamicDataManager(IMediaFileTypeDynamicDataDal repository, ILayeredCache layeredCache,
            ILogger logger)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _logger = logger;
        }

        /// <summary>
        /// Add new Dynamic Data value for MediaFileType
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="dynamicDataKeyValue">Dynamic Data value to add</param>
        /// <param name="userId">Id of the user called add</param>
        /// <returns>Created Dynamic Data value</returns>
        public GenericResponse<MediaFileTypeDynamicDataKeyValue> AddMediaFileDynamicDataValue(int groupId,
            MediaFileTypeDynamicDataKeyValue dynamicDataKeyValue, long userId)
        {
            var result = _repository.InsertMediaFileDynamicDataValue(groupId, dynamicDataKeyValue.MediaFileTypeId,
                dynamicDataKeyValue.MediaFileTypeKeyName, dynamicDataKeyValue.Value, userId);

            if (!result.Status.IsOkStatusCode())
            {
                return result;
            }

            var invalidationKey = LayeredCacheKeys.GetGroupMediaFileTypesInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                _logger.LogError(
                    $"Failed to set invalidation key on {nameof(AddMediaFileDynamicDataValue)}, key = {invalidationKey}");
            }

            return result;
        }

        /// <summary>
        /// Get list of the Dynamic Data values matched to the search criteria
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="idIn">List of Ids to fetch</param>
        /// <param name="mediaFileTypeId">MediaFileType Id</param>
        /// <param name="mediaFileTypeKeyName">Dynamic Data key name</param>
        /// <param name="valueEqual">Key value to match</param>
        /// <param name="valueStartsWith">Key value start with</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>List of the Dynamic Data values</returns>
        public GenericListResponse<MediaFileTypeDynamicDataKeyValue> GetMediaFileDynamicDataValues(int groupId,
            List<long> idIn,
            long? mediaFileTypeId, string mediaFileTypeKeyName, string valueEqual, string valueStartsWith,
            int pageIndex, int pageSize)
        {
            var result = new GenericListResponse<MediaFileTypeDynamicDataKeyValue>();

            var items = idIn?.Any() == true
                ? _repository.GetMediaFileDynamicDataKeyValuesByIds(groupId, idIn)
                : _repository.GetMediaFileDynamicDataKeyValuesByMediaFileTypeId(groupId, mediaFileTypeId,
                    mediaFileTypeKeyName);

            if (items == null)
            {
                return result;
            }

            items = ApplyMediaFileDynamicDataKeyValueFilters(items, valueEqual, valueStartsWith).ToList();

            result.TotalItems = items.Count;
            result.Objects = ApplyPagination(items, pageIndex, pageSize).ToList();
            result.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

            return result;
        }

        /// <summary>
        /// Deletes Dynamic Data value from MediaFileType and related MediaFiles.
        /// </summary>
        /// <param name="groupId">Group Id</param>
        /// <param name="id">Dynamic Data Value Id</param>
        /// <param name="userId">Id of the user called deletion</param>
        /// <returns>True if success, error status otherwise</returns>
        public GenericResponse<bool> DeleteMediaFileDynamicDataValue(int groupId, long id, long userId)
        {
            var result = _repository.DeleteMediaFileDynamicDataValue(groupId, id, userId);

            if (!result.IsOkStatusCode())
            {
                return result;
            }

            var invalidationKey = LayeredCacheKeys.GetGroupMediaFileTypesInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                _logger.LogError(
                    $"Failed to set invalidation key on {nameof(DeleteMediaFileDynamicDataValue)}, key = {invalidationKey}");
            }

            return result;
        }

        private static IEnumerable<MediaFileTypeDynamicDataKeyValue> ApplyMediaFileDynamicDataKeyValueFilters(
            IEnumerable<MediaFileTypeDynamicDataKeyValue> items, string valueEqual, string valueStartsWith)
        {
            if (items == null)
            {
                return null;
            }

            items = items.OrderBy(item => item.Value);

            if (!string.IsNullOrEmpty(valueEqual))
            {
                return items.Where(item => valueEqual.Equals(item.Value, StringComparison.InvariantCultureIgnoreCase));
            }

            if (!string.IsNullOrEmpty(valueStartsWith))
            {
                return items.Where(
                    item => item.Value.StartsWith(valueStartsWith, StringComparison.InvariantCultureIgnoreCase));
            }

            return items;
        }

        private static IEnumerable<MediaFileTypeDynamicDataKeyValue> ApplyPagination(
            IEnumerable<MediaFileTypeDynamicDataKeyValue> items, int pageIndex, int pageSize)
        {
            if (items == null)
            {
                return null;
            }

            if (pageSize > 0)
            {
                items = items.Skip(pageIndex * pageSize).Take(pageSize);
            }

            return items;
        }
    }
}
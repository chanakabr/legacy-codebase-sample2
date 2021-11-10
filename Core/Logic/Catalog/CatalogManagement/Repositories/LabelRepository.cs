using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.SearchObjects;
using CachingProvider.LayeredCache;
using DAL;
using KLogMonitor;
using Microsoft.Extensions.Logging;
using ODBCWrapper;

namespace ApiLogic.Catalog.CatalogManagement.Repositories
{
    public class LabelRepository : ILabelRepository
    {
        private readonly ILabelDal _labelDal;
        private readonly ILayeredCache _cache;
        private readonly ILogger _logger;

        private static readonly Lazy<LabelRepository> LazyInstance = new Lazy<LabelRepository>(() =>
                new LabelRepository(new LabelDal(), LayeredCache.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static LabelRepository Instance => LazyInstance.Value;

        public LabelRepository(ILabelDal labelDal, ILayeredCache cache)
            : this(labelDal,cache, new KLogger(nameof(LabelRepository)))
        {
        }

        public LabelRepository(ILabelDal labelDal, ILayeredCache cache, ILogger logger)
        {
            _labelDal = labelDal;
            _cache = cache;
            _logger = logger;
        }

        public GenericResponse<LabelValue> Add(long groupId, LabelValue labelValue, long updaterId)
        {
            try
            {
                var dataSet = _labelDal.Add(groupId, (int)labelValue.EntityAttribute, labelValue.Value.Trim(), updaterId);
                var response = CreateLabelValue(dataSet);
                if (response.IsOkStatusCode())
                {
                    InvalidateCache(groupId);
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Add)}: {e.Message}.");

                return new GenericResponse<LabelValue>();
            }
        }

        public GenericResponse<LabelValue> Update(long groupId, LabelValue labelValue, long updaterId)
        {
            try
            {
                var dataSet = _labelDal.Update(groupId, labelValue.Id, labelValue.Value.Trim(), updaterId);
                var response = CreateLabelValue(dataSet);
                if (response.IsOkStatusCode())
                {
                    InvalidateCache(groupId);
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Update)}: {e.Message}.");

                return new GenericResponse<LabelValue>();
            }
        }

        public Status Delete(int groupId, long labelId, long updaterId)
        {
            try
            {
                var result = _labelDal.Delete(groupId, labelId, updaterId);
                var response = result
                    ? Status.Ok
                    : new Status(eResponseStatus.LabelDoesNotExist);
                if (result)
                {
                    InvalidateCache(groupId);
                }

                return response;
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(Delete)}: {e.Message}.");

                return Status.Error;
            }
        }

        public GenericListResponse<LabelValue> List(long groupId)
        {
            try
            {
                List<LabelValue> labelValues = null;
                var labelsKey = LayeredCacheKeys.GetLabelsKey(groupId);
                var invalidationKeys = new List<string> { LayeredCacheKeys.GetLabelsInvalidationKey(groupId) };
                var cacheResult = _cache.Get(
                    labelsKey,
                    ref labelValues,
                    GetLabels,
                    new Dictionary<string, object> { { "groupId", groupId } },
                    (int)groupId,
                    LayeredCacheConfigNames.GET_LABELS_CACHE_CONFIG_NAME,
                    invalidationKeys);

                if (cacheResult)
                {
                    return new GenericListResponse<LabelValue>(Status.Ok, labelValues);
                }
                else
                {
                    _logger.LogError($"{nameof(List)} - Failed get data from cache {nameof(groupId)} = {groupId}.");

                    return new GenericListResponse<LabelValue>();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Error while executing {nameof(List)}: {e.Message}.");

                return new GenericListResponse<LabelValue>();
            }
        }

        public void InvalidateCache(long groupId)
        {
            var invalidationKey = LayeredCacheKeys.GetLabelsInvalidationKey(groupId);
            var result = _cache.SetInvalidationKey(invalidationKey);
            if (!result)
            {
                _logger.LogError("Failed to set invalidation key for labels. key = {0}.", invalidationKey);
            }
        }

        private List<LabelValue> CreateLabelValues(DataSet dataSet)
        {
            if (dataSet == null || dataSet.Tables.Count != 1 || dataSet.Tables[0] == null)
            {
                return null;
            }

            var labelValues = new List<LabelValue>();
            foreach (DataRow row in dataSet.Tables[0].Rows)
            {
                var labelValue = CreateLabelValue(row);
                labelValues.Add(labelValue);
            }

            return labelValues;
        }

        private GenericResponse<LabelValue> CreateLabelValue(DataSet dataSet)
        {
            if (dataSet == null || dataSet.Tables.Count != 1 || dataSet.Tables[0] == null || dataSet.Tables[0].Rows.Count != 1)
            {
                return new GenericResponse<LabelValue>();
            }

            GenericResponse<LabelValue> response;

            var row = dataSet.Tables[0].Rows[0];
            var dbStatusCode = Utils.GetIntSafeVal(row, "StatusCode", 0);
            if (dbStatusCode != 0)
            {
                var status = ConvertDbStatusCodeToStatus(dbStatusCode, row);
                response = new GenericResponse<LabelValue>(status);
            }
            else
            {
                var labelValue = CreateLabelValue(row);
                response = new GenericResponse<LabelValue>(Status.Ok, labelValue);
            }

            return response;
        }

        private LabelValue CreateLabelValue(DataRow row)
        {
            var id = Utils.GetLongSafeVal(row, "ID");
            var attributeId = Utils.GetIntSafeVal(row, "ENTITY_ATTRIBUTE");
            var value = Utils.GetSafeStr(row, "VALUE");
            var labelValue = new LabelValue(id, (EntityAttribute)attributeId, value);

            return labelValue;
        }

        private Status ConvertDbStatusCodeToStatus(int dbStatusCode, DataRow row)
        {
            switch (dbStatusCode)
            {
                case -222:
                    var collisionEntityAttribute = Utils.GetLongSafeVal(row, "CollisionEntityAttribute");
                    var collisionValue = Utils.GetSafeStr(row, "CollisionValue");

                    return new Status(eResponseStatus.LabelAlreadyInUse, $"{collisionValue} is already used in the context of {(EntityAttribute)collisionEntityAttribute}.");
                case -333:
                    return new Status(eResponseStatus.LabelDoesNotExist);
                default:
                    return Status.Error;
            }
        }

        private Tuple<List<LabelValue>, bool> GetLabels(Dictionary<string, object> funcParams)
        {
            List<LabelValue> labelValues = null;
            var result = false;
            try
            {
                var groupId = (long)funcParams["groupId"];
                var dataSet = _labelDal.Get(groupId);
                labelValues = CreateLabelValues(dataSet);
                result = labelValues != null;
            }
            catch (Exception e)
            {
                var parameters = funcParams != null
                    ? string.Join(";", funcParams.Select(x => $"{{key: {x.Key}, value:{x.Value}}}"))
                    : string.Empty;
                _logger.LogError(e, $"Error while executing {nameof(GetLabels)}({parameters}): {e.Message}.");
            }

            return new Tuple<List<LabelValue>, bool>(labelValues, result);
        }
    }
}

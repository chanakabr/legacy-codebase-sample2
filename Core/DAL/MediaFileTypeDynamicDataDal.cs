using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;
using ApiObjects.MediaFiles;
using ApiObjects.Response;
using ODBCWrapper;

namespace DAL
{
    public class MediaFileTypeDynamicDataDal : IMediaFileTypeDynamicDataDal
    {
        private static readonly Lazy<MediaFileTypeDynamicDataDal> Lazy = new Lazy<MediaFileTypeDynamicDataDal>(
            () => new MediaFileTypeDynamicDataDal(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IMediaFileTypeDynamicDataDal Instance => Lazy.Value;

        public List<MediaFileTypeDynamicDataKeyValue> GetMediaFileDynamicDataKeyValuesByMediaFileTypeId(
            int groupId, long? mediaFileTypeId, string mediaFileTypeKeyName)
        {
            var ds = GetMediaFileDynamicDataKeyValuesByMediaFileTypeIdFromDb(groupId, mediaFileTypeId,
                mediaFileTypeKeyName);
            return CreateMediaFileDynamicDataKeyValuesFromDataSet(ds);
        }

        public List<MediaFileTypeDynamicDataKeyValue> GetMediaFileDynamicDataKeyValuesByIds(int groupId,
            List<long> ids)
        {
            var ds = GetMediaFileDynamicDataKeyValuesByIdsFromDb(groupId, ids);
            return CreateMediaFileDynamicDataKeyValuesFromDataSet(ds);
        }

        public GenericResponse<MediaFileTypeDynamicDataKeyValue> InsertMediaFileDynamicDataValue(int groupId,
            long mediaFileTypeId, string keyName, string value, long userId)
        {
            var ds = InsertMediaFileDynamicDataValueToDb(groupId, mediaFileTypeId, keyName, value, userId);
            return CreateMediaFileDynamicDataKeyValueResponseFromDataSet(ds);
        }

        public GenericResponse<bool> DeleteMediaFileDynamicDataValue(int groupId, long id, long userId)
        {
            var response = new GenericResponse<bool>();
            var deletedItems = DeleteMediaFileDynamicDataValueFromDb(groupId, id, userId);

            if (deletedItems > 0)
            {
                response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                response.Object = true;
            }
            else
            {
                response.SetStatus(eResponseStatus.DynamicDataKeyValueDoesNotExist, eResponseStatus.DynamicDataKeyValueDoesNotExist.ToString());
            }

            return response;
        }

        private static DataSet GetMediaFileDynamicDataKeyValuesByMediaFileTypeIdFromDb(int groupId,
            long? mediaFileTypeId, string mediaFileTypeKeyName)
        {
            var sp = new StoredProcedure("Get_MediaFileDynamicDataKeyValuesByMediaFileTypeId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("GroupId", groupId);
            sp.AddParameter("MediaFileTypeId", mediaFileTypeId);
            sp.AddParameter("MediaFileTypeKeyName", mediaFileTypeKeyName);

            return sp.ExecuteDataSet();
        }

        private static DataSet GetMediaFileDynamicDataKeyValuesByIdsFromDb(int groupId, List<long> ids)
        {
            var sp = new StoredProcedure("Get_MediaFileDynamicDataKeyValuesByIds");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("GroupId", groupId);
            sp.AddIDListParameter<long>("@Ids", ids, "Id");

            return sp.ExecuteDataSet();
        }

        private static DataSet InsertMediaFileDynamicDataValueToDb(int groupId, long mediaFileTypeId, string keyName,
            string value, long userId)
        {
            var sp = new StoredProcedure("Insert_MediaFileDynamicDataValue");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("GroupId", groupId);
            sp.AddParameter("MediaFileTypeId", mediaFileTypeId);
            sp.AddParameter("MediaFileTypeKeyName", keyName);
            sp.AddParameter("Value", value);
            sp.AddParameter("UserId", userId);

            return sp.ExecuteDataSet();
        }

        private static int DeleteMediaFileDynamicDataValueFromDb(int groupId, long id, long userId)
        {
            var sp = new StoredProcedure("Delete_MediaFileDynamicDataValue");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("GroupId", groupId);
            sp.AddParameter("Id", id);
            sp.AddParameter("UserId", userId);

            return sp.ExecuteReturnValue<int>();
        }

        private static List<MediaFileTypeDynamicDataKeyValue> CreateMediaFileDynamicDataKeyValuesFromDataSet(DataSet ds)
        {
            var response = new List<MediaFileTypeDynamicDataKeyValue>();

            if (ds != null && ds.Tables.Count > 0)
            {
                var dt = ds.Tables[0];

                if (dt?.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        var dynamicDataKeyValue = CreateMediaFileDynamicDataKeyValueFromDataRow(dr);
                        if (dynamicDataKeyValue != null)
                        {
                            response.Add(dynamicDataKeyValue);
                        }
                    }
                }
            }

            return response;
        }

        private static MediaFileTypeDynamicDataKeyValue CreateMediaFileDynamicDataKeyValueFromDataRow(DataRow dr)
        {
            var id = ODBCWrapper.Utils.GetLongSafeVal(dr, "id", 0);

            if (id <= 0)
            {
                return null;
            }

            var mediaFileTypeId = ODBCWrapper.Utils.GetLongSafeVal(dr, "mediaFileTypeId", 0);
            var mediaFileTypeKeyName = ODBCWrapper.Utils.GetSafeStr(dr, "mediaFileTypeKeyName");
            var value = ODBCWrapper.Utils.GetSafeStr(dr, "value");

            var dynamicDataKeyValue = new MediaFileTypeDynamicDataKeyValue
            {
                Id = id,
                MediaFileTypeId = mediaFileTypeId,
                MediaFileTypeKeyName = mediaFileTypeKeyName,
                Value = value
            };

            return dynamicDataKeyValue;
        }

        private static GenericResponse<MediaFileTypeDynamicDataKeyValue>
            CreateMediaFileDynamicDataKeyValueResponseFromDataSet(DataSet ds)
        {
            var response = new GenericResponse<MediaFileTypeDynamicDataKeyValue>();

            if (ds?.Tables != null && ds.Tables.Count > 0)
            {
                var dt = ds.Tables[0];
                if (dt?.Rows != null && dt.Rows.Count == 1)
                {
                    response.Object = CreateMediaFileDynamicDataKeyValueFromDataRow(dt.Rows[0]);

                    if (response.Object != null)
                    {
                        response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                    }
                    else
                    {
                        response.SetStatus(eResponseStatus.DynamicDataKeyDoesNotExist,
                            eResponseStatus.DynamicDataKeyDoesNotExist.ToString());
                        return response;
                    }
                }
            }

            return response;
        }
    }
}
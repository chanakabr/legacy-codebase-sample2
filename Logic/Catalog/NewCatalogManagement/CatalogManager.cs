using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Tvinci.Core.DAL;

namespace Core.Catalog.NewCatalogManagement
{
    public class CatalogManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static Status CreateAssetStructResponseStatusFromResult(long result)
        {
            Status responseStatus = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            switch (result)
            {
                case -111:
                    responseStatus = new Status((int)eResponseStatus.AssetStructNameAlreadyInUse, eResponseStatus.AssetStructNameAlreadyInUse.ToString());
                    break;
                case -222:
                    responseStatus = new Status((int)eResponseStatus.AssetStructSystemNameAlreadyInUse, eResponseStatus.AssetStructSystemNameAlreadyInUse.ToString());
                    break;
                case -333:
                    responseStatus = new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                    break;
                default:
                    break;
            }

            return responseStatus;
        }

        private static AssetStruct CreateAssetStructFromIdAndDataRow(long id, DataRow dr)
        {
            AssetStruct result = null;
            if (id > 0)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                string systemName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                bool isPredefined = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_BASIC");
                DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                result = new AssetStruct(id, name, systemName, isPredefined, createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
                                         updateDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate.Value) : 0);
            }

            return result;
        }

        private static AssetStructResponse CreateAssetStructResponseFromDataSet(DataSet ds)
        {
            AssetStructResponse response = new AssetStructResponse();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {                
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dt.Rows[0], "ID", 0);
                    if (id > 0)
                    {
                        response.AssetStruct = CreateAssetStructFromIdAndDataRow(id, dt.Rows[0]);                        
                    }
                    else
                    {
                        response.Status = CreateAssetStructResponseStatusFromResult(id);
                    }
                }

                if (response.AssetStruct != null && ds.Tables.Count == 2)
                {
                    DataTable metasDt = ds.Tables[1];
                    if (response.AssetStruct != null && metasDt != null && metasDt.Rows != null && metasDt.Rows.Count > 0)
                    {
                        foreach (DataRow dr in metasDt.Rows)
                        {
                            long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID", 0);
                            long metaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID", 0);
                            if (!response.AssetStruct.MetaIds.Contains(metaId))
                            {
                                response.AssetStruct.MetaIds.Add(metaId);
                            }
                        }
                    }
                }
            }

            return response;
        }

        private static AssetStructListResponse CreateAssetStructListResponseFromDataSet(DataSet ds)
        {
            AssetStructListResponse response = new AssetStructListResponse();
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                Dictionary<long, AssetStruct> idToAssetStructMap = new Dictionary<long, AssetStruct>();
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0 && !idToAssetStructMap.ContainsKey(id))
                        {
                            AssetStruct assetStruct = CreateAssetStructFromIdAndDataRow(id, dr);
                            if (assetStruct != null)
                            {
                                idToAssetStructMap.Add(id, assetStruct);
                            }
                        }
                    }
                }

                DataTable metasDt = ds.Tables[1];
                if (metasDt != null && metasDt.Rows != null && metasDt.Rows.Count > 0 && idToAssetStructMap.Count > 0)
                {
                    foreach (DataRow dr in metasDt.Rows)
                    {
                        long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID", 0);
                        long metaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID", 0);
                        if (assetStructId > 0 && metaId > 0 && idToAssetStructMap.ContainsKey(assetStructId) && !idToAssetStructMap[assetStructId].MetaIds.Contains(metaId))
                        {
                            idToAssetStructMap[assetStructId].MetaIds.Add(metaId);
                        }
                    }
                }

                response.AssetStructs = idToAssetStructMap.Values.ToList();
                response.Status = new ApiObjects.Response.Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString());
            }

            return response;
        }

        public static AssetStructListResponse GetAssetStructsByIds(int groupId, List<long> ids)
        {
            AssetStructListResponse response = new AssetStructListResponse();
            try
            {
                DataSet ds = CatalogDAL.GetAssetStructsByIds(groupId, ids);
                response = CreateAssetStructListResponseFromDataSet(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static AssetStructListResponse GetAssetStructsByMetaIds(int groupId, List<long> metaIds)
        {
            AssetStructListResponse response = new AssetStructListResponse();
            try
            {
                DataSet ds = CatalogDAL.GetAssetStructsByMetaIds(groupId, metaIds);
                response = CreateAssetStructListResponseFromDataSet(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByMetaIds with groupId: {0} and metaIds: {1}", groupId, metaIds != null ? string.Join(",", metaIds) : string.Empty), ex);
            }

            return response;
        }        

        public static AssetStructResponse AddAssetStruct(int groupId, AssetStruct assetStructToadd, long userId)
        {
            AssetStructResponse result = new AssetStructResponse();
            try
            {
                List<KeyValuePair<long, int>> metaIdsToPriority = new List<KeyValuePair<long, int>>();
                if (assetStructToadd.MetaIds != null && assetStructToadd.MetaIds.Count > 0)
                {
                    int priority = 1;
                    foreach (long metaId in assetStructToadd.MetaIds)
                    {
                        metaIdsToPriority.Add(new KeyValuePair<long, int>(metaId, priority));
                        priority++;
                    }
                }
                DataSet ds = CatalogDAL.InsertAssetStruct(groupId, assetStructToadd.Name, assetStructToadd.SystemName, metaIdsToPriority, assetStructToadd.IsPredefined, userId);
                result = CreateAssetStructResponseFromDataSet(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed AddAssetStruct for groupId: {0} and assetStruct: {1}", groupId, assetStructToadd.ToString()), ex);
            }

            return result;
        }

        public static AssetStructResponse UpdateAssetStruct(int groupId, AssetStruct assetStructToUpdate, bool shouldUpdateMetaIds, long userId)
        {
            AssetStructResponse result = new AssetStructResponse();
            try
            {
                List<KeyValuePair<long, int>> metaIdsToPriority = null;
                if (assetStructToUpdate.MetaIds != null)
                {
                    metaIdsToPriority = new List<KeyValuePair<long, int>>();                                           
                    int priority = 1;
                    foreach (long metaId in assetStructToUpdate.MetaIds)
                    {
                        metaIdsToPriority.Add(new KeyValuePair<long, int>(metaId, priority));
                        priority++;
                    }
                }
                DataSet ds = CatalogDAL.UpdateAssetStruct(groupId, assetStructToUpdate.Name, assetStructToUpdate.SystemName, metaIdsToPriority, assetStructToUpdate.IsPredefined, userId);
                result = CreateAssetStructResponseFromDataSet(ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetStruct for groupId: {0} and assetStruct: {1}", groupId, assetStructToUpdate.ToString()), ex);
            }

            return result;
        }

        public static Status DeleteAssetStruct(int groupId, long id, long userId)
        {
            Status result = new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            try
            {
                long deleteResult = CatalogDAL.DeleteAssetStruct(groupId, id, userId);
                result = CreateAssetStructResponseStatusFromResult(deleteResult);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed UpdateAssetStruct for groupId: {0} and assetStructId: {1}", groupId, id), ex);
            }

            return result;
        }

    }
}
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

        public static AssetStructsResponse GetAssetStructsByIds(int groupId, List<long> ids)
        {
            AssetStructsResponse response = new AssetStructsResponse();
            try
            {
                DataSet ds = CatalogDAL.GetAssetStructsByIds(groupId, ids);
                CreateAssetStructResponseFromDataSet(response, ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByIds with groupId: {0} and ids: {1}", groupId, ids != null ? string.Join(",", ids) : string.Empty), ex);
            }

            return response;
        }

        public static AssetStructsResponse GetAssetStructsByMetaIds(int groupId, List<long> metaIds)
        {
            AssetStructsResponse response = new AssetStructsResponse();
            try
            {
                DataSet ds = CatalogDAL.GetAssetStructsByMetaIds(groupId, metaIds);
                CreateAssetStructResponseFromDataSet(response, ds);
            }
            catch (Exception ex)
            {
                log.Error(string.Format("Failed GetAssetStructsByMetaIds with groupId: {0} and metaIds: {1}", groupId, metaIds != null ? string.Join(",", metaIds) : string.Empty), ex);
            }

            return response;
        }

        private static void CreateAssetStructResponseFromDataSet(AssetStructsResponse response, DataSet ds)
        {
            if (ds != null && ds.Tables != null && ds.Tables.Count == 2)
            {
                Dictionary<long, AssetStruct> idToAssetStructMap = new Dictionary<long, AssetStruct>();
                DataTable dt = ds.Tables[0];
                if (dt != null && dt.Rows != null && dt.Rows.Count > 0)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        string name = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                        string systemName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                        bool isPredefined = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_BASIC");
                        DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                        DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                        if (id > 0 && !idToAssetStructMap.ContainsKey(id))
                        {
                            AssetStruct assetStruct = new AssetStruct(id, name, systemName, isPredefined, createDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(createDate.Value) : 0,
                                                                        updateDate.HasValue ? ODBCWrapper.Utils.DateTimeToUnixTimestampUtc(updateDate.Value) : 0);
                            idToAssetStructMap.Add(id, assetStruct);
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
        }

    }
}
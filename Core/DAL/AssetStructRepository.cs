using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Response;
using Newtonsoft.Json;
using ODBCWrapper;

namespace DAL
{
    public class AssetStructRepository : IAssetStructRepository
    {
        private static readonly Lazy<IAssetStructRepository> LazyInstance = new Lazy<IAssetStructRepository>(
            () => new AssetStructRepository(),
            LazyThreadSafetyMode.PublicationOnly);

        public static IAssetStructRepository Instance => LazyInstance.Value;

        public List<AssetStruct> GetAssetStructsByGroupId(int groupId)
        {
            StoredProcedure sp = new StoredProcedure("GetAssetStructsByGroupId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            return CreateAssetStructListFromDataSet(sp.ExecuteDataSet());
        }

        public GenericResponse<AssetStruct> InsertAssetStruct(
            int groupId,
            long userId,
            AssetStruct assetStructToAdd,
            List<KeyValuePair<string, string>> namesInOtherLanguages,
            List<KeyValuePair<long, int>> metaIdsToPriority)
        {
            StoredProcedure sp = new StoredProcedure("InsertAssetStruct");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Name", assetStructToAdd.Name);
            sp.AddParameter("@NamesInOtherLanguagesExist", namesInOtherLanguages != null && namesInOtherLanguages.Count > 0);
            sp.AddKeyValueListParameter("@NamesInOtherLanguages", namesInOtherLanguages, "key", "value");
            sp.AddParameter("@SystemName", assetStructToAdd.SystemName);
            sp.AddParameter("@IsPredefined", assetStructToAdd.IsPredefined == true ? 1 : 0);
            sp.AddParameter("@MetaIdsToPriorityExist", metaIdsToPriority != null && metaIdsToPriority.Count > 0);
            sp.AddKeyValueListParameter("@MetaIdsToPriority", metaIdsToPriority, "key", "value");
            sp.AddParameter("@UpdaterId", userId);
            sp.AddParameter("@Features", assetStructToAdd.GetCommaSeparatedFeatures());
            sp.AddParameter("@ConnectingMetaId", assetStructToAdd.ConnectingMetaId);
            sp.AddParameter("@ConnectedParentMetaId", assetStructToAdd.ConnectedParentMetaId);
            sp.AddParameter("@PluralName", assetStructToAdd.PluralName);
            sp.AddParameter("@ParentId", assetStructToAdd.ParentId);
            sp.AddParameter("@IsProgramStruct", assetStructToAdd.IsProgramAssetStruct ? 1 : 0);
            sp.AddParameter("@DynamicData", assetStructToAdd.DynamicData?.Count > 0 ? JsonConvert.SerializeObject(assetStructToAdd.DynamicData) : null);
            sp.AddParameter("@IsLinear", assetStructToAdd.IsLinearAssetStruct ? 1 : 0);

            return CreateAssetStructResponseFromDataSet(sp.ExecuteDataSet());
        }

        public GenericResponse<AssetStruct> UpdateAssetStruct(
            int groupId,
            long userId,
            AssetStruct assetStructToUpdate,
            bool shouldUpdateOtherNames,
            List<KeyValuePair<string, string>> namesInOtherLanguages,
            bool shouldUpdateMetaIds,
            List<KeyValuePair<long, int>> metaIdsToPriority)
        {
            StoredProcedure sp = new StoredProcedure("UpdateAssetStruct");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@Id", assetStructToUpdate.Id);
            sp.AddParameter("@Name", assetStructToUpdate.Name);
            sp.AddParameter("@ShouldUpdateOtherNames", shouldUpdateOtherNames ? 1 : 0);
            sp.AddKeyValueListParameter("@NamesInOtherLanguages", namesInOtherLanguages, "key", "value");
            sp.AddParameter("@SystemName", assetStructToUpdate.SystemName);
            sp.AddParameter("@ShouldUpdateMetaIds", shouldUpdateMetaIds ? 1 : 0);
            sp.AddKeyValueListParameter("@MetaIdsToPriority", metaIdsToPriority, "key", "value");
            sp.AddParameter("@UpdaterId", userId);
            sp.AddParameter("@Features", assetStructToUpdate.GetCommaSeparatedFeatures());
            sp.AddParameter("@ConnectingMetaId", assetStructToUpdate.ConnectingMetaId);
            sp.AddParameter("@ConnectedParentMetaId", assetStructToUpdate.ConnectedParentMetaId);
            sp.AddParameter("@PluralName", assetStructToUpdate.PluralName);
            sp.AddParameter("@ParentId", assetStructToUpdate.ParentId);
            sp.AddParameter("@DynamicData", assetStructToUpdate.DynamicData?.Count > 0
                ? JsonConvert.SerializeObject(assetStructToUpdate.DynamicData)
                : null);

            return CreateAssetStructResponseFromDataSet(sp.ExecuteDataSet());
        }

        private List<AssetStruct> CreateAssetStructListFromDataSet(DataSet ds)
        {
            List<AssetStruct> response = null;
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                Dictionary<long, AssetStruct> idToAssetStructMap = new Dictionary<long, AssetStruct>();
                DataTable dt = ds.Tables[0];
                EnumerableRowCollection<DataRow> translations = ds.Tables.Count > 2 ? ds.Tables[2].AsEnumerable() : new DataTable().AsEnumerable();
                if (dt != null && dt.Rows != null)
                {
                    foreach (DataRow dr in dt.Rows)
                    {
                        long id = ODBCWrapper.Utils.GetLongSafeVal(dr, "ID", 0);
                        if (id > 0 && !idToAssetStructMap.ContainsKey(id))
                        {
                            List<DataRow> assetStructTranslations = (from row in translations
                                where (Int64)row["TEMPLATE_ID"] == id
                                select row).ToList();
                            AssetStruct assetStruct = CreateAssetStruct(id, dr, assetStructTranslations);
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
                    Dictionary<long, Dictionary<int, long>> assetStructOrderedMetasMap = new Dictionary<long, Dictionary<int, long>>();

                    foreach (DataRow dr in metasDt.Rows)
                    {
                        long assetStructId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID", 0);
                        long metaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID", 0);
                        int order = ODBCWrapper.Utils.GetIntSafeVal(dr, "ORDER", 0);

                        if (assetStructId > 0 && metaId > 0 && order > 0 && idToAssetStructMap.ContainsKey(assetStructId))
                        {
                            if (assetStructOrderedMetasMap.ContainsKey(assetStructId))
                            {
                                assetStructOrderedMetasMap[assetStructId][order] = metaId;
                            }
                            else
                            {
                                assetStructOrderedMetasMap.Add(assetStructId, new Dictionary<int, long>() { { order, metaId } });
                            }

                            if (!idToAssetStructMap[assetStructId].AssetStructMetas.ContainsKey(metaId))
                            {
                                AssetStructMeta assetStructMeta = AssetStructMetaRepository.CreateAssetStructMeta(dr, assetStructId, metaId);
                                idToAssetStructMap[assetStructId].AssetStructMetas.Add(metaId, assetStructMeta);
                            }
                        }
                    }

                    foreach (AssetStruct assetStruct in idToAssetStructMap.Values)
                    {
                        if (assetStructOrderedMetasMap.ContainsKey(assetStruct.Id))
                        {
                            assetStruct.MetaIds = assetStructOrderedMetasMap[assetStruct.Id].OrderBy(x => x.Key).Select(x => x.Value).ToList();
                        }
                    }
                }

                response = idToAssetStructMap.Values.ToList();
            }

            return response;
        }

        private static GenericResponse<AssetStruct> CreateAssetStructResponseFromDataSet(DataSet ds, List<KeyValuePair<long, int>> insertedMetaIdsToPriority = null)
        {
            GenericResponse<AssetStruct> response = new GenericResponse<AssetStruct>();
            if (ds != null && ds.Tables != null && ds.Tables.Count > 0)
            {
                DataTable dtMediaTypes = ds.Tables[0];
                if (dtMediaTypes != null && dtMediaTypes.Rows != null && dtMediaTypes.Rows.Count == 1)
                {
                    long id = ODBCWrapper.Utils.GetLongSafeVal(dtMediaTypes.Rows[0], "ID", 0);
                    if (id > 0)
                    {
                        List<DataRow> assetStructTranslations = (ds.Tables.Count > 1 ? ds.Tables[1].AsEnumerable() : new DataTable().AsEnumerable()).ToList();
                        response.Object = CreateAssetStruct(id, dtMediaTypes.Rows[0], assetStructTranslations);

                        if (response.Object != null)
                        {
                            if (insertedMetaIdsToPriority != null && insertedMetaIdsToPriority.Count > 0)
                            {
                                foreach (KeyValuePair<long, int> metaIdToPriority in insertedMetaIdsToPriority)
                                {
                                    if (!response.Object.MetaIds.Contains(metaIdToPriority.Key))
                                    {
                                        response.Object.MetaIds.Add(metaIdToPriority.Key);
                                    }
                                }
                            }
                            else if (ds.Tables.Count == 3)
                            {
                                DataTable dtTemplateTopics = ds.Tables[2];
                                if (dtTemplateTopics != null && dtTemplateTopics.Rows != null && dtTemplateTopics.Rows.Count > 0)
                                {
                                    if (response.Object.MetaIds == null)
                                    {
                                        response.Object.MetaIds = new List<long>(dtTemplateTopics.Rows.Count);
                                    }

                                    foreach (DataRow dr in dtTemplateTopics.Rows)
                                    {
                                        long metaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID", 0);
                                        if (!response.Object.MetaIds.Contains(metaId))
                                        {
                                            response.Object.MetaIds.Add(metaId);
                                        }
                                    }
                                }
                            }

                            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
                        }
                    }
                    else
                    {
                        response.SetStatus(CreateAssetStructResponseStatusFromResult(id));
                    }
                }
                /// assetStruct does not exist
                else
                {
                    response.SetStatus(CreateAssetStructResponseStatusFromResult(0, new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString())));
                }
            }

            return response;
        }

        private static AssetStruct CreateAssetStruct(long id, DataRow dr, List<DataRow> assetStructTranslations)
        {
            AssetStruct result = null;
            if (id > 0)
            {
                string name = ODBCWrapper.Utils.GetSafeStr(dr, "DESCRIPTION");
                string systemName = ODBCWrapper.Utils.GetSafeStr(dr, "NAME");
                if (!string.IsNullOrEmpty(name) && !string.IsNullOrEmpty(systemName))
                {
                    DateTime? createDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "CREATE_DATE");
                    DateTime? updateDate = ODBCWrapper.Utils.GetNullableDateSafeVal(dr, "UPDATE_DATE");
                    List<LanguageContainer> namesInOtherLanguages = new List<LanguageContainer>();
                    if (assetStructTranslations != null && assetStructTranslations.Count > 0)
                    {
                        foreach (DataRow translationDr in assetStructTranslations)
                        {
                            string languageCode = ODBCWrapper.Utils.GetSafeStr(translationDr, "CODE3");
                            string translation = ODBCWrapper.Utils.GetSafeStr(translationDr, "TRANSLATION");
                            if (!string.IsNullOrEmpty(languageCode) && !string.IsNullOrEmpty(translation))
                            {
                                namesInOtherLanguages.Add(new LanguageContainer(languageCode, translation));
                            }
                        }
                    }

                    string commaSeparatedFeatures = ODBCWrapper.Utils.GetSafeStr(dr, "FEATURES");
                    HashSet<string> features = null;
                    if (!string.IsNullOrEmpty(commaSeparatedFeatures))
                    {
                        features = new HashSet<string>(commaSeparatedFeatures.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries));
                    }

                    long? parentId = ODBCWrapper.Utils.GetNullableLong(dr, "PARENT_TYPE_ID");
                    result = new AssetStruct()
                    {
                        Id = id,
                        Name = name,
                        NamesInOtherLanguages = namesInOtherLanguages,
                        SystemName = systemName,
                        IsPredefined = ODBCWrapper.Utils.ExtractBoolean(dr, "IS_BASIC"),
                        ParentId = parentId.HasValue && parentId.Value == 0 ? null : parentId,
                        CreateDate = createDate.HasValue ? Utils.DateTimeToUtcUnixTimestampSeconds(createDate.Value) : 0,
                        UpdateDate = updateDate.HasValue ? Utils.DateTimeToUtcUnixTimestampSeconds(updateDate.Value) : 0,
                        Features = features,
                        ConnectingMetaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "CONNECTING_META_ID"),
                        ConnectedParentMetaId = ODBCWrapper.Utils.GetLongSafeVal(dr, "CONNECTED_PARENT_META_ID"),
                        PluralName = ODBCWrapper.Utils.GetSafeStr(dr, "PLURAL_NAME"),
                        IsProgramAssetStruct = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_PROGRAM") == 1,
                        IsLinearAssetStruct = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_LINEAR") == 1,
                        DynamicData = ODBCWrapper.Utils.GetSafeStr(dr["dynamic_data"]) == null ? null : JsonConvert.DeserializeObject<List<KeyValuePair<string, string>>>(ODBCWrapper.Utils.GetSafeStr(dr["dynamic_data"]))
                    };
                }
            }

            return result;
        }

        private static Status CreateAssetStructResponseStatusFromResult(long result, Status status = null)
        {
            switch (result)
            {
                case -111:
                    return new Status((int)eResponseStatus.AssetStructNameAlreadyInUse, eResponseStatus.AssetStructNameAlreadyInUse.ToString());
                case -222:
                    return new Status((int)eResponseStatus.AssetStructSystemNameAlreadyInUse, eResponseStatus.AssetStructSystemNameAlreadyInUse.ToString());
                case -333:
                    return new Status((int)eResponseStatus.AssetStructDoesNotExist, eResponseStatus.AssetStructDoesNotExist.ToString());
                default:
                    return status ?? new Status((int)eResponseStatus.Error, eResponseStatus.Error.ToString());
            }
        }
    }
}
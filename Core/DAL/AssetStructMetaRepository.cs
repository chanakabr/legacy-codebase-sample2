using DAL.DTO;
using ODBCWrapper;
using System;
using System.Linq;
using System.Data;
using System.Threading;
using System.Collections.Generic;
using CouchbaseManager;
using Phx.Lib.Log;
using System.Reflection;

namespace DAL
{
    public interface IAssetStructMetaRepository
    {
        List<AssetStructMetaDTO> GetAssetStructMetaList(int groupId, long metaId);
        AssetStructMetaDTO UpdateAssetStructMeta(int groupId, long userId, AssetStructMetaDTO assetStructMetaDTO, out bool success);
        bool UpdateEpgAssetStructMetas(int groupId, List<KeyValuePair<long, string>> epgMetaIdsToValue, long userId);

        //Meta aliases
        bool GetGroupUsingAliases(int groupId);
    }

    public class AssetStructMetaRepository : IAssetStructMetaRepository
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<AssetStructMetaRepository> Lazy =
            new Lazy<AssetStructMetaRepository>(() => new AssetStructMetaRepository(),
                LazyThreadSafetyMode.PublicationOnly);

        public static IAssetStructMetaRepository Instance => Lazy.Value;

        public AssetStructMetaRepository()
        {
        }

        public AssetStructMetaDTO UpdateAssetStructMeta(int groupId, long userId, AssetStructMetaDTO assetStructMetaDTO, out bool success)
        {
            success = true;
            StoredProcedure sp = new StoredProcedure("UpdateAssetStructMeta");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@AssetStructId", assetStructMetaDTO.AssetStructId);
            sp.AddParameter("@MetaId", assetStructMetaDTO.MetaId);
            sp.AddParameter("@IngestReferencePath", assetStructMetaDTO.IngestReferencePath);
            sp.AddParameter("@ProtectFromIngest", assetStructMetaDTO.ProtectFromIngest);
            sp.AddParameter("@DefaultIngestValue", assetStructMetaDTO.DefaultIngestValue);
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@UserId", userId);
            sp.AddParameter("@IsInherited", assetStructMetaDTO.IsInherited);
            sp.AddParameter("@isLocationTag", assetStructMetaDTO.IsLocationTag);
            sp.AddParameter("@suppressedOrder", assetStructMetaDTO.SuppressedOrder);
            sp.AddParameter("@alias", assetStructMetaDTO.Alias);

            var response = sp.Execute();
            var dtoObject = CreateAssetStructMetaDtoListFromDT(response).FirstOrDefault();

            if (dtoObject == null)
            {
                success = false;
                return null;
            }

            if (!string.IsNullOrEmpty(dtoObject.Alias))
            {
                if (!SaveGroupUsingAliases(groupId))
                {
                    success = false;
                    log.Warn($"Failed to Save UpdateAssetStructMeta for group {groupId}");
                }
            }
            return dtoObject;
        }

        public List<AssetStructMetaDTO> GetAssetStructMetaList(int groupId, long metaId)
        {
            StoredProcedure sp = new StoredProcedure("GetAssetStructMetaListByMetaId");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@GroupId", groupId);
            sp.AddParameter("@MetaId", metaId);

            var response = sp.Execute();
            return CreateAssetStructMetaDtoListFromDT(response);
        }

        public bool UpdateEpgAssetStructMetas(int groupId, List<KeyValuePair<long, string>> epgMetaIdsToValue, long userId)
        {
            StoredProcedure sp = new StoredProcedure("UpdateEpgAssetStructMetas");
            sp.SetConnectionKey("MAIN_CONNECTION_STRING");
            sp.AddParameter("@groupId", groupId);
            sp.AddKeyValueListParameter<long, string>("@epgMetaIdsToValue", epgMetaIdsToValue, "key", "value");
            sp.AddParameter("@updaterId", userId);

            return sp.ExecuteReturnValue<int>() > 0;
        }

        private List<AssetStructMetaDTO> CreateAssetStructMetaDtoListFromDT(DataTable dt)
        {
            var assetStructMetaList = new List<AssetStructMetaDTO>();

            if (dt != null && dt.Rows != null)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    assetStructMetaList.Add(CreateAssetStructMetaDTO(dr, ODBCWrapper.Utils.GetLongSafeVal(dr, "TEMPLATE_ID"), ODBCWrapper.Utils.GetLongSafeVal(dr, "TOPIC_ID")));
                }
            }

            return assetStructMetaList;
        }

        public static AssetStructMetaDTO CreateAssetStructMetaDTO(DataRow dr, long assetStructId, long metaId)
        {
            var assetStructMeta = new AssetStructMetaDTO()
            {
                AssetStructId = assetStructId,
                MetaId = metaId,
                IngestReferencePath = ODBCWrapper.Utils.GetSafeStr(dr, "INGEST_REFERENCE_PATH"),
                ProtectFromIngest = ODBCWrapper.Utils.ExtractBoolean(dr, "PROTECT_FROM_INGEST"),
                DefaultIngestValue = ODBCWrapper.Utils.GetSafeStr(dr, "DEFAULT_INGEST_VALUE"),
                CreateDate = Utils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dr, "CREATE_DATE")),
                UpdateDate = Utils.DateTimeToUtcUnixTimestampSeconds(ODBCWrapper.Utils.GetDateSafeVal(dr, "UPDATE_DATE")),
                IsInherited = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_INHERITED") == 1,
                IsLocationTag = ODBCWrapper.Utils.GetIntSafeVal(dr, "IS_LOCATION_TAG") == 1,
                SuppressedOrder = ODBCWrapper.Utils.GetNullableInt(dr, "SUPPRESSED_ORDER"),
                Alias = ODBCWrapper.Utils.GetSafeStr(dr, "ALIAS_NAME")
            };
            return assetStructMeta;
        }

        /// <summary>
        /// If response has value the group is using aliases
        /// </summary>
        public bool GetGroupUsingAliases(int groupId)
        {
            var key = GetGroupUsingAliasesKey(groupId);
            var results = UtilsDal.GetObjectFromCB<long>(eCouchbaseBucket.OTT_APPS, key);
            return results > 0;
        }

        private string GetGroupUsingAliasesKey(int groupId)
        {
            return $"meta_group_using_aliases_{groupId}";
        }

        private bool SaveGroupUsingAliases(int groupId)
        {
            var key = GetGroupUsingAliasesKey(groupId);
            return UtilsDal.SaveObjectInCB(eCouchbaseBucket.OTT_APPS, key, Utils.GetUtcUnixTimestampNow(), false);
        }
    }
}

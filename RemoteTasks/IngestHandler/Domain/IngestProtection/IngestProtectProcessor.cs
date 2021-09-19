using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ApiObjects;
using ApiObjects.BulkUpload;
using IngestHandler.Common.Infrastructure;
using Tvinci.Core.DAL;
using TVinciShared;

namespace IngestHandler.Domain.IngestProtection
{
    public class IngestProtectProcessor : IIngestProtectProcessor
    {
        private static readonly Lazy<IDictionary<string, PropertyInfo>> EpgWriteProperties = new Lazy<IDictionary<string, PropertyInfo>>(RetrieveEpgWriteProperties);
        private readonly IEpgDal _epgDal;
        private readonly ICatalogManagerAdapter _catalogManagerAdapter;

        public IngestProtectProcessor(IEpgDal epgDal, ICatalogManagerAdapter catalogManagerAdapter)
        {
            _epgDal = epgDal;
            _catalogManagerAdapter = catalogManagerAdapter;
        }

        public void ProcessIngestProtect(int groupId, CRUDOperations<EpgProgramBulkUploadObject> crudOperations)
        {
            var protectedTopics = RetrieveProtectedTopics(groupId);
            if (protectedTopics.IsEmpty())
            {
                return;
            }

            foreach (var prog in crudOperations.ItemsToUpdate.Where(i => i.CbDocumentIdsMap != null))
            {
                var oldEpgsByLanguage = _epgDal.GetEpgDocs(prog.CbDocumentIdsMap.Values, true).ToDictionary(x => x.Language);
                foreach (var progEpgCbObject in prog.EpgCbObjects)
                {
                    if (!oldEpgsByLanguage.TryGetValue(progEpgCbObject.Language, out var oldEpg))
                    {
                        continue;
                    }

                    if (oldEpg == null)
                    {
                        continue;
                    }

                    ProtectFields(oldEpg, progEpgCbObject, protectedTopics);
                }
            }
        }

        private static IDictionary<string, PropertyInfo> RetrieveEpgWriteProperties()
        {
            var publicWriteProperties = typeof(EpgCB)
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Where(x => x.CanWrite)
                .ToDictionary(x => x.Name, x => x, StringComparer.InvariantCultureIgnoreCase);

            return publicWriteProperties;
        }

        private void ProtectFields(EpgCB oldEpg, EpgCB newEpg, IEnumerable<string> protectedTopics)
        {
            foreach (var protectedTopic in protectedTopics)
            {
                ProtectProperties(oldEpg, newEpg, protectedTopic);
                ProtectMetasAndTags(oldEpg, newEpg, protectedTopic);
            }
        }

        private void ProtectProperties(EpgCB oldEpg, EpgCB newEpg, string protectedProperty)
        {
            if (EpgWriteProperties.Value.TryGetValue(protectedProperty, out var propertyInfo))
            {
                var oldValue = propertyInfo.GetValue(oldEpg);
                propertyInfo.SetValue(newEpg, oldValue);
            }
        }

        private void ProtectMetasAndTags(EpgCB oldEpg, EpgCB newEpg, string protectedMetaOrTag)
        {
            newEpg.Metas.Remove(protectedMetaOrTag);
            newEpg.Tags.Remove(protectedMetaOrTag);

            if (oldEpg.Metas != null && oldEpg.Metas.ContainsKey(protectedMetaOrTag))
            {
                newEpg.Metas[protectedMetaOrTag] = oldEpg.Metas[protectedMetaOrTag];
            }

            if (oldEpg.Tags != null && oldEpg.Tags.ContainsKey(protectedMetaOrTag))
            {
                newEpg.Tags[protectedMetaOrTag] = oldEpg.Tags[protectedMetaOrTag];
            }
        }

        private string[] RetrieveProtectedTopics(int groupId)
        {
            // backward compatibility for non-OPC accounts.
            if (!_catalogManagerAdapter.DoesGroupUsesTemplates(groupId))
            {
                return new string[] { };
            }

            var catalogGroupCache = _catalogManagerAdapter.GetCatalogGroupCache(groupId);
            if (!catalogGroupCache.AssetStructsMapById.TryGetValue(catalogGroupCache.GetProgramAssetStructId(), out var programStruct))
            {
                return new string[] { };
            }

            var protectedMetasAndTagsById = programStruct.AssetStructMetas.Values
                .Where(x => x.ProtectFromIngest == true)
                .Select(x => x.MetaId)
                .ToArray();
            if (protectedMetasAndTagsById.IsEmpty())
            {
                return new string[] { };
            }

            var metaInfos = catalogGroupCache.TopicsMapById
                .Where(x => protectedMetasAndTagsById.Contains(x.Key))
                .Select(x => x.Value.SystemName)
                .ToArray();

            return metaInfos;
        }
    }
}
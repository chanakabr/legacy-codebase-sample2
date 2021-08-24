using System;
using System.Linq;
using ApiObjects;
using ApiObjects.BulkUpload;
using Tvinci.Core.DAL;
using TVinciShared;

namespace IngestHandler.Domain.IngestProtection
{
    public class IngestProtectProcessor : IIngestProtectProcessor
    {
        public void ProcessIngestProtect(CRUDOperations<EpgProgramBulkUploadObject> crudOperations, Lazy<string[]> protectedMetasAndTagsLazy)
        {
            if (protectedMetasAndTagsLazy.Value.IsEmpty())
            {
                return;
            }
            
            foreach (var prog in crudOperations.ItemsToUpdate.Where(i => i.CbDocumentIdsMap != null))
            {
                var oldEpgsByLanguage = EpgDal.GetEpgCBList(prog.CbDocumentIdsMap.Values.ToList()).ToDictionary(x => x.Language);
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

                    foreach (var protectedMetaOrTag in protectedMetasAndTagsLazy.Value)
                    {
                        progEpgCbObject.Metas.Remove(protectedMetaOrTag);
                        progEpgCbObject.Tags.Remove(protectedMetaOrTag);
                        
                        if (oldEpg.Metas != null && oldEpg.Metas.ContainsKey(protectedMetaOrTag))
                        {
                            progEpgCbObject.Metas[protectedMetaOrTag] = oldEpg.Metas[protectedMetaOrTag];
                        }

                        if (oldEpg.Tags != null && oldEpg.Tags.ContainsKey(protectedMetaOrTag))
                        {
                            progEpgCbObject.Tags[protectedMetaOrTag] = oldEpg.Tags[protectedMetaOrTag];
                        }
                    }
                }
            }
        }
    }
}
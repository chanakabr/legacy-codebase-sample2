using System;
using System.Threading;
using ApiObjects;
using Core.Catalog;

namespace ApiLogic.IndexManager.Mappings
{
    public class MappingTypeResolver : IMappingTypeResolver
    {
        private static readonly Lazy<IMappingTypeResolver> Lazy = new Lazy<IMappingTypeResolver>(() => new MappingTypeResolver(), LazyThreadSafetyMode.PublicationOnly);
        
        public static IMappingTypeResolver Instance => Lazy.Value;

        public string GetMappingType(bool isRecording, LanguageObj language = null)
        {
            var indexTypePrefix = isRecording ? IndexManagerV2.RECORDING_INDEX_TYPE : IndexManagerV2.EPG_INDEX_TYPE;
            if (language == null || language.IsDefault)
            {
                return indexTypePrefix;
            }

            return $"{indexTypePrefix}_{language.Code}";
        }

        public string ExtractLanguageCodeFromMappingType(string mappingType, out bool isDefaultLanguage)
        {
            isDefaultLanguage = false;
            if (mappingType == null)
            {
                return null;
            }

            var epgTypeSplit = mappingType.Split(new[] { "_" }, StringSplitOptions.RemoveEmptyEntries);
            if (epgTypeSplit.Length == 0)
            {
                return null;
            }

            if (epgTypeSplit.Length > 2)
            {
                return null;
            }

            if (epgTypeSplit.Length == 1)
            {
                isDefaultLanguage = true;
                return string.Empty;
            }

            return epgTypeSplit[1];
        }
    }
}
namespace ApiLogic.IndexManager.Mappings
{
    public interface IMappingTypeResolver
    {
        string GetMappingType(bool isRecording, ApiObjects.LanguageObj language = null);

        string ExtractLanguageCodeFromMappingType(string mappingType, out bool isDefaultLanguage);
    }
}
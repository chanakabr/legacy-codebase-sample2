using ApiObjects;

namespace ApiLogic.IndexManager.Helpers
{
    /// <summary>
    /// keeps all the common static method that can be used between the different index managers
    /// </summary>
    public static class IndexManagerCommonHelpers
    {
        public static string GetTranslationType(string type, LanguageObj language)
        {
            if (language.IsDefault)
            {
                return type;
            }
            
            return string.Concat(type, "_", language.Code);
        }
    }
}
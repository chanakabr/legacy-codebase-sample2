using System.Collections.Generic;

namespace ApiObjects.BulkUpload
{
    public class LanguagesInfo
    {
        public IDictionary<string, LanguageObj> Languages { get; set; }

        public LanguageObj DefaultLanguage { get; set; }
    }
}
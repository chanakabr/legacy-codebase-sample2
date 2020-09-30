using System;
using System.Collections.Generic;

namespace ApiObjects.Catalog
{
    public class MediaTagsTranslations
    {
        public long mediaId { get; set; }
        //public Dictionary<long, Dictionary<int, string>> Translations { get; set; }
        public List<TagTranslations> Translations { get; set; }
        public DateTime UpdateDate { get; set; }
    }
    
    public class TagTranslations
    {
        public int LanguageId;
        public int TagTypeId;
        public string Value;
        public long TagId;
    }

}
using System;
using System.Linq;
using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Response;

namespace IngestHandler.Common.Managers
{
    public static class ProgramValidator
    {
        internal static bool Validate(EpgProgramBulkUploadObject program, BulkUploadResult epg, LanguageObj defaultLanguage)
        {
            bool result = true;
            if (!ValidateMetadataLang(program))  
            {
                epg.AddError(eResponseStatus.Error, "Language value on meta data must not be empty.");
                result = false;
            }

            if (!ValidateTagsLang(program))
            {
                epg.AddError(eResponseStatus.Error, "Language value on tags data must not be empty.");
                result = false;
            }

            if (!ValidateTitle(program, defaultLanguage))
            {
                epg.AddError(eResponseStatus.Error, "Language value on title and value cannot be empty.");
                result = false;
            }

            if (!ValidateDescription(program, defaultLanguage))
            {
                epg.AddError(eResponseStatus.Error, "Language value on description and value cannot be empty.");
                result = false;
            }

            if (!ValidateIcon(program))
            {
                epg.AddError(eResponseStatus.Error, "Icon src cannot be empty");
                result = false;
            }

            if (!ValidateExternalId(program))
            {
                epg.AddError(eResponseStatus.Error, "External ID cannot be empty");
                result = false;
            }

            if (!ValidateProgramDates(program))
            {
                epg.AddError(eResponseStatus.Error, "Invalid Program Dates");
                return false;
            }

            return result;
        }

        private static bool ValidateIcon(EpgProgramBulkUploadObject p) =>
            p.ParsedProgramObject != null
            && p.ParsedProgramObject.icon == null
            || p.ParsedProgramObject.icon.All(x => x != null && !string.IsNullOrEmpty(x.src));

        private static bool ValidateTitle(EpgProgramBulkUploadObject p, LanguageObj defaultLanguage)
        {
            return ValidateFields(p.ParsedProgramObject?.title, defaultLanguage, title => title.lang, title => title.Value);
        }

        private static bool ValidateDescription(EpgProgramBulkUploadObject p, LanguageObj defaultLanguage)
        {
            return ValidateFields(p.ParsedProgramObject?.desc, defaultLanguage, desc => desc.lang, desc => desc.Value);
        }

        public static bool ValidateMetadataLang(EpgProgramBulkUploadObject p) =>
            //verify does not contain empty lang 
            p.ParsedProgramObject != null
            && p.ParsedProgramObject.metas == null
            || p.ParsedProgramObject?.metas != null && p.ParsedProgramObject.metas.SelectMany(x => x.MetaValues)
                .All(x => x != null && !string.IsNullOrEmpty(x.lang));

        public static bool ValidateTagsLang(EpgProgramBulkUploadObject p) =>
            //verify does not contain empty lang 
            (p.ParsedProgramObject != null && p.ParsedProgramObject.tags == null)
            || (p.ParsedProgramObject?.tags != null && p.ParsedProgramObject.tags.SelectMany(x => x.TagValues)
                .All(x => x != null && !string.IsNullOrEmpty(x.lang)));

        private static bool ValidateExternalId(EpgProgramBulkUploadObject p) =>
            //verify that external id is not empty
            p.ParsedProgramObject != null && !string.IsNullOrWhiteSpace(p.ParsedProgramObject.external_id);

        private static bool ValidateProgramDates(EpgProgramBulkUploadObject p) =>
            p.StartDate != null && p.EndDate != null && p.StartDate < p.EndDate;
        
        private static bool ValidateFields<T>(T[] fields, LanguageObj defaultLanguage, Func<T, string> langRetriever, Func<T, string> valueRetriever)
        {
            if (fields == null)
            {
                return false;
            }
            
            foreach (var f in fields)
            {
                if (f == null)
                {
                    return false;
                }

                var lang = langRetriever(f);
                if (string.IsNullOrEmpty(lang))
                {
                    return false;
                }

                if (string.Equals(lang, defaultLanguage.Code, StringComparison.InvariantCultureIgnoreCase))
                {
                    if (string.IsNullOrEmpty(valueRetriever(f)))
                    {
                        return false;
                    }
                }
            }

            return true;
        }
    }
}

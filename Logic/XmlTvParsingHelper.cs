using ApiObjects.BulkUpload;
using ApiObjects.Response;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ApiLogic
{
    public static class XmlTvParsingHelper
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public const string XML_TV_DATE_FORMAT = "yyyyMMddHHmmss";

        public static Dictionary<string, List<string>> ParseTags(this programme prog, string langCode, string defaultLangCode, BulkUploadProgramAssetResult response)
        {
            var tagsToSet = new Dictionary<string, List<string>>();
            if (prog?.tags == null) { return tagsToSet; }

            foreach (var tag in prog.tags)
            {
                var tagValue = GetMetaByLanguage(tag, langCode, defaultLangCode, out var tagParsingStatus);
                if (tagParsingStatus != eResponseStatus.OK)
                {
                    response.AddError(tagParsingStatus, $"Error parsing meta:[{tag.TagType}] for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]");
                }
                tagsToSet[tag.TagType] = tagValue;
            }

            return tagsToSet;
        }

        public static Dictionary<string, List<string>> ParseMetas(this programme prog, string langCode, string defaultLangCode, BulkUploadProgramAssetResult response)
        {
            var metasToSet = new Dictionary<string, List<string>>();
            if (prog?.metas == null) { return metasToSet; }

            foreach (var meta in prog.metas)
            {
                var mataValue = GetMetaByLanguage(meta, langCode, defaultLangCode, out var metaParsingStatus);
                if (metaParsingStatus != eResponseStatus.OK)
                {
                    response.AddError(metaParsingStatus, $"Error parsing meta:[{meta.MetaType}] for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]");
                }
                metasToSet[meta.MetaType] = mataValue;
            }

            return metasToSet;
        }

        public static DateTime ParseStartDate(this programme prog, BulkUploadProgramAssetResult response)
        {
            var result = default(DateTime);
            if (ParseXmlTvDateString(prog.start, out var progStartDate))
            {
                result = progStartDate;
            }
            else
            {
                response.AddError(eResponseStatus.EPGSProgramDatesError,
                    $"programExternalId:[{prog.external_id}], Start date:[{prog.start}] could not be parsed expected format:[{XML_TV_DATE_FORMAT}]");
            }

            return result;
        }

        public static DateTime ParseEndDate(this programme prog, BulkUploadProgramAssetResult response)
        {
            var result = default(DateTime);
            if (ParseXmlTvDateString(prog.stop, out var progStartDate))
            {
                result = progStartDate;
            }
            else
            {
                response.AddError(eResponseStatus.EPGSProgramDatesError,
                    $"programExternalId:[{prog.external_id}], Start date:[{prog.start}] could not be parsed expected format:[{XML_TV_DATE_FORMAT}]");
            }

            return result;
        }


        public static List<string> GetMetaByLanguage(this metas meta, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;

            var valuesByLang = meta.MetaValues.Where(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase));
            valuesByLang = valuesByLang ?? meta.MetaValues.Where(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase));
            if (valuesByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
                return new List<string>();
            }

            var valuesStrByLang = valuesByLang.Select(v => v.Value).ToList();
            return valuesStrByLang;
        }

        public static List<string> GetMetaByLanguage(this tags tags, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;

            var valuesByLang = tags.TagValues.Where(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase));
            valuesByLang = valuesByLang ?? tags.TagValues.Where(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase));
            if (valuesByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
                return new List<string>();
            }

            var valuesStrByLang = valuesByLang.Select(v => v.Value).ToList();
            return valuesStrByLang;
        }

        public static string GetDescriptionByLanguage(this desc[] desc, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;
            var valueByLang = desc.FirstOrDefault(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase))?.Value;
            valueByLang = valueByLang ?? desc.FirstOrDefault(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))?.Value;
            if (valueByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
            }

            return valueByLang;
        }

        public static string GetTitleByLanguage(this title[] title, string language, string defaultLanguage, out eResponseStatus parsingStatus)
        {
            parsingStatus = eResponseStatus.OK;
            var valueByLang = title.FirstOrDefault(t => t.lang.Equals(language, StringComparison.OrdinalIgnoreCase))?.Value;
            valueByLang = valueByLang ?? title.FirstOrDefault(t => t.lang.Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase))?.Value;
            if (valueByLang == null)
            {
                parsingStatus = eResponseStatus.EPGLanguageNotFound;
            }

            return valueByLang;
        }


        public static bool ParseXmlTvDateString(string dateStr, out DateTime theDate)
        {
            theDate = default(DateTime);
            if (string.IsNullOrEmpty(dateStr) || dateStr.Length < 14) { return false; }
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), XML_TV_DATE_FORMAT, null, System.Globalization.DateTimeStyles.None, out theDate);
            return res;
        }

        public static int ParseXmlTvEnableStatusValue(string val)
        {
            try
            {
                if (string.IsNullOrEmpty(val))
                    return 0; // 0 == none
                if (val == "false" || val == "2")
                    return 2;
                if (val == "true" || val == "1")
                    return 1;
                return 0;
            }
            catch
            {
                return 0;
            }
        }
    }
}

using ApiObjects.BulkUpload;
using ApiObjects.Response;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using TVinciShared;

namespace ApiLogic
{
    public static class XmlTvParsingHelper
    {
        private static readonly KLogger _Logger = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        public const string XML_TV_DATE_FORMAT = "yyyyMMddHHmmss";
        private const string ExternalOfferIdsTagName = "Offers";

        public static Dictionary<string, List<string>> ParseTags(
            this programme prog,
            string langCode,
            string defaultLangCode,
            BulkUploadProgramAssetResult response,
            bool isMultilingualFallback)
        {
            var tagsToSet = new Dictionary<string, List<string>>();
            if (prog?.tags == null) { return tagsToSet; }

            foreach (var tag in prog.tags)
            {
                var tagValue = GetMetaByLanguage(tag.TagValues,
                    tv => tv.lang,
                    tv => tv.Value,
                    langCode,
                    defaultLangCode,
                    out var tagParsingStatus,
                    isMultilingualFallback);
                if (tagParsingStatus != eResponseStatus.OK)
                {
                    response.AddError(tagParsingStatus, $"Error parsing meta:[{tag.TagType}] for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]");
                }
                else if (tagValue.Count > 0)
                {
                    tagsToSet[tag.TagType] = tagValue;
                }
            }

            return tagsToSet;
        }

        public static Dictionary<string, List<string>> ParseMetas(
            this programme prog,
            string langCode,
            string defaultLangCode,
            BulkUploadProgramAssetResult response,
            bool isMultilingualFallback)
        {
            var metasToSet = new Dictionary<string, List<string>>();
            if (prog?.metas == null) { return metasToSet; }

            foreach (var meta in prog.metas)
            {
                var metaValue = GetMetaByLanguage(meta.MetaValues,
                    mv => mv.lang,
                    mv => mv.Value,
                    langCode,
                    defaultLangCode,
                    out var metaParsingStatus,
                    isMultilingualFallback);
                if (metaParsingStatus != eResponseStatus.OK)
                {
                    response.AddError(metaParsingStatus, $"Error parsing meta:[{meta.MetaType}] for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]");
                }
                else if (metaValue.Count > 0)
                {
                    metasToSet[meta.MetaType] = metaValue;
                }
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
                    $"programExternalId:[{prog.external_id}], End date:[{prog.stop}] could not be parsed expected format:[{XML_TV_DATE_FORMAT}]");
            }

            return result;
        }

        private static List<string> GetMetaByLanguage<T>(
            this IEnumerable<T> metaValues,
            Func<T, string> langRetriever,
            Func<T, string> valueRetriever,
            string language,
            string defaultLanguage,
            out eResponseStatus parsingStatus,
            bool isMultilingualFallback)
        {
            parsingStatus = eResponseStatus.OK;
            var valuesByLang = metaValues.Where(t => langRetriever(t).Equals(language, StringComparison.OrdinalIgnoreCase) && valueRetriever(t) != null);
            if (valuesByLang.IsEmpty() && isMultilingualFallback)
            {
                valuesByLang = metaValues.Where(t => langRetriever(t).Equals(defaultLanguage, StringComparison.OrdinalIgnoreCase) && valueRetriever(t) != null);
            }

            return valuesByLang.Select(valueRetriever).ToList();
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
            bool res = DateTime.TryParseExact(dateStr.Substring(0, 14), XML_TV_DATE_FORMAT, null, DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal, out theDate);
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

        public static List<string> ParseExternalOfferIds(
            this programme prog,
            string langCode,
            string defaultLangCode,
            BulkUploadProgramAssetResult response)
        {
            var results = new List<string>();

            if (prog?.tags == null)
            {
                return results;
            }

            foreach (var tags in prog.tags.Where(t => t.TagType == ExternalOfferIdsTagName))
            {
                var tagValues = GetMetaByLanguage(tags.TagValues,
                    tv => tv.lang,
                    tv => tv.Value,
                    langCode,
                    defaultLangCode,
                    out var tagParsingStatus,
                    true);
                if (tagParsingStatus != eResponseStatus.OK)
                {
                    response.AddError(tagParsingStatus, $"Error parsing tags:[{ExternalOfferIdsTagName}] for programExternalID:[{prog.external_id}], lang:[{langCode}], defaultLang:[{defaultLangCode}]");
                }

                results.AddRange(tagValues);
            }

            return results.Distinct().ToList();
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Models.General;

namespace WebAPI.ModelsValidators
{
    public static class MultilingualStringValidator
    {
        public static void Validate(this KalturaMultilingualString model, string parameterName, bool shouldCheckDefaultLanguageIsSent = true,
            bool shouldValidateValues = true, bool shouldValidateRequestLanguage = true, bool shouldValidateLanguageExistsForGroup = true)
        {
            if (model.Values != null && model.Values.Count > 0)
            {
                HashSet<string> languageCodes = new HashSet<string>();
                HashSet<string> groupLanguageCodes = Utils.Utils.GetGroupLanguageCodes();
                if (string.IsNullOrEmpty(model.GroupDefaultLanguageCode))
                {
                    model.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                }

                if (string.IsNullOrEmpty(model.RequestLanguageCode))
                {
                    model.RequestLanguageCode = Utils.Utils.GetLanguageFromRequest();
                }

                foreach (KalturaTranslationToken token in model.Values)
                {
                    if (languageCodes.Contains(token.Language))
                    {
                        throw new BadRequestException(ApiException.DUPLICATE_LANGUAGE_SENT, token.Language);
                    }

                    if (shouldValidateValues && string.IsNullOrEmpty(token.Value))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTranslationToken.value");
                    }


                    if (shouldValidateLanguageExistsForGroup && !groupLanguageCodes.Contains(token.Language))
                    {
                        throw new BadRequestException(ApiException.GROUP_DOES_NOT_CONTAIN_LANGUAGE, token.Language);
                    }

                    languageCodes.Add(token.Language);
                }

                if (shouldCheckDefaultLanguageIsSent && !languageCodes.Contains(model.GroupDefaultLanguageCode))
                {
                    throw new BadRequestException(ApiException.DEFUALT_LANGUAGE_MUST_BE_SENT, parameterName);
                }

                if (shouldValidateRequestLanguage)
                {
                    if (string.IsNullOrEmpty(model.RequestLanguageCode) || model.RequestLanguageCode != "*")
                    {
                        throw new BadRequestException(ApiException.GLOBAL_LANGUAGE_MUST_BE_ASTERISK_FOR_WRITE_ACTIONS);
                    }
                }
            }
        }

        public static void ValidateForUpdate(this KalturaMultilingualString model, string parameterName)
        {
            if (model.Values != null && model.Values.Count > 0)
            {
                HashSet<string> groupLanguageCodes = Utils.Utils.GetGroupLanguageCodes();
                if (string.IsNullOrEmpty(model.GroupDefaultLanguageCode))
                {
                    model.GroupDefaultLanguageCode = Utils.Utils.GetDefaultLanguage();
                }

                bool doesDefaultLangHasValue = model.Values.Any(x => x.Language == model.GroupDefaultLanguageCode && !string.IsNullOrEmpty(x.Value));
                if (!doesDefaultLangHasValue && model.Values.Any(x => x.Language != model.GroupDefaultLanguageCode && !string.IsNullOrEmpty(x.Value)))
                {
                    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaTranslationToken.value, you can't translate an empty value");
                }
                else
                {
                    model.Validate(parameterName, true, false, true, true);
                }
            }
        }
    }
}
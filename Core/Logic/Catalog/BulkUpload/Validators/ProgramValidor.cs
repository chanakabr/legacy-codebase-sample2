using ApiObjects.BulkUpload;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiObjects.Response;

namespace ApiLogic.Catalog.BulkUpload.Validators
{
    public static  class ProgramValidor
    {
        internal static bool Validate(this EpgProgramBulkUploadObject program,BulkUploadResult epg)
        {
            bool result = true;
            if (!ValidateMetadataLang(program))
            {
                epg.AddError(eResponseStatus.Error,
                    "Language value on meta data must not be empty.");
                result = false;
            }

            if (!ValidateTagsLang(program))
            {
                epg.AddError(eResponseStatus.Error,
                    "Language value on tags data must not be empty.");
                result = false;
            }

            if (!ValidateTitleLang(program))
            {
                epg.AddError(eResponseStatus.Error,
                "Language value on title must not be empty.");
                result = false;
            }


            return result;
        }

        private static bool ValidateTitleLang(EpgProgramBulkUploadObject program)
        {
            return program.ParsedProgramObject.title != null && program.ParsedProgramObject.title.All(x => !string.IsNullOrEmpty(x.lang));
        }

        public static bool ValidateMetadataLang(EpgProgramBulkUploadObject program)
        {
            //verify does not contain empty lang 
            return program.ParsedProgramObject.metas != null && program.ParsedProgramObject.metas.SelectMany(x => x.MetaValues)
                .All(x => !string.IsNullOrEmpty(x.lang));
        }

        public static bool ValidateTagsLang(EpgProgramBulkUploadObject program)
        {
            //verify does not contain empty lang 
            return program.ParsedProgramObject.tags != null && program.ParsedProgramObject.tags.SelectMany(x => x.TagValues)
                .All(x => !string.IsNullOrEmpty(x.lang));
        }

        
    }
}

using ApiObjects;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public static class MediaFileTypeValidator
    {
        public static void validateForInsert(this KalturaMediaFileType model)
        {
            if (model.Name == null || model.Name.Trim().Length == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "mediaFileType.name");
            }

            if (model.Description == null || model.Description.Trim().Length == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY,
                    "mediaFileType.description");
            }

            if (model.StreamerType == null)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY,
                    "mediaFileType.streamerType");
            }
        }
    }
}
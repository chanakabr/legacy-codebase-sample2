using WebAPI.Models.Catalog;

namespace WebAPI.Validation
{
    public interface IMediaFileTypeValidator
    {
        void ValidateToAdd(KalturaMediaFileType mediaFileType, string argumentName);
        void ValidateToUpdate(KalturaMediaFileType mediaFileType, string argumentName);
    }
}
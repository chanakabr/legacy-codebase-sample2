using WebAPI.Models.Catalog;

namespace WebAPI.Validation
{
    public interface IMediaFileValidator
    {
        void ValidateToAdd(KalturaMediaFile mediaFile, string argumentName);
        void ValidateToUpdate(KalturaMediaFile mediaFile, string argumentName);
    }
}
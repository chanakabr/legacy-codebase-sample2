using WebAPI.Models.Catalog;

namespace WebAPI.ModelsValidators
{
    public interface IMediaFileLabelValidator
    {
        void ValidateToAdd(string commaSeparatedLabelValues, KalturaEntityAttribute entityAttribute, string argumentName);
        void ValidateToAdd(KalturaLabel label, string argumentName);
        void ValidateToUpdate(KalturaLabel label, string argumentName);
    }
}
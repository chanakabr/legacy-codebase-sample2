using WebAPI.Models.General;

namespace WebAPI.ModelsValidators
{
    public static class StringValueArrayValidator
    {
        public static bool HasObjects(this KalturaStringValueArray model)
        {
            return model.Objects != null && model.Objects.Count > 0;
        }
    }
}

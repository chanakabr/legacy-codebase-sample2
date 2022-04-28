using ApiObjects;
using ApiObjects.Response;

namespace ApiLogic.Api.Validators
{
    public interface ICatalogPartnerConfigValidator
    {
        Status Validate(long groupId, CatalogPartnerConfig catalogPartnerConfig);
    }
}
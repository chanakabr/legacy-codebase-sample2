
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
namespace RestfulTVPApi.ServiceInterface
{
    public interface IPricingRepository
    {
        TVPPro.SiteManager.TvinciPlatform.Pricing.CouponData GetCouponStatus(InitializationObject initObj, string sCouponCode);

        TVPPro.SiteManager.TvinciPlatform.Pricing.PPVModule GetPPVModuleData(InitializationObject initObj, int ppvCode);
    }
}
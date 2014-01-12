
using TVPApi;
using TVPPro.SiteManager.TvinciPlatform.ConditionalAccess;
namespace RestfulTVPApi.ServiceInterface
{
    public interface IConditionalAccessRepository
    {
        bool ActivateCampaign(InitializationObject initObj, int campaignID, string hashCode, int mediaID, string mediaLink, string senderEmail, string senderName,
                                                   CampaignActionResult status, VoucherReceipentInfo[] voucherReceipents);
    }
}
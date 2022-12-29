using WebAPI.Models.General;

namespace WebAPI.ObjectsConvertor.Extensions
{
    public static class AppTokenMapper
    {
        internal static int getSessionDuration(this KalturaAppToken model)
        {
            return model.SessionDuration.HasValue ? (int)model.SessionDuration : 0;
        }

        internal static int getPartnerId(this KalturaAppToken model)
        {
            return model.PartnerId.HasValue ? (int)model.PartnerId : 0;
        }

        internal static int getExpiry(this KalturaAppToken model)
        {
            return model.Expiry.HasValue ? (int)model.Expiry : 0;
        }
    }
}

using WebAPI.Managers.Models;
using WebAPI.Models.General;

namespace WebAPI.ModelsFactory
{
    public static class AppTokenFactory
    {
        public static KalturaAppToken Create(AppToken appToken)
        {
            var instance = new KalturaAppToken
            {
                Id = appToken.AppTokenId,
                Expiry = appToken.Expiry,
                PartnerId = appToken.PartnerId,
                SessionDuration = appToken.SessionDuration,
                HashType = appToken.HashType,
                SessionPrivileges = appToken.SessionPrivileges,
                SessionType = appToken.SessionType,
                Status = appToken.Status,
                Token = appToken.Token,
                SessionUserId = appToken.SessionUserId,
                CreateDate = appToken.CreateDate,
                UpdateDate = appToken.UpdateDate
            };
            return instance;
        }
    }
}

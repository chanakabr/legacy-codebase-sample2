using TVinciShared;
using WebAPI.Managers.Models;
using WebAPI.Models.Users;

namespace WebAPI.ModelsFactory
{
    public static class SessionFactory
    {
        public static KalturaSession Create(KS ks)
        {
            var payload = KSUtils.ExtractKSPayload(ks);
            var response = new KalturaSession(null)
            {
                ks = ks.ToString(),
                expiry = (int)DateUtils.DateTimeToUtcUnixTimestampSeconds(ks.Expiration),
                partnerId = ks.GroupId,
                privileges = KS.JoinPrivileges(ks.Privileges, ",", ":"),
                sessionType = ks.SessionType,
                userId = ks.UserId,
                udid = payload.UDID,
                createDate = payload.CreateDate
            };
            return response;
        }
    }

    public static class SessionInfoFactory
    {
        public static KalturaSessionInfo Create(KS ks)
        {
            var session = SessionFactory.Create(ks);
            var sessionInfo = new KalturaSessionInfo()
            {
                ks = session.ks,
                sessionType = session.sessionType,
                partnerId = session.partnerId,
                userId = session.userId,
                expiry = session.expiry,
                privileges = session.privileges,
                udid = session.udid,
                createDate = session.createDate
            };
            return sessionInfo;
        }
    }
}

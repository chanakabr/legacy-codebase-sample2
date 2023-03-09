using ApiObjects.Roles;

namespace ApiObjects.User
{
    public static class UserIdExtensions
    {
        public static bool IsAnonymous(this string userIdString)
        {
            return string.IsNullOrEmpty(userIdString) || userIdString == PredefinedRoleId.ANONYMOUS.ToString();
        }

        public static bool IsAnonymous(this long userId)
        {
            return userId == PredefinedRoleId.ANONYMOUS;
        }

        public static long ParseUserId(this string userIdString, long invalidValue = PredefinedRoleId.ANONYMOUS)
        {
            if (IsAnonymous(userIdString)) return PredefinedRoleId.ANONYMOUS;

            return long.TryParse(userIdString, out var userId) && userId > 0
                ? userId
                : invalidValue;
        }
    }
}

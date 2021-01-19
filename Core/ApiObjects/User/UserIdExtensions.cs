namespace ApiObjects.User
{
    public static class UserIdExtensions
    {
        private const int ANONYMOUS = 0;
        private const string ANONYMOUS_STRING = "0";        

        public static bool IsAnonymous(this string userIdString)
        {
            return string.IsNullOrEmpty(userIdString) || userIdString == ANONYMOUS_STRING;
        }

        public static bool IsAnonymous(this long userId)
        {
            return userId == ANONYMOUS;
        }

        public static long ParseUserId(this string userIdString, long invalidValue = ANONYMOUS)
        {
            if (IsAnonymous(userIdString)) return ANONYMOUS;

            return long.TryParse(userIdString, out var userId) && userId > 0
                ? userId
                : invalidValue;
        }
    }
}

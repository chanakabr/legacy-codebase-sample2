using System;

namespace TVinciShared
{
    public class SystemDateTime: IDisposable
    {
        private static DateTime? _utcNowForTest;

        public static DateTime UtcNow
        {
            get { return _utcNowForTest ?? DateTime.UtcNow; }
        }

        public static IDisposable UtcNowIs(DateTime dateTime)
        {
            _utcNowForTest = dateTime;
            return new SystemDateTime();
        }

        public void Dispose()
        {
            _utcNowForTest = null;
        }
    }
}

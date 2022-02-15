using System;

namespace DAL.BulkUpload
{
    public static class BulkUploadRetryStrategy
    {
        public static Func<int, TimeSpan> Linear = numOfTries => TimeSpan.FromMilliseconds(numOfTries * 100);

        public static Func<int, TimeSpan> Const = _ => TimeSpan.FromMilliseconds(50);
    }
}

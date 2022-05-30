using System;

namespace CouchbaseManager.Exceptions
{
    public class KeySizeExceededException : Exception
    {
        private const int CACHE_KEY_MAX_SIZE = 250;

        public string Key { get; }

        public override string Message =>
            string.Format("Cache key is too big and doesn't match CB limits of {1} bytes: {0}", Key,
                CACHE_KEY_MAX_SIZE);

        public KeySizeExceededException(string key)
        {
            Key = key;
        }
    }
}
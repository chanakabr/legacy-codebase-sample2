
using CachingProvider.LayeredCache;
using Moq;
using System;
using System.Collections.Generic;

namespace ApiLogic.Tests
{
    public static class LayeredCacheHelper
    {
        delegate void MockGetFromCache<T>(string key, ref T genericParameter, Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod, Dictionary<string, object> funcParameters,
            int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null, bool shouldUseAutoNameTypeHandling = false);

        public static Mock<ILayeredCache> GetLayeredCacheMock<T>(T mockedValue, bool cacheWork, bool setInvalidationKey)
        {
            var layeredCacheMock = new Mock<ILayeredCache>();
            layeredCacheMock.Setup(mockedValue, cacheWork, setInvalidationKey);

            return layeredCacheMock;
        }

        public static Mock<ILayeredCache> Setup<T>(this Mock<ILayeredCache> layeredCacheMock, T cacheValue, bool success, bool setInvalidationKey)
        {
            layeredCacheMock
                .Setup(x => x.Get(
                    It.IsAny<string>(),
                    ref It.Ref<T>.IsAny,
                    It.IsAny<Func<Dictionary<string, object>, Tuple<T, bool>>>(),
                    It.IsAny<Dictionary<string, object>>(),
                    It.IsAny<int>(),
                    It.IsAny<string>(),
                    It.IsAny<List<string>>(),
                    It.IsAny<bool>()))
                .Callback(new MockGetFromCache<T>((string key,
                    ref T genericParameter,
                    Func<Dictionary<string, object>, Tuple<T, bool>> fillObjectMethod,
                    Dictionary<string, object> funcParameters,
                    int groupId,
                    string layeredCacheConfigName,
                    List<string> inValidationKeys,
                    bool shouldUseAutoNameTypeHandling) =>
                {
                    genericParameter = cacheValue;
                }))
                .Returns(success);

            if (setInvalidationKey)
            {
                layeredCacheMock.Setup(x => x.SetInvalidationKey(It.IsAny<string>(), null))
                    .Returns(true);
            }

            return layeredCacheMock;
        }
    }
}
using System;
using System.Threading;

namespace KalturaRequestContext
{
    public class RequestContextUtilsInstance
    {
        private static readonly Lazy<IRequestContextUtils> Instance = new Lazy<IRequestContextUtils>(() => new RequestContextUtils(), LazyThreadSafetyMode.PublicationOnly);

        public static IRequestContextUtils Get() => Instance.Value;
    }
}
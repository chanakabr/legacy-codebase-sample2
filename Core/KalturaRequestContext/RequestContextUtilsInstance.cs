using System;
using System.Threading;

namespace KalturaRequestContext
{
    public class RequestContextUtilsInstance
    {
        private static readonly Lazy<RequestContextUtils> Instance = new Lazy<RequestContextUtils>(() => new RequestContextUtils(), LazyThreadSafetyMode.PublicationOnly);

        public static IRequestContextUtils Get() => Instance.Value;
        public static RequestContextUtils Setter() => Instance.Value;
    }
}
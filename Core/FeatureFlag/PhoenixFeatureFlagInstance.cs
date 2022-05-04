using System;
using System.Threading;
using FeatureFlag.Context;
using KalturaRequestContext;

namespace FeatureFlag
{
    public class PhoenixFeatureFlagInstance
    {
        private static readonly Lazy<IPhoenixFeatureFlag> Instance = new Lazy<IPhoenixFeatureFlag>(Create, LazyThreadSafetyMode.PublicationOnly);

        public static IPhoenixFeatureFlag Get() => Instance.Value;
        private static IPhoenixFeatureFlag Create() => new PhoenixFeatureFlag(new RequestContextUtilsAdapter(new RequestContextUtils()));
    }
}

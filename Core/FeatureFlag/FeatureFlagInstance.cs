using System;
using System.Threading;

namespace FeatureFlag
{
    public class FeatureFlagInstance
    {
        private static readonly Lazy<IFeatureFlag> Instance = new Lazy<IFeatureFlag>(Create, LazyThreadSafetyMode.PublicationOnly);

        public static IFeatureFlag Get() => Instance.Value;

        private static IFeatureFlag Create() => new LaunchDarklyFeatureFlag("sdk-29c492d3-8151-42d9-9e8a-986e35f8acf5"); // TODO key from config
    }
}
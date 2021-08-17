using System;
using System.Threading;

namespace FeatureFlag
{
    public static class FeatureFlagInstance
    {
        private static readonly Lazy<IFeatureFlag> Instance = new Lazy<IFeatureFlag>(Create, LazyThreadSafetyMode.PublicationOnly);

        public static IFeatureFlag Get() => Instance.Value;
        
        /// <summary>
        /// Replace dummy implementation with real one
        /// </summary>
        /// <returns></returns>
        private static IFeatureFlag Create() => new LaunchDarklyDummyFeatureFlag();
    }
}
using System.Runtime.CompilerServices;

[assembly:InternalsVisibleTo("Core.Metrics.Tests")]

namespace Core.Metrics.Internals
{
    internal enum DotNetMetric
    {
        GC,
        ThreadPool
    }
}
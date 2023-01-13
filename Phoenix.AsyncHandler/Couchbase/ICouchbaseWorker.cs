using System.Collections.Generic;

namespace Phoenix.AsyncHandler.Couchbase
{
    public interface ICouchbaseWorker
    {
        Dictionary<string, string> GetKronosFeatureToggel();
    }
}
using System;
using System.Collections.Generic;
using KLogMonitor;
using System.Reflection;
namespace Core.Catalog
{
    // TODO: Arthur: This was a class that was using simpleInjector
    // as part of preperation for net core conversion this will be a shim for a static service provider .. 
    // we will replace all calls to this with a service provider by net core when it will be implemented
    public static class Bootstrapper
    {
        
        public static void Bootstrap()
        {
            // See TODO Comment above...
        }
        
        public static ISearcher GetInstance<T>() where T : ISearcher
        {
            return new ElasticsearchWrapper();
        }
    }
}

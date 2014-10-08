using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleInjector;
using SimpleInjector.Extensions;
using SimpleInjector.Integration.Wcf;
using ApiObjects.SearchObjects;
using Catalog.Searchers;
namespace Catalog
{
    public static class Bootstrapper
    {
        private static Container container;
        private static bool isInitialized = false;
        public static void Bootstrap()
        {
            try
            {
                if (!isInitialized)
                {
                    // Create the container
                    container = new Container();
                    Type searcherType = Type.GetType(Utils.GetWSURL("media_searcher"));                   
                    
                    // Register your types, for instance:
                    container.Register(typeof(ISearcher), searcherType);                   
                    

                    // Register the container to the SimpleInjectorServiceHostFactory.
                    SimpleInjectorServiceHostFactory.SetContainer(container);

                    container.Verify();
                    isInitialized = true;
                }
            }
            catch (Exception ex)
            {
                Logger.Logger.Log("Catalog", "Exception :" + ex.Message, "Bootstrapper");
            }
        }

        public static object GetInstance(Type serviceType)
        {
            return container.GetInstance(serviceType);
        }
        public static T GetInstance<T>() where T : class
        {
            return container.GetInstance<T>();
        }

        public static IEnumerable<T> GetAllInstances<T>() where T : class
        {
            return container.GetAllInstances<T>();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SimpleInjector;
using SimpleInjector.Extensions;
using SimpleInjector.Integration.Wcf;
using ApiObjects.SearchObjects;
using Core.Catalog.Searchers;
using KLogMonitor;
using System.Reflection;
namespace Core.Catalog
{
    public static class Bootstrapper
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

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
                log.Error("Catalog - " + String.Concat("Ex Msg: ", ex.Message, " Ex Type: ", ex.GetType().Name, " ST: ", ex.StackTrace), ex);
            }
        }

        public static object GetInstance(Type serviceType)
        {
            Bootstrap();
            return container.GetInstance(serviceType);
        }
        public static T GetInstance<T>() where T : class
        {
            Bootstrap();
            return container.GetInstance<T>();
        }

        public static IEnumerable<T> GetAllInstances<T>() where T : class
        {
            Bootstrap();
            return container.GetAllInstances<T>();
        }
    }
}

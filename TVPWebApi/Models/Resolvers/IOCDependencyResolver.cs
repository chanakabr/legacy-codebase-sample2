using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Dependencies;
using SimpleInjector;

namespace TVPWebApi.Models
{
    public class IOCDependencyResolver : IDependencyResolver
    {
        private readonly Container container;

        public IOCDependencyResolver(Container container)
        {
            this.container = container;
        }

        public IDependencyScope BeginScope()
        {
            return this;
        }

        public object GetService(Type serviceType)
        {
            return ((IServiceProvider)this.container)
                .GetService(serviceType);
        }

        public IEnumerable<object> GetServices(Type serviceType)
        {
            return this.container.GetAllInstances(serviceType);
        }

        public void Dispose()
        {
        }
    }
}
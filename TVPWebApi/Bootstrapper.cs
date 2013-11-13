using System.Web.Http;
using SimpleInjector;
using TVPWebApi.Models;

namespace TVPWebApi
{
    public static class IOCBootstrapper
    {
        public static void Initialise()
        {
            var container = BuildIOCContainer();

            GlobalConfiguration.Configuration.DependencyResolver = new IOCDependencyResolver(container);
        }

        private static Container BuildIOCContainer()
        {
            var container = new Container();

            var services = GlobalConfiguration.Configuration.Services;

            var controllerTypes = services.GetHttpControllerTypeResolver().GetControllerTypes(services.GetAssembliesResolver());

            // register Web API controllers
            foreach (var controllerType in controllerTypes)
            {
                container.Register(controllerType);
            }

            // Register your types
            container.Register<IUsersService, UsersService>();
            container.Register<IMediasService, MediasService>();
            container.Register<IChannelsService, ChannelsService>();

            // Verify the container configuration
            container.Verify();

            return container;
        }
    }
}
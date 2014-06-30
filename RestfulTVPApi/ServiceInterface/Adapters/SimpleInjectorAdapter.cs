using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using ServiceStack.Configuration;
using SimpleInjector;

namespace RestfulTVPApi.ServiceInterface
{
    public class SimpleInjectorAdapter : IContainerAdapter
    {
        private readonly Container container;

        public SimpleInjectorAdapter()
        {
            this.container = new Container();

            this.container.Register<IUsersRepository, UsersRepository>();
            this.container.Register<IMediasRepository, MediasRepository>();
            this.container.Register<IChannelsRepository, ChannelsRepository>();
            this.container.Register<ISubscriptionsRepository, SubscriptionsRepository>();
            this.container.Register<ICollectionRepository, CollectionRepository>();
            this.container.Register<IGroupsRepository, GroupsRepository>();
            this.container.Register<INotificationsRepository, NotificationsRepository>();
            this.container.Register<IApiRepository, ApiRepository>();
            this.container.Register<ISiteRepository, SiteRepository>();
            this.container.Register<IDomainRepository, DomainRepository>();
        }

        public SimpleInjectorAdapter(Container container)
        {
            this.container = container;
        }

        public T Resolve<T>()
        {
            return (T)this.container.GetInstance(typeof(T));
        }

        public T TryResolve<T>()
        {
            IServiceProvider provider = this.container;
            object service = provider.GetService(typeof(T));
            return service != null ? (T)service : default(T);
        }
    }
}
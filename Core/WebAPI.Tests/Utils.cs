using AutoFixture;
using AutoFixture.Kernel;
using Microsoft.AspNetCore.Http;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using KalturaRequestContext;
using Microsoft.Extensions.DependencyInjection;
using WebAPI.Managers.Scheme;

namespace WebAPI.Tests
{
    public class ServiceProviderMock : IServiceProvider
    {
        public object GetService(Type serviceType)
        {
            throw new NotImplementedException();
        }
    }

    public class SessionMock : ISession
    {
        public bool IsAvailable => throw new NotImplementedException();

        public string Id => throw new NotImplementedException();

        public IEnumerable<string> Keys => throw new NotImplementedException();

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public Task CommitAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task LoadAsync(CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public void Remove(string key)
        {
            throw new NotImplementedException();
        }

        public void Set(string key, byte[] value)
        {
            throw new NotImplementedException();
        }

        public bool TryGetValue(string key, out byte[] value)
        {
            throw new NotImplementedException();
        }
    }

    public class ServiceScopeFactoryMock : IServiceScopeFactory
    {
        public IServiceScope CreateScope()
        {
            throw new NotImplementedException();
        }
    } 

    public class Utils
    {
        public static void SetUp()
        {
            var version = OldStandardAttribute.getCurrentRequestVersion();
            Fixture fixture = new Fixture();
            fixture.Customizations.Add(new TypeRelay(typeof(IServiceProvider), typeof(ServiceProviderMock)));
            fixture.Customizations.Add(new TypeRelay(typeof(ISession), typeof(SessionMock)));
            fixture.Customizations.Add(new TypeRelay(typeof(IServiceScopeFactory), typeof(ServiceScopeFactoryMock)));
            var httpContext = fixture.Create<DefaultHttpContext>();
            httpContext.Items[RequestContextConstants.REQUEST_VERSION] = version;
            var httpContextAccessor = new Mock<IHttpContextAccessor>();
            httpContextAccessor.SetupGet(x => x.HttpContext).Returns(httpContext);
            System.Web.HttpContext.Configure(httpContextAccessor.Object);
        }
    }
}

using AutoFixture;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Partner;
using WebAPI.ModelsValidators;

namespace WebAPI.Tests.Models.Partner
{
    [TestFixture]
    public class KalturaCatalogPartnerConfigTests
    {
        [Test]
        public void CheckValidateUpdate()
        {
            Fixture fixture = new Fixture();
            var kalturaCategoryVersion = fixture.Create<KalturaCatalogPartnerConfig>();
            kalturaCategoryVersion.CategoryManagement.DefaultCategoryTreeId = null;

            // validate empty name
            Assert.Throws(Is.TypeOf<BadRequestException>()
                .And.Property(nameof(BadRequestException.Code)).EqualTo((int)StatusCode.ArgumentCannotBeEmpty),
                () =>
                {
                    kalturaCategoryVersion.ValidateForUpdate();
                });
        }
    }
}

using AutoFixture;
using NUnit.Framework;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Partner;
using WebAPI.Models.Pricing;

namespace WebAPI.Tests.Models.Pricing
{
    [TestFixture]
    public class KalturaPriceDetailsTest
    {
        [Test]
        public void CheckValidateForAdd()
        {
            KalturaPriceDetails kalturaPriceDetails = new KalturaPriceDetails();

            // validate add
            Assert.Throws(Is.TypeOf<BadRequestException>()
                .And.Property(nameof(BadRequestException.Code)).EqualTo((int)StatusCode.ArgumentCannotBeEmpty),
                () =>
                {
                    kalturaPriceDetails.ValidateForAdd();
                });
        }
    }
}

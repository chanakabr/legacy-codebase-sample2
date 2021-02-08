using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;

namespace WebAPI.Tests.Models.Catalog
{
    [TestFixture]
    public class KalturaCategoryVersionTests
    {
        [Test]
        public void CheckValidateForAdd()
        {
            KalturaCategoryVersion kalturaCategoryVersion = new KalturaCategoryVersion();

            // validate empty name
            Assert.Throws(Is.TypeOf<BadRequestException>()
                .And.Property(nameof(BadRequestException.Code)).EqualTo((int)StatusCode.ArgumentCannotBeEmpty),
                () =>
                {
                    kalturaCategoryVersion.ValidateForAdd();
                });
        }
    }
}

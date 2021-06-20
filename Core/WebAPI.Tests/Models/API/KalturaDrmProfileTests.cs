using AutoFixture;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;

namespace WebAPI.Tests.Models.API
{
    [TestFixture]
    class KalturaDrmProfileTests
    {
        [TestCaseSource(nameof(ValidateForAddCases))]
        public void CheckValidateForAddErrors(KalturaDrmProfile kalturaDrmProfile, StatusCode expectedCode)
        {
            // validate empty name
            Assert.Throws(Is.TypeOf<BadRequestException>().And.Property(nameof(BadRequestException.Code))
                .EqualTo((int)expectedCode),
                () =>
                {
                    kalturaDrmProfile.ValidateForAdd();
                });
        }

        private static IEnumerable ValidateForAddCases()
        {
            Fixture fixture = new Fixture();

            var kalturaDrmProfile = fixture.Create<KalturaDrmProfile>();
            kalturaDrmProfile.Name = "";
            yield return new TestCaseData(kalturaDrmProfile, StatusCode.ArgumentCannotBeEmpty).SetName("ValidateForAddWithoutName");

            kalturaDrmProfile = fixture.Create<KalturaDrmProfile>();
            kalturaDrmProfile.AdapterUrl = "";
            yield return new TestCaseData(kalturaDrmProfile, StatusCode.ArgumentCannotBeEmpty).SetName("ValidateForAddWithoutAdapterUrl");

            kalturaDrmProfile = fixture.Create<KalturaDrmProfile>();
            kalturaDrmProfile.SystemName = "";
            yield return new TestCaseData(kalturaDrmProfile, StatusCode.ArgumentCannotBeEmpty).SetName("ValidateForAddWithoutSystemName");
        }
    }
}

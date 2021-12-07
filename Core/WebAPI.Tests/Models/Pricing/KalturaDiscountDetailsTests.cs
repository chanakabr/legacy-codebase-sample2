using AutoFixture;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Pricing;

namespace WebAPI.Tests.Models.Pricing
{
    [TestFixture]
    class KalturaDiscountDetailsTests
    {

        [TestCaseSource(nameof(ValidateForAddCases))]
        public void CheckValidateForAddErrors(KalturaDiscountDetails KalturaDiscountDetailsVersion, StatusCode expectedCode)
        {
            // validate empty name
            Assert.Throws(Is.TypeOf<BadRequestException>().And.Property(nameof(BadRequestException.Code))
                .EqualTo((int)expectedCode),
                () =>
                {
                    KalturaDiscountDetailsVersion.ValidateForAdd();
                });
        }

        private static IEnumerable ValidateForAddCases()
        {
            Fixture fixture = new Fixture();

            var KalturaDiscountDetailsVersion = fixture.Create<KalturaDiscountDetails>();
            KalturaDiscountDetailsVersion.name = "";
            yield return new TestCaseData(KalturaDiscountDetailsVersion, StatusCode.ArgumentCannotBeEmpty).SetName("ValidateForAddWithoutName12");

            KalturaDiscountDetailsVersion = fixture.Create<KalturaDiscountDetails>();
            KalturaDiscountDetailsVersion.StartDate = 0;
            yield return new TestCaseData(KalturaDiscountDetailsVersion, StatusCode.ArgumentCannotBeEmpty).SetName("ValidateForAddWithoutStartDate");

            KalturaDiscountDetailsVersion = fixture.Create<KalturaDiscountDetails>();
            KalturaDiscountDetailsVersion.EndtDate = 0;
            yield return new TestCaseData(KalturaDiscountDetailsVersion, StatusCode.ArgumentCannotBeEmpty).SetName("ValidateForAddWithoutEndtDate");

            KalturaDiscountDetailsVersion = fixture.Create<KalturaDiscountDetails>();
            KalturaDiscountDetailsVersion.MultiCurrencyDiscount = new List<KalturaDiscount>();
            yield return new TestCaseData(KalturaDiscountDetailsVersion, StatusCode.ArgumentCannotBeEmpty).SetName("ValidateForAddWithoutMultiCurrencyDiscount");

            KalturaDiscountDetailsVersion = fixture.Create<KalturaDiscountDetails>();
            KalturaDiscountDetailsVersion.WhenAlgoType = 3;
            yield return new TestCaseData(KalturaDiscountDetailsVersion, StatusCode.EnumValueNotSupported).SetName("ValidateForAddWitIncorrectEnum");

            KalturaDiscountDetailsVersion = fixture.Create<KalturaDiscountDetails>();
            KalturaDiscountDetailsVersion.WhenAlgoType = 1;
            KalturaDiscount discount = fixture.Create<KalturaDiscount>();
            discount.Amount = 0;
            discount.Percentage = 0;
            KalturaDiscountDetailsVersion.MultiCurrencyDiscount = new List<KalturaDiscount>();
            KalturaDiscountDetailsVersion.MultiCurrencyDiscount.Add(discount);
            yield return new TestCaseData(KalturaDiscountDetailsVersion, StatusCode.ArgumentsCannotBeEmpty).SetName("ValidateForAddWitDiscountAmount0AndDiscountPercentage0");

            KalturaDiscountDetailsVersion = fixture.Create<KalturaDiscountDetails>();
            KalturaDiscountDetailsVersion.WhenAlgoType = 1;
            discount = fixture.Create<KalturaDiscount>();
            discount.Amount = 1;
            discount.Percentage = 1;
            KalturaDiscountDetailsVersion.MultiCurrencyDiscount = new List<KalturaDiscount>();
            KalturaDiscountDetailsVersion.MultiCurrencyDiscount.Add(discount);
            yield return new TestCaseData(KalturaDiscountDetailsVersion, StatusCode.ArgumentsConflictsEachOther).SetName("ValidateForAddWitDiscountAmountAndDiscountPercentage");
        }
    }
}

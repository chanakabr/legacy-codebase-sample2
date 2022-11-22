using Newtonsoft.Json.Linq;
using NUnit.Framework;
using System.Collections;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Catalog;

namespace WebAPI.Tests.Managers
{
    [TestFixture]
    public class SchemeInputTests
    {
        [TestCaseSource(nameof(ValidateErrorCases))]
        public void CheckValidateError(SchemeInputAttribute schemeInput, StatusCode expectedCode, string propertyName, object propertyValue)
        {
            Assert.Throws(Is.TypeOf<BadRequestException>().And.Property(nameof(BadRequestException.Code))
                .EqualTo((int)expectedCode),
                () =>
                {
                    schemeInput.Validate(propertyName, propertyValue);
                });
        }

        private static IEnumerable ValidateErrorCases()
        {
            // DynamicType
            var schemeInput = new SchemeInputAttribute() { DynamicType = typeof(KalturaAssetType) };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentShouldBeEnum, "DynamicTypeProp", "notexist").SetName("DynamicType-ArgumentShouldBeEnum-notexist");

            schemeInput = new SchemeInputAttribute() { DynamicType = typeof(KalturaAssetType) };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentShouldBeEnum, "DynamicTypeProp", "5").SetName("DynamicType-ArgumentShouldBeEnum-number");

            // DynamicMinInt
            schemeInput = new SchemeInputAttribute() { DynamicMinInt = 10 };
            yield return new TestCaseData(schemeInput, StatusCode.InvalidArgument, "DynamicMinIntProp", "12,notanumber").SetName("DynamicMinInt-InvalidArgument");

            schemeInput = new SchemeInputAttribute() { DynamicMinInt = 4 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentShouldContainMinValueCrossed, "DynamicMinIntProp", "3,5").SetName("DynamicMinInt-ArgumentShouldContainMinValueCrossed");

            schemeInput = new SchemeInputAttribute() { DynamicMinInt = 7 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentsCannotBeEmpty, "DynamicMinIntProp", "").SetName("DynamicMinInt-ArgumentsCannotBeEmpty");

            // DynamicMaxInt
            schemeInput = new SchemeInputAttribute() { DynamicMaxInt = 14 };
            yield return new TestCaseData(schemeInput, StatusCode.InvalidArgument, "DynamicMaxIntProp", "12,notanumber").SetName("DynamicMaxInt-InvalidArgument");

            schemeInput = new SchemeInputAttribute() { DynamicMaxInt = 4 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentShouldContainMaxValueCrossed, "DynamicMaxIntProp", "3,5").SetName("DynamicMaxInt-ArgumentShouldContainMaxValueCrossed");

            schemeInput = new SchemeInputAttribute() { DynamicMaxInt = 7 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentsCannotBeEmpty, "DynamicMaxIntProp", "").SetName("DynamicMaxInt-ArgumentsCannotBeEmpty");

            // MaxLength
            schemeInput = new SchemeInputAttribute() { MaxLength = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMaxLengthCrossed, "MaxLengthProp", "1234").SetName("MaxLength-ArgumentMaxLengthCrossed");

            // MinLength
            schemeInput = new SchemeInputAttribute() { MinLength = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMinLengthCrossed, "MinLengthProp", "12").SetName("MinLength-ArgumentMinLengthCrossed");

            // MaxInteger
            schemeInput = new SchemeInputAttribute() { MaxInteger = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMaxValueCrossed, "MaxIntegerProp", 4).SetName("MaxInteger-ArgumentMaxValueCrossed");

            // MinInteger
            schemeInput = new SchemeInputAttribute() { MinInteger = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMinValueCrossed, "MinIntegerProp", 2).SetName("MinInteger-ArgumentMinValueCrossed");

            // MaxLong
            schemeInput = new SchemeInputAttribute() { MaxLong = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMaxValueCrossed, "MaxLongProp", 4).SetName("MaxLong-ArgumentMaxValueCrossed");

            // MinLong
            schemeInput = new SchemeInputAttribute() { MinLong = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMinValueCrossed, "MinLongProp", 2).SetName("MinLong-ArgumentMinValueCrossed");

            // MaxFloat
            schemeInput = new SchemeInputAttribute() { MaxFloat = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMaxValueCrossed, "MaxFloatProp", 4).SetName("MaxFloat-ArgumentMaxValueCrossed");

            // MinFloat
            schemeInput = new SchemeInputAttribute() { MinFloat = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMinValueCrossed, "MinFloatProp", 2).SetName("MinFloat-ArgumentMinValueCrossed");

            // Pattern
            schemeInput = new SchemeInputAttribute() { Pattern = @"\b[M]\w+" }; // (pattern for a word that starts with letter "M")
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMatchPatternCrossed, "PatternProp", "").SetName("Pattern-Empty");

            schemeInput = new SchemeInputAttribute() { Pattern = @"\b[M]\w+" }; // (pattern for a word that starts with letter "M")
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMatchPatternCrossed, "PatternProp", "TestInvalidMatchPattern").SetName("Pattern-ArgumentMatchPatternCrossed");

            schemeInput = new SchemeInputAttribute() { Pattern = "[a-9]" };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMatchPatternCrossed, "PatternProp", "TestInvalidPattern123").SetName("Pattern-ArgumentException");

            // MinItems
            schemeInput = new SchemeInputAttribute() { MinItems = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMinItemsCrossed, "MinItemsProp", new[] { 1, 2 }).SetName("MinItems-ArgumentMinItemsCrossed-array");

            schemeInput = new SchemeInputAttribute() { MinItems = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMinItemsCrossed, "MinItemsProp", new JArray() { 1, 2 }).SetName("MinItems-ArgumentMinItemsCrossed-jarray");

            // MaxItems
            schemeInput = new SchemeInputAttribute() { MaxItems = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMaxItemsCrossed, "MaxItemsProp", new[] { 1, 2 , 3, 4}).SetName("MaxItems-ArgumentMaxItemsCrossed-array");

            // MaxItems
            schemeInput = new SchemeInputAttribute() { MaxItems = 3 };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentMaxItemsCrossed, "MaxItemsProp", new JArray() { 1, 2, 3, 4 }).SetName("MaxItems-ArgumentMaxItemsCrossed-jarray");

            // UniqueItems
            schemeInput = new SchemeInputAttribute() { UniqueItems = true };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentsDuplicate, "UniqueItemsProp", new[] { 1, 2, 3, 3 }).SetName("UniqueItems-ArgumentsDuplicate-array");

            schemeInput = new SchemeInputAttribute() { UniqueItems = true };
            yield return new TestCaseData(schemeInput, StatusCode.ArgumentsDuplicate, "UniqueItemsProp", new JArray() { 1, 2, 3, 3 }).SetName("UniqueItems-ArgumentsDuplicate-jarray");
        }
    }
}

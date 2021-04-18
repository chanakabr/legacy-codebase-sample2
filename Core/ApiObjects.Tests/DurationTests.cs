using NUnit.Framework;
using System.Collections;

namespace ApiObjects.Tests
{
    [TestFixture]
    public class DurationTests
    {
        [TestCaseSource(nameof(CreateGetTvmDurationTestCases))]
        public void CheckGetTvmDuration(Duration duration, TvmDurationUnit unitExpected)
        {   
            Assert.That(duration.GetTvmDuration(), Is.EqualTo((int)unitExpected));
        }

        private static IEnumerable CreateGetTvmDurationTestCases()
        {
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.OneMonth), TvmDurationUnit.OneMonth).SetName("GetTvmDuration_OneMonth");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TwoMonths), TvmDurationUnit.TwoMonths).SetName("GetTvmDuration_TwoMonth");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThreeMonths), TvmDurationUnit.ThreeMonths).SetName("GetTvmDuration_ThreeMonth");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.FourMonths), TvmDurationUnit.FourMonths).SetName("GetTvmDuration_FourMonth");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.FiveMonths), TvmDurationUnit.FiveMonths).SetName("GetTvmDuration_FiveMonth");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.SixMonths), TvmDurationUnit.SixMonths).SetName("GetTvmDuration_SixMonth");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.NineMonths), TvmDurationUnit.NineMonths).SetName("GetTvmDuration_NineMonth");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.OneYear), TvmDurationUnit.OneYear).SetName("GetTvmDuration_OneYear");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TwoYears), TvmDurationUnit.TwoYears).SetName("GetTvmDuration_TwoYears");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThreeYears), TvmDurationUnit.ThreeYears).SetName("GetTvmDuration_ThreeYears");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.FourYears), TvmDurationUnit.FourYears).SetName("GetTvmDuration_FourYears");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FiveYears), TvmDurationUnit.FiveYears).SetName("GetTvmDuration_FiveYears");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TenYears), TvmDurationUnit.TenYears).SetName("GetTvmDuration_TenYears");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.OneHundredYears), TvmDurationUnit.OneHundredYears).SetName("GetTvmDuration_OneHundredYears");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.OneMinutes), TvmDurationUnit.OneMinutes).SetName("GetTvmDuration_OneMinutes");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThreeMinutes), TvmDurationUnit.ThreeMinutes).SetName("GetTvmDuration_ThreeMinutes");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.FiveMinutes),  TvmDurationUnit.FiveMinutes).SetName("GetTvmDuration_FiveMinutes");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TenMinutes), TvmDurationUnit.TenMinutes).SetName("GetTvmDuration_TenMinutes");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.FifteenMinutes), TvmDurationUnit.FifteenMinutes).SetName("GetTvmDuration_FifteenMinutes");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThirtyMinutes), TvmDurationUnit.ThirtyMinutes).SetName("GetTvmDuration_ThirtyMinutes");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.OneHours), TvmDurationUnit.OneHours).SetName("GetTvmDuration_OneHours");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TwoHours), TvmDurationUnit.TwoHours).SetName("GetTvmDuration_TwoHours");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThreeHours), TvmDurationUnit.ThreeHours).SetName("GetTvmDuration_ThreeHours");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.SixHours), TvmDurationUnit.SixHours).SetName("GetTvmDuration_SixHours");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.NineHours), TvmDurationUnit.NineHours).SetName("GetTvmDuration_NineHours");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TwelveHours), TvmDurationUnit.TwelveHours).SetName("GetTvmDuration_TwelveHours");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.EighteenHours), TvmDurationUnit.EighteenHours).SetName("GetTvmDuration_EighteenHours");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.OneDays), TvmDurationUnit.OneDays).SetName("GetTvmDuration_OneDays");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TwoDays), TvmDurationUnit.TwoDays).SetName("GetTvmDuration_TwoDaysTwoDays");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThreeDays), TvmDurationUnit.ThreeDays).SetName("GetTvmDuration_ThreeDays");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.FiveDays), TvmDurationUnit.FiveDays).SetName("GetTvmDuration_FiveDays");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TenDays), TvmDurationUnit.TenDays).SetName("GetTvmDuration_TenDays");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThirtyDays), TvmDurationUnit.ThirtyDays).SetName("GetTvmDuration_ThirtyDays");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThirtyOneDays), TvmDurationUnit.ThirtyOneDays).SetName("GetTvmDuration_ThirtyOneDays");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.OneWeeks), TvmDurationUnit.OneWeeks).SetName("GetTvmDuration_OneWeeks");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.TwoWeeks), TvmDurationUnit.TwoWeeks).SetName("GetTvmDuration_TwoWeeks");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.ThreeWeeks), TvmDurationUnit.ThreeWeeks).SetName("GetTvmDuration_ThreeWeeks");
            yield return new TestCaseData(new Duration((long) TvmDurationUnit.FourWeeks), TvmDurationUnit.FourWeeks).SetName("GetTvmDuration_FourWeeks");

            yield return new TestCaseData(new Duration((long) 45487), 0).SetName("GetTvmDuration_NumberNotInEnum1");
            yield return new TestCaseData(new Duration((long) 74), 0).SetName("GetTvmDuration_NumberNotInEnum2");
        }
    }
}
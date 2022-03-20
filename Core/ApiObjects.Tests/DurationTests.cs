using NUnit.Framework;
using System;
using System.Collections;

namespace ApiObjects.Tests
{
    [TestFixture]
    public class DurationTests
    {
        [TestCaseSource(nameof(CreateGetTvmDurationTestCases))]
        public void CheckGetTvmDuration(Duration duration, TvmDurationUnit unitExpected)
        {
            Assert.That(duration.GetTvmDuration(), Is.EqualTo((long)unitExpected));
        }

        private static IEnumerable CreateGetTvmDurationTestCases()
        {
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.OneMonth), TvmDurationUnit.OneMonth).SetName("GetTvmDuration_OneMonth");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TwoMonths), TvmDurationUnit.TwoMonths).SetName("GetTvmDuration_TwoMonth");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThreeMonths), TvmDurationUnit.ThreeMonths).SetName("GetTvmDuration_ThreeMonth");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FourMonths), TvmDurationUnit.FourMonths).SetName("GetTvmDuration_FourMonth");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FiveMonths), TvmDurationUnit.FiveMonths).SetName("GetTvmDuration_FiveMonth");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.SixMonths), TvmDurationUnit.SixMonths).SetName("GetTvmDuration_SixMonth");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.SevenMonths), TvmDurationUnit.SevenMonths).SetName("GetTvmDuration_SevenMonths");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.EightMonths), TvmDurationUnit.EightMonths).SetName("GetTvmDuration_EightMonths");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.NineMonths), TvmDurationUnit.NineMonths).SetName("GetTvmDuration_NineMonth");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TenMonths), TvmDurationUnit.TenMonths).SetName("GetTvmDuration_TenMonths");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ElevenMonths), TvmDurationUnit.ElevenMonths).SetName("GetTvmDuration_ElevenMonths");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.OneYear), TvmDurationUnit.OneYear).SetName("GetTvmDuration_OneYear");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TwoYears), TvmDurationUnit.TwoYears).SetName("GetTvmDuration_TwoYears");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThreeYears), TvmDurationUnit.ThreeYears).SetName("GetTvmDuration_ThreeYears");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FourYears), TvmDurationUnit.FourYears).SetName("GetTvmDuration_FourYears");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FiveYears), TvmDurationUnit.FiveYears).SetName("GetTvmDuration_FiveYears");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TenYears), TvmDurationUnit.TenYears).SetName("GetTvmDuration_TenYears");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.OneHundredYears), TvmDurationUnit.OneHundredYears).SetName("GetTvmDuration_OneHundredYears");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.OneMinutes), TvmDurationUnit.OneMinutes).SetName("GetTvmDuration_OneMinutes");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThreeMinutes), TvmDurationUnit.ThreeMinutes).SetName("GetTvmDuration_ThreeMinutes");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FiveMinutes), TvmDurationUnit.FiveMinutes).SetName("GetTvmDuration_FiveMinutes");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TenMinutes), TvmDurationUnit.TenMinutes).SetName("GetTvmDuration_TenMinutes");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FifteenMinutes), TvmDurationUnit.FifteenMinutes).SetName("GetTvmDuration_FifteenMinutes");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThirtyMinutes), TvmDurationUnit.ThirtyMinutes).SetName("GetTvmDuration_ThirtyMinutes");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.OneHours), TvmDurationUnit.OneHours).SetName("GetTvmDuration_OneHours");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TwoHours), TvmDurationUnit.TwoHours).SetName("GetTvmDuration_TwoHours");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThreeHours), TvmDurationUnit.ThreeHours).SetName("GetTvmDuration_ThreeHours");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.SixHours), TvmDurationUnit.SixHours).SetName("GetTvmDuration_SixHours");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.NineHours), TvmDurationUnit.NineHours).SetName("GetTvmDuration_NineHours");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TwelveHours), TvmDurationUnit.TwelveHours).SetName("GetTvmDuration_TwelveHours");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.EighteenHours), TvmDurationUnit.EighteenHours).SetName("GetTvmDuration_EighteenHours");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.OneDays), TvmDurationUnit.OneDays).SetName("GetTvmDuration_OneDays");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TwoDays), TvmDurationUnit.TwoDays).SetName("GetTvmDuration_TwoDaysTwoDays");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThreeDays), TvmDurationUnit.ThreeDays).SetName("GetTvmDuration_ThreeDays");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FiveDays), TvmDurationUnit.FiveDays).SetName("GetTvmDuration_FiveDays");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TenDays), TvmDurationUnit.TenDays).SetName("GetTvmDuration_TenDays");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThirtyDays), TvmDurationUnit.ThirtyDays).SetName("GetTvmDuration_ThirtyDays");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThirtyOneDays), TvmDurationUnit.ThirtyOneDays).SetName("GetTvmDuration_ThirtyOneDays");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.OneWeeks), TvmDurationUnit.OneWeeks).SetName("GetTvmDuration_OneWeeks");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.TwoWeeks), TvmDurationUnit.TwoWeeks).SetName("GetTvmDuration_TwoWeeks");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.ThreeWeeks), TvmDurationUnit.ThreeWeeks).SetName("GetTvmDuration_ThreeWeeks");
            yield return new TestCaseData(new Duration((long)TvmDurationUnit.FourWeeks), TvmDurationUnit.FourWeeks).SetName("GetTvmDuration_FourWeeks");

            yield return new TestCaseData(new Duration((long)45487), 0).SetName("GetTvmDuration_NumberNotInEnum1");
            yield return new TestCaseData(new Duration((long)74), 0).SetName("GetTvmDuration_NumberNotInEnum2");
        }

       
        [TestCaseSource(nameof(CheckSlidingWindowStartTestCases))]
        public void CheckGetSlidingWindowStart(long code, DateTime resultDate)
        {
            DateTime date = Duration.GetSlidingWindowStart(code);
            Assert.IsTrue((date - resultDate).Duration() < TimeSpan.FromSeconds(20));
        }

        private static IEnumerable CheckSlidingWindowStartTestCases()
        {
            yield return new TestCaseData(1, DateTime.UtcNow.AddMinutes(-1)).SetName("GetSlidingWindowStart_OneMinutes");
            yield return new TestCaseData(3, DateTime.UtcNow.AddMinutes(-3)).SetName("GetSlidingWindowStart_ThreeMinutes");
            yield return new TestCaseData(5, DateTime.UtcNow.AddMinutes(-5)).SetName("GetSlidingWindowStart_FiveMinutes");
            yield return new TestCaseData(10, DateTime.UtcNow.AddMinutes(-10)).SetName("GetSlidingWindowStart_TenMinutes");
            yield return new TestCaseData(15, DateTime.UtcNow.AddMinutes(-15)).SetName("GetSlidingWindowStart_FifteenMinutes");
            yield return new TestCaseData(30, DateTime.UtcNow.AddMinutes(-30)).SetName("GetSlidingWindowStart_ThirtyMinutes");
            yield return new TestCaseData(60, DateTime.UtcNow.AddHours(-1)).SetName("GetSlidingWindowStart_OneHours");
            yield return new TestCaseData(120, DateTime.UtcNow.AddHours(-2)).SetName("GetSlidingWindowStart_TwoHours");
            yield return new TestCaseData(180, DateTime.UtcNow.AddHours(-3)).SetName("GetSlidingWindowStart_ThreeHours");
            yield return new TestCaseData(360, DateTime.UtcNow.AddHours(-6)).SetName("GetSlidingWindowStart_SixHours");
            yield return new TestCaseData(540, DateTime.UtcNow.AddHours(-9)).SetName("GetSlidingWindowStart_NineHours");
            yield return new TestCaseData(720, DateTime.UtcNow.AddHours(-12)).SetName("GetSlidingWindowStart_TwelveHours");
            yield return new TestCaseData(1080, DateTime.UtcNow.AddHours(-18)).SetName("GetSlidingWindowStart_EighteenHours");
            yield return new TestCaseData(1440, DateTime.UtcNow.AddDays(-1)).SetName("GetSlidingWindowStart_OneDays");
            yield return new TestCaseData(1440, DateTime.UtcNow.AddDays(-1)).SetName("GetSlidingWindowStart_OneDays");
            yield return new TestCaseData(2880, DateTime.UtcNow.AddDays(-2)).SetName("GetSlidingWindowStart_TwoDays");
            yield return new TestCaseData(4320, DateTime.UtcNow.AddDays(-3)).SetName("GetSlidingWindowStart_ThreeDays");
            yield return new TestCaseData(7200, DateTime.UtcNow.AddDays(-5)).SetName("GetSlidingWindowStart_FiveDays");
            yield return new TestCaseData(14400, DateTime.UtcNow.AddDays(-10)).SetName("GetSlidingWindowStart_TenDays");
            yield return new TestCaseData(43200, DateTime.UtcNow.AddDays(-30)).SetName("GetSlidingWindowStart_ThirtyDays");
            yield return new TestCaseData(44600, DateTime.UtcNow.AddDays(-31)).SetName("GetSlidingWindowStart_ThirtyOneDays");
            yield return new TestCaseData(10080, DateTime.UtcNow.AddDays(-7)).SetName("GetSlidingWindowStart_OneWeeks");
            yield return new TestCaseData(20160, DateTime.UtcNow.AddDays(-14)).SetName("GetSlidingWindowStart_TwoWeeks");
            yield return new TestCaseData(30240, DateTime.UtcNow.AddDays(-21)).SetName("GetSlidingWindowStart_ThreeWeeks");
            yield return new TestCaseData(40320, DateTime.UtcNow.AddDays(-28)).SetName("GetSlidingWindowStart_FourWeeks");
            yield return new TestCaseData(1111111, DateTime.UtcNow.AddMonths(-1)).SetName("GetSlidingWindowStart_OneMonth");
            yield return new TestCaseData(2222222, DateTime.UtcNow.AddMonths(-2)).SetName("GetSlidingWindowStart_TwoMonths");
            yield return new TestCaseData(3333333, DateTime.UtcNow.AddMonths(-3)).SetName("GetSlidingWindowStart_ThreeMonths");
            yield return new TestCaseData(4444444, DateTime.UtcNow.AddMonths(-4)).SetName("GetSlidingWindowStart_FourMonths");
            yield return new TestCaseData(5555555, DateTime.UtcNow.AddMonths(-5)).SetName("GetSlidingWindowStart_FiveMonths");
            yield return new TestCaseData(6666666, DateTime.UtcNow.AddMonths(-6)).SetName("GetSlidingWindowStart_SixMonths");
            yield return new TestCaseData(7777777, DateTime.UtcNow.AddMonths(-7)).SetName("GetSlidingWindowStart_SevenMonths");
            yield return new TestCaseData(8888888, DateTime.UtcNow.AddMonths(-8)).SetName("GetSlidingWindowStart_EightMonths");
            yield return new TestCaseData(9999999, DateTime.UtcNow.AddMonths(-9)).SetName("GetSlidingWindowStart_NineMonths");
            yield return new TestCaseData(10000000, DateTime.UtcNow.AddMonths(-10)).SetName("GetSlidingWindowStart_TenMonths");
            yield return new TestCaseData(11000000, DateTime.UtcNow.AddMonths(-11)).SetName("GetSlidingWindowStart_ElevenMonths");
            yield return new TestCaseData(11111111, DateTime.UtcNow.AddYears(-1)).SetName("GetSlidingWindowStart_OneYear");
            yield return new TestCaseData(22222222, DateTime.UtcNow.AddYears(-2)).SetName("GetSlidingWindowStart_TwoYears");
            yield return new TestCaseData(33333333, DateTime.UtcNow.AddYears(-3)).SetName("GetSlidingWindowStart_ThreeYears");
            yield return new TestCaseData(44444444, DateTime.UtcNow.AddYears(-4)).SetName("GetSlidingWindowStart_FourYears");
            yield return new TestCaseData(55555555, DateTime.UtcNow.AddYears(-5)).SetName("GetSlidingWindowStart_FiveYears");
            yield return new TestCaseData(100000000, DateTime.UtcNow.AddYears(-10)).SetName("GetSlidingWindowStart_TenYears");
            yield return new TestCaseData(999999999, DateTime.UtcNow.AddYears(-100)).SetName("GetSlidingWindowStart_OneHundredYears");
            yield return new TestCaseData(123, DateTime.UtcNow.AddMinutes(-123)).SetName("GetSlidingWindowStart_TvmDurationUnitNotExists");
        }

        [TestCaseSource(nameof(CheckGetDaysFromDurationTestCases))]
        public void CheckGetDaysFromDuration(int code, int days)
        {
            Assert.That(Duration.GetDaysFromDuration(code), Is.EqualTo(days));
        }

        private static IEnumerable CheckGetDaysFromDurationTestCases()
        {
            yield return new TestCaseData(12340, 8).SetName("GetDaysFromDurationCodeNotExists");
            yield return new TestCaseData(2880, 2).SetName("GetDaysFromDuration_TwoDays");
            // yield return new TestCaseData(100000000, 3652).SetName("GetDaysFromDuration_TenYears");
            // yield return new TestCaseData(999999999, 36525).SetName("GetDaysFromDuration_OneHundredYears");
        }
    }
}
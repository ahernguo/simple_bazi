using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public class DailyFortuneServiceTests {
        private static readonly DateTime BirthDate = new(1990, 1, 1, 12, 0, 0);

        [Fact]
        public void LiuRiFromDate_SixtyDaysApart_ReturnsSameGanZhi() {
            var first = LiuRi.FromDate(new DateTime(2026, 1, 1));
            var second = LiuRi.FromDate(new DateTime(2026, 3, 2));

            Assert.Equal(first.Gan, second.Gan);
            Assert.Equal(first.Zhi, second.Zhi);
        }

        [Fact]
        public void AnalyzeMonth_ValidMonth_ReturnsEveryCalendarDay() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();

            var results = service.AnalyzeMonth(info, 2026, 2);

            Assert.Equal(28, results.Count);
            Assert.Equal(new DateTime(2026, 2, 1), results[0].Day.Date);
            Assert.Equal(new DateTime(2026, 2, 28), results[^1].Day.Date);
            Assert.All(results, result => Assert.NotEqual(default, result.Context.MonthGan));
        }

        [Fact]
        public void AnalyzeDate_MaleWealthSignal_AlsoCreatesRomanceSignal() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();
            var result = FindResult(service, info, signal => signal.Topic == DailyFortuneTopic.Wealth);

            Assert.Contains(result.Signals, signal => signal.Topic == DailyFortuneTopic.Romance);
        }

        [Fact]
        public void AnalyzeDate_MovingWithSameYearBranch_DoesNotTreatSameBranchAsClash() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();
            var date = new DateTime(2026, 1, 1);
            var day = LiuRi.FromDate(date);

            var result = service.AnalyzeDate(info, date, [day.Zhi]);
            var moving = Assert.Single(result.Signals, signal => signal.Topic == DailyFortuneTopic.Moving);

            Assert.Equal(DailySignalLevel.VerificationRequired, moving.Level);
        }

        [Fact]
        public void AnalyzeDate_MovingClashesWithHouseholdYearBranch_ReturnsAttention() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();
            var date = new DateTime(2026, 1, 1);
            var day = LiuRi.FromDate(date);
            var clashingBranch = GetClashingBranch(day.Zhi);

            var result = service.AnalyzeDate(info, date, [clashingBranch]);
            var moving = Assert.Single(result.Signals, signal => signal.Topic == DailyFortuneTopic.Moving);

            Assert.Equal(DailySignalLevel.Attention, moving.Level);
            Assert.Contains("同住者年支", moving.Summary);
        }

        [Fact]
        public void AnalyzeDate_UserFacingSignals_DoNotContainCourseAttribution() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();
            var signals = Enumerable.Range(0, 60)
                .SelectMany(offset => service.AnalyzeDate(info, new DateTime(2026, 1, 1).AddDays(offset)).Signals)
                .ToArray();

            Assert.NotEmpty(signals);
            Assert.All(signals, signal => {
                Assert.DoesNotContain("課程", signal.Title);
                Assert.DoesNotContain("課程", signal.Summary);
                Assert.DoesNotContain("課程", signal.Advice);
            });
        }

        [Theory]
        [InlineData(1899, 1)]
        [InlineData(2101, 1)]
        [InlineData(2026, 0)]
        [InlineData(2026, 13)]
        public void AnalyzeMonth_InvalidRange_Throws(int year, int month) {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();

            Assert.Throws<ArgumentOutOfRangeException>(() => service.AnalyzeMonth(info, year, month));
        }

        private static DailyFortuneResult FindResult(
            DailyFortuneService service,
            BaZiInfo info,
            Predicate<DailyFortuneSignal> predicate
        ) {
            for (var offset = 0; offset < 60; offset++) {
                var result = service.AnalyzeDate(info, new DateTime(2026, 1, 1).AddDays(offset));
                if (result.Signals.Any(signal => predicate(signal))) {
                    return result;
                }
            }

            throw new Xunit.Sdk.XunitException("六十甲子循環內找不到預期訊號。");
        }

        private static DiZhi GetClashingBranch(DiZhi branch) {
            return branch switch {
                DiZhi.Zi => DiZhi.Wu,
                DiZhi.Wu => DiZhi.Zi,
                DiZhi.Chou => DiZhi.Wei,
                DiZhi.Wei => DiZhi.Chou,
                DiZhi.Yin => DiZhi.Shen,
                DiZhi.Shen => DiZhi.Yin,
                DiZhi.Mao => DiZhi.You,
                DiZhi.You => DiZhi.Mao,
                DiZhi.Chen => DiZhi.Xu,
                DiZhi.Xu => DiZhi.Chen,
                DiZhi.Si => DiZhi.Hai,
                DiZhi.Hai => DiZhi.Si,
                _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
            };
        }
    }
}

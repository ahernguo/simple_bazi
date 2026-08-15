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
        public void AnalyzeDate_WealthSignal_IncludesCapacityAndActualTenGodFactors() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();
            var result = FindResult(service, info, signal => signal.Topic == DailyFortuneTopic.Wealth);
            var wealth = Assert.Single(result.Signals, signal => signal.Topic == DailyFortuneTopic.Wealth);

            Assert.Contains(GetStrengthText(info.StrengthStatus), wealth.Summary);
            Assert.True(wealth.Summary.Contains("大運") || wealth.Summary.Contains("查無"));
            Assert.NotNull(wealth.TenGodFactors);
            Assert.NotEmpty(wealth.TenGodFactors);
            Assert.All(wealth.TenGodFactors, factor =>
                Assert.Contains(factor.TenGod, new[] { ShiShen.Cai, ShiShen.ShihShang }));
        }

        [Fact]
        public void AnalyzeDate_WeakChartInUnsupportiveDaYun_WealthIsAttention() {
            var info = new BaZiInfo(new DateTime(1970, 1, 15, 12, 0, 0), 2);
            const int year = 1973;
            var service = new DailyFortuneService();
            var result = FindResult(
                service,
                info,
                signal => signal.Topic == DailyFortuneTopic.Wealth,
                new DateTime(year, 1, 1)
            );
            var wealth = Assert.Single(result.Signals, signal => signal.Topic == DailyFortuneTopic.Wealth);

            Assert.Equal(GeJu.ShenRuo, info.StrengthStatus);
            Assert.Equal(DailySignalLevel.Attention, wealth.Level);
            Assert.Contains("身弱", wealth.Summary);
            Assert.Contains("大運", wealth.Summary);
            Assert.Contains("支出", wealth.Advice);
            Assert.All(wealth.TenGodFactors!, factor => Assert.False(factor.IsFavorable));
        }

        [Fact]
        public void AnalyzeDate_WeakChartInSupportiveDaYun_WealthIsOpportunity() {
            var info = new BaZiInfo(new DateTime(1970, 1, 15, 12, 0, 0), 2);
            var year = FindSupportiveDaYunYear(info);
            var service = new DailyFortuneService();
            var result = FindResult(
                service,
                info,
                signal => signal.Topic == DailyFortuneTopic.Wealth,
                new DateTime(year, 1, 1)
            );
            var wealth = Assert.Single(result.Signals, signal => signal.Topic == DailyFortuneTopic.Wealth);

            Assert.Equal(GeJu.ShenRuo, info.StrengthStatus);
            Assert.Equal(DailySignalLevel.Opportunity, wealth.Level);
            Assert.Contains("大運主作用在喜用方向", wealth.Summary);
            Assert.DoesNotContain("支出", wealth.Advice);
            Assert.All(wealth.TenGodFactors!, factor => Assert.True(factor.IsFavorable));
        }

        [Fact]
        public void AnalyzeDate_SuspectedCongChart_WealthRequiresVerification() {
            var info = new BaZiInfo(new DateTime(1970, 2, 15, 12, 0, 0), 2);
            var service = new DailyFortuneService();
            var result = FindResult(service, info, signal => signal.Topic == DailyFortuneTopic.Wealth);
            var wealth = Assert.Single(result.Signals, signal => signal.Topic == DailyFortuneTopic.Wealth);

            Assert.True(info.RequiresStrengthVerification);
            Assert.Equal(DailySignalLevel.VerificationRequired, wealth.Level);
            Assert.Contains("疑似從強格", wealth.Summary);
            Assert.Contains("回驗", wealth.Summary);
        }

        [Fact]
        public void AnalyzeDate_CareerAndRomanceSignals_IncludeCapacityAssessment() {
            var info = new BaZiInfo(BirthDate, 1);
            var service = new DailyFortuneService();
            var careerResult = FindResult(service, info, signal => signal.Topic == DailyFortuneTopic.Career);
            var romanceResult = FindResult(service, info, signal => signal.Topic == DailyFortuneTopic.Romance);
            var career = Assert.Single(careerResult.Signals, signal => signal.Topic == DailyFortuneTopic.Career);
            var romance = Assert.Single(romanceResult.Signals, signal => signal.Topic == DailyFortuneTopic.Romance);

            Assert.Contains(GetStrengthText(info.StrengthStatus), career.Summary);
            Assert.Contains(GetStrengthText(info.StrengthStatus), romance.Summary);
            Assert.NotNull(career.TenGodFactors);
            Assert.NotNull(romance.TenGodFactors);
        }

        [Fact]
        public void AnalyzeDate_HealthAndInterpersonalSignals_IncludePeriodOrFavorabilityContext() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();
            var healthResult = FindResult(service, info, signal => signal.Topic == DailyFortuneTopic.Health);
            var interpersonalResult = FindResult(service, info, signal => signal.Topic == DailyFortuneTopic.Interpersonal);
            var health = Assert.Single(healthResult.Signals, signal => signal.Topic == DailyFortuneTopic.Health);
            var interpersonal = Assert.Single(
                interpersonalResult.Signals,
                signal => signal.Topic == DailyFortuneTopic.Interpersonal
            );

            Assert.True(health.Summary.Contains("大運") || health.Summary.Contains("查無"));
            Assert.True(
                interpersonal.Summary.Contains("長期喜忌")
                || interpersonal.Summary.Contains("本命喜用")
            );
            Assert.NotNull(interpersonal.TenGodFactors);
            Assert.All(interpersonal.TenGodFactors, factor => Assert.Equal(ShiShen.BiJie, factor.TenGod));
        }

        [Fact]
        public void AnalyzeMonthDetails_TravelBackgroundPeriodsCoverEveryDay() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();

            var analysis = service.AnalyzeMonthDetails(info, 2026, 2);

            Assert.NotEmpty(analysis.TravelSafetyBackgrounds);
            Assert.Equal(new DateTime(2026, 2, 1), analysis.TravelSafetyBackgrounds[0].StartDate);
            Assert.Equal(new DateTime(2026, 2, 28), analysis.TravelSafetyBackgrounds[^1].EndDate);
            for (var index = 1; index < analysis.TravelSafetyBackgrounds.Count; index++) {
                Assert.Equal(
                    analysis.TravelSafetyBackgrounds[index - 1].EndDate.AddDays(1),
                    analysis.TravelSafetyBackgrounds[index].StartDate
                );
            }
            Assert.All(
                analysis.TravelSafetyBackgrounds,
                background => Assert.DoesNotContain(background.Sources, source => source.Label.Contains("流日"))
            );
        }

        [Fact]
        public void AnalyzeMonth_TravelSafetySignalsRequireDailyTriggerAndIncludeFavorabilityFactor() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();
            var signals = service.AnalyzeMonth(info, 2026, 2)
                .SelectMany(result => result.Signals)
                .Where(signal => signal.Topic == DailyFortuneTopic.TravelSafety)
                .ToArray();

            Assert.NotEmpty(signals);
            Assert.All(signals, signal => {
                Assert.Contains("流日地支", signal.Summary);
                Assert.True(signal.Summary.Contains("相沖") || signal.Summary.Contains("補齊"));
                var factor = Assert.Single(signal.TenGodFactors!);
                Assert.Equal("流日地支", factor.Source);
            });
        }

        [Fact]
        public void AnalyzeMonth_TravelBackgroundAlone_DoesNotFlagEveryDay() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new DailyFortuneService();

            for (var year = 2024; year <= 2032; year++) {
                for (var month = 1; month <= 12; month++) {
                    var analysis = service.AnalyzeMonthDetails(info, year, month);
                    DailyTravelSafetyPeriodBackground? background = analysis.TravelSafetyBackgrounds
                        .FirstOrDefault(candidate =>
                            (candidate.EndDate - candidate.StartDate).Days >= 11
                            && (candidate.TravelBranchCount >= 3 || candidate.Punishments.Count > 0));
                    if (background is null) {
                        continue;
                    }

                    var periodResults = analysis.Results
                        .Where(result => result.Day.Date >= background.StartDate && result.Day.Date <= background.EndDate)
                        .ToArray();
                    Assert.Contains(
                        periodResults,
                        result => result.Signals.All(signal => signal.Topic != DailyFortuneTopic.TravelSafety)
                    );
                    return;
                }
            }

            throw new Xunit.Sdk.XunitException("找不到可驗證固定交通背景的月份區段。");
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
            Assert.Equal("搬家命盤初篩未通過", moving.Title);
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
            Predicate<DailyFortuneSignal> predicate,
            DateTime? startDate = null
        ) {
            for (var offset = 0; offset < 60; offset++) {
                var result = service.AnalyzeDate(info, (startDate ?? new DateTime(2026, 1, 1)).AddDays(offset));
                if (result.Signals.Any(signal => predicate(signal))) {
                    return result;
                }
            }

            throw new Xunit.Sdk.XunitException("六十甲子循環內找不到預期訊號。");
        }

        private static string GetStrengthText(GeJu strengthStatus) {
            return strengthStatus switch {
                GeJu.ShenQiang => "身強",
                GeJu.ShenRuo => "身弱",
                GeJu.CongQiang => "從強格",
                GeJu.CongRuo => "從弱格",
                _ => throw new ArgumentOutOfRangeException(nameof(strengthStatus), strengthStatus, null)
            };
        }

        private static int FindSupportiveDaYunYear(BaZiInfo info) {
            foreach (DaYun daYun in info.DaYunList) {
                for (var year = daYun.StartYear; year <= daYun.EndYear; year++) {
                    var primaryTenGod = daYun.GetPrimaryTenGod(info.DayZhu.Gan, year);
                    var element = TenGodElementResolver.Resolve(info.RiZhu, primaryTenGod);
                    if (info.LikeWuXing.Contains(element)) {
                        return year;
                    }
                }
            }

            throw new Xunit.Sdk.XunitException("測試命盤找不到喜用大運年份。");
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

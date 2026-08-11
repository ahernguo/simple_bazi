using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public class FortuneServiceTests {
        private static readonly DateTime BirthDate = new(1990, 1, 1, 12, 0, 0);
        private static readonly DateTime OverlappingXingPoBirthDate = new(1992, 2, 17, 12, 0, 0);

        private static string GetInteractionHtml(string firstId, string firstZhi, string secondId, string secondZhi, string relation) {
            return $"{firstId} <span class=\"border-bottom-dash\">{firstZhi}</span> 與{secondId} <span class=\"border-bottom-dash\">{secondZhi}</span> 形成「<strong class=\"text-danger\">{relation}</strong>」";
        }

        private static int GetLiuNianYear(BaZiInfo info, DiZhi branch) {
            return info.DaYunList
                .SelectMany(daYun => daYun.LiuNianList)
                .First(liuNian => liuNian.Zhi == branch)
                .Year;
        }

        [Fact]
        public void GetLiuYueMonths_ExistingYear_ReturnsTwelveOrderedMonths() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var months = service.GetLiuYueMonths(info, 2026);

            Assert.Equal(12, months.Count);
            Assert.Equal(Enumerable.Range(0, 12), months.Select(month => month.Index));
            Assert.Equal("正", months[0].MonthInChinese);
            Assert.Equal(TianGan.Geng, months[0].Gan);
            Assert.Equal(DiZhi.Yin, months[0].Zhi);
            Assert.Equal("臘", months[11].MonthInChinese);
            Assert.Equal(TianGan.Xin, months[11].Gan);
            Assert.Equal(DiZhi.Chou, months[11].Zhi);
        }

        [Fact]
        public void GetLiuYueMonths_MissingYear_ReturnsEmptyList() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var months = service.GetLiuYueMonths(info, 1800);

            Assert.Empty(months);
        }

        [Fact]
        public void GetLiuYueStartInfo_FirstMonth_ReturnsLiChunTime() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var startInfo = service.GetLiuYueStartInfo(info, 2026, 0);

            Assert.NotNull(startInfo);
            LiuYueStartInfo actual = startInfo!;
            Assert.Equal("立春", actual.JieQiName);
            Assert.Equal(new DateTime(2026, 2, 4, 4, 2, 8), actual.StartDate);
            Assert.Equal("驚蟄", actual.EndJieQiName);
            Assert.Equal(new DateTime(2026, 3, 5, 21, 59, 0), actual.EndDate);
            Assert.Equal(TianGan.Geng, actual.Month.Gan);
            Assert.Equal(DiZhi.Yin, actual.Month.Zhi);
        }

        [Fact]
        public void GetLiuYueStartInfo_LastMonth_ReturnsNextYearXiaoHanTime() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var startInfo = service.GetLiuYueStartInfo(info, 2026, 11);

            Assert.NotNull(startInfo);
            LiuYueStartInfo actual = startInfo!;
            Assert.Equal("小寒", actual.JieQiName);
            Assert.Equal(new DateTime(2027, 1, 5, 22, 9, 58), actual.StartDate);
            Assert.Equal("立春", actual.EndJieQiName);
            Assert.Equal(2027, actual.EndDate.Year);
            Assert.Equal(2, actual.EndDate.Month);
            Assert.True(actual.EndDate > actual.StartDate);
            Assert.Equal(TianGan.Xin, actual.Month.Gan);
            Assert.Equal(DiZhi.Chou, actual.Month.Zhi);
        }

        [Fact]
        public void GetLiuYueStartInfo_SixthMonth_ReturnsYiWeiDateRange() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var startInfo = service.GetLiuYueStartInfo(info, 2026, 5);

            Assert.NotNull(startInfo);
            LiuYueStartInfo actual = startInfo!;
            Assert.Equal(new DateTime(2026, 7, 7, 9, 56, 57), actual.StartDate);
            Assert.Equal(new DateTime(2026, 8, 7, 19, 42, 43), actual.EndDate);
            Assert.Equal(TianGan.Yi, actual.Month.Gan);
            Assert.Equal(DiZhi.Wei, actual.Month.Zhi);
        }

        [Fact]
        public void GetLiuYueStartInfo_AllMonths_ReturnsOrderedJieQi() {
            string[] expectedNames = ["立春", "驚蟄", "清明", "立夏", "芒種", "小暑", "立秋", "白露", "寒露", "立冬", "大雪", "小寒"];
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            DateTime? previousEndDate = null;

            for (var index = 0; index < expectedNames.Length; index++) {
                var startInfo = service.GetLiuYueStartInfo(info, 2026, index);

                Assert.NotNull(startInfo);
                LiuYueStartInfo actual = startInfo!;
                Assert.Equal(expectedNames[index], actual.JieQiName);
                if (previousEndDate is not null) {
                    Assert.Equal(previousEndDate.Value, actual.StartDate);
                }
                Assert.True(actual.EndDate > actual.StartDate);
                previousEndDate = actual.EndDate;
            }
        }

        [Fact]
        public void GetLiuYueStartInfo_InvalidMonth_ReturnsNull() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var startInfo = service.GetLiuYueStartInfo(info, 2026, 12);

            Assert.Null(startInfo);
        }

        [Fact]
        public void GetTaiSuiAnalysis_SameYearBranch_ReturnsDirectValueTaiSui() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            int targetYear = GetLiuNianYear(info, info.YearZhu.Zhi);

            var analysis = service.GetTaiSuiAnalysis(info, targetYear);

            Assert.NotNull(analysis);
            TaiSuiAnalysisResult actual = analysis!;
            Assert.Equal("蛇", actual.NatalZodiac);
            Assert.Equal(actual.NatalZodiac, actual.AnnualZodiac);
            Assert.Contains(TaiSuiInteractionType.SameBranch, actual.DirectInteractions);
            Assert.True(actual.HasDirectTaiSui);
            Assert.False(actual.IsZodiacClash);
        }

        [Fact]
        public void GetTaiSuiAnalysis_ClashingYearBranch_ReturnsZodiacClash() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            IList<DiZhi> clashPair = BaZiDefine.Chong.Single(pair => pair.Contains(info.YearZhu.Zhi));
            DiZhi clashBranch = clashPair.Single(branch => branch != info.YearZhu.Zhi);
            int targetYear = GetLiuNianYear(info, clashBranch);

            var analysis = service.GetTaiSuiAnalysis(info, targetYear);

            Assert.NotNull(analysis);
            TaiSuiAnalysisResult actual = analysis!;
            Assert.Equal(clashBranch, actual.AnnualBranch);
            Assert.Contains(TaiSuiInteractionType.SixClash, actual.DirectInteractions);
            Assert.True(actual.IsZodiacClash);
        }

        [Fact]
        public void GetTaiSuiAnalysis_OverlappingRelations_PreservesEveryTaiSuiType() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            Assert.Equal(DiZhi.Si, info.YearZhu.Zhi);
            int yinYear = GetLiuNianYear(info, DiZhi.Yin);
            int shenYear = GetLiuNianYear(info, DiZhi.Shen);

            var yinAnalysis = service.GetTaiSuiAnalysis(info, yinYear);
            var shenAnalysis = service.GetTaiSuiAnalysis(info, shenYear);

            Assert.NotNull(yinAnalysis);
            Assert.Contains(TaiSuiInteractionType.Punishment, yinAnalysis!.DirectInteractions);
            Assert.Contains(TaiSuiInteractionType.SixHarm, yinAnalysis.DirectInteractions);
            Assert.NotNull(shenAnalysis);
            Assert.Contains(TaiSuiInteractionType.Punishment, shenAnalysis!.DirectInteractions);
            Assert.Contains(TaiSuiInteractionType.SixBreak, shenAnalysis.DirectInteractions);
        }

        [Fact]
        public void GetTaiSuiAnalysis_IndirectSameBranch_ListsNatalPillar() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            int targetYear = GetLiuNianYear(info, info.MonthZhu.Zhi);

            var analysis = service.GetTaiSuiAnalysis(info, targetYear);

            Assert.NotNull(analysis);
            TaiSuiPillarInteraction monthInteraction = Assert.Single(
                analysis!.IndirectInteractions.Where(interaction => interaction.PillarName == "月柱")
            );
            Assert.Equal(info.MonthZhu.Zhi, monthInteraction.NatalBranch);
            Assert.Contains(TaiSuiInteractionType.SameBranch, monthInteraction.Interactions);
        }

        [Fact]
        public void GetTaiSuiAnalysis_UncertainBirthTime_ExcludesHourPillar() {
            var info = new BaZiInfo(BirthDate, 2, false);
            var service = new FortuneService();
            int targetYear = GetLiuNianYear(info, info.HourZhu.Zhi);

            var analysis = service.GetTaiSuiAnalysis(info, targetYear);

            Assert.NotNull(analysis);
            Assert.False(analysis!.IsHourPillarIncluded);
            Assert.DoesNotContain(
                analysis.IndirectInteractions,
                interaction => interaction.PillarName == "時柱"
            );
        }

        [Fact]
        public void GetTaiSuiAnalysis_MissingYear_ReturnsNull() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var analysis = service.GetTaiSuiAnalysis(info, 1800);

            Assert.Null(analysis);
        }

        [Fact]
        public void GetTaiSuiAnalysis_AnnualBranchRepeatsNatalInteraction_ReturnsReinforcement() {
            var info = new BaZiInfo(OverlappingXingPoBirthDate, 2);
            var service = new FortuneService();
            var analysis = service.GetLiuNianYears(info)
                .Select(year => service.GetTaiSuiAnalysis(info, year))
                .FirstOrDefault(result => result?.HasReinforcedInteraction == true);

            Assert.NotNull(analysis);
            Assert.NotEmpty(analysis!.ReinforcedInteractions!);
            Assert.All(analysis.ReinforcedInteractions!, reinforcement =>
                Assert.NotEmpty(reinforcement.Interactions));
        }

        [Fact]
        public void LiuNianTopicAnalysis_AnyYear_AddsChildTimingCard() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            var targetYear = service.GetLiuNianYears(info).First();

            var html = service.LiuNianTopicAnalysis(info, targetYear).ToString();

            Assert.Contains("子女緣分", html);
            Assert.Contains("子息星時機", html);
        }

        [Fact]
        public void LiuYueTopicAnalysis_DoesNotExtrapolateChildTimingRuleToMonth() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var html = service.LiuYueTopicAnalysis(info, 2026, 0).ToString();

            Assert.DoesNotContain("子女緣分", html);
        }

        [Fact]
        public void SelfXing_UsesHaiInsteadOfXu() {
            Assert.Contains(DiZhi.Hai, BaZiDefine.SelfXing);
            Assert.DoesNotContain(DiZhi.Xu, BaZiDefine.SelfXing);
        }

        [Fact]
        public void LiuYueAnalysis_ExistingMonth_UsesLiuYueWording() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var html = service.LiuYueAnalysis(info, 2026, 0).Value;

            Assert.Contains("2026 正月流月分析", html);
            Assert.Contains("庚", html);
            Assert.Contains("寅", html);
            Assert.Contains("節氣月", html);
            Assert.DoesNotContain("今年流年運勢", html);
            Assert.DoesNotContain("這一年", html);
        }

        [Fact]
        public void LiuYueAnalysis_LiuNianAndLiuYueChong_ListsActualParticipants() {
            var info = new BaZiInfo(new DateTime(1980, 1, 15, 10, 0, 0), 2);
            var service = new FortuneService();

            var html = service.LiuYueAnalysis(info, 2026, 10).Value;

            Assert.Contains("流年 <span class=\"border-bottom-dash\">午</span> 與流月 <span class=\"border-bottom-dash\">子</span> 形成", html);
            Assert.Contains("相沖", html);
        }

        [Fact]
        public void LiuNianAnalysis_ExistingYear_PreservesLiuNianOutput() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var html = service.LiuNianAnalysis(info, 2026).Value;

            Assert.Contains("2026 流年分析", html);
            Assert.Contains("丙", html);
            Assert.Contains("午", html);
            Assert.Contains("topic-ten-god ", html);
            Assert.Contains("title=\"", html);
            Assert.Contains("依本命格局列為", html);
            Assert.True(
                html.Contains("topic-ten-god-favorable", StringComparison.Ordinal)
                || html.Contains("topic-ten-god-unfavorable", StringComparison.Ordinal)
            );
        }

        [Fact]
        public void DaYunAnalysis_SelectedYear_UsesCorrespondingDaYunAndFormatsTenGodFavorability() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            var selectedDaYun = info.DaYunList.First(daYun => daYun != info.CurrentDaYun);
            int targetYear = selectedDaYun.LiuNianList[0].Year;

            var html = service.DaYunAnalysis(info, targetYear).Value;

            Assert.Contains("大運分析", html);
            Assert.Contains($"{selectedDaYun.StartYear} 年～{selectedDaYun.EndYear} 年", html);
            Assert.Contains("前五年", html);
            Assert.Contains("以天干為主", html);
            Assert.Contains("topic-ten-god ", html);
            Assert.Contains("title=\"", html);
            Assert.Contains("依本命格局列為", html);
            Assert.True(
                html.Contains("topic-ten-god-favorable", StringComparison.Ordinal)
                || html.Contains("topic-ten-god-unfavorable", StringComparison.Ordinal)
            );
        }

        [Fact]
        public void DaYunAnalysis_LastFiveYears_UsesBranchAsPrimaryInfluence() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();
            var selectedDaYun = info.DaYunList.First();
            int targetYear = selectedDaYun.StartYear + 5;

            var html = service.DaYunAnalysis(info, targetYear).Value;

            Assert.Contains("後五年", html);
            Assert.Contains("以地支為主", html);
            Assert.Equal(DaYunPhase.LastFiveYears, selectedDaYun.GetPhase(targetYear));
            Assert.Equal(DiZhi.Hai, selectedDaYun.Zhi);
            Assert.Equal(ShiShen.QiSha, selectedDaYun.GetPrimaryTenGod(info.DayZhu.Gan, targetYear));
            Assert.Contains("topic-notice mb-0", html);
        }

        [Fact]
        public void DaYunAnalysis_MissingYear_ReturnsEmptyMarkup() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var html = service.DaYunAnalysis(info, 1800).Value;

            Assert.Null(html);
        }

        [Fact]
        public void GetDaYunAndLiuNianYinAnalysis_ExistingYear_ReturnsOrderedPeriods() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var results = service.GetDaYunAndLiuNianYinAnalysis(info, 2026);

            Assert.Equal(2, results.Count);
            Assert.Equal("大運", results[0].PeriodName);
            Assert.Equal("流年", results[1].PeriodName);
        }

        [Fact]
        public void GetLiuYueYinAnalysis_ExistingMonth_ReturnsLiuYuePeriod() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var results = service.GetLiuYueYinAnalysis(info, 2026, 0);

            var result = Assert.Single(results);
            Assert.Equal("流月", result.PeriodName);
        }

        [Fact]
        public void LiuNianAnalysis_UncertainBirthTime_SkipsHourPillarInteractions() {
            DateTime birthDate = new(1990, 1, 1, 0, 0, 0);
            var accurateInfo = new BaZiInfo(birthDate, 2);
            var uncertainInfo = new BaZiInfo(birthDate, 2, false);
            var service = new FortuneService();
            int targetYear = service.GetLiuNianYears(accurateInfo)
                .First(year => service.LiuNianAnalysis(accurateInfo, year).Value?.Contains("時柱 <span", StringComparison.Ordinal) == true);

            var accurateHtml = service.LiuNianAnalysis(accurateInfo, targetYear).Value;
            var uncertainHtml = service.LiuNianAnalysis(uncertainInfo, targetYear).Value;

            Assert.Contains("時柱 <span", accurateHtml);
            Assert.DoesNotContain("時柱 <span", uncertainHtml);
        }

        [Fact]
        public void DaYunAnalysis_SamePairIsXingAndPo_OnlyListsXing() {
            var info = new BaZiInfo(OverlappingXingPoBirthDate, 2);
            var service = new FortuneService();
            var daYun = Assert.Single(info.DaYunList.Where(item => item.Zhi == DiZhi.Si));

            var html = service.DaYunAnalysis(info, daYun.StartYear).Value;

            Assert.Contains(GetInteractionHtml("年柱", "申", "大運", "巳", "相刑"), html);
            Assert.DoesNotContain(GetInteractionHtml("年柱", "申", "大運", "巳", "破"), html);
            Assert.Contains(GetInteractionHtml("日柱", "亥", "大運", "巳", "相沖"), html);
            Assert.Contains(GetInteractionHtml("月柱", "寅", "大運", "巳", "害"), html);
        }

        [Fact]
        public void LiuNianAnalysis_SamePairIsXingAndChong_OnlyListsXingForThatPair() {
            var info = new BaZiInfo(OverlappingXingPoBirthDate, 2);
            var service = new FortuneService();
            var daYun = Assert.Single(info.DaYunList.Where(item => item.Zhi == DiZhi.Si));
            var liuNian = Assert.Single(daYun.LiuNianList.Where(item => item.Zhi == DiZhi.Yin));

            var html = service.LiuNianAnalysis(info, liuNian.Year).Value;

            Assert.Contains(GetInteractionHtml("年柱", "申", "流年", "寅", "相刑"), html);
            Assert.DoesNotContain(GetInteractionHtml("年柱", "申", "流年", "寅", "相沖"), html);
            Assert.Contains(GetInteractionHtml("大運", "巳", "流年", "寅", "害"), html);
        }

        [Fact]
        public void LiuYueAnalysis_SamePairIsXingAndPo_DoesNotListPo() {
            var info = new BaZiInfo(OverlappingXingPoBirthDate, 2);
            var service = new FortuneService();
            var daYun = Assert.Single(info.DaYunList.Where(item => item.Zhi == DiZhi.Si));
            var liuNian = daYun.LiuNianList.First(item => item.Zhi != DiZhi.Shen);
            var liuYue = Assert.Single(liuNian.LiuYueList.Where(item => item.Zhi == DiZhi.Shen));

            var html = service.LiuYueAnalysis(info, liuNian.Year, liuYue.Index).Value;

            Assert.Contains("三刑", html);
            Assert.DoesNotContain(GetInteractionHtml("月柱", "寅", "流月", "申", "相沖"), html);
            Assert.DoesNotContain(GetInteractionHtml("大運", "巳", "流月", "申", "破"), html);
            Assert.Contains(GetInteractionHtml("日柱", "亥", "流月", "申", "害"), html);
        }

        [Fact]
        public void LiuNianTopicAnalysis_ExistingYear_UsesNeutralTenGodReferencesInFourTopicCards() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var html = service.LiuNianTopicAnalysis(info, 2026).Value;

            Assert.Contains("element-fire fw-semibold", html);
            Assert.Contains("topic-ten-god-reference", html);
            Assert.DoesNotContain("topic-ten-god ", html);
            Assert.DoesNotContain("topic-ten-god-favorable", html);
            Assert.DoesNotContain("topic-ten-god-unfavorable", html);
            Assert.DoesNotContain("title=\"", html);
            Assert.Equal(4, html.Split("class=\"topic-notice mb-0\"").Length - 1);
            Assert.Equal(4, html.Split("fa-solid fa-triangle-exclamation topic-notice-icon").Length - 1);
            Assert.DoesNotContain("alert alert-secondary mb-0", html);
            Assert.DoesNotContain("alert alert-danger mb-0", html);
            Assert.Contains("財富與事業", html);
            Assert.Contains("感情姻緣", html);
            Assert.Contains("子女緣分", html);
            Assert.Contains("健康注意事項", html);
            Assert.Contains("本年訊號", html);
            Assert.Contains("醫療優先", html);
        }

        [Fact]
        public void LiuYueTopicAnalysis_ExistingMonth_UsesMonthlyWordingWithoutAnnualCrisisRule() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var html = service.LiuYueTopicAnalysis(info, 2026, 0).Value;

            Assert.Contains("本月訊號", html);
            Assert.Contains("本月桃花時機", html);
            Assert.Contains("本月健康氣象", html);
            Assert.Contains("topic-ten-god-reference", html);
            Assert.DoesNotContain("topic-ten-god-favorable", html);
            Assert.DoesNotContain("topic-ten-god-unfavorable", html);
            Assert.DoesNotContain("title=\"", html);
            Assert.DoesNotContain("關係查證提醒", html);
        }

        [Fact]
        public void LiuNianTopicAnalysis_MissingYear_ReturnsEmptyMarkup() {
            var info = new BaZiInfo(BirthDate, 2);
            var service = new FortuneService();

            var html = service.LiuNianTopicAnalysis(info, 1800).Value;

            Assert.Null(html);
        }
    }
}

using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class FuYinAnalysisServiceTests {
        private readonly FuYinAnalysisService _service = new();

        [Fact]
        public void AnalyzeNatalPillars_CompleteGanZhiDuplicate_ReturnsFuYin() {
            IReadOnlyList<Zhu> pillars = [
                CreatePillar("年柱", "甲", "子"),
                CreatePillar("月柱", "丙", "寅"),
                CreatePillar("日柱", "戊", "辰"),
                CreatePillar("時柱", "甲", "子")
            ];

            var result = _service.AnalyzeNatalPillars(pillars, true);

            var match = Assert.Single(result.Matches);
            Assert.True(result.HasFuYin);
            Assert.Equal(TianGan.Jia, match.Gan);
            Assert.Equal(DiZhi.Zi, match.Zhi);
            Assert.Equal(["年柱", "時柱"], match.PillarNames);
        }

        [Fact]
        public void AnalyzeNatalPillars_OnlyGanOrZhiDuplicate_DoesNotReturnFuYin() {
            IReadOnlyList<Zhu> pillars = [
                CreatePillar("年柱", "甲", "子"),
                CreatePillar("月柱", "甲", "寅"),
                CreatePillar("日柱", "戊", "子"),
                CreatePillar("時柱", "庚", "午")
            ];

            var result = _service.AnalyzeNatalPillars(pillars, true);

            Assert.False(result.HasFuYin);
            Assert.Empty(result.Matches);
        }

        [Fact]
        public void Analyze_InaccurateBirthTime_ExcludesHourPillar() {
            var info = new BaZiInfo(new DateTime(1990, 1, 1), 2, false);

            var result = _service.AnalyzeNatal(info);

            Assert.False(result.IncludesHourPillar);
            Assert.DoesNotContain(
                result.Matches,
                match => match.PillarNames.Contains("時柱", StringComparer.Ordinal)
            );
        }

        [Fact]
        public void AnalyzePeriodPillars_ExactGanZhiMatch_ReturnsPeriodFuYin() {
            IReadOnlyList<IGanZhi> sourcePillars = [
                CreatePillar("年柱", "甲", "子"),
                CreatePillar("月柱", "丙", "寅")
            ];
            var periodPillar = CreatePillar("流年", "甲", "子");

            var result = _service.AnalyzePeriodPillars(sourcePillars, periodPillar, true);

            var match = Assert.Single(result.Matches);
            Assert.True(result.HasFuYin);
            Assert.Equal("年柱", match.SourcePillar.Id);
            Assert.Equal("流年", match.PeriodPillar.Id);
            Assert.Contains("這個流年內", match.Situation);
            Assert.Contains("不直接等同吉凶", match.Situation);
        }

        [Fact]
        public void AnalyzePeriodPillars_OnlyGanOrZhiMatches_DoesNotReturnPeriodFuYin() {
            IReadOnlyList<IGanZhi> sourcePillars = [
                CreatePillar("年柱", "甲", "子"),
                CreatePillar("月柱", "丙", "寅")
            ];
            var periodPillar = CreatePillar("流月", "甲", "寅");

            var result = _service.AnalyzePeriodPillars(sourcePillars, periodPillar, true);

            Assert.False(result.HasFuYin);
            Assert.Empty(result.Matches);
        }

        private static Zhu CreatePillar(string id, string gan, string zhi) {
            return new Zhu(id, gan, zhi, "比肩", ["比肩"]);
        }
    }
}

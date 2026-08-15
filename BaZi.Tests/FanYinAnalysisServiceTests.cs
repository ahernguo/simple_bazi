using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class FanYinAnalysisServiceTests {
        private readonly FanYinAnalysisService _service = new();

        [Fact]
        public void AnalyzeNatalPillars_TianGanRestrictsAndDiZhiClashes_ReturnsFanYin() {
            IReadOnlyList<Zhu> pillars = [
                CreatePillar("年柱", "甲", "子"),
                CreatePillar("日柱", "庚", "午")
            ];

            var result = _service.AnalyzeNatalPillars(pillars, false);

            var match = Assert.Single(result.Matches);
            Assert.True(result.HasFanYin);
            Assert.Equal(["年柱", "日柱"], match.Pillars.Select(pillar => pillar.Id));
        }

        [Fact]
        public void AnalyzePeriodPillars_TianGanRestrictsAndDiZhiClashes_ReturnsPeriodFanYin() {
            IReadOnlyList<IGanZhi> sourcePillars = [CreatePillar("日柱", "甲", "子")];
            var periodPillar = CreatePillar("流年", "庚", "午");

            var result = _service.AnalyzePeriodPillars(sourcePillars, periodPillar, true);

            var match = Assert.Single(result.Matches);
            Assert.True(result.HasFanYin);
            Assert.Equal("日柱", match.SourcePillar.Id);
            Assert.Equal("流年", match.PeriodPillar.Id);
            Assert.Contains("拉扯、變動或重新安排", match.Situation);
            Assert.Contains("不代表結果必然不利", match.Situation);
        }

        [Fact]
        public void AnalyzePeriodPillars_FullyCombinesOneExistingFanYinPillar_ReturnsCombination() {
            var first = CreatePillar("日柱", "甲", "子");
            var second = CreatePillar("大運", "庚", "午");
            IReadOnlyList<IGanZhi> sourcePillars = [first, second];
            var periodPillar = CreatePillar("流年", "乙", "未");

            var result = _service.AnalyzePeriodPillars(sourcePillars, periodPillar, true);

            var combination = Assert.Single(result.Combinations);
            Assert.Same(first, combination.FirstFanYinPillar);
            Assert.Same(second, combination.SecondFanYinPillar);
            Assert.Same(second, combination.CombinedPillar);
            Assert.Same(periodPillar, combination.CombiningPillar);
        }

        [Fact]
        public void AnalyzePeriodPillars_OnlyGanOrZhiCombines_DoesNotReturnCombination() {
            IReadOnlyList<IGanZhi> sourcePillars = [
                CreatePillar("日柱", "甲", "子"),
                CreatePillar("大運", "庚", "午")
            ];
            var periodPillar = CreatePillar("流月", "乙", "丑");

            var result = _service.AnalyzePeriodPillars(sourcePillars, periodPillar, true);

            Assert.Empty(result.Combinations);
        }

        private static Zhu CreatePillar(string id, string gan, string zhi) {
            return new Zhu(id, gan, zhi, "比肩", ["比肩"]);
        }
    }
}

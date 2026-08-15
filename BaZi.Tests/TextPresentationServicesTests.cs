using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class TextPresentationServicesTests {
        private readonly BranchRelationshipTextService _relationshipTextService = new();
        private readonly SemanticTextService _semanticTextService = new();
        private readonly EarthlyBranchRelationshipEngine _relationshipEngine = new();
        private readonly BaZiService _baZiService = new();

        [Fact]
        public void Segment_FormatsGanZhiElementAndMarksTenGodNeutrally() {
            const string text = "流年為丙午；夫妻星為木（財星）。";

            var segments = _semanticTextService.Segment(text);

            Assert.Equal(text, string.Concat(segments.Select(segment => segment.Text)));
            Assert.Contains(segments, segment =>
                segment.Text == "丙"
                && segment.Kind == SemanticTextKind.Element
                && segment.Element == WuXing.Huo);
            Assert.Contains(segments, segment =>
                segment.Text == "午"
                && segment.Kind == SemanticTextKind.Element
                && segment.Element == WuXing.Huo);
            Assert.Contains(segments, segment =>
                segment.Text == "木"
                && segment.Kind == SemanticTextKind.Element
                && segment.Element == WuXing.Mu);

            var tenGod = Assert.Single(segments, segment => segment.Text == "財星");
            Assert.Equal(SemanticTextKind.TenGod, tenGod.Kind);
            Assert.Null(tenGod.Element);
            Assert.Equal(ShiShen.Cai, tenGod.TenGod);
        }

        [Fact]
        public void Segment_DoesNotFormatOrdinaryCharactersAsStemOrElement() {
            const string text = "自己管理金錢，飲水依個人健康狀況調整。";

            var segments = _semanticTextService.Segment(text);

            Assert.Equal(text, string.Concat(segments.Select(segment => segment.Text)));
            Assert.DoesNotContain(segments, segment => segment.Kind != SemanticTextKind.Plain);
        }

        [Fact]
        public void Segment_DoesNotFormatWeiInWeiXingChengPhrase() {
            const string text = "日支丑與時支巳未形成相刑或相沖。";

            var segments = _semanticTextService.Segment(text);

            Assert.Equal(2, segments.Count(segment => segment.Kind == SemanticTextKind.Element));
            Assert.Contains(segments, segment => segment.Text == "丑" && segment.Element == WuXing.Tu);
            Assert.Contains(segments, segment => segment.Text == "巳" && segment.Element == WuXing.Huo);
            Assert.DoesNotContain(segments, segment =>
                segment.Text == "未"
                && segment.Kind == SemanticTextKind.Element);
        }

        [Fact]
        public void Segment_FormatsAllBranchesInThreeXingNames() {
            const string text = "寅巳申或丑戌未三刑";

            var segments = _semanticTextService.Segment(text);

            Assert.Equal(6, segments.Count(segment => segment.Kind == SemanticTextKind.Element));
        }

        [Fact]
        public void BuildRelationshipSections_CombinationDoesNotPromiseRelationship() {
            BranchRelationshipAnalysis analysis = CreateRelationshipAnalysis();

            IReadOnlyList<CompatibilitySection> sections = _relationshipTextService.BuildRelationshipSections(analysis);
            var text = Flatten(sections);

            Assert.Contains("不等於戀愛同意", text);
            Assert.DoesNotContain("一定會交往", text);
            Assert.DoesNotContain("注定相愛", text);
        }

        [Fact]
        public void BuildRelationshipSections_ExplicitDecline_PrioritizesStatedBoundary() {
            BranchRelationshipAnalysis analysis = CreateRelationshipAnalysis();

            IReadOnlyList<CompatibilitySection> sections = _relationshipTextService.BuildRelationshipSections(analysis, true);
            var text = Flatten(sections);

            Assert.Contains("應以此表態為準", text);
            Assert.Contains("不把命理結果當成施壓", text);
            Assert.DoesNotContain("應繼續追求", text);
        }

        [Fact]
        public void BuildRelationshipSections_SameSexCharts_UsesNeutralRoles() {
            BaZiInfo chartA = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 12, 9, 0, 0), 2);
            BaZiInfo chartB = _baZiService.GetBaZiInfo(new DateTime(1992, 2, 17, 0, 0, 0), 2, false);
            BranchRelationshipAnalysis analysis = _relationshipEngine.Analyze(chartA, chartB);

            var text = Flatten(_relationshipTextService.BuildRelationshipSections(analysis));

            Assert.DoesNotContain("丈夫", text);
            Assert.DoesNotContain("妻子", text);
            Assert.DoesNotContain("男方", text);
            Assert.DoesNotContain("女方", text);
        }

        private BranchRelationshipAnalysis CreateRelationshipAnalysis() {
            BaZiInfo chartA = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 12, 9, 0, 0), 2);
            BaZiInfo chartB = _baZiService.GetBaZiInfo(new DateTime(1992, 2, 17, 0, 0, 0), 1, false);
            return _relationshipEngine.Analyze(chartA, chartB);
        }

        private static string Flatten(IReadOnlyList<CompatibilitySection> sections) {
            return string.Join(
                "\n",
                sections.SelectMany(section => new[] { section.Title, section.Summary }
                    .Concat(section.Details)
                    .Concat(section.Notes ?? []))
            );
        }
    }
}

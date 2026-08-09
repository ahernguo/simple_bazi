using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class PersonalOverviewTextServiceTests {
        private readonly PersonalOverviewTextService _service = new();

        [Fact]
        public void Segment_FormatsGanZhiElementAndMarksTenGodNeutrally() {
            const string text = "流年為丙午；夫妻星為木（財星）。";

            var segments = _service.Segment(text);

            Assert.Equal(text, string.Concat(segments.Select(segment => segment.Text)));
            Assert.Contains(segments, segment =>
                segment.Text == "丙"
                && segment.Kind == PersonalOverviewTextKind.Element
                && segment.Element == WuXing.Huo);
            Assert.Contains(segments, segment =>
                segment.Text == "午"
                && segment.Kind == PersonalOverviewTextKind.Element
                && segment.Element == WuXing.Huo);
            Assert.Contains(segments, segment =>
                segment.Text == "木"
                && segment.Kind == PersonalOverviewTextKind.Element
                && segment.Element == WuXing.Mu);

            var tenGod = Assert.Single(segments, segment => segment.Text == "財星");
            Assert.Equal(PersonalOverviewTextKind.TenGod, tenGod.Kind);
            Assert.Null(tenGod.Element);
        }

        [Fact]
        public void Segment_DoesNotFormatOrdinaryCharactersAsStemOrElement() {
            const string text = "自己管理金錢，飲水依個人健康狀況調整。";

            var segments = _service.Segment(text);

            Assert.Equal(text, string.Concat(segments.Select(segment => segment.Text)));
            Assert.DoesNotContain(segments, segment => segment.Kind != PersonalOverviewTextKind.Plain);
        }

        [Fact]
        public void Segment_DoesNotFormatWeiInWeiXingChengPhrase() {
            const string text = "日支丑與時支巳未形成相刑或相沖。";

            var segments = _service.Segment(text);

            Assert.Equal(2, segments.Count(segment => segment.Kind == PersonalOverviewTextKind.Element));
            Assert.Contains(segments, segment => segment.Text == "丑" && segment.Element == WuXing.Tu);
            Assert.Contains(segments, segment => segment.Text == "巳" && segment.Element == WuXing.Huo);
            Assert.DoesNotContain(segments, segment =>
                segment.Text == "未"
                && segment.Kind == PersonalOverviewTextKind.Element);
        }

        [Fact]
        public void Segment_FormatsAllBranchesInThreeXingNames() {
            const string text = "寅巳申或丑戌未三刑";

            var segments = _service.Segment(text);

            Assert.Equal(6, segments.Count(segment => segment.Kind == PersonalOverviewTextKind.Element));
        }
    }
}

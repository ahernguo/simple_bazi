using System.Text;
using BaZi.Models;

namespace BaZi.Services {

    /// <summary>將個人概述文字拆成可安全套用五行與十神樣式的語意片段。</summary>
    public sealed class PersonalOverviewTextService {
        private static readonly IReadOnlyList<string> TenGodTokens = [
            "比肩",
        "劫財",
        "食神",
        "傷官",
        "偏財",
        "正財",
        "七殺",
        "正官",
        "偏印",
        "正印",
        "比劫",
        "食傷",
        "財星",
        "官殺",
        "印星"
        ];

        private static readonly IReadOnlyDictionary<char, TianGan> Stems =
            new Dictionary<char, TianGan> {
                ['甲'] = TianGan.Jia,
                ['乙'] = TianGan.Yi,
                ['丙'] = TianGan.Bing,
                ['丁'] = TianGan.Ding,
                ['戊'] = TianGan.Wu,
                ['己'] = TianGan.Ji,
                ['庚'] = TianGan.Geng,
                ['辛'] = TianGan.Xin,
                ['壬'] = TianGan.Ren,
                ['癸'] = TianGan.Gui
            };

        private static readonly IReadOnlyDictionary<char, DiZhi> Branches =
            new Dictionary<char, DiZhi> {
                ['子'] = DiZhi.Zi,
                ['丑'] = DiZhi.Chou,
                ['寅'] = DiZhi.Yin,
                ['卯'] = DiZhi.Mao,
                ['辰'] = DiZhi.Chen,
                ['巳'] = DiZhi.Si,
                ['午'] = DiZhi.Wu,
                ['未'] = DiZhi.Wei,
                ['申'] = DiZhi.Shen,
                ['酉'] = DiZhi.You,
                ['戌'] = DiZhi.Xu,
                ['亥'] = DiZhi.Hai
            };

        private static readonly IReadOnlyDictionary<char, WuXing> Elements =
            new Dictionary<char, WuXing> {
                ['木'] = WuXing.Mu,
                ['火'] = WuXing.Huo,
                ['土'] = WuXing.Tu,
                ['金'] = WuXing.Jin,
                ['水'] = WuXing.Shui
            };

        /// <summary>將文字切成一般、五行與十神片段。</summary>
        public IReadOnlyList<PersonalOverviewTextSegment> Segment(string text) {
            ArgumentNullException.ThrowIfNull(text);

            if (text.Length == 0) {
                return [];
            }

            var segments = new List<PersonalOverviewTextSegment>();
            var plainText = new StringBuilder();
            var index = 0;
            while (index < text.Length) {
                if (TryGetTenGod(text, index, out var tenGodToken)) {
                    FlushPlainText(segments, plainText);
                    segments.Add(new PersonalOverviewTextSegment(
                        tenGodToken,
                        PersonalOverviewTextKind.TenGod
                    ));
                    index += tenGodToken.Length;
                    continue;
                }

                if (TryGetGanZhi(text, index, out var stem, out var branch)) {
                    FlushPlainText(segments, plainText);
                    segments.Add(CreateElementSegment(text[index].ToString(), stem.ToWuXing()));
                    segments.Add(CreateElementSegment(text[index + 1].ToString(), branch.ToWuXing()));
                    index += 2;
                    continue;
                }

                if (Elements.TryGetValue(text[index], out var element) && IsElementContext(text, index)) {
                    FlushPlainText(segments, plainText);
                    segments.Add(CreateElementSegment(text[index].ToString(), element));
                    index++;
                    continue;
                }

                if (Branches.TryGetValue(text[index], out branch) && IsBranchContext(text, index)) {
                    FlushPlainText(segments, plainText);
                    segments.Add(CreateElementSegment(text[index].ToString(), branch.ToWuXing()));
                    index++;
                    continue;
                }

                plainText.Append(text[index]);
                index++;
            }

            FlushPlainText(segments, plainText);
            return segments;
        }

        private static bool TryGetTenGod(string text, int index, out string token) {
            foreach (var candidate in TenGodTokens) {
                if (text.AsSpan(index).StartsWith(candidate, StringComparison.Ordinal)) {
                    token = candidate;
                    return true;
                }
            }

            token = string.Empty;
            return false;
        }

        private static bool TryGetGanZhi(
            string text,
            int index,
            out TianGan stem,
            out DiZhi branch
        ) {
            if (index + 1 < text.Length
                && Stems.TryGetValue(text[index], out stem)
                && Branches.TryGetValue(text[index + 1], out branch)) {
                return true;
            }

            stem = default;
            branch = default;
            return false;
        }

        private static bool IsElementContext(string text, int index) {
            var previous = index > 0 ? text[index - 1] : '\0';
            var next = index + 1 < text.Length ? text[index + 1] : '\0';
            if (next == '（' || next == ':' || next == '：' || next == '生' || next == '剋') {
                return true;
            }

            if (previous == '生' || previous == '剋') {
                return true;
            }

            if (next == '、' || next == '／' || previous == '、' || previous == '／') {
                return true;
            }

            if (char.IsWhiteSpace(next) && index + 2 < text.Length && char.IsDigit(text[index + 2])) {
                return true;
            }

            if (text.AsSpan(index + 1).StartsWith("弱項", StringComparison.Ordinal)
                || text.AsSpan(index + 1).StartsWith("直接剋制", StringComparison.Ordinal)) {
                return true;
            }

            return EndsWithAny(
                text.AsSpan(0, index),
                "為",
                "以",
                "於",
                "皆有",
                "最多的",
                "夫妻星是",
                "子息星是"
            ) && IsElementBoundary(next);
        }

        private static bool IsElementBoundary(char next) {
            return next == '\0'
                || char.IsWhiteSpace(next)
                || next is '，' or '。' or '；' or '、' or '／' or '（' or '）' or '為' or '或';
        }

        private static bool IsBranchContext(string text, int index) {
            if (text[index] == '未' && text.AsSpan(index + 1).StartsWith("形成", StringComparison.Ordinal)) {
                return false;
            }

            var previousIsBranch = index > 0 && Branches.ContainsKey(text[index - 1]);
            var nextIsBranch = index + 1 < text.Length && Branches.ContainsKey(text[index + 1]);
            if (previousIsBranch || nextIsBranch) {
                return true;
            }

            return EndsWithAny(
                text.AsSpan(0, index),
                "年支",
                "月支",
                "日支",
                "時支",
                "年支為",
                "月支為",
                "日支為",
                "時支為",
                "夫妻宮",
                "子息宮"
            );
        }

        private static bool EndsWithAny(ReadOnlySpan<char> text, params string[] candidates) {
            foreach (var candidate in candidates) {
                if (text.EndsWith(candidate, StringComparison.Ordinal)) {
                    return true;
                }
            }

            return false;
        }

        private static PersonalOverviewTextSegment CreateElementSegment(string text, WuXing element) {
            return new PersonalOverviewTextSegment(
                text,
                PersonalOverviewTextKind.Element,
                element
            );
        }

        private static void FlushPlainText(
            ICollection<PersonalOverviewTextSegment> segments,
            StringBuilder plainText
        ) {
            if (plainText.Length == 0) {
                return;
            }

            segments.Add(new PersonalOverviewTextSegment(plainText.ToString()));
            plainText.Clear();
        }
    }
}

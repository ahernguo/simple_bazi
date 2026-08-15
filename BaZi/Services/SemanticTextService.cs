using System.Text;
using BaZi.Models;

namespace BaZi.Services {

    /// <summary>將分析文案拆成一般文字、五行與十神片段。</summary>
    public sealed class SemanticTextService {
        private static readonly IReadOnlyDictionary<string, ShiShen> TenGodTokens =
            new Dictionary<string, ShiShen> {
                ["比肩"] = ShiShen.BiJian,
                ["劫財"] = ShiShen.JieCai,
                ["食神"] = ShiShen.ShihShen,
                ["傷官"] = ShiShen.ShangGuan,
                ["偏財"] = ShiShen.PianCai,
                ["正財"] = ShiShen.ZhengCai,
                ["七殺"] = ShiShen.QiSha,
                ["正官"] = ShiShen.ZhengGuan,
                ["偏印"] = ShiShen.PianYin,
                ["正印"] = ShiShen.ZhengYin,
                ["比劫"] = ShiShen.BiJie,
                ["食傷"] = ShiShen.ShihShang,
                ["財星"] = ShiShen.Cai,
                ["官殺"] = ShiShen.GuanSha,
                ["印星"] = ShiShen.Yin
            };

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
        public IReadOnlyList<SemanticTextSegment> Segment(string text) {
            ArgumentNullException.ThrowIfNull(text);

            if (text.Length == 0) {
                return [];
            }

            var segments = new List<SemanticTextSegment>();
            var plainText = new StringBuilder();
            var index = 0;
            while (index < text.Length) {
                if (TryGetTenGod(text, index, out string tenGodToken, out ShiShen tenGod)) {
                    FlushPlainText(segments, plainText);
                    segments.Add(new SemanticTextSegment(
                        tenGodToken,
                        SemanticTextKind.TenGod,
                        TenGod: tenGod
                    ));
                    index += tenGodToken.Length;
                    continue;
                }

                if (TryGetGanZhi(text, index, out TianGan stem, out DiZhi branch)) {
                    FlushPlainText(segments, plainText);
                    segments.Add(CreateElementSegment(text[index].ToString(), stem.ToWuXing()));
                    segments.Add(CreateElementSegment(text[index + 1].ToString(), branch.ToWuXing()));
                    index += 2;
                    continue;
                }

                if (Elements.TryGetValue(text[index], out WuXing element) && IsElementContext(text, index)) {
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

        private static bool TryGetTenGod(
            string text,
            int index,
            out string token,
            out ShiShen tenGod
        ) {
            foreach (KeyValuePair<string, ShiShen> candidate in TenGodTokens) {
                if (text.AsSpan(index).StartsWith(candidate.Key, StringComparison.Ordinal)) {
                    token = candidate.Key;
                    tenGod = candidate.Value;
                    return true;
                }
            }

            token = string.Empty;
            tenGod = default;
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
            char previous = index > 0 ? text[index - 1] : '\0';
            char next = index + 1 < text.Length ? text[index + 1] : '\0';
            if (next is '（' or ':' or '：' or '生' or '剋' || previous is '生' or '剋') {
                return true;
            }

            if (next is '、' or '／' || previous is '、' or '／') {
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

            bool previousIsBranch = index > 0 && Branches.ContainsKey(text[index - 1]);
            bool nextIsBranch = index + 1 < text.Length && Branches.ContainsKey(text[index + 1]);
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
            foreach (string candidate in candidates) {
                if (text.EndsWith(candidate, StringComparison.Ordinal)) {
                    return true;
                }
            }

            return false;
        }

        private static SemanticTextSegment CreateElementSegment(string text, WuXing element) {
            return new SemanticTextSegment(text, SemanticTextKind.Element, element);
        }

        private static void FlushPlainText(
            ICollection<SemanticTextSegment> segments,
            StringBuilder plainText
        ) {
            if (plainText.Length == 0) {
                return;
            }

            segments.Add(new SemanticTextSegment(plainText.ToString()));
            plainText.Clear();
        }
    }
}

using System.Text;
using System.Text.Encodings.Web;
using System.Text.RegularExpressions;
using BaZi.Models;
using Microsoft.AspNetCore.Components;

namespace BaZi.Components.Compatibility {

    public partial class CompatibilityResultView {
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

        private static readonly Regex DisplayTokenPattern = new(
            "正財|偏財|正官|七殺|食神|傷官|正印|偏印|比肩|劫財|比劫|食傷|財星|官殺|印星|[金木水火土甲乙丙丁戊己庚辛壬癸子丑寅卯辰巳午未申酉戌亥]",
            RegexOptions.Compiled | RegexOptions.CultureInvariant
        );

        [Parameter, EditorRequired]
        public CompatibilityResult Result { get; set; } = default!;

        private static string FormatBirth(BaZiInfo info) {
            return info.IsBirthTimeAccurate
                ? $"{info.SolarDate:yyyy/MM/dd HH:mm}，{info.Gender.ToSexString()}"
                : $"{info.SolarDate:yyyy/MM/dd}，時辰未知，{info.Gender.ToSexString()}";
        }

        private MarkupString FormatText(
            string text,
            CompatibilityTenGodSubject subject,
            bool showsFavorability
        ) {
            var html = new StringBuilder();
            var subjectInfo = GetSubjectInfo(subject);
            var position = 0;

            foreach (Match match in DisplayTokenPattern.Matches(text)) {
                html.Append(HtmlEncoder.Default.Encode(text[position..match.Index]));

                if (subjectInfo.Info is not null && TenGodTokens.TryGetValue(match.Value, out var tenGod)) {
                    AppendTenGod(
                        html,
                        match.Value,
                        tenGod,
                        subjectInfo.Info,
                        subjectInfo.Label,
                        showsFavorability
                    );
                } else if (TryGetElement(match.Value, out var element) && IsElementReference(text, match.Index)) {
                    html.Append($"<span class=\"{GetElementColorClass(element)} fw-semibold\">{match.Value}</span>");
                } else if (TryGetGanZhiElement(text, match.Index, match.Value, out var ganZhiElement)) {
                    html.Append($"<span class=\"{GetElementColorClass(ganZhiElement)} fw-semibold\">{match.Value}</span>");
                } else {
                    html.Append(HtmlEncoder.Default.Encode(match.Value));
                }

                position = match.Index + match.Length;
            }

            html.Append(HtmlEncoder.Default.Encode(text[position..]));
            return new MarkupString(html.ToString());
        }

        private (BaZiInfo? Info, string Label) GetSubjectInfo(CompatibilityTenGodSubject subject) {
            return subject switch {
                CompatibilityTenGodSubject.Self => (Result.Self, "自己"),
                CompatibilityTenGodSubject.Other => (Result.Other, "對方"),
                _ => (null, string.Empty)
            };
        }

        private static void AppendTenGod(
            StringBuilder html,
            string displayText,
            ShiShen tenGod,
            BaZiInfo info,
            string subjectLabel,
            bool showsFavorability
        ) {
            if (!showsFavorability) {
                html.Append("<span class=\"topic-ten-god-reference\">");
                html.Append(HtmlEncoder.Default.Encode(displayText));
                html.Append("</span>");
                return;
            }

            var element = TenGodElementResolver.Resolve(info.RiZhu, tenGod);
            var isFavorable = info.LikeWuXing.Contains(element);
            var stateClass = isFavorable ? "topic-ten-god-favorable" : "topic-ten-god-unfavorable";
            var stateText = isFavorable ? "喜用神（相對較好）" : "忌神（相對較需留意）";
            var tooltip = $"{displayText}屬{element.ToWuXingString()}，依{subjectLabel}本命格局列為{stateText}";

            html.Append($"<span class=\"topic-ten-god {stateClass}\" title=\"");
            html.Append(HtmlEncoder.Default.Encode(tooltip));
            html.Append("\">");
            html.Append(HtmlEncoder.Default.Encode(displayText));
            html.Append("</span>");
        }

        private static bool TryGetElement(string text, out WuXing element) {
            element = text switch {
                "木" => WuXing.Mu,
                "火" => WuXing.Huo,
                "土" => WuXing.Tu,
                "金" => WuXing.Jin,
                "水" => WuXing.Shui,
                _ => default
            };

            return text is "木" or "火" or "土" or "金" or "水";
        }

        private static bool IsElementReference(string text, int index) {
            var previous = index > 0 ? text[index - 1] : '\0';
            var next = index + 1 < text.Length ? text[index + 1] : '\0';
            if (previous is ('為' or '屬') && next is ('\0' or '，' or '、' or '。' or '；' or '）')) {
                return true;
            }

            var suffix = text.AsSpan(index + 1);
            return suffix.StartsWith("身強") || suffix.StartsWith("身弱");
        }

        private static bool TryGetGanZhiElement(
            string text,
            int index,
            string value,
            out WuXing element
        ) {
            var prefix = text[..index];
            if (prefix.EndsWith("天干", StringComparison.Ordinal) && TryGetStemElement(value, out element)) {
                return true;
            }

            if ((prefix.EndsWith("地支", StringComparison.Ordinal)
                    || prefix.EndsWith("年支", StringComparison.Ordinal)
                    || prefix.EndsWith("月支", StringComparison.Ordinal)
                    || prefix.EndsWith("日支", StringComparison.Ordinal)
                    || prefix.EndsWith("時支", StringComparison.Ordinal))
                && TryGetBranchElement(value, out element)) {
                return true;
            }

            element = default;
            return false;
        }

        private static bool TryGetStemElement(string value, out WuXing element) {
            element = value switch {
                "甲" or "乙" => WuXing.Mu,
                "丙" or "丁" => WuXing.Huo,
                "戊" or "己" => WuXing.Tu,
                "庚" or "辛" => WuXing.Jin,
                "壬" or "癸" => WuXing.Shui,
                _ => default
            };
            return value is "甲" or "乙" or "丙" or "丁" or "戊" or "己" or "庚" or "辛" or "壬" or "癸";
        }

        private static bool TryGetBranchElement(string value, out WuXing element) {
            element = value switch {
                "寅" or "卯" => WuXing.Mu,
                "巳" or "午" => WuXing.Huo,
                "辰" or "戌" or "丑" or "未" => WuXing.Tu,
                "申" or "酉" => WuXing.Jin,
                "亥" or "子" => WuXing.Shui,
                _ => default
            };
            return value is "子" or "丑" or "寅" or "卯" or "辰" or "巳"
                or "午" or "未" or "申" or "酉" or "戌" or "亥";
        }

        private static string GetElementColorClass(WuXing element) {
            return element switch {
                WuXing.Mu => "element-wood",
                WuXing.Huo => "element-fire",
                WuXing.Tu => "element-earth",
                WuXing.Jin => "element-metal",
                WuXing.Shui => "element-water",
                _ => string.Empty
            };
        }

        private static string GetToneClass(CompatibilityTone tone) {
            return tone switch {
                CompatibilityTone.Positive => "border-success",
                CompatibilityTone.Caution => "border-warning",
                CompatibilityTone.Notice => "border-info",
                _ => "border-secondary"
            };
        }

        private static string GetToneIcon(CompatibilityTone tone) {
            return tone switch {
                CompatibilityTone.Positive => "fa-circle-check text-success",
                CompatibilityTone.Caution => "fa-triangle-exclamation text-warning",
                CompatibilityTone.Notice => "fa-circle-info text-info",
                _ => "fa-compass"
            };
        }
    }
}

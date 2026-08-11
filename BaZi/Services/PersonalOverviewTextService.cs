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

        /// <summary>將結構化地支命中轉成中性、非決定論的感情合盤 Cards。</summary>
        /// <param name="analysis">地支規則引擎的分析結果。</param>
        /// <param name="otherHasExplicitlyDeclinedRomance">對方是否已明確拒絕發展感情。</param>
        public IReadOnlyList<CompatibilitySection> BuildRelationshipSections(
            BranchRelationshipAnalysis analysis,
            bool otherHasExplicitlyDeclinedRomance = false
        ) {
            ArgumentNullException.ThrowIfNull(analysis);

            // 固定組合以地支條目交叉核對：https://zh.wikipedia.org/wiki/%E5%9C%B0%E6%94%AF#%E7%B5%84%E5%90%88
            // 傳統解讀範圍參考開放指南；該頁也指出合化須另有條件，沖不必然為凶：https://bazi8.net/zh/learn/clashes-combinations
            // 命理內容只作民俗文化參考。NCC 研究整理的節目規範亦要求不得將四柱視為普遍的人生預測方式：
            // https://www.ncc.gov.tw/chinese/files/17121/3501_38434_171213_1.pdf
            // 溝通、界線與同意建議依衛福部資料，不由命理命中取代當事人表態：
            // https://www.mohw.gov.tw/cp-2704-81496-1.html
            var sections = new List<CompatibilitySection> {
                CreateDataCompletenessSection(analysis)
            };

            AddRelationSection(
                sections,
                analysis,
                BranchRelationshipType.SixCombination,
                "六合候選",
                "六合可解讀為靠近、熟悉感與吸引力；它不等於戀愛、關係承諾，吸引力強有時為好麻吉或閨密，不一定發展成戀人關係。",
                CompatibilityTone.Positive,
                ["只標示組合與合化五行候選；是否合化仍須另看月令、透干、根氣、鄰接與其他條件。"]
            );
            AddRelationSection(
                sections,
                analysis,
                BranchRelationshipType.SixClash,
                "六沖互動",
                "六沖可視為節奏差異、立場拉扯、變動；可能有爭吵或習慣上的狀況，但不代表分手或不適合。",
                CompatibilityTone.Caution,
                ["可把命中的柱位當成優先核對題目，不把單一地支關係當作感情成敗。"]
            );
            AddRelationSection(
                sections,
                analysis,
                BranchRelationshipType.SixHarm,
                "六害互動",
                "六害常描述較隱性的摩擦，例如好意被誤解、信任敏感或不容易直接說明的不滿。",
                CompatibilityTone.Caution,
                ["此處只列可能的互動假設，仍應以雙方實際感受與行為核對。"]
            );
            AddRelationSection(
                sections,
                analysis,
                BranchRelationshipType.Punishment,
                "相刑互動",
                "相刑為高反應、防衛被觸發、反覆糾結或壓力累積；較容易陷入情緒、溝通不良與猜測對方想法等狀況，需多注意互動狀況。",
                CompatibilityTone.Caution,
                ["'三支齊全' 會觸發三刑，為較嚴重的狀況；'部分成立' 則需要多注意、觀察；'自刑候選' 表示同支出現兩次，容易自己糾結。"]
            );
            AddRelationSection(
                sections,
                analysis,
                BranchRelationshipType.ThreeMeeting,
                "三會候選",
                "三會可視為共同方向或互動節奏較集中；互有好感或默契，但不一定發展成戀人狀態，有可能是麻吉或閨密，仍需雙方互相認可",
                CompatibilityTone.Notice,
                ["'三支齊' 全仍不直接宣告成局或合化，亦不據此計算感情成功率。"]
            );

            if (analysis.Hits.Count == 0) {
                sections.Add(new CompatibilitySection(
                    "未見指定地支互動",
                    "已知柱位沒有六合、六沖、六害、相刑或三會，感情上無特殊狀況；不代表關係絕對順利或有狀況。",
                    [],
                    CompatibilityTone.Information
                ));
            }

            sections.Add(CreateRelationshipAdviceSection(analysis, otherHasExplicitlyDeclinedRomance));
            return sections;
        }

        private static CompatibilitySection CreateDataCompletenessSection(
            BranchRelationshipAnalysis analysis
        ) {
            var threePillarCharts = new List<string>();
            if (analysis.UsesThreePillarsForA) {
                threePillarCharts.Add("自己");
            }

            if (analysis.UsesThreePillarsForB) {
                threePillarCharts.Add("對方");
            }

            var summary = threePillarCharts.Count == 0
                ? "雙方出生時辰皆標示為已知，本結果使用四柱分析。"
                : $"{string.Join("與", threePillarCharts)}的出生時辰未知，本結果以三柱分析；未知時柱完全不參與規則判定。";
            return new CompatibilitySection(
                "資料完整度",
                summary,
                [],
                threePillarCharts.Count == 0
                    ? CompatibilityTone.Information
                    : CompatibilityTone.Notice
            );
        }

        private static void AddRelationSection(
            ICollection<CompatibilitySection> sections,
            BranchRelationshipAnalysis analysis,
            BranchRelationshipType relationType,
            string title,
            string summary,
            CompatibilityTone tone,
            IReadOnlyList<string> notes
        ) {
            var hits = analysis.Hits.Where(hit => hit.RelationType == relationType).ToArray();
            if (hits.Length == 0) {
                return;
            }

            sections.Add(new CompatibilitySection(
                title,
                summary,
                [.. hits.Select(DescribeHit)],
                tone,
                Notes: notes
            ));
        }

        private static string DescribeHit(BranchRelationshipHit hit) {
            var relationName = GetRelationName(hit.RelationType);
            var members = string.Concat(hit.Members.Select(member => member.ToZhiString()));
            var scope = hit.Scope switch {
                BranchRelationshipScope.NatalA => "自己原局",
                BranchRelationshipScope.NatalB => "對方原局",
                BranchRelationshipScope.CrossChart => "跨命盤",
                _ => throw new ArgumentOutOfRangeException(nameof(hit), hit.Scope, null)
            };
            var completion = hit.Completion switch {
                BranchRelationshipCompletion.Pair => "成對命中",
                BranchRelationshipCompletion.Partial => "部分成立",
                BranchRelationshipCompletion.Complete => "三支齊全",
                BranchRelationshipCompletion.Self => "自刑候選",
                BranchRelationshipCompletion.Candidate => "組合候選",
                _ => throw new ArgumentOutOfRangeException(nameof(hit), hit.Completion, null)
            };
            var transform = hit.TransformElement is null
                ? string.Empty
                : $"，傳統合化五行為{hit.TransformElement.Value.ToWuXingString()}，目前不判定已合化";
            var confidence = hit.Confidence == BranchRelationshipConfidence.Hypothetical
                ? "，含假設時柱"
                : string.Empty;
            var occurrences = hit.Occurrences
                .OrderByDescending(occurrence => occurrence.Sources.Any(source => source.Position == BranchRelationshipPillarPosition.Day))
                .Select(DescribeOccurrence);
            return $"{scope}的{members}{relationName}共 {hit.OccurrenceCount} 組：{string.Join("；", occurrences)}。{completion}{transform}{confidence}。";
        }

        private static string DescribeOccurrence(BranchRelationshipOccurrence occurrence) {
            return string.Join("＋", occurrence.Sources.Select(source => {
                var participant = source.Participant == BranchRelationshipParticipant.A ? "自己" : "對方";
                var position = source.Position switch {
                    BranchRelationshipPillarPosition.Year => "年支",
                    BranchRelationshipPillarPosition.Month => "月支",
                    BranchRelationshipPillarPosition.Day => "日支",
                    BranchRelationshipPillarPosition.Hour => "時支",
                    _ => throw new ArgumentOutOfRangeException(nameof(source), source.Position, null)
                };
                return $"{participant}{position}{source.Branch.ToZhiString()}";
            }));
        }

        private static CompatibilitySection CreateRelationshipAdviceSection(
            BranchRelationshipAnalysis analysis,
            bool otherHasExplicitlyDeclinedRomance
        ) {
            if (otherHasExplicitlyDeclinedRomance) {
                return new CompatibilitySection(
                    "現實資訊與界線優先",
                    "對方已明確表示不發展感情，應以此表態為準；任何六合、三會或其他命中都不能改寫其意願。",
                    ["按對方表態維持關係界線，不把命理結果當成施壓、說服或反覆邀約的理由。"],
                    CompatibilityTone.Caution,
                    Notes: ["關係改變應由對方主動且明確表示，沉默、禮貌互動或接受朋友邀約都不等於戀愛同意。"]
                );
            }

            var hasSupport = analysis.Hits.Any(hit => hit.RelationType is
                BranchRelationshipType.SixCombination or BranchRelationshipType.ThreeMeeting);
            var hasPressure = analysis.Hits.Any(hit => hit.RelationType is
                BranchRelationshipType.SixClash or BranchRelationshipType.SixHarm or BranchRelationshipType.Punishment);
            var summary = (hasSupport, hasPressure) switch {
                (true, true) => "支持與壓力同時存在，既有靠近與協調，也有推拉、誤解或高反應情境。相處上需多協調、尊重對方，減少猜想與邊界處理",
                (true, false) => "感情上可互相扶持、互補；仍需用實際相處確認，不能由命盤代替雙方意願。",
                (false, true) => "感情壓力、爭吵可能較多，但不等於關係失敗，重點是雙方是否願意安全而平等地處理。",
                _ => "感情互動上沒有特殊狀況；感情品質仍取決於實際行為、溝通、信任與界線。"
            };
            return new CompatibilitySection(
                "整體互動與溝通建議",
                summary,
                [
                    "利用具體事件核對感受與期待，八字僅呈現「表徵」或「意象」還需實際相處才能確定。",
                    "任何親密互動都需要雙方清楚同意；亦需尊重對方的拒絕或不同意。",
                    "沉默、禮貌互動或接受邀約都不等於戀愛同意。"
                ],
                hasPressure ? CompatibilityTone.Notice : CompatibilityTone.Positive,
                Notes: ["命理命中屬傳統文化詮釋，不是人格診斷、科學預測或感情成功率。"]
            );
        }

        private static string GetRelationName(BranchRelationshipType relationType) {
            return relationType switch {
                BranchRelationshipType.SixCombination => "六合",
                BranchRelationshipType.SixClash => "六沖",
                BranchRelationshipType.SixHarm => "六害",
                BranchRelationshipType.Punishment => "相刑",
                BranchRelationshipType.ThreeMeeting => "三會",
                _ => throw new ArgumentOutOfRangeException(nameof(relationType), relationType, null)
            };
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

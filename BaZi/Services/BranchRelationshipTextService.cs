using BaZi.Models;

namespace BaZi.Services {

    /// <summary>將結構化地支命中轉成中性、非決定論的合盤文案。</summary>
    public sealed class BranchRelationshipTextService {
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
                BranchRelationshipType.SixBreak,
                "六破互動",
                "六破代表既有安排容易被打斷、承諾反覆或合作節奏不穩；問題通常需要重新確認與修補。",
                CompatibilityTone.Caution,
                ["先核對具體事件與責任，不把六破直接斷成關係破裂。"]
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
                BranchRelationshipType.ThreeCombination,
                "三合互動",
                "三合或兩支候選表示方向較容易靠攏；兩支只算半合或暗拱，三支齊全也仍須另查成局條件。",
                CompatibilityTone.Positive,
                ["只標示組合候選，不直接判定已合化，也不以此保證感情結果。"]
            );
            AddRelationSection(
                sections,
                analysis,
                BranchRelationshipType.ThreeMeeting,
                "三會候選",
                "三會可視為共同方向或互動節奏較集中；互有好感或默契，但不一定發展成戀人狀態，有可能是麻吉或閨密，仍需雙方互相認可",
                CompatibilityTone.Notice,
                ["三支齊全仍不直接宣告成局或合化，亦不據此計算感情成功率。"]
            );

            if (analysis.Hits.Count == 0) {
                sections.Add(new CompatibilitySection(
                    "未見指定地支互動",
                    "已知柱位沒有六合、六沖、六害、六破、相刑、三合或三會；這只表示未命中指定組合，不代表關係絕對順利或一定有狀況。",
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
                : $"{string.Join("與", threePillarCharts)}的時柱為吉時吉分，本結果以三柱分析；未知時柱完全不參與規則判定。";
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
            var relationName = hit.RelationType.ToDisplayName();
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
                BranchRelationshipType.SixCombination
                    or BranchRelationshipType.ThreeCombination
                    or BranchRelationshipType.ThreeMeeting);
            var hasPressure = analysis.Hits.Any(hit => hit.RelationType is
                BranchRelationshipType.SixClash
                    or BranchRelationshipType.SixHarm
                    or BranchRelationshipType.SixBreak
                    or BranchRelationshipType.Punishment);
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

    }
}

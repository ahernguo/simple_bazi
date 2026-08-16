using BaZi.Models;

namespace BaZi.Services {

    /// <summary>依課程筆記提供兩人命盤的分層合盤分析。</summary>
    public sealed class CompatibilityService {
        private static readonly IReadOnlyDictionary<(WuXing Element, GeJu Strength), (WuXing Element, GeJu Strength)[]> RomanceComplementTargets =
            new Dictionary<(WuXing, GeJu), (WuXing, GeJu)[]> {
                [(WuXing.Jin, GeJu.ShenQiang)] = [(WuXing.Huo, GeJu.ShenQiang), (WuXing.Mu, GeJu.ShenQiang), (WuXing.Jin, GeJu.ShenRuo)],
                [(WuXing.Jin, GeJu.ShenRuo)] = [(WuXing.Tu, GeJu.ShenQiang), (WuXing.Jin, GeJu.ShenQiang)],
                [(WuXing.Mu, GeJu.ShenQiang)] = [(WuXing.Mu, GeJu.ShenRuo), (WuXing.Tu, GeJu.ShenQiang), (WuXing.Jin, GeJu.ShenQiang)],
                [(WuXing.Mu, GeJu.ShenRuo)] = [(WuXing.Shui, GeJu.ShenQiang), (WuXing.Mu, GeJu.ShenQiang)],
                [(WuXing.Shui, GeJu.ShenQiang)] = [(WuXing.Shui, GeJu.ShenRuo), (WuXing.Huo, GeJu.ShenQiang), (WuXing.Tu, GeJu.ShenQiang)],
                [(WuXing.Shui, GeJu.ShenRuo)] = [(WuXing.Shui, GeJu.ShenQiang), (WuXing.Jin, GeJu.ShenQiang)],
                [(WuXing.Huo, GeJu.ShenQiang)] = [(WuXing.Jin, GeJu.ShenQiang), (WuXing.Shui, GeJu.ShenQiang), (WuXing.Huo, GeJu.ShenRuo)],
                [(WuXing.Huo, GeJu.ShenRuo)] = [(WuXing.Mu, GeJu.ShenQiang), (WuXing.Huo, GeJu.ShenQiang)],
                [(WuXing.Tu, GeJu.ShenQiang)] = [(WuXing.Shui, GeJu.ShenQiang), (WuXing.Tu, GeJu.ShenRuo)],
                [(WuXing.Tu, GeJu.ShenRuo)] = [(WuXing.Huo, GeJu.ShenQiang), (WuXing.Tu, GeJu.ShenQiang)]
            };

        private static readonly IReadOnlyDictionary<(WuXing Element, GeJu Strength), (WuXing Element, GeJu Strength)[]> FriendComplementTargets =
            new Dictionary<(WuXing, GeJu), (WuXing, GeJu)[]> {
                [(WuXing.Huo, GeJu.ShenQiang)] = [(WuXing.Jin, GeJu.ShenQiang), (WuXing.Shui, GeJu.ShenQiang), (WuXing.Huo, GeJu.ShenRuo)],
                [(WuXing.Mu, GeJu.ShenQiang)] = [(WuXing.Tu, GeJu.ShenQiang), (WuXing.Jin, GeJu.ShenQiang), (WuXing.Mu, GeJu.ShenRuo)],
                [(WuXing.Jin, GeJu.ShenQiang)] = [(WuXing.Mu, GeJu.ShenQiang), (WuXing.Huo, GeJu.ShenQiang), (WuXing.Jin, GeJu.ShenRuo)],
                [(WuXing.Shui, GeJu.ShenQiang)] = [(WuXing.Huo, GeJu.ShenQiang), (WuXing.Tu, GeJu.ShenQiang), (WuXing.Shui, GeJu.ShenRuo)],
                [(WuXing.Tu, GeJu.ShenQiang)] = [(WuXing.Shui, GeJu.ShenQiang), (WuXing.Mu, GeJu.ShenQiang), (WuXing.Tu, GeJu.ShenRuo)],
                [(WuXing.Huo, GeJu.ShenRuo)] = [(WuXing.Mu, GeJu.ShenQiang), (WuXing.Huo, GeJu.ShenQiang)],
                [(WuXing.Mu, GeJu.ShenRuo)] = [(WuXing.Shui, GeJu.ShenQiang), (WuXing.Mu, GeJu.ShenQiang)],
                [(WuXing.Shui, GeJu.ShenRuo)] = [(WuXing.Jin, GeJu.ShenQiang), (WuXing.Shui, GeJu.ShenQiang)],
                [(WuXing.Jin, GeJu.ShenRuo)] = [(WuXing.Tu, GeJu.ShenQiang), (WuXing.Jin, GeJu.ShenQiang)],
                [(WuXing.Tu, GeJu.ShenRuo)] = [(WuXing.Huo, GeJu.ShenQiang), (WuXing.Tu, GeJu.ShenQiang)]
            };

        private readonly BaZiService _baZiService;
        private readonly TenGodAnalysisService _tenGodService;
        private readonly EarthlyBranchRelationshipEngine _branchRelationshipEngine;
        private readonly BranchRelationshipTextService _textService;

        public CompatibilityService(
            BaZiService baZiService,
            TenGodAnalysisService tenGodService,
            EarthlyBranchRelationshipEngine branchRelationshipEngine,
            BranchRelationshipTextService textService
        ) {
            _baZiService = baZiService;
            _tenGodService = tenGodService;
            _branchRelationshipEngine = branchRelationshipEngine;
            _textService = textService;
        }

        public CompatibilityResult Analyze(
            BaZiInfo self,
            CompatibilityBirthInput otherInput,
            CompatibilityRelationship relationship
        ) {
            ArgumentNullException.ThrowIfNull(otherInput);
            var other = _baZiService.GetBaZiInfo(
                otherInput.ToDateTime(),
                otherInput.Gender,
                otherInput.IsBirthTimeAccurate
            );
            return Analyze(self, other, relationship);
        }

        public CompatibilityResult Analyze(
            BaZiInfo self,
            BaZiInfo other,
            CompatibilityRelationship relationship
        ) {
            ArgumentNullException.ThrowIfNull(self);
            ArgumentNullException.ThrowIfNull(other);

            var sections = relationship switch {
                CompatibilityRelationship.Romance => AnalyzeRomance(self, other),
                CompatibilityRelationship.Parent => AnalyzeParent(self, other),
                CompatibilityRelationship.Child => AnalyzeChild(self, other),
                CompatibilityRelationship.Sibling => AnalyzeSibling(self, other),
                CompatibilityRelationship.Friend => AnalyzeFriend(self, other),
                CompatibilityRelationship.Colleague => AnalyzeTenGodRelationship(other, relationship),
                _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null)
            };

            BranchRelationshipAnalysis? branchRelationships = null;
            IReadOnlyList<CompatibilitySection> internetSourceSections = [];
            if (relationship == CompatibilityRelationship.Romance) {
                branchRelationships = _branchRelationshipEngine.Analyze(self, other);
                internetSourceSections = _textService.BuildRelationshipSections(branchRelationships);
            }

            return new CompatibilityResult(
                relationship,
                self,
                other,
                sections,
                GetLimitations(relationship),
                branchRelationships,
                internetSourceSections
            );
        }

        private IReadOnlyList<CompatibilitySection> AnalyzeParent(BaZiInfo self, BaZiInfo other) {
            return [
                .. AnalyzeTenGodRelationship(other, CompatibilityRelationship.Parent),
                CreateParentStarSection(self)
            ];
        }

        private IReadOnlyList<CompatibilitySection> AnalyzeChild(BaZiInfo self, BaZiInfo other) {
            return [
                CreateChildAffinitySection(self),
                .. AnalyzeTenGodRelationship(other, CompatibilityRelationship.Child)
            ];
        }

        private CompatibilitySection CreateParentStarSection(BaZiInfo self) {
            var motherLocations = GetStarLocations(self, ShiShen.Yin);
            var fatherLocations = GetStarLocations(self, ShiShen.Cai);
            var affectedLocations = motherLocations.Concat(fatherLocations)
                .Where(location => IsBranchUnderNatalPressure(self, location.Branch))
                .DistinctBy(location => (location.PillarName, location.Group))
                .ToArray();
            var details = new List<string> {
                $"母親、女性長輩或主要照顧者看印星：{DescribeLocations(motherLocations)}。",
                $"父親或男性長輩看財星：{DescribeLocations(fatherLocations)}。"
            };
            if (affectedLocations.Length > 0) {
                details.Add($"家人星所在支參與本命刑沖：{string.Join("、", affectedLocations.Select(location => $"{location.PillarName}地支{location.Branch.ToZhiString()}"))}。這代表家庭承接、支援或關係變動較容易被觸發，不等於家人一定生病或出事。");
            } else {
                details.Add("已知本命內，家人星所在支未直接參與六沖或相刑。");
            }

            return new CompatibilitySection(
                "自己命盤的家人星",
                "自己的命盤只反映自己如何承接家人事件；要判斷父母本人的狀況，仍以父母本人命盤、實際健康與生活資料為準。",
                details,
                affectedLocations.Length > 0 ? CompatibilityTone.Caution : CompatibilityTone.Information,
                CompatibilityTenGodSubject.Self,
                ["印星與財星是關係定位，不是一顆星固定對應一位家人。", "交通、健康、照護、財務與法律事項必須依現實證據及專業意見處理。"]
            );
        }

        private CompatibilitySection CreateChildAffinitySection(BaZiInfo self) {
            var childGroup = self.Gender == Sex.Male ? ShiShen.GuanSha : ShiShen.ShihShang;
            var childElement = self.Gender == Sex.Male
                ? BaZiDefine.RestrictBy[self.RiZhu]
                : BaZiDefine.Generation[self.RiZhu];
            var locations = GetStarLocations(self, childGroup);
            var details = new List<string> {
                $"依{self.Gender.ToSexString()}命口徑，子息星為{childGroup.ToShenString()}，子息星五行為{childElement.ToWuXingString()}。",
                locations.Count == 0
                    ? "已知柱位未見明顯子息星；這只表示命盤訊號較弱，不等於沒有子女或不孕。"
                    : $"子息星共見於{DescribeLocations(locations)}；數量只表示緣分訊號，不等於生育數量。"
            };

            var tone = CompatibilityTone.Information;
            if (!self.IsBirthTimeAccurate) {
                details.Add("出生時辰不確定，時柱子息宮完全不參與判定；補齊準確時辰後結果可能改變。");
                tone = CompatibilityTone.Notice;
            } else {
                var hourHasChildStar = locations.Any(location => location.PillarName == "時柱");
                var dayHourClash = IsClash(self.DayZhu.Zhi, self.HourZhu.Zhi);
                var dayHourPunishment = IsPunishment(self.DayZhu.Zhi, self.HourZhu.Zhi);
                details.Add(hourHasChildStar
                    ? $"時柱子息宮直接見{childGroup.ToShenString()}，與子女的連結訊號較直接。"
                    : $"時柱子息宮未直接見{childGroup.ToShenString()}；仍須保留其他柱的子息星訊號。");
                if (dayHourClash || dayHourPunishment) {
                    details.Add($"日支{self.DayZhu.Zhi.ToZhiString()}與時支{self.HourZhu.Zhi.ToZhiString()}形成{(dayHourClash ? "相沖" : "相刑")}，親子之間較容易出現距離、作息或溝通摩擦；不能據此判定子女健康或生產方式。");
                    tone = CompatibilityTone.Caution;
                }
            }

            return new CompatibilitySection(
                "子息宮與子息星",
                "時柱是子息宮，子息星與子息宮必須分開定位，再用實際意願、家庭條件與醫療資料核對。",
                details,
                tone,
                CompatibilityTenGodSubject.Self,
                ["子息星多寡不能推算必然生育數量、性別或生育力。", "備孕、懷孕、分娩與輔助生殖技術須由合格生殖醫學與婦產科團隊評估。"]
            );
        }

        private IReadOnlyList<CompatibilitySection> AnalyzeRomance(BaZiInfo self, BaZiInfo other) {
            var selfZhi = self.YearZhu.Zhi;
            var otherZhi = other.YearZhu.Zhi;
            var sameTrineGroup = selfZhi != otherZhi
                && BaZiDefine.ThreeHe.Values.Any(group => group.Contains(selfZhi) && group.Contains(otherZhi));
            var isClash = IsClash(selfZhi, otherZhi);

            var zodiacSummary = sameTrineGroup
                ? $"{self.ShengXiao}與{other.ShengXiao}同屬一組生肖三合，氣場較易協調。"
                : isClash
                    ? $"{self.ShengXiao}與{other.ShengXiao}形成六沖，視為變動與衝突，但關係是否失敗仍視實際相處而定。"
                    : selfZhi == otherZhi
                        ? $"兩人同為{self.ShengXiao}，沒有六沖的狀況；關係仍視實際相處而定。"
                        : "雙方生肖沒有三合、六沖的狀況；雖非天造地設的一對，但也無特別不合的狀況。";
            var zodiacTone = sameTrineGroup
                ? CompatibilityTone.Positive
                : isClash ? CompatibilityTone.Caution : CompatibilityTone.Information;

            var selfSpouse = GetSpouseElement(self);
            var otherSpouse = GetSpouseElement(other);
            var sameSpouse = selfSpouse == otherSpouse;
            var selfMatchesOther = selfSpouse == other.RiZhu;
            var otherMatchesSelf = otherSpouse == self.RiZhu;

            var complement = EvaluateDirectionalRule(self, other, RomanceComplementTargets, true);
            var reverseComplement = EvaluateDirectionalRule(other, self, RomanceComplementTargets, true);
            var complementSummary = DescribeDirectionalMatches(complement, reverseComplement, "與本命五行能量互補。", "非互補組合，但也沒有形成特別不合的狀況。");
            var complementTone = GetRuleTone(complement, reverseComplement);

            var exactDayPillar = self.DayZhu.Gan == other.DayZhu.Gan && self.DayZhu.Zhi == other.DayZhu.Zhi;
            var sameStructure = GetVisibleElementCounts(self).OrderBy(item => item.Key)
                .SequenceEqual(GetVisibleElementCounts(other).OrderBy(item => item.Key));
            var structureSummary = exactDayPillar
                ? "雙方日柱干支相同，較容易互相理解；要留意喜忌與低潮可能同步。"
                : sameStructure
                    ? "雙方已知表層干支的五行數量相同、結構相似；不代表命局相同，但傾向、偏好等會較為相似"
                    : "雙方日柱與表層五行結構均不相同，傾向、偏好差異較大，但不代表不合";

            var selfSpouseInPalace = self.DayZhu.ZhiWuXing == selfSpouse;
            var otherSpouseInPalace = other.DayZhu.ZhiWuXing == otherSpouse;
            var palaceSummary = (selfSpouseInPalace, otherSpouseInPalace) switch {
                (true, true) => "雙方表層日支五行都符合各自夫妻星坐夫妻宮的公式。",
                (true, false) => "自己的表層日支五行符合夫妻星坐夫妻宮；對方未符合。",
                (false, true) => "對方的表層日支五行符合夫妻星坐夫妻宮；自己未符合。",
                _ => "雙方表層日支五行皆非夫妻星坐夫妻宮；此項沒有形成加分條件，但不代表不合。"
            };
            var loveDetails = _tenGodService.GetDominantGroups(other).SelectMany(group => group.LeadingStars.Select(star => $"{star.ToShenString()}： {TenGodAnalysisService.GetViewsOnLove(star)}")).ToArray();
            return [
                new CompatibilitySection(
                "生肖初篩",
                zodiacSummary,
                [],
                zodiacTone,
                Notes: ["同組兩個生肖不等於完整三合局。", "六沖表示爭吵多、意見不合，不表示離婚或不適合。"]
            ),
            new CompatibilitySection(
                "共同姻緣五行",
                sameSpouse
                    ? $"雙方夫妻星同為{selfSpouse.ToWuXingString()}，感情頻率與婚姻價值較相似。"
                    : $"自己的夫妻星為{selfSpouse.ToWuXingString()}，對方為{otherSpouse.ToWuXingString()}；各自呈現不同的感情取向，但不代表彼此不合。",
                [],
                sameSpouse ? CompatibilityTone.Positive : CompatibilityTone.Information,
                Notes: ["男命的夫妻星為日主所剋的五行(財星)；女命的夫妻星為剋日主的五行(官殺)。", "同性戀判斷相同，如男同性戀雙方均看日主所剋的五行(財星)"]
            ),
            new CompatibilitySection(
                "互為夫妻星",
                selfMatchesOther && otherMatchesSelf
                    ? "雙方日主互為彼此夫妻星，互相有吸引力。"
                    : selfMatchesOther || otherMatchesSelf
                        ? (selfMatchesOther ? "對方日主符合自己的夫妻星，對方容易吸引自己" : "自己的日主符合對方的夫妻星，對方容易被自己吸引")
                        : "雙方日主未互為夫妻星，可能較無吸引力",
                [],
                selfMatchesOther && otherMatchesSelf ? CompatibilityTone.Positive : CompatibilityTone.Information,
                Notes: ["以命主性別決定財星或官殺的推論，並非性別認同或關係品質。", "吸引力強不代表成為情侶或追求，有可能是好麻吉或好閨密。"]
            ),
            new CompatibilitySection(
                "五行能量互補",
                complementSummary,
                [],
                complementTone,
                Notes: ["只比較本命日主與身強／身弱，不考慮目前大運。", "從強、從弱沒有進行推論。"]
            ),
            new CompatibilitySection(
                "結構相似度",
                structureSummary,
                [],
                exactDayPillar || sameStructure ? CompatibilityTone.Notice : CompatibilityTone.Information,
                Notes: ["僅比較各自命盤，表示各自的吸引力、欣賞程度"]
            ),
            new CompatibilitySection(
                "夫妻星坐夫妻宮",
                palaceSummary,
                [],
                selfSpouseInPalace || otherSpouseInPalace ? CompatibilityTone.Positive : CompatibilityTone.Information,
                Notes: ["藏干未有固定權重，故不作等量推論。", "不是關係永久穩定的保證。"]
            ),
            new CompatibilitySection(
                "對方戀愛關係",
                string.Empty,
                loveDetails,
                CompatibilityTone.Information
            ),
        ];
        }

        private IReadOnlyList<CompatibilitySection> AnalyzeTenGodRelationship(
            BaZiInfo other,
            CompatibilityRelationship relationship
        ) {
            var dominantGroups = _tenGodService.GetDominantGroups(other);
            var mainStars = _tenGodService.GetMainStars(other);
            var groupNames = string.Join("、", dominantGroups.Select(group => $"{group.Group.ToShenString()}（{group.Count}）"));
            var mainStarNames = string.Join("、", mainStars.Select(star => star.ToShenString()).Distinct());
            var details = dominantGroups.Select(group => GetAdvice(relationship, group)).ToArray();

            return [
                new CompatibilitySection(
                "對方的主要十神",
                $"五組合併統計以{groupNames}最多；外顯主星為{mainStarNames}。",
                [],
                CompatibilityTone.Notice,
                CompatibilityTenGodSubject.Other,
                ["並列最多的十神組合為主要參考。", "主星與副星分層參考，仍需與對方長期行為來驗證。"]
            ),
            new CompatibilitySection(
                "相處建議",
                "以下是依對方最多十神組轉成的溝通假設。",
                details,
                CompatibilityTone.Positive,
                CompatibilityTenGodSubject.Other
            ),
        ];
        }

        private IReadOnlyList<CompatibilitySection> AnalyzeSibling(BaZiInfo self, BaZiInfo other) {
            var siblingStars = _tenGodService.GetAllStars(self)
                .Where(star => star is ShiShen.BiJian or ShiShen.JieCai)
                .ToArray();
            var locations = GetSiblingLocations(self);
            var supportive = self.StrengthStatus is GeJu.ShenRuo or GeJu.CongQiang;
            var signalSummary = siblingStars.Length == 0
                ? "自己的命盤統計未見比肩或劫財；手足訊號不突出、緣份較少，但不代表沒有感情。"
                : $"自己的命盤共見 {siblingStars.Length} 個比肩／劫財訊號，出現在{string.Join("、", locations)}。";
            var tendency = supportive
                ? "比劫對身弱／從強較偏助力；仍須以對方能力與意願確認。"
                : "身強／從弱遇比劫較需管理競爭、借貸與資源分配；雖不代表會爭財，但仍需注意彼此狀況。";
            var dominantGroups = _tenGodService.GetDominantGroups(other);

            return [
                new CompatibilitySection(
                "手足訊號",
                signalSummary,
                [tendency],
                supportive ? CompatibilityTone.Positive : CompatibilityTone.Caution,
                CompatibilityTenGodSubject.Self,
                TenGodShowsFavorability: true
            ),
            new CompatibilitySection(
                "手足本人的互動入口",
                $"對方最多的十神組為{string.Join("、", dominantGroups.Select(group => group.Group.ToShenString()))}。",
                dominantGroups.Select(group => GetAdvice(CompatibilityRelationship.Sibling, group)).ToArray(),
                CompatibilityTone.Notice,
                CompatibilityTenGodSubject.Other
            )
            ];
        }

        private static IReadOnlyList<CompatibilitySection> AnalyzeFriend(BaZiInfo self, BaZiInfo other) {
            var forward = EvaluateDirectionalRule(self, other, FriendComplementTargets, false);
            var reverse = EvaluateDirectionalRule(other, self, FriendComplementTargets, false);
            var complementSummary = DescribeDirectionalMatches(forward, reverse, "朋友互補可補足盲點。", "雙方並非互補組合。僅表示非天生互相吸引，不代表不能當朋友或不適合。");
            var prefersLead = self.StrengthStatus is GeJu.ShenQiang or GeJu.CongRuo;
            var partnership = prefersLead
                ? "由自己保有主要決策權；若合夥仍要把授權、帳目與退出條件書面化。"
                : "善用團隊與夥伴力量；仍要確認能力、信用、權責與實際交付。";

            return [
                new CompatibilitySection(
                "朋友五行互補",
                complementSummary,
                [],
                GetRuleTone(forward, reverse),
                Notes: ["從格在選長期夥伴時著重降低共同盲點，但不自動判定。"]
            ),
            new CompatibilitySection("合作方式", partnership, ["友誼不等於信用背書；借貸、投資、擔保或合夥須另查現金流、契約與風險。", "可先用小規模任務驗證溝通與交付，再決定是否擴大合作。"], CompatibilityTone.Caution)
            ];
        }

        private static string GetAdvice(CompatibilityRelationship relationship, TenGodGroupStatistic group) {
            var leading = group.LeadingStars.Count == 0
                ? string.Empty
                : $"（組內以{string.Join("、", group.LeadingStars.Select(star => star.ToShenString()))}較多）";
            var advice = (relationship, group.Group) switch {
                (CompatibilityRelationship.Parent, ShiShen.Cai) => "以具體投入、回報與預算溝通；涉及風險時把資料、成本及底線說清楚。",
                (CompatibilityRelationship.Parent, ShiShen.GuanSha) => "重視承諾、計畫與責任；尊重成年子女的隱私與決定，不以秩序合理化控制。",
                (CompatibilityRelationship.Parent, ShiShen.ShihShang) => "從興趣與討論切入，允許不同想法；把期待拆成可完成的小步驟。",
                (CompatibilityRelationship.Parent, ShiShen.Yin) => "給空間並清楚表達需要與界線；尊重不是討好，關懷也不等於替對方決定。",
                (CompatibilityRelationship.Parent, ShiShen.BiJie) => "重視平等、義氣與被認同；分歧宜私下談，金錢和人情仍保留明確界線。",
                (CompatibilityRelationship.Child, ShiShen.Cai) => "用零用錢、儲蓄與選擇練習資源管理，也要說明價格不等於人的價值。",
                (CompatibilityRelationship.Child, ShiShen.GuanSha) => "孩子可能自我要求高；降低羞辱式壓力，允許犯錯，將挑戰與可取得的支持一起說明。",
                (CompatibilityRelationship.Child, ShiShen.ShihShang) => "提供探索與表達空間，用短時段、階段目標和明確收尾協助聚焦。",
                (CompatibilityRelationship.Child, ShiShen.Yin) => "採低壓而具體的引導，說清楚開始、下一步與完成；若生活功能受影響應尋求專業協助。",
                (CompatibilityRelationship.Child, ShiShen.BiJie) => "可透過同儕與團隊活動建立動機；用透明安全規則取代全面監控。",
                (CompatibilityRelationship.Sibling, ShiShen.Cai) => "維持互惠並明確致謝；理財意見仍須自行查證，不當作投資保證。",
                (CompatibilityRelationship.Sibling, ShiShen.GuanSha) => "尊重合理作息、角色與承諾；長幼秩序不能取代平等與自主。",
                (CompatibilityRelationship.Sibling, ShiShen.ShihShang) => "從興趣與新點子開啟互動；提出的方案仍要做成本與安全驗證。",
                (CompatibilityRelationship.Sibling, ShiShen.BiJie) => "可在自願下共享朋友圈或請其牽線；介紹不等於推薦、擔保或信用審查。",
                (CompatibilityRelationship.Sibling, ShiShen.Yin) => "把需求、希望協助的內容與期限說清楚，同時尊重對方拒絕。",
                (CompatibilityRelationship.Colleague, ShiShen.Cai) => "說清分工、投入、回報與長期效益；偏財重結果時仍要補齊細節與風險。",
                (CompatibilityRelationship.Colleague, ShiShen.GuanSha) => "承諾前確認範圍與期限，做不到要及早透明；七殺型可用條列目標建立並肩感。",
                (CompatibilityRelationship.Colleague, ShiShen.ShihShang) => "給創意與做法空間，同時定義里程碑、期限與驗收標準。",
                (CompatibilityRelationship.Colleague, ShiShen.Yin) => "減少瑣碎干預並尊重私人領域，但交付仍用明確工作標準驗收。",
                (CompatibilityRelationship.Colleague, ShiShen.BiJie) => "以平等、互助與信用建立合作；仍保留職業界線並揭露利益衝突。",
                _ => "把命理分類視為待驗證的溝通假設，依對方回饋調整。"
            };

            return $"{group.Group.ToShenString()}{leading}：{advice}";
        }

        private static IReadOnlyList<FamilyStarLocation> GetStarLocations(BaZiInfo info, ShiShen group) {
            var pillars = new List<Zhu> { info.YearZhu, info.MonthZhu, info.DayZhu };
            if (info.IsBirthTimeAccurate) {
                pillars.Add(info.HourZhu);
            }

            return [.. pillars
                .Select(pillar => new FamilyStarLocation(
                    pillar.Id,
                    pillar.Zhi,
                    group,
                    pillar.ZhuXing != ShiShen.RiZhu && pillar.ZhuXing.ToCombined() == group,
                    pillar.FuXing.Any(star => star.ToCombined() == group)
                ))
                .Where(location => location.HasStem || location.HasHiddenStem)];
        }

        private static string DescribeLocations(IReadOnlyCollection<FamilyStarLocation> locations) {
            if (locations.Count == 0) {
                return "已知柱位未見明顯落點";
            }

            return string.Join("、", locations.Select(location => {
                var source = (location.HasStem, location.HasHiddenStem) switch {
                    (true, true) => "透干且地支藏干亦見",
                    (true, false) => "天干透出",
                    _ => "地支藏干可見"
                };
                return $"{location.PillarName}地支{location.Branch.ToZhiString()}（{source}）";
            }));
        }

        private bool IsBranchUnderNatalPressure(BaZiInfo info, DiZhi branch) {
            var branches = new List<DiZhi> {
                info.YearZhu.Zhi,
                info.MonthZhu.Zhi,
                info.DayZhu.Zhi
            };
            if (info.IsBirthTimeAccurate) {
                branches.Add(info.HourZhu.Zhi);
            }

            return branches.Any(other => IsClash(branch, other)
                    || (other != branch && IsPunishment(branch, other)))
                || (branches.Count(other => other == branch) >= 2 && IsPunishment(branch, branch));
        }

        private bool IsClash(DiZhi first, DiZhi second) {
            return _branchRelationshipEngine.HasRelationship(
                first,
                second,
                BranchRelationshipType.SixClash
            );
        }

        private bool IsPunishment(DiZhi first, DiZhi second) {
            return _branchRelationshipEngine.HasRelationship(
                first,
                second,
                BranchRelationshipType.Punishment
            );
        }

        private static IReadOnlyList<string> GetSiblingLocations(BaZiInfo info) {
            IReadOnlyList<Zhu> pillars = info.IsBirthTimeAccurate
                ? [info.YearZhu, info.MonthZhu, info.DayZhu, info.HourZhu]
                : [info.YearZhu, info.MonthZhu, info.DayZhu];
            return [.. pillars
            .Where(pillar => pillar.ZhuXing is ShiShen.BiJian or ShiShen.JieCai
                || pillar.FuXing.Any(star => star is ShiShen.BiJian or ShiShen.JieCai))
            .Select(pillar => pillar.Id)];
        }

        private static WuXing GetSpouseElement(BaZiInfo info) {
            return info.Gender == Sex.Male
                ? BaZiDefine.Restricting[info.RiZhu]
                : BaZiDefine.RestrictBy[info.RiZhu];
        }

        private static IReadOnlyDictionary<WuXing, int> GetVisibleElementCounts(BaZiInfo info) {
            var elements = new List<WuXing> {
                info.YearZhu.GanWuXing,
                info.YearZhu.ZhiWuXing,
                info.MonthZhu.GanWuXing,
                info.MonthZhu.ZhiWuXing,
                info.DayZhu.GanWuXing,
                info.DayZhu.ZhiWuXing
            };
            if (info.IsBirthTimeAccurate) {
                elements.Add(info.HourZhu.GanWuXing);
                elements.Add(info.HourZhu.ZhiWuXing);
            }

            return elements.GroupBy(element => element).ToDictionary(group => group.Key, group => group.Count());
        }

        private static DirectionResult EvaluateDirectionalRule(
            BaZiInfo source,
            BaZiInfo target,
            IReadOnlyDictionary<(WuXing Element, GeJu Strength), (WuXing Element, GeJu Strength)[]> rules,
            bool hasEarthDiscrepancy
        ) {
            var sourceKey = (source.RiZhu, source.StrengthStatus);
            var targetKey = (target.RiZhu, target.StrengthStatus);
            if (source.StrengthStatus is GeJu.CongQiang or GeJu.CongRuo
                || target.StrengthStatus is GeJu.CongQiang or GeJu.CongRuo) {
                return new DirectionResult(RuleMatch.NeedsReview, sourceKey, targetKey, "從格沒有完整指定配對表");
            }

            if (hasEarthDiscrepancy
                && sourceKey == (WuXing.Tu, GeJu.ShenQiang)
                && target.StrengthStatus == GeJu.ShenQiang
                && target.RiZhu is WuXing.Mu or WuXing.Jin) {
                return new DirectionResult(RuleMatch.NeedsReview, sourceKey, targetKey, "土身強配對的原始規則存在差異");
            }

            var matches = rules.TryGetValue(sourceKey, out var targets) && targets.Contains(targetKey);
            return new DirectionResult(matches ? RuleMatch.Match : RuleMatch.NoMatch, sourceKey, targetKey, null);
        }

        private static string DescribeDirectionalMatches(
            DirectionResult forward,
            DirectionResult reverse,
            string matchText,
            string noMatchText
        ) {
            if (forward.Match == RuleMatch.NeedsReview || reverse.Match == RuleMatch.NeedsReview) {
                var reasons = new[] { forward.Reason, reverse.Reason }.Where(reason => reason is not null).Distinct();
                return $"此組合需要人工核對：{string.Join("；", reasons)}。現有規則未完整定義此狀況，因此不作確定推論。";
            }

            if (forward.Match == RuleMatch.Match && reverse.Match == RuleMatch.Match) {
                return $"雙向都{matchText}";
            }

            if (forward.Match == RuleMatch.Match || reverse.Match == RuleMatch.Match) {
                return $"只有{(forward.Match == RuleMatch.Match ? "自己看對方" : "對方看自己")}的方向{matchText}配對表具有方向性，因此保留單向結果。";
            }

            return noMatchText;
        }

        private static CompatibilityTone GetRuleTone(DirectionResult forward, DirectionResult reverse) {
            if (forward.Match == RuleMatch.NeedsReview || reverse.Match == RuleMatch.NeedsReview) {
                return CompatibilityTone.Notice;
            }

            return forward.Match == RuleMatch.Match || reverse.Match == RuleMatch.Match
                ? CompatibilityTone.Positive
                : CompatibilityTone.Information;
        }

        private static IReadOnlyList<string> GetLimitations(CompatibilityRelationship relationship) {
            var limitations = new List<string> {
            "分析採八字日元法整理，屬傳統文化參考，不是人格診斷、科學預測或關係保證。",
            "命盤僅為先天的「傾向」或「意象」，雙方仍需以實際行為、溝通、意願來訂定最終關係。"
        };

            if (relationship is CompatibilityRelationship.Friend or CompatibilityRelationship.Sibling) {
                limitations.Add("借貸、投資、擔保、遺產或合夥應使用書面契約，並另做財務與法律查證。");
            }

            if (relationship == CompatibilityRelationship.Colleague) {
                limitations.Add("招募、考核、薪酬、升遷與解僱仍須以能力、行為、績效、制度與法規為準，不可由命盤決定。");
            }

            if (relationship is CompatibilityRelationship.Parent or CompatibilityRelationship.Child) {
                limitations.Add("健康、安全、教育與家庭衝突應依現實情況尋求合格的醫療、教育、法律或社福專業協助。");
            }

            return limitations;
        }

        private enum RuleMatch {
            NoMatch,
            Match,
            NeedsReview
        }

        private sealed record DirectionResult(
            RuleMatch Match,
            (WuXing Element, GeJu Strength) Source,
            (WuXing Element, GeJu Strength) Target,
            string? Reason
        );

        private sealed record FamilyStarLocation(
            string PillarName,
            DiZhi Branch,
            ShiShen Group,
            bool HasStem,
            bool HasHiddenStem
        );
    }
}

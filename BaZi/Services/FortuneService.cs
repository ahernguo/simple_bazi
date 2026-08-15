using BaZi.Models;
using Microsoft.AspNetCore.Components;

namespace BaZi.Services {

    /// <summary>提供大運、流年與流月分析服務</summary>
    public class FortuneService {
        private readonly FuYinAnalysisService _fuYinAnalysisService;
        private readonly FanYinAnalysisService _fanYinAnalysisService;
        private readonly PeriodFavorabilityService _periodFavorabilityService;
        private readonly TenGodPresentationService _tenGodPresentationService;
        private readonly EarthlyBranchRelationshipEngine _relationshipEngine;

        private enum PeriodScope {
            LiuNian,
            LiuYue
        }

        private static readonly (string Key, string Name, int Month, int YearOffset)[] LiuYueStartTerms = [
            ("立春", "立春", 2, 0),
        ("惊蛰", "驚蟄", 3, 0),
        ("清明", "清明", 4, 0),
        ("立夏", "立夏", 5, 0),
        ("芒种", "芒種", 6, 0),
        ("小暑", "小暑", 7, 0),
        ("立秋", "立秋", 8, 0),
        ("白露", "白露", 9, 0),
        ("寒露", "寒露", 10, 0),
        ("立冬", "立冬", 11, 0),
        ("大雪", "大雪", 12, 0),
        ("小寒", "小寒", 1, 1)
        ];

        // 以下地支表依筆記 3-2「講義列出的財星組合」照錄，不用通用藏干表自行補齊。
        // 待核對：木日主的丑藏己土卻未列入，外部「八字日元法」資料也未提供本課程的取捨門檻。
        // 以下地支表依筆記 3-4「五日主官殺流日速查」照錄。
        // 待核對：木日主的辰是課程「濕土生金」例外，不代表辰的主氣或藏干已改為金。
        // 以下夫妻星地支依筆記 4-4 的男女十組講義表照錄，不以對稱推理擴寫。
        // 待核對：火日主女命未列辰、金日主男命未列未等差異，在筆記與外部資料中仍沒有一致門檻。
        private static readonly IReadOnlyDictionary<(WuXing DayMaster, Sex Gender), DiZhi[]> SpouseBranches =
            new Dictionary<(WuXing DayMaster, Sex Gender), DiZhi[]> {
                [(WuXing.Mu, Sex.Female)] = [DiZhi.Shen, DiZhi.You, DiZhi.Chou],
                [(WuXing.Mu, Sex.Male)] = [DiZhi.Chen, DiZhi.Xu, DiZhi.Chou, DiZhi.Wei],
                [(WuXing.Huo, Sex.Female)] = [DiZhi.Hai, DiZhi.Zi, DiZhi.Chou, DiZhi.Shen],
                [(WuXing.Huo, Sex.Male)] = [DiZhi.Shen, DiZhi.You, DiZhi.Chou],
                [(WuXing.Tu, Sex.Female)] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen, DiZhi.Hai],
                [(WuXing.Tu, Sex.Male)] = [DiZhi.Hai, DiZhi.Zi, DiZhi.Chou, DiZhi.Chen, DiZhi.Shen],
                [(WuXing.Jin, Sex.Female)] = [DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Xu],
                [(WuXing.Jin, Sex.Male)] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen, DiZhi.Hai],
                [(WuXing.Shui, Sex.Female)] = [DiZhi.Chen, DiZhi.Xu, DiZhi.Chou, DiZhi.Wei],
                [(WuXing.Shui, Sex.Male)] = [DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Xu]
            };

        public FortuneService()
            : this(
                new FuYinAnalysisService(),
                new FanYinAnalysisService(),
                new PeriodFavorabilityService(),
                new TenGodPresentationService(),
                new EarthlyBranchRelationshipEngine()
            ) {
        }

        public FortuneService(
            FuYinAnalysisService fuYinAnalysisService,
            FanYinAnalysisService fanYinAnalysisService
        ) : this(
            fuYinAnalysisService,
            fanYinAnalysisService,
            new PeriodFavorabilityService(),
            new TenGodPresentationService(),
            new EarthlyBranchRelationshipEngine()
        ) {
        }

        public FortuneService(
            FuYinAnalysisService fuYinAnalysisService,
            FanYinAnalysisService fanYinAnalysisService,
            PeriodFavorabilityService periodFavorabilityService,
            TenGodPresentationService tenGodPresentationService,
            EarthlyBranchRelationshipEngine relationshipEngine
        ) {
            _fuYinAnalysisService = fuYinAnalysisService;
            _fanYinAnalysisService = fanYinAnalysisService;
            _periodFavorabilityService = periodFavorabilityService;
            _tenGodPresentationService = tenGodPresentationService;
            _relationshipEngine = relationshipEngine;
        }

        public IReadOnlyList<int> GetLiuNianYears(BaZiInfo info) {
            return info.DaYunList
                .SelectMany(daYun => daYun.LiuNianList)
                .Select(liuNian => liuNian.Year)
                .Distinct()
                .Order()
                .ToArray();
        }

        /// <summary>比較本命四柱與指定流年，取得犯太歲及生肖相沖分析。</summary>
        /// <param name="info">八字命盤。</param>
        /// <param name="targetYear">要分析的西元年份。</param>
        /// <returns>犯太歲分析；找不到指定流年時傳回 <see langword="null"/>。</returns>
        public TaiSuiAnalysisResult? GetTaiSuiAnalysis(BaZiInfo info, int targetYear) {
            ArgumentNullException.ThrowIfNull(info);

            var (_, liuNian) = FindLiuNian(info, targetYear);
            if (liuNian is null) {
                return null;
            }

            var directInteractions = GetTaiSuiInteractions(info.YearZhu.Zhi, liuNian.Zhi);
            var allNatalPillars = new List<(string Name, Zhu Pillar)> {
                ("年柱", info.YearZhu),
                ("月柱", info.MonthZhu),
                ("日柱", info.DayZhu)
            };
            if (info.IsBirthTimeAccurate) {
                allNatalPillars.Add(("時柱", info.HourZhu));
            }

            var indirectInteractions = new List<TaiSuiPillarInteraction>();
            foreach ((string pillarName, Zhu pillar) in allNatalPillars.Where(item => item.Name != "年柱")) {
                var interactions = GetTaiSuiInteractions(pillar.Zhi, liuNian.Zhi);
                if (interactions.Count > 0) {
                    indirectInteractions.Add(new TaiSuiPillarInteraction(
                        pillarName,
                        pillar.Zhi,
                        interactions
                    ));
                }
            }

            var reinforcedInteractions = new List<TaiSuiReinforcedInteraction>();
            foreach ((string repeatedName, Zhu repeatedPillar) in allNatalPillars.Where(item => item.Pillar.Zhi == liuNian.Zhi)) {
                foreach ((string relatedName, Zhu relatedPillar) in allNatalPillars.Where(item => item.Name != repeatedName)) {
                    var interactions = GetTaiSuiInteractions(repeatedPillar.Zhi, relatedPillar.Zhi)
                        .Where(interaction => interaction != TaiSuiInteractionType.SameBranch)
                        .ToArray();
                    if (interactions.Length > 0) {
                        reinforcedInteractions.Add(new TaiSuiReinforcedInteraction(
                            repeatedName,
                            repeatedPillar.Zhi,
                            relatedName,
                            relatedPillar.Zhi,
                            interactions
                        ));
                    }
                }
            }

            return new TaiSuiAnalysisResult(
                targetYear,
                liuNian.Gan,
                liuNian.Zhi,
                GetZodiac(liuNian.Zhi),
                GetZodiac(info.YearZhu.Zhi),
                info.YearZhu.Zhi,
                directInteractions,
                indirectInteractions,
                info.IsBirthTimeAccurate,
                reinforcedInteractions
            );
        }

        public IReadOnlyList<LiuYue> GetLiuYueMonths(BaZiInfo info, int? targetYear) {
            if (targetYear is null)
                return [];

            var (_, liuNian) = FindLiuNian(info, targetYear.Value);
            return liuNian?.LiuYueList ?? [];
        }

        /// <summary>取得指定年份所屬大運及流年的伏吟、反吟分析。</summary>
        /// <param name="info">八字命盤。</param>
        /// <param name="targetYear">要分析的西元年份。</param>
        /// <returns>依大運、流年排序的分析結果；找不到年份時傳回空集合。</returns>
        public IReadOnlyList<FortuneYinAnalysisResult> GetDaYunAndLiuNianYinAnalysis(
            BaZiInfo info,
            int targetYear
        ) {
            var (daYun, liuNian) = FindLiuNian(info, targetYear);
            if (daYun is null || liuNian is null) {
                return [];
            }

            return [
                CreateYinAnalysis(info, daYun),
                CreateYinAnalysis(info, liuNian, [daYun])
            ];
        }

        /// <summary>取得指定流月的伏吟、反吟分析。</summary>
        /// <param name="info">八字命盤。</param>
        /// <param name="targetYear">流月所屬的西元年份。</param>
        /// <param name="targetMonthIndex">流月序號，範圍為 0 到 11。</param>
        /// <returns>流月分析結果；找不到指定流月時傳回空集合。</returns>
        public IReadOnlyList<FortuneYinAnalysisResult> GetLiuYueYinAnalysis(
            BaZiInfo info,
            int targetYear,
            int targetMonthIndex
        ) {
            var (daYun, liuNian) = FindLiuNian(info, targetYear);
            if (daYun is null || liuNian is null) {
                return [];
            }

            var liuYue = liuNian.LiuYueList.FirstOrDefault(month => month.Index == targetMonthIndex);
            if (liuYue is null) {
                return [];
            }

            return [CreateYinAnalysis(info, liuYue, [daYun, liuNian])];
        }

        /// <summary>取得指定流月正式生效的起訖節氣與日期時間。</summary>
        /// <param name="info">八字命盤資料。</param>
        /// <param name="targetYear">流年年份。</param>
        /// <param name="targetMonthIndex">流月序號，範圍為 0 到 11。</param>
        /// <returns>流月起訖資訊；找不到對應資料時傳回 <see langword="null"/>。</returns>
        public LiuYueStartInfo? GetLiuYueStartInfo(BaZiInfo info, int targetYear, int targetMonthIndex) {
            if (targetMonthIndex < 0 || targetMonthIndex >= LiuYueStartTerms.Length)
                return null;

            var month = GetLiuYueMonths(info, targetYear)
                .FirstOrDefault(item => item.Index == targetMonthIndex);
            if (month is null)
                return null;

            var startTerm = LiuYueStartTerms[targetMonthIndex];
            int endTermIndex = (targetMonthIndex + 1) % LiuYueStartTerms.Length;
            var endTerm = LiuYueStartTerms[endTermIndex];
            int startYear = targetYear + startTerm.YearOffset;
            int endYear = targetMonthIndex == LiuYueStartTerms.Length - 1
                ? targetYear + 1
                : targetYear + endTerm.YearOffset;
            DateTime? startDate = GetJieQiDate(startTerm.Key, startYear, startTerm.Month);
            DateTime? endDate = GetJieQiDate(endTerm.Key, endYear, endTerm.Month);
            if (startDate is null || endDate is null)
                return null;

            return new LiuYueStartInfo(
                month,
                startTerm.Name,
                startDate.Value,
                endTerm.Name,
                endDate.Value
            );
        }

        private static DateTime? GetJieQiDate(string termKey, int solarYear, int solarMonth) {
            var lunar = Lunar.Solar.FromYmdHms(solarYear, solarMonth, 15, 12, 0, 0).Lunar;
            if (!lunar.JieQiTable.TryGetValue(termKey, out var solar))
                return null;

            return new DateTime(
                solar.Year,
                solar.Month,
                solar.Day,
                solar.Hour,
                solar.Minute,
                solar.Second
            );
        }

        public bool IsCurrentDaYun(DaYun dy, BaZiInfo info) {
            int currentYear = DateTime.Now.Year;
            var nextDaYun = info.DaYunList.FirstOrDefault(x => x.StartYear > dy.StartYear);
            if (nextDaYun == null) {
                return currentYear >= dy.StartYear;
            }
            return currentYear >= dy.StartYear && currentYear < nextDaYun.StartYear;
        }

        private static bool TryFindInteraction<T>(
            IList<IList<IGanZhi>> sources,
            IList<T> target,
            Func<IGanZhi, T> valueSelector,
            int requiredCount,
            bool mustIncludeLast,
            out IList<IGanZhi> participants,
            out int sourceIndex
        ) where T : notnull {
            for (var index = 0; index < sources.Count; index++) {
                if (TrySelectParticipants(sources[index], target, valueSelector, requiredCount, mustIncludeLast, out participants)) {
                    sourceIndex = index;
                    return true;
                }
            }

            participants = [];
            sourceIndex = -1;
            return false;
        }

        private static bool TryFindAnyInteraction<T>(
            IList<IList<IGanZhi>> sources,
            IList<IList<T>> targets,
            Func<IGanZhi, T> valueSelector,
            int requiredCount,
            bool mustIncludeLast,
            out IList<IGanZhi> participants,
            out int sourceIndex
        ) where T : notnull {
            for (var index = 0; index < sources.Count; index++) {
                foreach (var target in targets) {
                    if (TrySelectParticipants(sources[index], target, valueSelector, requiredCount, mustIncludeLast, out participants)) {
                        sourceIndex = index;
                        return true;
                    }
                }
            }

            participants = [];
            sourceIndex = -1;
            return false;
        }

        private static bool TrySelectParticipants<T>(
            IList<IGanZhi> source,
            IList<T> target,
            Func<IGanZhi, T> valueSelector,
            int requiredCount,
            bool mustIncludeLast,
            out IList<IGanZhi> participants
        ) where T : notnull {
            participants = [];
            if (source.Count == 0)
                return false;

            var matchedValues = new HashSet<T>();
            var matches = new List<IGanZhi>();
            foreach (var item in source) {
                var value = valueSelector(item);
                if (target.Contains(value) && matchedValues.Add(value)) {
                    matches.Add(item);
                }
            }

            if (matchedValues.Count < requiredCount)
                return false;

            var last = source[^1];
            var lastValue = valueSelector(last);
            if (mustIncludeLast && !target.Contains(lastValue))
                return false;

            if (!mustIncludeLast) {
                participants = matches.Take(requiredCount).ToList();
                return true;
            }

            participants = matches
                .Where(item => !EqualityComparer<T>.Default.Equals(valueSelector(item), lastValue))
                .Take(requiredCount - 1)
                .Append(last)
                .ToList();
            return participants.Count == requiredCount;
        }

        private static bool TryFindSelfInteraction(
            IList<IList<IGanZhi>> sources,
            IList<DiZhi> targets,
            out IList<IGanZhi> participants,
            out int sourceIndex
        ) {
            for (var index = 0; index < sources.Count; index++) {
                var source = sources[index];
                if (source.Count >= 2 && targets.Contains(source[0].Zhi) && source.All(item => item.Zhi == source[0].Zhi)) {
                    participants = source.Take(2).ToList();
                    sourceIndex = index;
                    return true;
                }
            }

            participants = [];
            sourceIndex = -1;
            return false;
        }

        private void CreateThreeHeDesc(
            IList<IGanZhi> a,
            WuXing wuXing,
            bool isFavorable,
            System.Text.StringBuilder html
        ) {
            var stateClass = isFavorable ? "text-success" : "text-danger";
            var alertClass = isFavorable ? "alert-success" : "alert-warning";
            var stateLabel = FormatFavorabilityLabel(
                isFavorable,
                $"三合形成{wuXing.ToWuXingString()}，依本命格局與目前大運背景列為{(isFavorable ? "喜用" : "忌神")}方向。喜用神會幫扶命主，為運勢加分；忌神會耗洩命主，相對不利"
            );
            var description = isFavorable
                ? $"合成五行落在{stateLabel}，可視為整體平衡的加分訊號；仍須搭配其他刑沖與現實狀況。"
                : $"合成五行落在{stateLabel}，可能加重失衡；不能因形成三合就一律判成好事。";
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span>、{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 與{a[2].Id} <span class=""border-bottom-dash"">{a[2].Zhi.ToZhiString()}</span> 形成「<strong class=""{stateClass}"">三合{wuXing.ToWuXingString()}局</strong>」</div>");
            html.AppendLine($@"    <div class=""alert {alertClass} analysis-item-explanation mb-0""><strong>格局判讀：</strong>{description}</div>");
            html.AppendLine(@"</div>");
        }

        private void CreateFiveHeDesc(IList<IGanZhi> a, int type, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Gan.ToGanString()}</span> 與{a[1].Id} <span class=""border-bottom-dash"">{a[1].Gan.ToGanString()}</span> 形成「<strong class=""text-success"">合相</strong>」</div>");
            if (type == 0) {
                html.AppendLine(@"    <div class=""alert alert-success analysis-item-explanation mb-0""><strong>幫助：</strong>補充所需的能量，提升整體運勢、壞事化小、趨吉避凶</div>");
            } else if (type == 1) {
                html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-0""><strong>狀況：</strong>原本的{FormatFavorabilityLabel(true, "被合走的五行依本命格局與目前大運背景列為喜用方向\r\n喜用神可以幫扶命主，讓運勢加分")}被用走，較容易感到受限或不順。</div>");
            } else {
                html.AppendLine($@"    <div class=""alert alert-success analysis-item-explanation mb-0""><strong>幫助：</strong>原本的{FormatFavorabilityLabel(false, "被合走的五行依本命格局與目前大運背景列為忌神方向\r\n忌神會耗洩命主，對運勢沒有加分(相對不利)")}受牽制，可視為減輕失衡的訊號。</div>");
            }
            html.AppendLine(@"</div>");
        }

        private void CreateSixHeDesc(IList<IGanZhi> a, int type, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span> 與{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 形成「<strong class=""text-success"">合相</strong>」</div>");
            if (type == 0) {
                html.AppendLine(@"    <div class=""alert alert-success analysis-item-explanation mb-0""><strong>幫助：</strong>補充所需的能量，提升整體運勢、壞事化小、趨吉避凶</div>");
            } else if (type == 1) {
                html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-0""><strong>狀況：</strong>原本的{FormatFavorabilityLabel(true, "被合走的五行依本命格局與目前大運背景列為喜用方向\r\n喜用神可以幫扶命主，讓運勢加分")}被用走，較容易感到受限或不順。</div>");
            } else {
                html.AppendLine($@"    <div class=""alert alert-success analysis-item-explanation mb-0""><strong>幫助：</strong>原本的{FormatFavorabilityLabel(false, "被合走的五行依本命格局與目前大運背景列為忌神方向\r\n忌神會耗洩命主，對運勢沒有加分(相對不利)")}受牽制，可視為減輕失衡的訊號。</div>");
            }
            html.AppendLine(@"</div>");
        }

        private void CreateTwoXingDesc(IList<IGanZhi> a, string means, string advice, string force, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span> 與{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 形成「<strong class=""text-danger"">相刑</strong>」</div>");
            html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-2""><div><strong>阻力：</strong>外在、力度{force}</div><div><strong>狀況：</strong>{means}</div></div>");
            html.AppendLine($@"    <div class=""alert alert-info analysis-item-explanation mb-0""><strong>建議：</strong>{advice}</div>");
            html.AppendLine(@"</div>");
        }

        private void CreateThreeXingDesc(IList<IGanZhi> a, string means, string advice, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span>、{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 與{a[2].Id} <span class=""border-bottom-dash"">{a[2].Zhi.ToZhiString()}</span> 形成「<strong class=""text-danger"">三刑</strong>」</div>");
            html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-2""><div><strong>阻力：</strong>外在、力度強</div><div><strong>狀況：</strong>{means}</div></div>");
            html.AppendLine($@"    <div class=""alert alert-info analysis-item-explanation mb-0""><strong>建議：</strong>{advice}</div>");
            html.AppendLine(@"</div>");
        }

        private void CreateSelfXingDesc(IList<IGanZhi> a, string means, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span> 與{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 形成「<strong class=""text-danger"">自刑</strong>」</div>");
            html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-2""><div><strong>阻力：</strong>內在、力度中</div><div><strong>狀況：</strong>{means}</div></div>");
            html.AppendLine(@"    <div class=""alert alert-info analysis-item-explanation mb-0""><strong>建議：</strong>適度釋放壓力</div>");
            html.AppendLine(@"</div>");
        }

        private void CreateChongDesc(IList<IGanZhi> a, string means, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span> 與{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 形成「<strong class=""text-danger"">相沖</strong>」</div>");
            html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-0""><div><strong>阻力：</strong>外在、力度中</div><div><strong>狀況：</strong>{means}</div></div>");
            html.AppendLine(@"</div>");
        }

        private void CreatePoDesc(IList<IGanZhi> a, string means, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span> 與{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 形成「<strong class=""text-danger"">破</strong>」</div>");
            html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-0""><div><strong>阻力：</strong>內在、力度小</div><div><strong>狀況：</strong>{means}</div></div>");
            html.AppendLine(@"</div>");
        }

        private void CreateHaiDesc(IList<IGanZhi> a, string means, System.Text.StringBuilder html) {
            html.AppendLine(@"<div class=""analysis-item mb-3"">");
            html.AppendLine($@"    <div class=""analysis-item-main"">{a[0].Id} <span class=""border-bottom-dash"">{a[0].Zhi.ToZhiString()}</span> 與{a[1].Id} <span class=""border-bottom-dash"">{a[1].Zhi.ToZhiString()}</span> 形成「<strong class=""text-danger"">害</strong>」</div>");
            html.AppendLine($@"    <div class=""alert alert-warning analysis-item-explanation mb-0""><div><strong>阻力：</strong>內在、力度小</div><div><strong>狀況：</strong>{means}</div></div>");
            html.AppendLine(@"</div>");
        }

        private (bool bad, IDictionary<HeHui, WuXing> heHui) CheckConflict(
            BaZiInfo info,
            IList<IList<IGanZhi>>? ganPair,
            IList<IList<IGanZhi>>? threePair,
            IList<IList<IGanZhi>> twoPair,
            Func<WuXing, bool> isFavorable,
            System.Text.StringBuilder html
        ) {
            var bad = false;
            var heHui = new Dictionary<HeHui, WuXing>();
            if (!info.IsBirthTimeAccurate) {
                ganPair = ganPair is null ? null : RemoveHourPillarFromSources(info, ganPair);
                threePair = threePair is null ? null : RemoveHourPillarFromSources(info, threePair);
                twoPair = RemoveHourPillarFromSources(info, twoPair);
            }
            // 天干五合
            if ((ganPair != null) && TryFindAnyInteraction(ganPair, BaZiDefine.FiveHe, item => item.Gan, 2, true, out var ganInteraction, out _)) {
                // 檢查這個被合走的天干是喜用神還是忌神
                var type = 0;
                var sourceElement = ganInteraction[0].Gan.ToWuXing();
                if (isFavorable(sourceElement)) {
                    // 原本是喜用神，被合走反而會變不順
                    type = 1;
                } else {
                    // 原本是忌神，被合走反而會比較順
                    type = 2;
                }
                CreateFiveHeDesc(ganInteraction, type, html);
                heHui.Add(HeHui.FiveHe, sourceElement);
            }
            // 地支三合
            if (threePair != null) {
                foreach (var wx in BaZiDefine.WuXingList) {
                    if (BaZiDefine.ThreeHe.ContainsKey(wx)
                        && TryFindInteraction(threePair, BaZiDefine.ThreeHe[wx], item => item.Zhi, 3, true, out var heInteraction, out _)) {
                        CreateThreeHeDesc(heInteraction, wx, isFavorable(wx), html);   // 三合沒有說會有合絆的狀況，只有說天干五合跟地支六合
                        if (!heHui.ContainsKey(HeHui.ThreeHe))
                            heHui.Add(HeHui.ThreeHe, wx);
                        break;
                    }
                }
            }
            // 地支三會、六合
            if (threePair != null) {
                foreach (var wx in BaZiDefine.WuXingList) {
                    if (BaZiDefine.ThreeHui.ContainsKey(wx)
                        && TryFindInteraction(threePair, BaZiDefine.ThreeHui[wx], item => item.Zhi, 3, true, out _, out _)) {
                        // 三會沒有要特別什麼狀況，理論上只跟判讀大運、流年時，看五行會不會很不平衡之類，所以這邊就不做 CreateThreeHuiDesc 了!
                        if (!heHui.ContainsKey(HeHui.ThreeHui))
                            heHui.Add(HeHui.ThreeHui, wx);
                    }
                    if (BaZiDefine.SixHe.ContainsKey(wx)
                        && TryFindInteraction(threePair, BaZiDefine.SixHe[wx], item => item.Zhi, 2, true, out var heInteraction, out _)) {
                        // 判斷這個被用走的地支是好的還壞的
                        var type = 0;
                        var firstElement = heInteraction[0].Zhi.ToWuXing();
                        var secondElement = heInteraction[1].Zhi.ToWuXing();
                        if (isFavorable(firstElement) || isFavorable(secondElement)) {
                            // 原本是喜用神，被合走反而會變不順
                            type = 1;
                        } else {
                            // 原本是忌神，被合走反而會比較順
                            type = 2;
                            if (!heHui.ContainsKey(HeHui.SixHe))
                                heHui.Add(HeHui.SixHe, wx);
                        }
                        CreateSixHeDesc(heInteraction, type, html);
                    }
                }
            }
            var combinationNote = heHui.Count > 0
                ? "；合會與刑沖破害同時保留，是否減輕或加重須依完整命局另行核對"
                : string.Empty;
            IList<IGanZhi> threeXingInteraction;
            IList<IGanZhi> twoXingInteraction;
            int idx;
            // 無恩之刑 = 寅巳申 (外在, 力度強)
            if ((threePair != null)
                && TryFindInteraction(threePair, BaZiDefine.ThreeXing[0], item => item.Zhi, 3, true, out threeXingInteraction, out idx)) {
                var msg = "談判破局、反覆失誤、衝動行事";
                msg += combinationNote;
                CreateThreeXingDesc(threeXingInteraction, msg, "三思後行、注意車關", html);
                bad = true;
            } else if (TryFindInteraction(twoPair, BaZiDefine.ThreeXing[0], item => item.Zhi, 2, true, out twoXingInteraction, out idx)) {
                var msg = "談判受挫、容易失誤、容易衝動";
                msg += combinationNote;
                CreateTwoXingDesc(twoXingInteraction, msg, "三思後行", "強", html);
                bad = true;
            }
            // 恃勢之刑 = 丑戌未 (外在, 力度強)
            if ((threePair != null)
                && TryFindInteraction(threePair, BaZiDefine.ThreeXing[1], item => item.Zhi, 3, true, out threeXingInteraction, out idx)) {
                var msg = (idx == 0)
                    ? "與父母緣薄、協助調停家庭內鬥或房產糾紛、捲入官司 (心中糾結累積、不開心)"
                    : "與子女緣薄、子女紛爭、房產糾紛 (心中糾結累積、不開心)";
                msg += combinationNote;
                CreateThreeXingDesc(threeXingInteraction, msg, "注意車關、不動產問題", html);
                bad = true;
            } else if (TryFindInteraction(twoPair, BaZiDefine.TwoXing[1], item => item.Zhi, 2, true, out twoXingInteraction, out idx)) {
                var msg = idx switch {
                    0 => "家族問題、祖業糾紛 (心中糾結累積、不開心)",
                    1 => "與父母關係較差、協助調停家庭內鬥或房產糾紛、捲入官司 (心中糾結累積、不開心)",
                    2 => "與另一半關係較差、容易有口角糾紛 (心中糾結累積、不開心)",
                    3 => "與子女關係較差、容易有口角糾紛 (心中糾結累積、不開心)",
                    _ => "與家人緣薄、家庭內鬥、房產糾紛 (心中糾結累積、不開心)"
                };
                msg += combinationNote;
                CreateTwoXingDesc(twoXingInteraction, msg, "注意情緒性用詞", "強", html);
                bad = true;
            }
            // 恩愛之刑 = 子卯 (外在, 力度小)
            if (TryFindInteraction(twoPair, [DiZhi.Zi, DiZhi.Mao], item => item.Zhi, 2, true, out twoXingInteraction, out idx)) {
                var msg = idx switch {
                    0 => "與長輩較容易想法不同",
                    1 => "與父母較容易想法不同",
                    2 => "與另一半較容易糾結、感情爭執",
                    3 => "與子女較容易想法不同",
                    _ => "感情糾紛、口角"
                };
                msg += combinationNote;
                CreateTwoXingDesc(twoXingInteraction, msg, "注意溝通", "小", html);
                bad = true;
            }
            // 自刑 = 辰辰、午午、酉酉、亥亥 (內在)
            if (TryFindSelfInteraction(twoPair, BaZiDefine.SelfXing, out var selfXingInteraction, out idx)) {
                var msg = "內耗、情緒糾結、鑽牛角尖";
                msg += combinationNote;
                CreateSelfXingDesc(selfXingInteraction, msg, html);
                bad = true;
            }
            // 沖 = 子午、丑未、寅申、卯酉、辰戌、巳亥
            if (TryFindAnyInteraction(twoPair, BaZiDefine.Chong, item => item.Zhi, 2, true, out var pairInteraction, out idx)) {
                var msg = idx switch {
                    0 => "跟長輩聚少離多、易口角衝突",
                    1 => "跟父母聚少離多、晚婚、易口角衝突、住所不穩定",
                    2 => "跟另一半聚少離多、易口角衝突、事情易突發變動",
                    3 => "跟子女聚少離多、易口角衝突",
                    _ => "衝突、車關、變動、突發、搬遷"
                };
                msg += combinationNote;
                CreateChongDesc(pairInteraction, msg, html);
                bad = true;
            }
            // 破 = 寅亥、巳申、子酉、午卯、戌未、丑辰
            if (TryFindAnyInteraction(twoPair, BaZiDefine.Po, item => item.Zhi, 2, true, out pairInteraction, out idx)) {
                var msg = "做事有阻力、破局、容易受傷";
                msg += combinationNote;
                CreatePoDesc(pairInteraction, msg, html);
                bad = true;
            }
            // 害 = 卯辰、寅巳、午丑、子未、酉戌、申亥
            if (TryFindAnyInteraction(twoPair, BaZiDefine.Hai, item => item.Zhi, 2, true, out pairInteraction, out idx)) {
                var msg = idx switch {
                    0 => "人際壓力、與長輩相處易有摩擦、情緒不悅",
                    1 => "人際壓力、與父母相處易有摩擦、情緒不悅",
                    2 => "人際壓力、與另一半相處易有摩擦、情緒不悅",
                    3 => "人際壓力、與子女相處易有摩擦、情緒不悅",
                    _ => "傷害、人際壓力、影響父母/夫妻/子女關係、情緒不悅、易摩擦"
                };
                msg += combinationNote;
                CreateHaiDesc(pairInteraction, msg, html);
                bad = true;
            }
            return (bad, heHui);
        }

        private static IList<IList<IGanZhi>> RemoveHourPillarFromSources(
            BaZiInfo info,
            IList<IList<IGanZhi>> sources
        ) {
            return sources
                .Select(source => (IList<IGanZhi>)source
                    .Where(item => !ReferenceEquals(item, info.HourZhu))
                    .ToList())
                .ToList();
        }

        public bool HasSelfConflict(BaZiInfo info, out MarkupString desc) {
            /* 本身命盤只檢查地支的相刑。三刑是 (日+月|日+時)+(大運/流年)，天干五合只有在流年時判斷 */
            var twoPair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.DayZhu, info.MonthZhu },
            new List<IGanZhi>() { info.DayZhu, info.HourZhu }
        };
            var html = new System.Text.StringBuilder();
            html.AppendLine("<div class=\"card analysis-card p-3 mt-3\">");
            html.AppendLine("    <h5 class=\"card-title analysis-card-title\">本身的衝突</h5>");
            html.AppendLine("    <hr class=\"analysis-card-divider\" aria-hidden=\"true\" />");
            var (bad, _) = CheckConflict(info, null, null, twoPair, info.LikeWuXing.Contains, html);
            html.AppendLine("</div>");
            desc = new MarkupString(html.ToString());
            return bad;
        }

        private bool HasDaYunConflict(BaZiInfo info, DaYun daYun, System.Text.StringBuilder html) {
            /* 這邊是判斷大運，所以最後一個一定要放大運，在 CheckConflict 裡面會有 mustLast，也就是跟大運有合、有刑沖破害的才會記錄 */
            var threePair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.DayZhu, info.MonthZhu, daYun },
            new List<IGanZhi>() { info.DayZhu, info.HourZhu, daYun }
        };
            var twoPair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.YearZhu, daYun },
            new List<IGanZhi>() { info.MonthZhu, daYun },
            new List<IGanZhi>() { info.DayZhu, daYun },
            new List<IGanZhi>() { info.HourZhu, daYun }
        };
            var (bad, _) = CheckConflict(info, null, threePair, twoPair, info.LikeWuXing.Contains, html);
            return bad;
        }

        private (bool bad, IDictionary<HeHui, WuXing> heHui) HasNianConflict(BaZiInfo info, DaYun daYun, LiuNian ln, System.Text.StringBuilder html) {
            /* 如果當前沒有大運，直接離開。可能是系統出錯或排盤有誤 */
            if (daYun is null)
                return (false, new Dictionary<HeHui, WuXing>());
            /* 這邊是判斷大運，所以最後一個一定要放流年，在 CheckConflict 裡面會有 mustLast，也就是跟流年有合、有刑沖破害的才會記錄
            * 天干部分，有可能會是大運干+流年干合，這樣就有可能形成忌神被合走 */
            var ganPair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.YearZhu, daYun, ln },
            new List<IGanZhi>() { info.MonthZhu, daYun, ln },
            new List<IGanZhi>() { info.DayZhu, daYun, ln },
            new List<IGanZhi>() { info.HourZhu, daYun, ln }
        };
            var threePair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.DayZhu, info.MonthZhu, daYun, ln },
            new List<IGanZhi>() { info.DayZhu, info.HourZhu, daYun, ln }
        };
            var twoPair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.YearZhu, ln },
            new List<IGanZhi>() { info.MonthZhu, ln },
            new List<IGanZhi>() { info.DayZhu, ln },
            new List<IGanZhi>() { info.HourZhu, ln },
            new List<IGanZhi>() { daYun, ln }
        };
            var daYunContext = EvaluateDaYunContext(info, daYun, ln.Year);
            return CheckConflict(
                info,
                ganPair,
                threePair,
                twoPair,
                element => IsEffectivePeriodFavorable(info, daYunContext, element.ToShiShen(info.RiZhu)),
                html
            );
        }

        private (bool bad, IDictionary<HeHui, WuXing> heHui) HasYueConflict(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            LiuYue liuYue,
            System.Text.StringBuilder html
        ) {
            var ganPair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.YearZhu, daYun, liuNian, liuYue },
            new List<IGanZhi>() { info.MonthZhu, daYun, liuNian, liuYue },
            new List<IGanZhi>() { info.DayZhu, daYun, liuNian, liuYue },
            new List<IGanZhi>() { info.HourZhu, daYun, liuNian, liuYue }
        };
            var threePair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.DayZhu, info.MonthZhu, daYun, liuNian, liuYue },
            new List<IGanZhi>() { info.DayZhu, info.HourZhu, daYun, liuNian, liuYue }
        };
            var twoPair = new List<IList<IGanZhi>> {
            new List<IGanZhi>() { info.YearZhu, liuYue },
            new List<IGanZhi>() { info.MonthZhu, liuYue },
            new List<IGanZhi>() { info.DayZhu, liuYue },
            new List<IGanZhi>() { info.HourZhu, liuYue },
            new List<IGanZhi>() { daYun, liuYue },
            new List<IGanZhi>() { liuNian, liuYue }
        };
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            return CheckConflict(
                info,
                ganPair,
                threePair,
                twoPair,
                element => IsEffectivePeriodFavorable(info, daYunContext, element.ToShiShen(info.RiZhu)),
                html
            );
        }

        private void CreateYunDesc(BaZiInfo info, ShiShen yun, System.Text.StringBuilder html) {
            html.Append("<span>");
            /* 依照格局來判斷是否好運 */
            if (info.StrengthStatus == GeJu.ShenQiang) {
                switch (yun) {
                    case ShiShen.PianCai:
                    case ShiShen.ZhengCai:
                        if (info.Gender == Sex.Male) {
                            html.Append("易得財、姻緣桃花佳(正緣)，是個 <strong class=\"text-success\">好運</strong>");
                        } else {
                            html.Append("易得財，是個 <strong class=\"text-success\">好運</strong>");
                        }
                        break;
                    case ShiShen.ZhengGuan:
                    case ShiShen.QiSha:
                        if (info.Gender == Sex.Male) {
                            html.Append("工作運好、受賞識升職，是個 <strong class=\"text-success\">好運</strong>");
                        } else {
                            html.Append("工作運好、受賞識升職、姻緣桃花佳(正緣、旺夫)，是個 <strong class=\"text-success\">好運</strong>");
                        }
                        break;
                    case ShiShen.ShihShen:
                    case ShiShen.ShangGuan:
                        if (info.Gender == Sex.Male) {
                            html.Append("容易出名、才華被看見、文昌(考試)運好，是個 <strong class=\"text-success\">好運</strong>");
                        } else {
                            html.Append("容易出名、才華被看見、文昌(考試)運好、易懷孕、育兒教兒好，是個 <strong class=\"text-success\">好運</strong>");
                        }
                        break;
                    case ShiShen.PianYin:
                    case ShiShen.ZhengYin:
                        html.Append("女性長輩的壓力、女性長輩身體不好、思考較弱、創意不佳、員工/小孩容易出狀況，是個 <strong class=\"text-warning\">相對較差的運</strong>");
                        break;
                    case ShiShen.BiJian:
                    case ShiShen.JieCai:
                        if (info.Gender == Sex.Male) {
                            html.Append("朋友手足關係煩惱、犯小人、因朋友惹禍、財運差、姻緣桃花差(無/爛桃花)，是個 <strong class=\"text-danger\">壞運</strong>");
                        } else {
                            html.Append("朋友手足關係煩惱、犯小人、因朋友惹禍，是個 <strong class=\"text-danger\">壞運</strong>");
                        }
                        break;
                    default:
                        throw new System.ComponentModel.InvalidEnumArgumentException(nameof(yun), (int)yun, typeof(ShiShen));
                }
            } else if (info.StrengthStatus == GeJu.ShenRuo) {
                /* 計算哪個十神比較多 */
                var allStars = new List<ShiShen>() {
                info.YearZhu.ZhuXing,
                info.MonthZhu.ZhuXing,
                info.HourZhu.ZhuXing
            };
                allStars.AddRange(info.YearZhu.FuXing);
                allStars.AddRange(info.MonthZhu.FuXing);
                allStars.AddRange(info.DayZhu.FuXing);
                allStars.AddRange(info.HourZhu.FuXing);
                var counts = new Dictionary<ShiShen, int>() {
                { ShiShen.ZhengCai, allStars.Count(s => (s == ShiShen.ZhengCai) || (s == ShiShen.PianCai))},
                { ShiShen.ZhengGuan, allStars.Count(s => (s == ShiShen.ZhengGuan) || (s == ShiShen.QiSha))},
                { ShiShen.ShihShen, allStars.Count(s => (s == ShiShen.ShihShen) || (s == ShiShen.ShangGuan))}
            }.OrderByDescending(kvp => kvp.Value).First();
                /* 依照十神多的去判斷優劣 */
                if (counts.Key == ShiShen.ZhengCai) {
                    // 正財偏財多
                    switch (yun) {
                        case ShiShen.PianYin:
                        case ShiShen.ZhengYin:
                        case ShiShen.BiJian:
                        case ShiShen.JieCai:
                            if (info.Gender == Sex.Male) {
                                html.Append("得財容易、受男性長輩幫助、姻緣桃花佳(正緣)、婚姻順利，是個 <strong class=\"text-success\">好運</strong>");
                            } else {
                                html.Append("得財容易、受男性長輩幫助、婚姻順利，是個 <strong class=\"text-success\">好運</strong>");
                            }
                            break;
                        case ShiShen.ZhengCai:
                        case ShiShen.PianCai:
                            if (info.Gender == Sex.Male) {
                                html.Append("破財、因財惹禍、男性長輩易給壓力、感情不順，是個 <strong class=\"text-danger\">壞運</strong>");
                            } else {
                                html.Append("破財、因財惹禍、男性長輩易給壓力，是個 <strong class=\"text-danger\">壞運</strong>");
                            }
                            break;
                        case ShiShen.ZhengGuan:
                        case ShiShen.QiSha:
                            html.Append("工作壓力大、官司纏身，是個 <strong class=\"text-danger\">壞運</strong>");
                            break;
                        case ShiShen.ShihShen:
                        case ShiShen.ShangGuan:
                            html.Append("想太多、鑽牛角尖、精神狀態差、禍從口出，是個 <strong class=\"text-danger\">壞運</strong>");
                            break;
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(yun), (int)yun, typeof(ShiShen));
                    }
                } else if (counts.Key == ShiShen.ZhengGuan) {
                    // 正官七殺多
                    switch (yun) {
                        case ShiShen.PianYin:
                        case ShiShen.ZhengYin:
                        case ShiShen.BiJian:
                        case ShiShen.JieCai:
                            if (info.Gender == Sex.Male) {
                                html.Append("升官、工作順利、貴人相助、子息運好，是個 <strong class=\"text-success\">好運</strong>");
                            } else {
                                html.Append("升官、工作順利、貴人相助、姻緣加分，是個 <strong class=\"text-success\">好運</strong>");
                            }
                            break;
                        case ShiShen.ZhengCai:
                        case ShiShen.PianCai:
                            if (info.Gender == Sex.Male) {
                                html.Append("破財、因財惹禍、男性長輩易給壓力、感情不順，是個 <strong class=\"text-danger\">壞運</strong>");
                            } else {
                                html.Append("破財、因財惹禍、男性長輩易給壓力、婚姻感情困擾，是個 <strong class=\"text-danger\">壞運</strong>");
                            }
                            break;
                        case ShiShen.ZhengGuan:
                        case ShiShen.QiSha:
                            html.Append("工作壓力大、官司纏身，是個 <strong class=\"text-danger\">壞運</strong>");
                            break;
                        case ShiShen.ShihShen:
                        case ShiShen.ShangGuan:
                            html.Append("想太多、鑽牛角尖、精神狀態差、禍從口出，是個 <strong class=\"text-danger\">壞運</strong>");
                            break;
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(yun), (int)yun, typeof(ShiShen));
                    }
                } else if (counts.Key == ShiShen.ShihShen) {
                    // 正官七殺多
                    switch (yun) {
                        case ShiShen.PianYin:
                        case ShiShen.ZhengYin:
                        case ShiShen.BiJian:
                        case ShiShen.JieCai:
                            if (info.Gender == Sex.Male) {
                                html.Append("易得功名、得到好的下屬，是個 <strong class=\"text-success\">好運</strong>");
                            } else {
                                html.Append("易得功名、得到好的下屬、子息運好，是個 <strong class=\"text-success\">好運</strong>");
                            }
                            break;
                        case ShiShen.ZhengCai:
                        case ShiShen.PianCai:
                            if (info.Gender == Sex.Male) {
                                html.Append("破財、因財惹禍、男性長輩易給壓力、感情不順，是個 <strong class=\"text-danger\">壞運</strong>");
                            } else {
                                html.Append("破財、因財惹禍、男性長輩易給壓力、婚姻感情困擾，是個 <strong class=\"text-danger\">壞運</strong>");
                            }
                            break;
                        case ShiShen.ZhengGuan:
                        case ShiShen.QiSha:
                            html.Append("工作壓力大、官司纏身，是個 <strong class=\"text-danger\">壞運</strong>");
                            break;
                        case ShiShen.ShihShen:
                        case ShiShen.ShangGuan:
                            html.Append("想太多、鑽牛角尖、精神狀態差、禍從口出，是個 <strong class=\"text-danger\">壞運</strong>");
                            break;
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(yun), (int)yun, typeof(ShiShen));
                    }
                }
            } else if (info.StrengthStatus == GeJu.CongQiang) {
                switch (yun) {
                    case ShiShen.PianYin:
                    case ShiShen.ZhengYin:
                    case ShiShen.BiJian:
                    case ShiShen.JieCai:
                        html.Append("運勢加分、做事整體順暢，是個 <strong class=\"text-success\">好運</strong>");
                        break;
                    case ShiShen.ZhengCai:
                    case ShiShen.PianCai:
                    case ShiShen.ZhengGuan:
                    case ShiShen.QiSha:
                    case ShiShen.ShihShen:
                    case ShiShen.ShangGuan:
                        html.Append("破格、運勢相對不順，是個 <strong class=\"text-warning\">相對較差的運</strong>");
                        break;
                    default:
                        throw new System.ComponentModel.InvalidEnumArgumentException(nameof(yun), (int)yun, typeof(ShiShen));
                }
            } else if (info.StrengthStatus == GeJu.CongRuo) {
                switch (yun) {
                    case ShiShen.PianYin:
                    case ShiShen.ZhengYin:
                    case ShiShen.BiJian:
                    case ShiShen.JieCai:
                        html.Append("破格、運勢相對不順，是個 <strong class=\"text-warning\">相對較差的運</strong>");
                        break;
                    case ShiShen.ZhengCai:
                    case ShiShen.PianCai:
                    case ShiShen.ZhengGuan:
                    case ShiShen.QiSha:
                    case ShiShen.ShihShen:
                    case ShiShen.ShangGuan:
                        html.Append("運勢加分、做事整體順暢，是個 <strong class=\"text-success\">好運</strong>");
                        break;
                    default:
                        throw new System.ComponentModel.InvalidEnumArgumentException(nameof(yun), (int)yun, typeof(ShiShen));
                }
            }
            html.Append("</span>");
        }

        private string CreatePeriodByGoodYun(Sex gender, ShiShen ganYun, ShiShen zhiYun, bool qiang) {
            /* 因 ganYun 跟 zhiYun 是 `|` 過的兩個十神 */
            switch (ganYun) {
                case ShiShen.Cai: {
                    switch (zhiYun) {
                        case ShiShen.Cai:   //兩個都是財運。身強/從弱為好流年；身弱/從強為壞流年
                            if (qiang) {
                                return (gender == Sex.Male) ? "得財機會多、感情運好" : "得財機會多";
                            } else {
                                return (gender == Sex.Male) ? "破財、因財惹禍、男性長輩易給壓力、感情不順" : "破財、因財惹禍、男性長輩易給壓力";
                            }
                        case ShiShen.GuanSha:   //(財)(官殺)身強/從弱為好流年；身弱/從強為壞流年。  財的事、工作果
                            if (qiang) {
                                return "事業攀升，名利齊來。代表這一年你的經濟實力足以撐起你的野心，容易獲得實權職位，或是創業規模擴大並獲得社會認可";
                            } else {
                                return (gender == Sex.Male) ? "容易因為貪財、貪玩或過度追求物質而引發身體疾病、法律糾紛或職場壓迫。可能因為感情問題而導致事業受損或名譽掃地。這一年應「棄財保身」，不宜承擔過度風險" : "容易因為貪財、貪玩或過度追求物質而引發身體疾病、法律糾紛或職場壓迫。這一年應「棄財保身」，不宜承擔過度風險";
                            }
                        case ShiShen.ShihShang: //(財)(食傷)身強/從弱為好流年；身弱/從強為壞流年。  財的事、才華果
                            if (qiang) {
                                return "你的創意與技術非常有價值，投資眼光精準，適合開發新市場、推出新產品，金錢收益會超出預期";
                            } else {
                                return "心有餘而力不足。雖然有很多賺錢的想法，但實際執行起來卻體力不支或資金斷線。容易發生「財多身弱」，看得到賺不到，或為了賺錢而嚴重損害健康";
                            }
                        case ShiShen.BiJie: //(財)身強/從弱為好流年；身弱/從強為壞流年。(比劫)身強/從弱為壞流年；身弱/從強為好流年。  財的事、朋友果
                            if (qiang) {
                                return "容易發生朋友借錢不還、被小人劫財、或是感情上出現競爭對手。這一年應避免任何形式的擔保、借貸，也不宜進行大額合資";
                            } else {
                                return "這一年適合與他人合作，雖然需要將獲利分出去，但整體進帳會比自己單幹更多。這也代表能透過人際關係獲得賺錢的消息";
                            }
                        case ShiShen.Yin:   //(財)身強/從弱為好流年；身弱/從強為壞流年。(印)身強/從弱為壞流年；身弱/從強為好流年。  財的事、長輩果
                            if (qiang) {
                                return "你能將腦中的想法付諸實現，或是透過房地產、長輩資助、買賣合約獲得實質收益。這是一種將「虛名」轉化為「實財」的過程";
                            } else {
                                return "名譽受損，學業/健康受阻。容易因為貪小便宜、短視近利而做出違背原則的事，導致長輩不滿、職位不保或合約糾紛。對於學生來說，代表因玩樂而荒廢學業";
                            }
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(zhiYun), (int)zhiYun, typeof(ShiShen));
                    }
                }
                case ShiShen.GuanSha: {
                    switch (zhiYun) {
                        case ShiShen.Cai:   //(官殺)(財)身強/從弱為好流年；身弱/從強為壞流年。  工作的事、財果
                            if (qiang) {
                                return "名利雙收、經濟實力能轉化為社會地位，容易獲得升遷、掌權，或是創業成功並建立良好的品牌聲望";
                            } else {
                                return "容易發生「因錢招災」或「過勞生病」。在職場上可能為了業績而承擔超過負荷的責任，導致身心俱疲。這一年不宜投機，應保守求穩";
                            }
                        case ShiShen.GuanSha:   // 兩個都是官殺。身強/從弱為好流年；身弱/從強為壞流年
                            if (qiang) {
                                return (gender == Sex.Male) ? "工作運佳(被賞識、機會多)、子息運好" : "工作運佳(被賞識、機會多)、桃花感情好";
                            } else {
                                return (gender == Sex.Male) ? "工作壓力大、官司纏身" : "工作壓力大、官司纏身、感情容易有爛桃花";
                            }
                        case ShiShen.ShihShang: //(官殺)(食傷)身強/從弱為好流年；身弱/從強為壞流年。  工作的事、才華果
                            if (qiang) {
                                return "大刀闊斧、威震四方，能用智慧和手段化解難題，在競爭中取得勝利。這一年你的表現會極具攻擊性且有成效，能開創出屬於自己的局面";
                            } else {
                                return "極度焦慮與是非。容易在極大壓力下做出衝動的錯誤判斷，或是因為口舌是非惹上官司。要注意官司糾紛或被職場霸凌。這一年最忌諱與體制對抗，應低調尋求長輩的幫助";
                            }
                        case ShiShen.BiJie: //(官殺)身強/從弱為好流年；身弱/從強為壞流年。(比劫)身強/從弱為壞流年；身弱/從強為好流年。  工作的事、朋友果
                            if (qiang) {
                                return "能在團隊或同儕競爭中脫穎而出，獲得領導權或管理權。這是一個建立名譽、整頓的絕佳時機";
                            } else {
                                return "容易遇到「遇人不淑」的情況。雖然身邊有朋友，但大家一起面對困難時容易互相拖累，甚至因為利益分配不均而反目。這一年應防範朋友帶來的連累";
                            }
                        case ShiShen.Yin:   //(官殺)身強/從弱為好流年；身弱/從強為壞流年。(印)身強/從弱為壞流年；身弱/從強為好流年。  工作的事、長輩果
                            if (qiang) {
                                return "名大於實、停滯不前。雖然名聲好聽或職位高，但其實只是虛職，且會讓你變得更加固執、不願變通。容易陷入「自以為是」的陷阱，或是被各種虛名所累，反而限制了發展空間";
                            } else {
                                return "逢凶化吉、手握實權、壓力轉化為名譽，原本艱難的任務反而讓你獲得長輩或上司的信任，進而獲得升遷、學位或權力。這是一個非常利於考試與職涯晉升的年份";
                            }
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(zhiYun), (int)zhiYun, typeof(ShiShen));
                    }
                }
                case ShiShen.ShihShang: {
                    switch (zhiYun) {
                        case ShiShen.Cai:   //(食傷)(財)身強/從弱為好流年；身弱/從強為壞流年。  才華的事、財果
                            if (qiang) {
                                return "財源廣進。這是一個非常適合創業、開發新產品、業務擴張的年份。你的點子能精準變現，投資眼光也較準確";
                            } else {
                                return "容易發生「財多身弱」的情況。看得到吃不到，或是為了賺錢而把身體搞垮。容易為了利益而奔波勞累，卻留不住財富";
                            }
                        case ShiShen.GuanSha:   //(食傷)(官殺)身強/從弱為好流年；身弱/從強為壞流年。  才華的事、工作果
                            if (qiang) {
                                return "你有能力解決別人解決不了的難題，能抗住壓力並反制對手。這是一個破繭而出、獲得社會地位提升、展現領導魄力的年份";
                            } else {
                                return "容易因為一時口快或行為不檢點而觸犯法規、惹怒上司，導致工作丟失或名譽受損。這一年務必謹言慎行，低調做人";
                            }
                        case ShiShen.ShihShang: // 兩個都是食傷。身強/從弱為好流年；身弱/從強為壞流年
                            if (qiang) {
                                return (gender == Sex.Male) ? "才華被看見、被肯定、有機會出名、文昌(考試)運好" : "才華被看見、被肯定、有機會出名、文昌(考試)運好、子息運好(容易懷孕、教子佳)";
                            } else {
                                return (gender == Sex.Male) ? "想太多、鑽牛角尖、精神狀態差、禍從口出" : "想太多、鑽牛角尖、精神狀態差、禍從口出、子息運差(不易懷孕、孩子叛逆)";
                            }
                        case ShiShen.BiJie: //(食傷)身強/從弱為好流年；身弱/從強為壞流年。(比劫)身強/從弱為壞流年；身弱/從強為好流年。  工作的事、朋友果
                            if (qiang) {
                                return "適合技術合作或團體表演。你的影響力會透過朋友或社群擴散出去。行動力極強，只要想做的目標，通常能靠著團隊力量推動成功";
                            } else {
                                return "容易受朋友煽動而去做一些消耗自己的事。雖然有人陪你一起忙，但最終成果有限，且會讓你感到疲憊不堪";
                            }
                        case ShiShen.Yin:   //(食傷)身強/從弱為好流年；身弱/從強為壞流年。(印)身強/從弱為壞流年；身弱/從強為好流年。  工作的事、長輩果
                            if (qiang) {
                                return "懷才不遇、悶悶不樂。容易感到被束縛，說話沒人聽，或是計畫被臨時喊卡。這一年要注意心理健康，避免鑽牛角尖，且要防範長輩或合約帶來的困擾";
                            } else {
                                return "這一年你會變得冷靜，不再盲目衝動。雖然可能會停止一些計畫，但這是為了更好的調整。在學習與沉澱中，你會找到新的方向";
                            }
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(zhiYun), (int)zhiYun, typeof(ShiShen));
                    }
                }
                case ShiShen.BiJie: {
                    switch (zhiYun) {
                        case ShiShen.Cai:   //(比劫)身強/從弱為壞流年；身弱/從強為好流年。(財)身強/從弱為好流年；身弱/從強為壞流年。  朋友的事、財果
                            if (qiang) {
                                return "極易發生破財、受騙或投資失利。容易因為面子問題借錢給人而收不回來，或是感情上出現競爭者。這一年應嚴守錢袋，切忌合夥";
                            } else {
                                return "適合團隊合作或合夥創業。這一年會因為朋友、同事的引薦或協力而賺到錢。雖然賺到的錢需要分紅給他人，但整體收入會比自己單打獨鬥更多";
                            }
                        case ShiShen.GuanSha:   //(比劫)身強/從弱為壞流年；身弱/從強為好流年。(官殺)身強/從弱為好流年；身弱/從強為壞流年。  朋友的事、工作果
                            if (qiang) {
                                return "雖然競爭激烈，但你具備戰勝競爭者、獲得職位提升的機會。這是一個展現領導力、在眾多競爭者中脫穎而出的年份";
                            } else {
                                return "在艱難的環境中會遇到同舟共濟的戰友。雖然環境嚴苛、職責沉重，但因為有同事或朋友分擔，能平安度過危機";
                            }
                        case ShiShen.ShihShang: //(比劫)身強/從弱為壞流年；身弱/從強為好流年。(食傷)身強/從弱為好流年；身弱/從強為壞流年。  朋友的事、才華果
                            if (qiang) {
                                return "容易變為好大喜功或思慮過重。想法非常多但容易流於空談，或是因為過度自信而口無遮攔，導致口舌是非。這一年雖然很忙碌，但往往是瞎忙，成就感不高";
                            } else {
                                return "行動力增強，原本想做但不敢做的事，現在有了勇氣與體力去執行。雖然依舊辛苦，但產出會變多、勞而有獲";
                            }
                        case ShiShen.BiJie: // 兩個都是比劫。身強/從弱為壞流年；身弱/從強為好流年
                            if (qiang) {
                                return (gender == Sex.Male) ? "朋友手足關係煩惱、犯小人、因朋友惹禍、財運不佳、爛桃花" : "朋友手足關係煩惱、犯小人、因朋友惹禍、財運不佳";
                            } else {
                                return (gender == Sex.Male) ? "得財容易、受男性長輩幫助、姻緣桃花佳(正緣)、婚姻順利" : "得財容易、受男性長輩幫助、婚姻順利";
                            }
                        case ShiShen.Yin:   //(比劫)(印)身強/從弱為壞流年；身弱/從強為好流年。  朋友的事、長輩果
                            if (qiang) {
                                return "容易變得固執己見、剛愎自用。因為長輩的保護與自我都太強，會聽不進任何建議，導致人際關係孤立。這一年也容易出現懶散、不想工作的狀態，因為「能量太飽和」而失去前進的動力";
                            } else {
                                return "能量值補滿。非常適合學習、深造或創業初期的擴張。身體健康會改善，心情也會變得自信樂觀。這是一個蓄勢待發、廣積糧草的年份";
                            }
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(zhiYun), (int)zhiYun, typeof(ShiShen));
                    }
                }
                case ShiShen.Yin: {
                    switch (zhiYun) {
                        case ShiShen.Cai:   //(印)身強/從弱為壞流年；身弱/從強為好流年。(財)身強/從弱為好流年；身弱/從強為壞流年。  長輩的事、財果
                            if (qiang) {
                                return "能夠將腦中的想法轉化為實際的收益、學以致用、知識變現，或是透過買賣房地產、長輩資助而獲得財富";
                            } else {
                                return "容易因小失大，或是因為貪圖小利而毀掉名譽。在學業上容易因玩樂分心，在職場上則可能因為合約與金錢處理不當而產生糾紛";
                            }
                        case ShiShen.GuanSha:   //(印)身強/從弱為壞流年；身弱/從強為好流年。(官殺)身強/從弱為好流年；身弱/從強為壞流年。  長輩的事、工作果
                            if (qiang) {
                                return "容易感到懷才不遇。雖然有職位或名聲，但壓力巨大且缺乏實質收益。容易變得過於自負，聽不進建言，導致事業停滯或決策失誤";
                            } else {
                                return "容易獲得提拔、升遷或考運極佳。長輩與長官會成為你的貴人，幫助你化解危機";
                            }
                        case ShiShen.ShihShang: //(印)身強/從弱為壞流年；身弱/從強為好流年。(食傷)身強/從弱為好流年；身弱/從強為壞流年。  長輩的事、才華果
                            if (qiang) {
                                return "容易閉門造車、過度保守、不願變通，導致才華被埋沒";
                            } else {
                                return "容易有志難伸，想做的很多但實踐力不足。心靈層面容易感到壓抑、憂鬱，或是因為過度思考而錯失良機。要注意消化系統或健康上的小毛病";
                            }
                        case ShiShen.BiJie: //(印)(比劫)身強/從弱為壞流年；身弱/從強為好流年。  長輩的事、朋友果
                            if (qiang) {
                                return "容易出現「群眾分財」或「貴人變小人」的情況。長輩的關心變成壓力，朋友的出現是為了分你的資源或造成你的損耗。這一年應保持低調，避免大規模的社交與金錢借貸";
                            } else {
                                return "這是合作創業或尋求支援的好時機。長輩會引薦同輩來幫你，或是你與朋友共同學習某項技能。你會感到充滿底氣，不再孤軍奮戰";
                            }
                        case ShiShen.Yin:   //兩個都是印。身強/從弱為壞流年；身弱/從強為好流年
                            if (qiang) {
                                return "女性長輩的壓力、女性長輩身體不好、思考較弱、創意不佳、員工/小孩容易出狀況";
                            } else {
                                return (gender == Sex.Male) ? "得財容易、受男性長輩幫助、姻緣桃花佳(正緣)、婚姻順利" : "得財容易、受男性長輩幫助、婚姻順利";
                            }
                        default:
                            throw new System.ComponentModel.InvalidEnumArgumentException(nameof(zhiYun), (int)zhiYun, typeof(ShiShen));
                    }
                }
                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException(nameof(ganYun), (int)ganYun, typeof(ShiShen));
            }
        }

        private sealed record PeriodDirectionAssessment(
            ShiShen GanTenGod,
            ShiShen ZhiTenGod,
            IReadOnlyList<ShiShen> FavorableTenGods,
            IReadOnlyList<ShiShen> UnfavorableTenGods
        ) {
            public bool HasFavorable => FavorableTenGods.Count > 0;

            public bool HasUnfavorable => UnfavorableTenGods.Count > 0;
        }

        private DaYunFavorabilityContext EvaluateDaYunContext(BaZiInfo info, DaYun daYun, int targetYear) {
            return _periodFavorabilityService.EvaluateDaYun(info, daYun, targetYear);
        }

        private PeriodDirectionAssessment EvaluatePeriodDirection(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext,
            ShiShen ganTenGod,
            ShiShen zhiTenGod
        ) {
            var ganGroup = ganTenGod.ToCombined();
            var zhiGroup = zhiTenGod.ToCombined();
            var groups = new[] { ganGroup, zhiGroup }.Distinct().ToArray();
            return new PeriodDirectionAssessment(
                ganGroup,
                zhiGroup,
                groups.Where(group => IsEffectivePeriodFavorable(info, daYunContext, group)).ToArray(),
                groups.Where(group => !IsEffectivePeriodFavorable(info, daYunContext, group)).ToArray()
            );
        }

        private PeriodDirectionAssessment EvaluatePeriodDirection(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext,
            IGanZhi period
        ) {
            return EvaluatePeriodDirection(
                info,
                daYunContext,
                period.Gan.ToShiShen(info.DayZhu.Gan),
                period.Zhi.ToShiShen(info.DayZhu.Gan)
            );
        }

        private bool IsOriginalFavorable(BaZiInfo info, ShiShen tenGod) {
            return _periodFavorabilityService.IsNatalFavorable(info, tenGod);
        }

        private bool IsEffectivePeriodFavorable(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext,
            ShiShen tenGod
        ) {
            return _periodFavorabilityService.IsPeriodFavorable(info, daYunContext, tenGod);
        }

        private bool UsesExhaustingPeriodDirection(BaZiInfo info, DaYunFavorabilityContext daYunContext) {
            return _periodFavorabilityService.IsPeriodFavorable(info, daYunContext, ShiShen.Cai);
        }

        private string DescribeDaYunContext(
            BaZiInfo info,
            DaYunFavorabilityContext context
        ) {
            var phase = context.Phase == DaYunPhase.FirstFiveYears ? "前五年天干" : "後五年地支";
            var primary = FormatTenGod(info, context.PrimaryTenGod);
            var secondary = FormatTenGod(info, context.SecondaryTenGod);
            var interaction = context.PrimaryIsFavorable == context.SecondaryIsFavorable
                ? $"主次能量同屬{FormatFavorabilityLabel(context.PrimaryIsFavorable, "依本命格局判斷大運主次能量的喜忌方向")}"
                : $"主次一喜一忌，不能把整段大運寫成全好或全壞；目前仍以{phase}為主";
            return $"大運{phase}主作用為{primary}，次要背景為{secondary}；{interaction}。";
        }

        private string DescribePeriodDirection(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext,
            PeriodDirectionAssessment assessment,
            string periodLabel
        ) {
            var gan = FormatEffectiveTenGod(info, daYunContext, assessment.GanTenGod);
            var zhi = FormatEffectiveTenGod(info, daYunContext, assessment.ZhiTenGod);
            var result = (assessment.HasFavorable, assessment.HasUnfavorable) switch {
                (true, false) => "兩側皆在當期喜用方向",
                (false, true) => "兩側皆偏當期忌神方向",
                _ => "喜用與忌神訊號並存"
            };
            return $"{periodLabel}天干為{gan}、地支主氣為{zhi}，{result}。";
        }

        private string DescribeLayeredPeriodResult(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext,
            PeriodDirectionAssessment yearAssessment,
            PeriodDirectionAssessment currentAssessment,
            PeriodScope period
        ) {
            if (info.RequiresStrengthVerification) {
                return $"目前屬疑似{info.StrengthStatus.ToGeJuString()}，以下只依順勢／破格方向暫列；仍須用多個過往大運、流年與實際事件回驗後再定案。";
            }

            if (daYunContext.PrimaryIsFavorable) {
                if (!yearAssessment.HasUnfavorable && !currentAssessment.HasUnfavorable) {
                    return "大運主背景已有支撐，短期訊號也在可承接方向；可把握機會，但不代表結果保證。";
                }

                if (period == PeriodScope.LiuYue && yearAssessment.HasUnfavorable && currentAssessment.HasFavorable) {
                    return "大運仍有支撐，流年較有耗損，本月則提供短期窗口；宜縮小規模並設定停損、界線與檢核點。";
                }

                return "大運主背景在喜用方向，但短期同時有耗損或失衡訊號；仍可承接，宜控制負荷與風險。";
            }

            if (yearAssessment.HasFavorable || currentAssessment.HasFavorable) {
                return period == PeriodScope.LiuNian
                    ? "大運主背景仍偏忌神，流年只能提供短期補強；可小步調整，不宜把一年好轉解讀成十年背景已翻轉。"
                    : "大運主背景仍偏忌神；流年或流月雖有短期補強，也不宜反客為主，重大行動應保守。";
            }

            return "大運與短期訊號都偏失衡方向，宜保守安排、降低負荷，等待較有支撐的時間窗口。";
        }

        private void CreatePeriodDesc(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            ShiShen ganYun,
            ShiShen zhiYun,
            PeriodScope period,
            System.Text.StringBuilder html
        ) {
            var gy = ganYun.ToCombined();
            var zy = zhiYun.ToCombined();
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var yearAssessment = EvaluatePeriodDirection(info, daYunContext, liuNian);
            var currentAssessment = EvaluatePeriodDirection(info, daYunContext, gy, zy);
            var periodLabel = period == PeriodScope.LiuNian ? "流年" : "流月";
            var eventDescription = CreatePeriodByGoodYun(
                info.Gender,
                gy,
                zy,
                UsesExhaustingPeriodDirection(info, daYunContext)
            );

            html.AppendLine($"<span>{AdaptPeriodDescription(eventDescription, period)}</span>");
            html.AppendLine("<ul class=\"mb-0 mt-2\">");
            html.AppendLine($"    <li><strong>大運背景：</strong>{DescribeDaYunContext(info, daYunContext)}</li>");
            if (period == PeriodScope.LiuYue) {
                html.AppendLine($"    <li><strong>流年背景：</strong>{DescribePeriodDirection(info, daYunContext, yearAssessment, "流年")}</li>");
            }
            html.AppendLine($"    <li><strong>{periodLabel}判讀：</strong>{DescribePeriodDirection(info, daYunContext, currentAssessment, periodLabel)}</li>");
            html.AppendLine($"    <li><strong>綜合：</strong>{DescribeLayeredPeriodResult(info, daYunContext, yearAssessment, currentAssessment, period)}</li>");
            html.AppendLine("</ul>");
        }

        private static string AdaptPeriodDescription(string description, PeriodScope period) {
            if (period == PeriodScope.LiuNian)
                return description;

            return description
                .Replace("這一年", "這個月", StringComparison.Ordinal)
                .Replace("今年", "本月", StringComparison.Ordinal)
                .Replace("年份", "月份", StringComparison.Ordinal)
                .Replace("流年", "流月", StringComparison.Ordinal);
        }

        public MarkupString DaYunAnalysis(BaZiInfo info, int targetYear) {
            var (daYun, _) = FindLiuNian(info, targetYear);
            if (daYun is null)
                return new MarkupString();

            var html = new System.Text.StringBuilder();
            html.AppendLine("<div class=\"card analysis-card p-3 mt-3\">");
            html.AppendLine("    <h5 class=\"card-title analysis-card-title\">大運分析</h5>");
            html.AppendLine("    <hr class=\"analysis-card-divider\" aria-hidden=\"true\" />");
            /* 先列出是否有衝突 */
            var conflict = new System.Text.StringBuilder();
            var bad = HasDaYunConflict(info, daYun, conflict);
            if (bad) {
                html.AppendLine(@"    <div class=""mb-3"">");
                html.AppendLine(conflict.ToString());
                html.AppendLine(@"    </div>");
            }
            html.AppendLine(@"    <div class=""analysis-item mb-3"">");
            var daYunContext = EvaluateDaYunContext(info, daYun, targetYear);
            var phase = daYunContext.Phase;
            var selectedYun = daYun.GetPrimaryTenGod(info.DayZhu.Gan, targetYear);
            var secondaryYun = phase == DaYunPhase.FirstFiveYears
                ? daYun.Zhi.ToShiShen(info.DayZhu.Gan)
                : daYun.Gan.ToShiShen(info.DayZhu.Gan);
            var selectedYunDisplay = FormatTenGod(info, selectedYun, selectedYun.ToYunString());
            var secondaryYunDisplay = FormatTenGod(info, secondaryYun, secondaryYun.ToYunString());
            var ganDisplay = FormatElement(daYun.Gan.ToWuXing(), daYun.Gan.ToGanString());
            var zhiDisplay = FormatElement(daYun.Zhi.ToWuXing(), daYun.Zhi.ToZhiString());
            var phaseText = phase == DaYunPhase.FirstFiveYears
                ? $"前五年（{daYun.StartYear}～{Math.Min(daYun.StartYear + 4, daYun.EndYear)}）以天干為主"
                : $"後五年（{daYun.StartYear + 5}～{daYun.EndYear}）以地支為主";
            html.AppendLine("        <div class=\"analysis-item-main\">");
            html.AppendLine($"            <div><strong>{daYun.StartYear} 年～{daYun.EndYear} 年：</strong>{ganDisplay}{zhiDisplay}</div>");
            html.AppendLine($"            <div>{targetYear} 年位於{phaseText}，主要走 {selectedYunDisplay}；次要能量為 {secondaryYunDisplay}。</div>");
            html.AppendLine("        </div>");
            html.AppendLine("        <div class=\"alert alert-info analysis-item-explanation mb-0\">");
            CreateYunDesc(info, selectedYun, html);
            html.AppendLine($"<div class=\"mt-2\">{DescribeDaYunContext(info, daYunContext)}</div>");
            html.AppendLine("        </div>");
            html.AppendLine(@"    </div>");

            var nextDaYun = info.DaYunList.FirstOrDefault(x => x.StartYear > daYun.StartYear);
            if (nextDaYun != null && targetYear == nextDaYun.StartYear - 1) {
                html.AppendLine(@"    <div class=""analysis-item mb-3"">");
                var nextDaYunContext = EvaluateDaYunContext(info, nextDaYun, nextDaYun.StartYear);
                var nextYun = nextDaYun.GetPrimaryTenGod(info.DayZhu.Gan, nextDaYun.StartYear);
                var nextYunDisplay = FormatTenGod(info, nextYun, nextYun.ToYunString());
                html.Append($"        <div class=\"analysis-item-main\"><strong class=\"text-danger\">[明年切換大運]</strong> {nextDaYun.StartYear} 年～{nextDaYun.EndYear} 年；新大運前五年以天干為主，先走 {nextYunDisplay}。</div>");
                html.AppendLine("        <div class=\"alert alert-warning analysis-item-explanation mb-0\">");
                CreateYunDesc(info, nextYun, html);
                html.AppendLine($"<div class=\"mt-2\">{DescribeDaYunContext(info, nextDaYunContext)}</div>");
                html.AppendLine("        </div>");
                html.AppendLine(@"    </div>");
            }

            AppendTopicNotice(html, "大運前五年以天干為主，地支仍有作用；後五年以地支為主，天干作用較弱。流年天干與地支仍須一併判斷。");
            html.AppendLine(@"</div>");
            return new MarkupString(html.ToString());
        }

        public MarkupString LiuNianAnalysis(BaZiInfo info, int targetYear) {
            var (daYun, liuNian) = FindLiuNian(info, targetYear);
            if (daYun is null || liuNian is null)
                return new MarkupString();

            var html = GetPeriodHtml(
                info,
                daYun,
                liuNian,
                liuNian,
                $"{targetYear} 流年分析",
                $"{liuNian.Year} ",
                $" 年 ({liuNian.Age} 歲)",
                PeriodScope.LiuNian,
                conflict => HasNianConflict(info, daYun, liuNian, conflict)
            );
            return new MarkupString(html);
        }

        public MarkupString LiuYueAnalysis(BaZiInfo info, int targetYear, int targetMonthIndex) {
            var (daYun, liuNian) = FindLiuNian(info, targetYear);
            if (daYun is null || liuNian is null)
                return new MarkupString();

            var liuYue = liuNian.LiuYueList.FirstOrDefault(month => month.Index == targetMonthIndex);
            if (liuYue is null)
                return new MarkupString();

            var html = GetPeriodHtml(
                info,
                daYun,
                liuNian,
                liuYue,
                $"{targetYear} {liuYue.MonthInChinese}月流月分析",
                $"{targetYear} 年 {liuYue.MonthInChinese}月 ",
                "（節氣月）",
                PeriodScope.LiuYue,
                conflict => HasYueConflict(info, daYun, liuNian, liuYue, conflict)
            );
            return new MarkupString(html);
        }

        /// <summary>取得流年的財富事業、感情姻緣與健康重點</summary>
        public MarkupString LiuNianTopicAnalysis(BaZiInfo info, int targetYear) {
            var (daYun, liuNian) = FindLiuNian(info, targetYear);
            if (daYun is null || liuNian is null)
                return new MarkupString();

            return new MarkupString(GetTopicAnalysisHtml(
                info,
                daYun,
                liuNian,
                liuNian,
                PeriodScope.LiuNian
            ));
        }

        /// <summary>取得流月的財富事業、感情姻緣與健康重點</summary>
        public MarkupString LiuYueTopicAnalysis(BaZiInfo info, int targetYear, int targetMonthIndex) {
            var (daYun, liuNian) = FindLiuNian(info, targetYear);
            if (daYun is null || liuNian is null)
                return new MarkupString();

            var liuYue = liuNian.LiuYueList.FirstOrDefault(month => month.Index == targetMonthIndex);
            if (liuYue is null)
                return new MarkupString();

            return new MarkupString(GetTopicAnalysisHtml(
                info,
                daYun,
                liuNian,
                liuYue,
                PeriodScope.LiuYue
            ));
        }

        private string GetTopicAnalysisHtml(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period
        ) {
            var html = new System.Text.StringBuilder();
            AppendWealthCareerCard(info, daYun, liuNian, periodGanZhi, period, html);
            AppendRelationshipCard(info, daYun, liuNian, periodGanZhi, period, html);
            AppendChildTimingCard(info, daYun, liuNian, periodGanZhi, period, html);
            AppendHealthCard(info, daYun, liuNian, periodGanZhi, period, html);
            return html.ToString();
        }

        private void AppendWealthCareerCard(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period,
            System.Text.StringBuilder html
        ) {
            var ganElement = periodGanZhi.Gan.ToWuXing();
            var zhiElement = periodGanZhi.Zhi.ToWuXing();
            var wealthElement = BaZiDefine.Restricting[info.RiZhu];
            var outputElement = BaZiDefine.Generation[info.RiZhu];
            var careerElement = BaZiDefine.RestrictBy[info.RiZhu];
            var ganHasWealth = ganElement == wealthElement;
            var zhiHasWealth = CourseRuleCatalog.PeriodWealthBranches[info.RiZhu].Contains(periodGanZhi.Zhi);
            var ganHasOutput = ganElement == outputElement;
            var zhiHasOutput = zhiElement == outputElement;
            var ganHasCareer = ganElement == careerElement;
            var zhiHasCareer = CourseRuleCatalog.CareerBranches[info.RiZhu].Contains(periodGanZhi.Zhi);
            var periodLabel = period == PeriodScope.LiuNian ? "本年" : "本月";
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var currentAssessment = EvaluatePeriodDirection(info, daYunContext, periodGanZhi);
            var ganDisplay = FormatElement(ganElement, periodGanZhi.Gan.ToGanString());
            var zhiDisplay = FormatElement(zhiElement, periodGanZhi.Zhi.ToZhiString());
            var ganTenGod = FormatEffectiveTenGod(info, daYunContext, periodGanZhi.Gan.ToShiShen(info.DayZhu.Gan));
            var zhiTenGod = FormatEffectiveTenGod(info, daYunContext, periodGanZhi.Zhi.ToShiShen(info.DayZhu.Gan));
            var wealthStar = FormatEffectiveTenGod(info, daYunContext, ShiShen.Cai);
            var outputStar = FormatEffectiveTenGod(info, daYunContext, ShiShen.ShihShang);
            var careerStar = FormatEffectiveTenGod(info, daYunContext, ShiShen.GuanSha);
            var exhaustingSignalsAreFavorable = IsEffectivePeriodFavorable(
                info,
                daYunContext,
                ShiShen.Cai
            );
            var opportunityAlertClass = (currentAssessment.HasFavorable, currentAssessment.HasUnfavorable) switch {
                (true, false) => "alert-success",
                (false, true) => "alert-warning",
                _ => "alert-info"
            };

            html.AppendLine("<div class=\"card analysis-card p-3 mt-3\">");
            html.AppendLine("    <h5 class=\"card-title analysis-card-title\"><i class=\"fa-solid fa-coins me-2\"></i>財富與事業</h5>");
            html.AppendLine("    <hr class=\"analysis-card-divider\" aria-hidden=\"true\" />");
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine($"        <strong class=\"analysis-item-main\">{periodLabel}訊號</strong>");
            html.AppendLine("        <div class=\"alert alert-info analysis-item-explanation mb-0\">");
            html.AppendLine($"        <span><strong>{ganDisplay}{zhiDisplay}</strong>：天干為{ganTenGod}，地支主氣為{zhiTenGod}；你的財五行為{FormatElement(wealthElement)}（{wealthStar}）、事業五行為{FormatElement(careerElement)}（{careerStar}）。</span>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine("        <strong class=\"analysis-item-main\">機會判讀</strong>");
            html.AppendLine($"        <div class=\"alert {opportunityAlertClass} analysis-item-explanation mb-0\">");
            html.AppendLine("        <ul class=\"mb-0\">");

            if (ganHasWealth && zhiHasWealth && exhaustingSignalsAreFavorable) {
                html.AppendLine($"            <li>{periodLabel}天干、地支皆見{wealthStar}，且落在當期喜用方向，財務機會較集中；並非一定發財，仍要檢查現金流與風險。</li>");
            } else if (ganHasWealth && zhiHasWealth) {
                html.AppendLine($"            <li>{periodLabel}天干、地支皆見{wealthStar}，但落在當期忌神方向；財務議題會較集中，也可能表現為必要支出、負債或因財承壓，不可直接解讀為得財。</li>");
            } else if ((ganHasWealth || zhiHasWealth) && (ganHasOutput || zhiHasOutput)) {
                if (exhaustingSignalsAreFavorable) {
                    html.AppendLine($"            <li>{periodLabel}同見{wealthStar}與{outputStar}，形成「{outputStar}生{wealthStar}」，且為當期喜用方向；可把專業、作品或提案轉成小規模、可驗證的收入機會。</li>");
                } else {
                    html.AppendLine($"            <li>{periodLabel}形式上同見{outputStar}生{wealthStar}，但兩者皆為當期忌神方向；較可能是為財奔波、投入過量或身心負荷，不能單據「食傷生財」就評為可得財。</li>");
                }
            } else if (ganHasWealth || zhiHasWealth) {
                html.AppendLine(exhaustingSignalsAreFavorable
                    ? $"            <li>{periodLabel}見{wealthStar}，且為當期喜用方向，資源與金錢機會較容易被引動；這是機會，不等於必然獲利。</li>"
                    : $"            <li>{periodLabel}見{wealthStar}，但為當期忌神方向，金錢議題雖會被引動，也可能先表現為支出、負擔或財多身弱；不宜擴大風險。</li>");
            } else if (ganHasOutput || zhiHasOutput) {
                html.AppendLine(exhaustingSignalsAreFavorable
                    ? $"            <li>{periodLabel}以{outputStar}訊號為主，且為當期喜用方向；可透過技術、表達、內容或成果輸出間接帶動財。</li>"
                    : $"            <li>{periodLabel}以{outputStar}訊號為主，但為當期忌神方向；容易產出想法多、執行消耗大的狀況，先控制工作量，不宜將輸出直接推論成獲利。</li>");
            } else {
                html.AppendLine($"            <li>{periodLabel}未見明顯{wealthStar}或{outputStar}主訊號，表示近期財運較無特殊狀況，以正財穩定收入為主。</li>");
            }

            if (ganHasCareer && zhiHasWealth && exhaustingSignalsAreFavorable) {
                html.AppendLine($"            <li>{periodLabel}形成「{careerStar}天干 × {wealthStar}地支」工作訊號，可優先準備面試、提案、簽約或重要工作決策。</li>");
            } else if (ganHasCareer || zhiHasCareer) {
                html.AppendLine(exhaustingSignalsAreFavorable
                    ? $"            <li>{periodLabel}見{careerStar}工作訊號，職責、上司、客戶或外在要求可能增加；準備充分時可主動爭取機會。</li>"
                    : $"            <li>{periodLabel}見{careerStar}，但為當期忌神方向；較可能先表現為職責、上司、客戶或制度壓力，應先縮小承諾並確認資源。</li>");
            } else if ((ganHasWealth || zhiHasWealth) && exhaustingSignalsAreFavorable) {
                html.AppendLine($"            <li>{wealthStar}可生{careerStar}，若有轉職或升遷計畫，可把{periodLabel}列為候選窗口，再以職缺、能力與契約條件篩選。</li>");
            } else if (ganHasWealth || zhiHasWealth) {
                html.AppendLine($"            <li>{wealthStar}雖可生{careerStar}，但目前財星為當期忌神方向；金錢壓力可能連動工作負擔，不宜只因見財就推論升遷或轉職有利。</li>");
            } else {
                html.AppendLine($"            <li>{periodLabel}未見明顯{careerStar}工作訊號，表示近期工作上較無特殊狀況，宜穩定累積履歷、技能與可量化成果。</li>");
            }

            html.AppendLine("        </ul>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine("        <strong class=\"analysis-item-main\">承接與行動</strong>");
            html.AppendLine("        <div class=\"alert alert-info analysis-item-explanation mb-0\">");
            html.AppendLine($"        <span>{GetCapacityAdvice(info, daYun, liuNian, periodGanZhi, period)}</span>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            AppendTopicNotice(html, "財務與職涯內容為「意象」、「傾向」或「機會」，不應單獨作為投資、借貸、創業或轉職決策依據。");
            html.AppendLine("</div>");
        }

        private void AppendRelationshipCard(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period,
            System.Text.StringBuilder html
        ) {
            var ganElement = periodGanZhi.Gan.ToWuXing();
            var zhiElement = periodGanZhi.Zhi.ToWuXing();
            var spouseElement = info.Gender == Sex.Male
                ? BaZiDefine.Restricting[info.RiZhu]
                : BaZiDefine.RestrictBy[info.RiZhu];
            var spouseShiShen = info.Gender == Sex.Male ? ShiShen.Cai : ShiShen.GuanSha;
            var spouseBranches = SpouseBranches[(info.RiZhu, info.Gender)];
            var ganHasSpouse = ganElement == spouseElement;
            var zhiHasSpouse = spouseBranches.Contains(periodGanZhi.Zhi);
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var daYunGanHasSpouse = daYun.Gan.ToWuXing() == spouseElement;
            var daYunZhiHasSpouse = spouseBranches.Contains(daYun.Zhi);
            var daYunPrimaryHasSpouse = daYunContext.Phase == DaYunPhase.FirstFiveYears
                ? daYunGanHasSpouse
                : daYunZhiHasSpouse;
            var daYunSecondaryHasSpouse = daYunContext.Phase == DaYunPhase.FirstFiveYears
                ? daYunZhiHasSpouse
                : daYunGanHasSpouse;
            var sameZodiacGroup = period == PeriodScope.LiuNian
                && IsSameSanHeGroup(info.YearZhu.Zhi, periodGanZhi.Zhi);
            var periodLabel = period == PeriodScope.LiuNian ? "本年" : "本月";
            var ganDisplay = FormatElement(ganElement, periodGanZhi.Gan.ToGanString());
            var zhiDisplay = FormatElement(zhiElement, periodGanZhi.Zhi.ToZhiString());
            var birthYearBranch = FormatElement(info.YearZhu.Zhi.ToWuXing(), info.YearZhu.Zhi.ToZhiString());
            var spouseStar = FormatEffectiveTenGod(info, daYunContext, spouseShiShen);
            var peerStar = FormatEffectiveTenGod(info, daYunContext, ShiShen.BiJie);

            // 待核對：筆記 4-5 只明定「流年」夫妻星＋比劫為桃花危機，沒有把公式延伸到流月。
            // 因此流月只顯示夫妻星時機，不把相同組合直接判成感情競爭訊號。
            var spouseWithPeer = period == PeriodScope.LiuNian
                && ((ganHasSpouse && zhiElement == info.RiZhu)
                    || (ganElement == info.RiZhu && zhiHasSpouse));
            var spouseStarOnStem = spouseWithPeer && ganHasSpouse;

            html.AppendLine("<div class=\"card analysis-card p-3 mt-3\">");
            html.AppendLine("    <h5 class=\"card-title analysis-card-title\"><i class=\"fa-solid fa-heart me-2\"></i>感情姻緣</h5>");
            html.AppendLine("    <hr class=\"analysis-card-divider\" aria-hidden=\"true\" />");
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine($"        <strong class=\"analysis-item-main\">{periodLabel}桃花時機</strong>");
            html.AppendLine("        <div class=\"alert alert-info analysis-item-explanation mb-0\">");
            html.AppendLine($"        <span>{periodLabel}<strong>{ganDisplay}{zhiDisplay}</strong>；依{info.Gender.ToSexString()}命口徑，你的夫妻星為{FormatElement(spouseElement)}（{spouseStar}）。</span>");
            html.AppendLine("        <ul class=\"mb-0 mt-2\">");

            if (ganHasSpouse && zhiHasSpouse) {
                html.AppendLine($"            <li>{periodLabel}天干、地支皆見夫妻星（{spouseStar}），感情意願或關係議題較集中。</li>");
            } else if (ganHasSpouse || zhiHasSpouse) {
                html.AppendLine($"            <li>{periodLabel}一側見夫妻星（{spouseStar}），有桃花的機會出現；但仍決於雙方意願、關係基礎與相處狀況。</li>");
            } else {
                html.AppendLine($"            <li>{periodLabel}未見夫妻星（{spouseStar}），較無桃花狀況</li>");
            }

            if (daYunPrimaryHasSpouse) {
                var phaseLabel = daYunContext.Phase == DaYunPhase.FirstFiveYears ? "前五年天干" : "後五年地支";
                html.AppendLine($"            <li>目前大運的{phaseLabel}主作用見夫妻星（{spouseStar}），長期背景與短期訊號可一併觀察；不代表必然交往或結婚。</li>");
            } else if (daYunSecondaryHasSpouse) {
                var strongerPhase = daYunContext.Phase == DaYunPhase.FirstFiveYears ? "後五年地支" : "前五年天干";
                html.AppendLine($"            <li>此柱大運另一側含夫妻星（{spouseStar}），但目前不是主作用；以{strongerPhase}階段較為明顯。</li>");
            }

            if (sameZodiacGroup) {
                html.AppendLine($"            <li>流年地支{zhiDisplay}與出生年支{birthYearBranch}同屬一組生肖三合，可列為另一條桃花時間線；可期待但仍決於雙方意願、關係基礎與相處狀況。</li>");
            }

            html.AppendLine("        </ul>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");

            if (spouseWithPeer) {
                html.AppendLine("    <div class=\"alert alert-warning analysis-item-explanation mb-3\">");
                html.AppendLine($"        <strong>關係查證提醒：</strong>{periodLabel}同見夫妻星（{spouseStar}）與{peerStar}，表示競爭者、共同圈子或關係不透明的訊號{(spouseStarOnStem ? "，且夫妻星透在天干，訊號較為強烈" : "")}。有訊號不代表有實際作為，不可視為為劈腿或第三者的狀況，但可多留意彼此界線與關係。");
                html.AppendLine("    </div>");
            }

            var (raDesc, raAccept) = GetRelationshipAdvice(
                info,
                daYun,
                liuNian,
                periodGanZhi,
                period,
                ganHasSpouse || zhiHasSpouse
            );
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine("        <strong class=\"analysis-item-main\">承接與相處</strong>");
            html.AppendLine($"        <div class=\"alert {(raAccept ? "alert-success" : "alert-info")} analysis-item-explanation mb-0\">");
            if (raAccept) {
                html.AppendLine($"        <span>{raDesc} 單身者可增加安全且合意的認識機會；已有對象者可進一步討論關係或規劃未來，但仍需互相尊重，不能用日期向對方施壓。</span>");
            } else {
                html.AppendLine($"        <span>{raDesc}</span>");
            }
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            AppendTopicNotice(html, "感情判讀不代表必然遇見、結婚、分手或出現第三者，僅為一種訊號，仍須以實際關係來探討");
            html.AppendLine("</div>");
        }

        private void AppendChildTimingCard(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period,
            System.Text.StringBuilder html
        ) {
            if (period != PeriodScope.LiuNian) {
                return;
            }

            var childElement = info.Gender == Sex.Male
                ? BaZiDefine.RestrictBy[info.RiZhu]
                : BaZiDefine.Generation[info.RiZhu];
            var childStar = info.Gender == Sex.Male ? ShiShen.GuanSha : ShiShen.ShihShang;
            var ganHasChildStar = periodGanZhi.Gan.ToWuXing() == childElement;
            var zhiHasChildStar = periodGanZhi.Zhi.ToWuXing() == childElement;
            var daYunPhase = daYun.GetPhase(liuNian.Year);
            var daYunPrimaryElement = daYunPhase == DaYunPhase.FirstFiveYears
                ? daYun.Gan.ToWuXing()
                : daYun.Zhi.ToWuXing();
            var daYunHasChildStar = daYunPrimaryElement == childElement;
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var childStarText = FormatEffectiveTenGod(info, daYunContext, childStar);
            var childElementText = FormatElement(childElement);
            var ganDisplay = FormatElement(periodGanZhi.Gan.ToWuXing(), periodGanZhi.Gan.ToGanString());
            var zhiDisplay = FormatElement(periodGanZhi.Zhi.ToWuXing(), periodGanZhi.Zhi.ToZhiString());

            html.AppendLine("<div class=\"card analysis-card p-3 mt-3\">");
            html.AppendLine("    <h5 class=\"card-title analysis-card-title\"><i class=\"fa-solid fa-baby me-2\"></i>子女緣分</h5>");
            html.AppendLine("    <hr class=\"analysis-card-divider\" aria-hidden=\"true\" />");
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine($"        <strong class=\"analysis-item-main\">{liuNian.Year} 年子息星時機</strong>");
            html.AppendLine("        <div class=\"alert alert-info analysis-item-explanation mb-0\">");
            html.AppendLine($"        <span>依{info.Gender.ToSexString()}命口徑，子息星為{childStarText}，子息星五行為{childElementText}；流年為<strong>{ganDisplay}{zhiDisplay}</strong>。</span>");
            html.AppendLine("        <ul class=\"mb-0 mt-2\">");
            if (ganHasChildStar && zhiHasChildStar) {
                html.AppendLine($"            <li>流年天干與地支主氣都見{childStarText}，子女緣分與家庭規劃訊號較集中。</li>");
            } else if (ganHasChildStar || zhiHasChildStar) {
                html.AppendLine($"            <li>流年{(ganHasChildStar ? "天干透出" : "地支主氣見")}{childStarText}，可列為子女緣分與家庭規劃的觀察時間窗。</li>");
            } else {
                html.AppendLine($"            <li>流年天干與地支主氣未直接見{childStarText}，本年沒有明顯子息星時間窗。</li>");
            }
            if (daYunHasChildStar) {
                html.AppendLine($"            <li>目前大運{(daYunPhase == DaYunPhase.FirstFiveYears ? "前五年以天干" : "後五年以地支")}為主的階段也見{childStarText}，長期背景與流年訊號可一併觀察。</li>");
            }
            html.AppendLine("        </ul>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            if (!info.IsBirthTimeAccurate) {
                html.AppendLine("    <div class=\"alert alert-info analysis-item-explanation\">出生時辰不確定，時柱子息宮未納入；補齊準確時辰後，子息宮判讀可能改變。</div>");
            }
            AppendTopicNotice(html, "子息星只代表命理時間訊號，不保證懷孕、生育數量或療程結果；備孕、懷孕、分娩與輔助生殖技術必須依本人意願及生殖醫學專業評估。");
            html.AppendLine("</div>");
        }

        private void AppendHealthCard(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period,
            System.Text.StringBuilder html
        ) {
            var counts = CountChartElements(info);
            var minimumCount = counts.Values.Min();
            var weakElements = CourseRuleCatalog.ElementOrder.Where(element => counts[element] == minimumCount).ToArray();
            var countDescription = string.Join("、", CourseRuleCatalog.ElementOrder.Select(element => $"{FormatElement(element)} {counts[element]}"));
            var weakDescription = string.Join("；", weakElements.Select(element => $"{FormatElement(element)}：{CourseRuleCatalog.HealthParts[element]}"));
            var periodLabel = period == PeriodScope.LiuNian ? "本年" : "本月";
            var threePunishment = GetHealthThreePunishment(info, daYun, liuNian, periodGanZhi, period);
            var ganDisplay = FormatElement(periodGanZhi.Gan.ToWuXing(), periodGanZhi.Gan.ToGanString());
            var zhiDisplay = FormatElement(periodGanZhi.Zhi.ToWuXing(), periodGanZhi.Zhi.ToZhiString());

            html.AppendLine("<div class=\"card analysis-card p-3 mt-3\">");
            html.AppendLine("    <h5 class=\"card-title analysis-card-title\"><i class=\"fa-solid fa-heart-pulse me-2\"></i>健康注意事項</h5>");
            html.AppendLine("    <hr class=\"analysis-card-divider\" aria-hidden=\"true\" />");
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine("        <strong class=\"analysis-item-main\">本命五行初判</strong>");
            html.AppendLine("        <div class=\"alert alert-info analysis-item-explanation mb-0\">");
            html.AppendLine($"        <span>八字主氣計數：{countDescription}。數量最少的五行為 <strong>{string.Join("、", weakElements.Select(element => FormatElement(element)))}</strong>；需留意：{weakDescription}。</span>");
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");
            html.AppendLine("    <div class=\"analysis-item mb-3\">");
            html.AppendLine($"        <div class=\"analysis-item-main\"><strong>{periodLabel}健康氣象</strong> · {periodLabel}<strong>{ganDisplay}{zhiDisplay}</strong></div>");
            html.AppendLine("        <div class=\"alert alert-warning analysis-item-explanation mb-0\">");
            html.AppendLine(GetHealthPeriodAdvice(info, daYun, liuNian, periodGanZhi, period, weakElements));
            html.AppendLine("        </div>");
            html.AppendLine("    </div>");

            if (threePunishment is not null) {
                html.AppendLine("    <div class=\"alert alert-warning analysis-item-explanation mb-3\">");
                html.AppendLine($"        <strong>三刑訊號：</strong>本命、大運與目前期間合看出現{threePunishment}。外傷、壓力或健康波動的高注意訊號，不代表一定會有狀況，但多注意行車安全、身體健康狀況。");
                html.AppendLine("    </div>");
            }

            AppendTopicNotice(html, "<strong>醫療優先：</strong>五行與器官對應僅是「表徵」或「意象」，不能診斷疾病或估算風險。有症狀、慢性病、檢查異常或用藥疑問時，請直接依照醫療專業處理，不可等待有利大運、流年或流月才處理。");
            html.AppendLine("</div>");
        }

        private string GetCapacityAdvice(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period
        ) {
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var yearAssessment = EvaluatePeriodDirection(info, daYunContext, liuNian);
            var currentAssessment = EvaluatePeriodDirection(info, daYunContext, periodGanZhi);
            var wealthStar = FormatEffectiveTenGod(info, daYunContext, ShiShen.Cai);
            var careerStar = FormatEffectiveTenGod(info, daYunContext, ShiShen.GuanSha);
            var outputStar = FormatEffectiveTenGod(info, daYunContext, ShiShen.ShihShang);
            var sealStar = FormatTenGod(info, ShiShen.Yin);
            var peerStar = FormatTenGod(info, ShiShen.BiJie);
            var layers = new System.Text.StringBuilder();
            layers.Append(DescribeDaYunContext(info, daYunContext));
            layers.Append(DescribePeriodDirection(info, daYunContext, yearAssessment, "流年"));
            if (period == PeriodScope.LiuYue) {
                layers.Append(DescribePeriodDirection(info, daYunContext, currentAssessment, "流月"));
            }
            layers.Append(DescribeLayeredPeriodResult(info, daYunContext, yearAssessment, currentAssessment, period));

            var action = info.StrengthStatus switch {
                GeJu.ShenRuo when daYunContext.PrimaryIsFavorable => $"大運主作用已用{sealStar}、{peerStar}幫扶，{wealthStar}、{careerStar}或{outputStar}可作為當期機會方向；仍須控制資金與工作負荷。",
                GeJu.ShenRuo => $"大運主作用仍未扶起日主，{wealthStar}、{careerStar}或{outputStar}較可能先形成負荷；優先穩定現金流、健康與支援資源。",
                GeJu.ShenQiang => $"{wealthStar}、{careerStar}與{outputStar}可作為耗、剋、洩的機會方向；仍應設定預算、停損與契約檢查。",
                GeJu.CongQiang or GeJu.CongRuo => "從格只依順勢／破格方向暫列，不因看見財星就直接推論得財。",
                _ => "請先確認格局與大運背景，再判斷是否能承接機會。"
            };
            return $"{layers} {action}";
        }

        private (string Desc, bool Accept) GetRelationshipAdvice(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period,
            bool hasSpouseSignal
        ) {
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var yearAssessment = EvaluatePeriodDirection(info, daYunContext, liuNian);
            var currentAssessment = EvaluatePeriodDirection(info, daYunContext, periodGanZhi);
            var spouseTenGod = info.Gender == Sex.Male ? ShiShen.Cai : ShiShen.GuanSha;
            var spouseStar = FormatEffectiveTenGod(info, daYunContext, spouseTenGod);
            var spouseIsFavorable = IsEffectivePeriodFavorable(info, daYunContext, spouseTenGod);
            var canAccept = hasSpouseSignal
                && !info.RequiresStrengthVerification
                && daYunContext.PrimaryIsFavorable
                && yearAssessment.HasFavorable
                && !yearAssessment.HasUnfavorable
                && currentAssessment.HasFavorable
                && !currentAssessment.HasUnfavorable
                && spouseIsFavorable;
            var layers = new System.Text.StringBuilder();
            layers.Append(DescribeDaYunContext(info, daYunContext));
            layers.Append(DescribePeriodDirection(info, daYunContext, yearAssessment, "流年"));
            if (period == PeriodScope.LiuYue) {
                layers.Append(DescribePeriodDirection(info, daYunContext, currentAssessment, "流月"));
            }

            if (!hasSpouseSignal) {
                return ($"{layers}目前期間未直接見夫妻星（{spouseStar}），先以日常互動與關係經營為主。", false);
            }

            if (canAccept) {
                return ($"{layers}夫妻星（{spouseStar}）位於當期可承接方向，且大運、流年具支撐。", true);
            }

            var caution = info.RequiresStrengthVerification
                ? "疑似從格仍須先用過往事件回驗；目前只把夫妻星視為時間訊號。"
                : "夫妻星時間窗已出現，但大運、流年或流月的承接條件仍有耗損；先建立身心穩定、支持系統與關係界線。";
            return ($"{layers}{caution}", false);
        }

        private sealed record HealthLayerAssessment(
            IReadOnlyList<WuXing> SupportedWeakElements,
            IReadOnlyList<WuXing> ChallengedWeakElements
        ) {
            public bool HasSupport => SupportedWeakElements.Count > 0;

            public bool HasChallenge => ChallengedWeakElements.Count > 0;
        }

        private sealed record DaYunHealthAssessment(
            HealthLayerAssessment Primary,
            HealthLayerAssessment Secondary
        ) {
            public bool HasSupport => Primary.HasSupport
                || (!Primary.HasChallenge && Secondary.HasSupport);

            public bool HasChallenge => Primary.HasChallenge
                || (!Primary.HasSupport && Secondary.HasChallenge);
        }

        private string GetHealthPeriodAdvice(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period,
            IReadOnlyCollection<WuXing> weakElements
        ) {
            // 待核對：筆記 5-2 的特定流日缺少一致選取理由，此處只採 5-1 的分層規則。
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var daYunPrimaryElement = daYunContext.Phase == DaYunPhase.FirstFiveYears
                ? daYun.Gan.ToWuXing()
                : daYun.Zhi.ToWuXing();
            var daYunSecondaryElement = daYunContext.Phase == DaYunPhase.FirstFiveYears
                ? daYun.Zhi.ToWuXing()
                : daYun.Gan.ToWuXing();
            var yearElements = new[] { liuNian.Gan.ToWuXing(), liuNian.Zhi.ToWuXing() };
            var currentElements = new[] { periodGanZhi.Gan.ToWuXing(), periodGanZhi.Zhi.ToWuXing() };
            var phaseLabel = daYunContext.Phase == DaYunPhase.FirstFiveYears
                ? "前五年天干為主"
                : "後五年地支為主";
            var html = new System.Text.StringBuilder();
            html.AppendLine("<ul class=\"mb-0\">");

            if (info.StrengthStatus is GeJu.CongQiang or GeJu.CongRuo) {
                html.AppendLine($"    <li><strong>大運背景：</strong>{DescribeCongDaYunHealthLayer(info, daYunPrimaryElement, daYunSecondaryElement, phaseLabel)}</li>");
                html.AppendLine($"    <li><strong>流年背景：</strong>{DescribeCongHealthLayer(info, yearElements, "流年")}</li>");
                if (period == PeriodScope.LiuYue) {
                    html.AppendLine($"    <li><strong>流月窗口：</strong>{DescribeCongHealthLayer(info, currentElements, "流月")}</li>");
                }
                html.AppendLine("    <li><strong>綜合：</strong>從格健康判讀以順勢、成格／破格為主，不因短期五行剛好補到命盤缺項就改判；疑似從格仍須用實際健康狀況與過往年份回驗。</li>");
                html.AppendLine("</ul>");
                return html.ToString();
            }

            var daYunAssessment = new DaYunHealthAssessment(
                EvaluateHealthLayer(weakElements, [daYunPrimaryElement]),
                EvaluateHealthLayer(weakElements, [daYunSecondaryElement])
            );
            var yearAssessment = EvaluateHealthLayer(weakElements, yearElements);
            var currentAssessment = EvaluateHealthLayer(weakElements, currentElements);
            html.AppendLine($"    <li><strong>大運背景：</strong>{DescribeDaYunHealthLayer(daYunAssessment, phaseLabel)}。</li>");
            html.AppendLine($"    <li><strong>流年背景：</strong>{DescribeHealthLayer(yearAssessment, "流年")}。</li>");
            if (period == PeriodScope.LiuYue) {
                html.AppendLine($"    <li><strong>流月窗口：</strong>{DescribeHealthLayer(currentAssessment, "流月")}。</li>");
            }
            html.AppendLine($"    <li><strong>綜合：</strong>{DescribeHealthLayerResult(daYunAssessment, yearAssessment, currentAssessment, period)}</li>");
            html.AppendLine("</ul>");
            return html.ToString();
        }

        private static HealthLayerAssessment EvaluateHealthLayer(
            IReadOnlyCollection<WuXing> weakElements,
            IReadOnlyCollection<WuXing> sourceElements
        ) {
            var supported = weakElements
                .Where(weakElement => sourceElements.Any(element =>
                    element == weakElement || BaZiDefine.Generation[element] == weakElement))
                .ToArray();
            var challenged = weakElements
                .Where(weakElement => sourceElements.Any(element =>
                    element == BaZiDefine.RestrictBy[weakElement]
                    || element == BaZiDefine.Generation[weakElement]
                    || element == BaZiDefine.Restricting[weakElement]))
                .ToArray();
            return new HealthLayerAssessment(supported, challenged);
        }

        private static string DescribeHealthLayer(HealthLayerAssessment assessment, string periodLabel) {
            var supported = string.Join("、", assessment.SupportedWeakElements.Select(element => FormatElement(element)));
            var challenged = string.Join("、", assessment.ChallengedWeakElements.Select(element => FormatElement(element)));
            return (assessment.HasSupport, assessment.HasChallenge) switch {
                (true, true) => $"{periodLabel}同時補強 {supported}，也對 {challenged} 帶來剋、洩或耗",
                (true, false) => $"{periodLabel}對弱項 {supported} 有同我或生我的補強",
                (false, true) => $"{periodLabel}對弱項 {challenged} 帶來剋、洩或耗",
                _ => $"{periodLabel}對本命最弱五行沒有明顯直接作用"
            };
        }

        private static string DescribeDaYunHealthLayer(
            DaYunHealthAssessment assessment,
            string phaseLabel
        ) {
            var primary = DescribeHealthLayer(assessment.Primary, "主作用");
            var secondary = DescribeHealthLayer(assessment.Secondary, "次要背景");
            return $"{phaseLabel}；{primary}；{secondary}。綜合以主作用為優先，次要背景只作加減分";
        }

        private static string DescribeHealthLayerResult(
            DaYunHealthAssessment daYun,
            HealthLayerAssessment liuNian,
            HealthLayerAssessment current,
            PeriodScope period
        ) {
            var currentRisk = period == PeriodScope.LiuNian ? liuNian.HasChallenge : current.HasChallenge;
            var currentSupport = period == PeriodScope.LiuNian ? liuNian.HasSupport : current.HasSupport;
            if (daYun.HasChallenge && liuNian.HasChallenge && currentRisk) {
                return "大運、流年與目前期間對弱項形成跨層耗損，保養優先級較高；這是注意訊號，不代表必然發病。";
            }

            if (daYun.HasSupport && currentRisk) {
                return "大運仍有長期支撐，短期則出現剋、洩或耗；留意當期狀況，但不把短期波動寫成整段大運都差。";
            }

            if (daYun.HasChallenge && (liuNian.HasSupport || currentSupport)) {
                return "大運背景較需留意，流年或目前期間提供短期補強；可把握時間檢查、休養或處理既有症狀。";
            }

            if (daYun.HasSupport || liuNian.HasSupport || currentSupport) {
                return "目前至少一個時間層對弱項有補強，可視為相對支撐；仍應維持例行保健並依實際症狀就醫。";
            }

            return "各時間層沒有明顯補強；維持保守作息，並以病史、症狀與檢查結果為準。";
        }

        private string DescribeCongHealthLayer(
            BaZiInfo info,
            IReadOnlyCollection<WuXing> elements,
            string periodLabel
        ) {
            var favorable = elements.Where(info.LikeWuXing.Contains).Distinct().ToArray();
            var unfavorable = elements.Where(info.UnlikeWuXing.Contains).Distinct().ToArray();
            var favorableText = string.Join("、", favorable.Select(element => FormatElement(element)));
            var unfavorableText = string.Join("、", unfavorable.Select(element => FormatElement(element)));
            return (favorable.Length, unfavorable.Length) switch {
                ( > 0, 0) => $"{periodLabel}見 {favorableText}，皆在{FormatFavorabilityLabel(true, "從格依順勢原則列為喜用方向\r\n喜用神可以幫扶命主，對運勢加分")}",
                (0, > 0) => $"{periodLabel}見 {unfavorableText}，皆偏{FormatFavorabilityLabel(false, "從格依破格原則列為忌神方向\r\n忌神會耗洩命主，對運勢沒有加分(相對不利)")}",
                _ => $"{periodLabel}同見喜用的 {favorableText} 與忌神的 {unfavorableText}，順勢與逆勢並存"
            };
        }

        private string DescribeCongDaYunHealthLayer(
            BaZiInfo info,
            WuXing primaryElement,
            WuXing secondaryElement,
            string phaseLabel
        ) {
            var primary = FormatElement(primaryElement);
            var secondary = FormatElement(secondaryElement);
            var primaryDirection = FormatFavorabilityLabel(
                info.LikeWuXing.Contains(primaryElement),
                "從格依順勢／破格原則判斷大運當期主作用"
            );
            var secondaryDirection = FormatFavorabilityLabel(
                info.LikeWuXing.Contains(secondaryElement),
                "從格依順勢／破格原則判斷大運當期次要背景"
            );
            return $"{phaseLabel}；主作用 {primary} 為{primaryDirection}，次要背景 {secondary} 為{secondaryDirection}；主次不同時不將整段大運寫成全好或全壞。";
        }

        private static IReadOnlyDictionary<WuXing, int> CountChartElements(BaZiInfo info) {
            var counts = CourseRuleCatalog.ElementOrder.ToDictionary(element => element, _ => 0);
            Zhu[] pillars = [info.YearZhu, info.MonthZhu, info.DayZhu, info.HourZhu];
            foreach (var pillar in pillars) {
                counts[pillar.GanWuXing]++;
                counts[pillar.ZhiWuXing]++;
            }

            return counts;
        }

        private static bool IsSameSanHeGroup(DiZhi source, DiZhi target) {
            return BaZiDefine.ThreeHe.Values.Any(group => group.Contains(source) && group.Contains(target));
        }

        private static string? GetHealthThreePunishment(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            PeriodScope period
        ) {
            var branches = new HashSet<DiZhi> {
            info.YearZhu.Zhi,
            info.MonthZhu.Zhi,
            info.DayZhu.Zhi,
            daYun.Zhi,
            liuNian.Zhi
        };
            if (info.IsBirthTimeAccurate) {
                branches.Add(info.HourZhu.Zhi);
            }
            if (period == PeriodScope.LiuYue) {
                branches.Add(periodGanZhi.Zhi);
            }

            var punishment = BaZiDefine.ThreeXing.FirstOrDefault(group => group.All(branches.Contains));
            return punishment is null
                ? null
                : $"{string.Join(string.Empty, punishment.Select(branch => FormatElement(branch.ToWuXing(), branch.ToZhiString())))}三刑";
        }

        private static (DaYun? daYun, LiuNian? liuNian) FindLiuNian(BaZiInfo info, int targetYear) {
            foreach (var daYun in info.DaYunList) {
                var liuNian = daYun.LiuNianList.FirstOrDefault(item => item.Year == targetYear);
                if (liuNian is not null)
                    return (daYun, liuNian);
            }

            return (null, null);
        }

        private IReadOnlyList<TaiSuiInteractionType> GetTaiSuiInteractions(
            DiZhi natalBranch,
            DiZhi annualBranch
        ) {
            var interactions = new List<TaiSuiInteractionType>();
            if (natalBranch == annualBranch) {
                interactions.Add(TaiSuiInteractionType.SameBranch);
            }

            foreach (BranchRelationshipRuleMatch match in _relationshipEngine.MatchPair(natalBranch, annualBranch)) {
                TaiSuiInteractionType? interaction = match.RelationType switch {
                    BranchRelationshipType.SixClash => TaiSuiInteractionType.SixClash,
                    BranchRelationshipType.Punishment => TaiSuiInteractionType.Punishment,
                    BranchRelationshipType.SixHarm => TaiSuiInteractionType.SixHarm,
                    BranchRelationshipType.SixBreak => TaiSuiInteractionType.SixBreak,
                    _ => null
                };
                if (interaction is not null && !interactions.Contains(interaction.Value)) {
                    interactions.Add(interaction.Value);
                }
            }

            return interactions;
        }

        private static string GetZodiac(DiZhi branch) {
            return branch switch {
                DiZhi.Zi => "鼠",
                DiZhi.Chou => "牛",
                DiZhi.Yin => "虎",
                DiZhi.Mao => "兔",
                DiZhi.Chen => "龍",
                DiZhi.Si => "蛇",
                DiZhi.Wu => "馬",
                DiZhi.Wei => "羊",
                DiZhi.Shen => "猴",
                DiZhi.You => "雞",
                DiZhi.Xu => "狗",
                DiZhi.Hai => "豬",
                _ => throw new ArgumentOutOfRangeException(nameof(branch), branch, null)
            };
        }

        private FortuneYinAnalysisResult CreateYinAnalysis(
            BaZiInfo info,
            IGanZhi periodPillar,
            IReadOnlyList<IGanZhi>? earlierFortunePillars = null
        ) {
            var fuYin = _fuYinAnalysisService.AnalyzePeriod(info, periodPillar, earlierFortunePillars);
            var fanYin = _fanYinAnalysisService.AnalyzePeriod(info, periodPillar, earlierFortunePillars);
            return new FortuneYinAnalysisResult(periodPillar.Id, fuYin, fanYin);
        }

        private string GetPeriodHtml(
            BaZiInfo info,
            DaYun daYun,
            LiuNian liuNian,
            IGanZhi periodGanZhi,
            string title,
            string displayPrefix,
            string displaySuffix,
            PeriodScope period,
            Func<System.Text.StringBuilder, (bool bad, IDictionary<HeHui, WuXing> heHui)> checkConflict
        ) {
            var html = new System.Text.StringBuilder();
            html.AppendLine("<div class=\"card analysis-card p-3 mt-3\">");
            html.AppendLine($"    <h5 class=\"card-title analysis-card-title\">{title}</h5>");
            html.AppendLine("    <hr class=\"analysis-card-divider\" aria-hidden=\"true\" />");
            /* 先列出是否有衝突 */
            var conflict = new System.Text.StringBuilder();
            var (_, heHui) = checkConflict(conflict);
            if (conflict.Length > 0) {
                html.AppendLine(@"    <div class=""mb-3"">");
                html.AppendLine(conflict.ToString());
                html.AppendLine(@"    </div>");
            }
            /* 流年與流月的天干、地支都要判斷。要注意是否有三合、六合、三會改變屬性 */
            var ganYun = periodGanZhi.Gan.ToShiShen(info.DayZhu.Gan);
            ShiShen zhiYun; //會是兩者合併的十神，例如 '比肩+劫財'
            if (heHui.ContainsKey(HeHui.ThreeHui)) {
                zhiYun = heHui[HeHui.ThreeHui].ToShiShen(info.RiZhu);
            } else if (heHui.ContainsKey(HeHui.ThreeHe)) {
                zhiYun = heHui[HeHui.ThreeHe].ToShiShen(info.RiZhu);
            } else if (heHui.ContainsKey(HeHui.SixHe)) {
                zhiYun = heHui[HeHui.SixHe].ToShiShen(info.RiZhu);
            } else {    //沒有合會時，以地支的 '主氣' 為主
                zhiYun = periodGanZhi.Zhi.ToWuXing().ToShiShen(info.RiZhu);
            }
            html.AppendLine(@"    <div class=""analysis-item mb-0"">");
            var ganColor = ElementPresentationService.GetCssClass(periodGanZhi.Gan.ToWuXing());
            var zhiColor = ElementPresentationService.GetCssClass(periodGanZhi.Zhi.ToWuXing());
            var daYunContext = EvaluateDaYunContext(info, daYun, liuNian.Year);
            var ganYunDisplay = FormatEffectiveTenGod(info, daYunContext, ganYun, ganYun.ToYunString());
            var zhiYunDisplay = FormatEffectiveTenGod(info, daYunContext, zhiYun, zhiYun.ToYunString());
            if (zhiYun.HasFlag(ganYun)) {
                html.Append($"        <div class=\"analysis-item-main\">{displayPrefix}<strong><span class=\"{ganColor}\">{periodGanZhi.Gan.ToGanString()}</span><span class=\"{zhiColor}\">{periodGanZhi.Zhi.ToZhiString()}</span></strong>{displaySuffix} 走 {zhiYunDisplay}</div>");
            } else {
                html.Append($"        <div class=\"analysis-item-main\">{displayPrefix}<strong><span class=\"{ganColor}\">{periodGanZhi.Gan.ToGanString()}</span><span class=\"{zhiColor}\">{periodGanZhi.Zhi.ToZhiString()}</span></strong>{displaySuffix} 走 {ganYunDisplay}、{zhiYunDisplay}</div>");
            }
            html.AppendLine("        <div class=\"alert alert-info analysis-item-explanation mb-0\">");
            CreatePeriodDesc(info, daYun, liuNian, ganYun, zhiYun, period, html);
            html.AppendLine("        </div>");
            html.AppendLine(@"    </div>");
            html.AppendLine(@"</div>");
            return html.ToString();
        }

        private static void AppendTopicNotice(System.Text.StringBuilder html, string content) {
            html.AppendLine("    <blockquote class=\"topic-notice mb-0\">");
            html.AppendLine("        <i class=\"fa-solid fa-quote-left topic-notice-icon\" aria-hidden=\"true\"></i>");
            html.AppendLine($"        <div>{content}</div>");
            html.AppendLine("    </blockquote>");
        }

        private static string FormatElement(WuXing element, string? displayText = null) {
            var text = displayText ?? element.ToWuXingString();
            return $"<span class=\"{ElementPresentationService.GetCssClass(element)} fw-semibold\">{text}</span>";
        }

        private string FormatTenGod(BaZiInfo info, ShiShen tenGod, string? displayText = null) {
            TenGodPresentation presentation = _tenGodPresentationService.CreateFavorability(
                info,
                tenGod,
                IsOriginalFavorable(info, tenGod),
                "依本命格局",
                displayText
            );
            return _tenGodPresentationService.ToHtml(presentation);
        }

        private string FormatEffectiveTenGod(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext,
            ShiShen tenGod,
            string? displayText = null
        ) {
            TenGodPresentation presentation = _tenGodPresentationService.CreateFavorability(
                info,
                tenGod,
                IsEffectivePeriodFavorable(info, daYunContext, tenGod),
                _periodFavorabilityService.GetPeriodReason(info, daYunContext),
                displayText
            );
            return _tenGodPresentationService.ToHtml(presentation);
        }

        private string FormatFavorabilityLabel(bool isFavorable, string tooltip) {
            TenGodPresentation presentation = _tenGodPresentationService.CreateDirection(isFavorable, tooltip);
            return _tenGodPresentationService.ToHtml(presentation);
        }

        private static WuXing GetTenGodElement(BaZiInfo info, ShiShen tenGod) {
            return tenGod.ToCombined() switch {
                ShiShen.Cai => BaZiDefine.Restricting[info.RiZhu],
                ShiShen.GuanSha => BaZiDefine.RestrictBy[info.RiZhu],
                ShiShen.ShihShang => BaZiDefine.Generation[info.RiZhu],
                ShiShen.Yin => BaZiDefine.GenerateBy[info.RiZhu],
                ShiShen.BiJie => info.RiZhu,
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(tenGod), (int)tenGod, typeof(ShiShen))
            };
        }

    }
}

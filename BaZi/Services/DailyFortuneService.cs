using BaZi.Models;

namespace BaZi.Services {

    /// <summary>依筆記中的明確規則提供流日主題分析。</summary>
    public sealed class DailyFortuneService {
        private readonly PeriodFavorabilityService _periodFavorabilityService;
        private readonly EarthlyBranchRelationshipEngine _relationshipEngine;

        private static readonly IReadOnlyDictionary<WuXing, HashSet<string>> FemalePureRomanceDays =
            new Dictionary<WuXing, HashSet<string>> {
                [WuXing.Jin] = Days("丙午", "丙戌", "丁巳", "丁未"),
                [WuXing.Mu] = Days("庚申", "辛酉", "辛丑"),
                [WuXing.Shui] = Days("戊戌", "戊辰", "己巳", "戊午", "己未", "己丑"),
                [WuXing.Huo] = Days("壬子", "癸亥", "壬辰"),
                [WuXing.Tu] = Days("甲寅", "乙卯", "甲辰", "乙亥")
            };

        private static readonly IReadOnlyDictionary<WuXing, HashSet<string>> FemaleMixedRomanceDays =
            new Dictionary<WuXing, HashSet<string>> {
                [WuXing.Jin] = Days("甲戌", "乙未", "丙寅", "丁卯"),
                [WuXing.Mu] = Days("庚辰", "辛丑", "戊申", "己酉", "己丑"),
                [WuXing.Shui] = Days("丙午", "丁未", "丁巳", "丙戌"),
                [WuXing.Huo] = Days("壬申", "癸酉", "癸丑", "庚子", "庚辰", "辛亥"),
                [WuXing.Tu] = Days("甲子", "壬寅", "壬辰", "癸卯")
            };

        private static readonly IReadOnlyDictionary<WuXing, HashSet<string>> ExtremeHealthDays =
            new Dictionary<WuXing, HashSet<string>> {
                [WuXing.Mu] = Days("甲寅", "乙卯"),
                [WuXing.Huo] = Days("丙午", "丁巳"),
                [WuXing.Tu] = Days("戊辰", "己丑", "戊戌", "己未"),
                [WuXing.Jin] = Days("庚申", "辛酉"),
                [WuXing.Shui] = Days("壬子", "癸亥")
            };

        private static readonly IReadOnlyDictionary<WuXing, HashSet<string>> WeakElementHealthDays =
            new Dictionary<WuXing, HashSet<string>> {
                [WuXing.Jin] = Days("丙寅", "丙午", "丙戌", "丁巳", "丁未"),
                [WuXing.Mu] = Days("庚申", "辛酉", "辛丑"),
                [WuXing.Shui] = Days("戊戌", "戊辰", "戊午", "己未", "己丑", "己巳"),
                [WuXing.Huo] = Days("壬子", "壬申", "壬辰", "癸亥", "癸酉", "癸丑"),
                [WuXing.Tu] = Days("甲寅", "甲子", "甲辰", "乙卯", "乙亥", "乙未")
            };

        public DailyFortuneService()
            : this(new PeriodFavorabilityService(), new EarthlyBranchRelationshipEngine()) {
        }

        public DailyFortuneService(
            PeriodFavorabilityService periodFavorabilityService,
            EarthlyBranchRelationshipEngine relationshipEngine
        ) {
            _periodFavorabilityService = periodFavorabilityService;
            _relationshipEngine = relationshipEngine;
        }

        /// <summary>分析指定國曆月份的每一天。</summary>
        public IReadOnlyList<DailyFortuneResult> AnalyzeMonth(
            BaZiInfo info,
            int year,
            int month,
            IReadOnlyCollection<DiZhi>? householdYearBranches = null
        ) {
            return AnalyzeMonthDetails(info, year, month, householdYearBranches).Results;
        }

        /// <summary>分析指定國曆月份，並整理交通期間背景。</summary>
        public DailyFortuneMonthAnalysis AnalyzeMonthDetails(
            BaZiInfo info,
            int year,
            int month,
            IReadOnlyCollection<DiZhi>? householdYearBranches = null
        ) {
            ArgumentNullException.ThrowIfNull(info);
            if (year is < 1900 or > 2100) {
                throw new ArgumentOutOfRangeException(nameof(year), year, "年份必須介於 1900 與 2100。 ");
            }
            if (month is < 1 or > 12) {
                throw new ArgumentOutOfRangeException(nameof(month), month, "月份必須介於 1 與 12。 ");
            }

            var results = new List<DailyFortuneResult>();
            for (var day = 1; day <= DateTime.DaysInMonth(year, month); day++) {
                results.Add(AnalyzeDate(info, new DateTime(year, month, day), householdYearBranches));
            }
            return new DailyFortuneMonthAnalysis(results, CreateTravelSafetyBackgrounds(info, results));
        }

        /// <summary>分析單一國曆日期。</summary>
        public DailyFortuneResult AnalyzeDate(
            BaZiInfo info,
            DateTime date,
            IReadOnlyCollection<DiZhi>? householdYearBranches = null
        ) {
            ArgumentNullException.ThrowIfNull(info);
            var eightChar = Lunar.Solar.FromDate(date.Date.AddHours(12)).Lunar.EightChar;
            var day = new LiuRi(date.Date, eightChar.DayGan.ToTianGan(), eightChar.DayZhi.ToDiZhi());
            var context = new DailyPeriodContext(
                eightChar.YearGan.ToTianGan(),
                eightChar.YearZhi.ToDiZhi(),
                eightChar.MonthGan.ToTianGan(),
                eightChar.MonthZhi.ToDiZhi(),
                FindDaYun(info, date.Year)
            );
            var signals = new List<DailyFortuneSignal>();

            AddWealthSignal(info, day, context, signals);
            AddCareerSignal(info, day, context, signals);
            AddRomanceSignal(info, day, context, signals);
            AddHealthSignals(info, day, context, signals);
            AddInterpersonalSignal(info, day, context, signals);
            AddTravelSafetySignal(info, day, context, signals);
            AddMovingSignal(info, day, householdYearBranches, signals);

            return new DailyFortuneResult(day, context, signals);
        }

        private void AddWealthSignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var wealth = BaZiDefine.Restricting[info.RiZhu];
            var output = BaZiDefine.Generation[info.RiZhu];
            var ganElement = day.Gan.ToWuXing();
            var branchElements = day.Zhi.ToFullWuXing();
            var acceptedBranch = CourseRuleCatalog.DailyWealthBranches[info.RiZhu].Contains(day.Zhi);
            var ganHasWealth = ganElement == wealth;
            var ganHasOutput = ganElement == output;
            var zhiHasWealth = acceptedBranch && branchElements.ContainsKey(wealth);
            var zhiHasOutput = acceptedBranch && branchElements.ContainsKey(output);

            string? summary = null;
            if (ganHasWealth && zhiHasWealth) {
                summary = "天干與地支同時帶財星，是本月財務行動力最集中的候選日。";
            } else if ((ganHasWealth && zhiHasOutput) || (ganHasOutput && zhiHasWealth)) {
                summary = "食傷與財星相接，形成食傷生財的候選日。";
            } else if (ganHasOutput && zhiHasOutput) {
                summary = "天干與地支皆帶食傷，能量可間接流向財星。";
            }

            if (summary is null) {
                return;
            }

            var capacity = EvaluateOpportunityCapacity(info, day.Date.Year, context);
            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.Wealth,
                capacity.Level,
                capacity.CanAct ? "財務行動窗口" : "財務事件注意窗口",
                $"{summary}{capacity.Summary}",
                capacity.CanAct
                    ? "可安排報價、收款、談合作與檢視現金流；先設定預算、付款與停損條件，機會不等於必然獲利。"
                    : "先穩定現金流、工作負荷與支援資源，避免為求財擴張、借貸或加碼；此日也可能表現為支出或財來財去。",
                GetTenGodFactors(info, day, context, [ShiShen.Cai, ShiShen.ShihShang])
            ));
        }

        private void AddCareerSignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var career = BaZiDefine.RestrictBy[info.RiZhu];
            var wealth = BaZiDefine.Restricting[info.RiZhu];
            if (day.Gan.ToWuXing() != career) {
                return;
            }

            var branchElements = day.Zhi.ToFullWuXing();
            var isPrimary = CourseRuleCatalog.CareerBranches[info.RiZhu].Contains(day.Zhi);
            var isSupplementary = branchElements.ContainsKey(wealth);
            if (!isPrimary && !isSupplementary) {
                return;
            }

            var capacity = EvaluateOpportunityCapacity(info, day.Date.Year, context);
            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.Career,
                capacity.Level,
                capacity.CanAct
                    ? (isPrimary ? "主要工作機會日" : "財生官的工作候選日")
                    : "工作壓力與機會並見日",
                (isPrimary
                    ? "官殺天干與含官殺能量的地支同時出現，工作與外在要求較集中。"
                    : "官殺落在天干，地支帶財，形成財生官的工作訊號。")
                    + capacity.Summary,
                capacity.CanAct
                    ? "可安排面試、提案或重要會議；簽約與轉職仍要核對工作量、薪資、責任及退出條件。"
                    : "先控制工作負荷、補足人力與專業支援；重大簽約或轉職不要只因日期而決定。",
                GetTenGodFactors(info, day, context, [ShiShen.GuanSha, ShiShen.Cai])
            ));
        }

        private void AddRomanceSignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var key = GetDayKey(day);
            var capacity = EvaluateOpportunityCapacity(info, day.Date.Year, context);
            if (info.Gender == Sex.Male) {
                var wealthSignal = signals.FirstOrDefault(signal => signal.Topic == DailyFortuneTopic.Wealth);
                if (wealthSignal is null) {
                    return;
                }

                signals.Add(new DailyFortuneSignal(
                    DailyFortuneTopic.Romance,
                    capacity.Level,
                    capacity.CanAct ? "男命桃花候選日" : "男命桃花承接注意日",
                    $"財星是男命夫妻星，此日同時符合財旺或食傷生財條件。{capacity.Summary}",
                    capacity.CanAct
                        ? "可增加互動、安排約會或表達心意；關係結果仍取決於雙方意願與實際相處。"
                        : "桃花訊號仍存在，但先放慢承諾與金錢投入，觀察界線、誠信、壓力及雙方意願。",
                    GetTenGodFactors(info, day, context, [ShiShen.Cai, ShiShen.ShihShang])
                ));
                return;
            }

            var isPure = FemalePureRomanceDays[info.RiZhu].Contains(key);
            var isMixed = FemaleMixedRomanceDays[info.RiZhu].Contains(key);
            if (!isPure && !isMixed) {
                return;
            }

            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.Romance,
                capacity.Level,
                capacity.CanAct ? "女命桃花候選日" : "女命桃花承接注意日",
                (isPure
                    ? "夫妻星官殺的訊號集中，感情互動機會較明顯。"
                    : "官殺與財星形成財生官的桃花訊號。"
                ) + capacity.Summary,
                capacity.CanAct
                    ? "可增加互動、安排約會或釐清關係；不以日期代替界線、尊重與雙方意願。"
                    : "桃花訊號仍存在，但官殺也可能成為壓力；先觀察安全感、界線與相處品質，不急著作重大承諾。",
                GetTenGodFactors(info, day, context, [ShiShen.GuanSha, ShiShen.Cai])
            ));
        }

        private void AddHealthSignals(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            if (info.StrengthStatus is GeJu.CongQiang or GeJu.CongRuo) {
                AddCongHealthSignal(info, day, context, signals);
                return;
            }

            var key = GetDayKey(day);
            if (ExtremeHealthDays[info.RiZhu].Contains(key)) {
                var affected = BaZiDefine.Restricting[info.RiZhu];
                var background = EvaluateHealthBackground(info, day.Date.Year, context, affected);
                signals.Add(new DailyFortuneSignal(
                    DailyFortuneTopic.Health,
                    background.IsStackedRisk ? DailySignalLevel.HighAttention : DailySignalLevel.Attention,
                    background.IsStackedRisk ? "同五行極旺疊加注意日" : "同五行極旺保養日",
                    $"日主同五行能量集中，較需留意{CourseRuleCatalog.HealthParts[affected]}的負擔。{background.Summary}",
                    "行程排鬆、飲食清淡、多休息並降低高風險活動；有症狀直接依醫療專業處理。"
                ));
            }

            foreach (var weakElement in GetWeakElements(info)) {
                if (!WeakElementHealthDays[weakElement].Contains(key)) {
                    continue;
                }

                var background = EvaluateHealthBackground(info, day.Date.Year, context, weakElement);
                signals.Add(new DailyFortuneSignal(
                    DailyFortuneTopic.Health,
                    background.IsStackedRisk ? DailySignalLevel.HighAttention : DailySignalLevel.Attention,
                    background.IsStackedRisk ? "最弱五行疊加注意日" : "最弱五行保養日",
                    $"命盤最弱的{weakElement.ToWuXingString()}受到強勢五行牽制，較需留意{CourseRuleCatalog.HealthParts[weakElement]}。{background.Summary}",
                    "提前調整飲食、作息與運動強度；日期只用來安排保守作息，不作疾病診斷。"
                ));
            }
        }

        private void AddCongHealthSignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var elements = new[] { day.Gan.ToWuXing(), day.Zhi.ToWuXing() };
            var isBreaking = info.StrengthStatus == GeJu.CongQiang
                ? elements.Any(element => element != info.RiZhu && element != BaZiDefine.GenerateBy[info.RiZhu])
                : elements.Any(element => element == info.RiZhu || element == BaZiDefine.GenerateBy[info.RiZhu]);
            if (!isBreaking) {
                return;
            }

            var background = EvaluateCongBackground(info, day.Date.Year, context);
            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.Health,
                background.IsStackedRisk ? DailySignalLevel.HighAttention : DailySignalLevel.Attention,
                background.IsStackedRisk ? "從格逆勢疊加注意日" : "從格逆勢保養日",
                $"流日出現逆勢五行，容易打亂原本從格的能量方向。{background.Summary}",
                "降低行程密度並維持規律作息；有症狀、慢性病或用藥問題時直接諮詢醫療專業。"
            ));
        }

        private void AddInterpersonalSignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var sameElement = info.RiZhu;
            var branchElements = day.Zhi.ToFullWuXing();
            if (day.Gan.ToWuXing() != sameElement || !branchElements.ContainsKey(sameElement)) {
                return;
            }

            var hasAnnualClash = IsClash(day.Zhi, context.YearZhi);
            var peerContext = EvaluatePeerBackground(info, day.Date.Year, context);
            var isHighAttention = hasAnnualClash || peerContext.IsStackedRisk;
            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.Interpersonal,
                isHighAttention ? DailySignalLevel.HighAttention : DailySignalLevel.Attention,
                hasAnnualClash
                    ? "比劫疊加流年相沖"
                    : (peerContext.IsStackedRisk ? "比劫跨期間疊加注意日" : "比劫互動密集日"),
                hasAnnualClash
                    ? $"同我能量強，並與流年地支相沖，競爭、口舌與權責摩擦更容易集中。{peerContext.Summary}"
                    : $"流日天干、地支都推高同我比劫，人際競爭與意見碰撞較明顯。{peerContext.Summary}",
                "重要溝通書面化；合作、分潤、權責與交付條件先寫清楚，避免情緒性表態。",
                GetTenGodFactors(info, day, context, [ShiShen.BiJie])
            ));
        }

        private void AddTravelSafetySignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var background = CreateTravelSafetyBackground(info, context, day.Date, day.Date);
            var backgroundBranches = background.Sources.Select(source => source.Branch).ToArray();
            DiZhi[] combinedBranches = [.. backgroundBranches, day.Zhi];
            IList<DiZhi>? completedPunishment = BaZiDefine.ThreeXing.FirstOrDefault(group =>
                !group.All(backgroundBranches.Contains)
                && group.All(combinedBranches.Contains));
            var clashedSources = background.Sources
                .Where(source => IsClash(day.Zhi, source.Branch))
                .ToArray();
            if (completedPunishment is null && clashedSources.Length == 0) {
                return;
            }

            var isHighAttention = completedPunishment is not null || clashedSources.Length >= 2;
            var details = new List<string>();
            if (completedPunishment is not null) {
                details.Add(
                    $"流日地支{day.Zhi.ToZhiString()}補齊{string.Join(string.Empty, completedPunishment.Select(branch => branch.ToZhiString()))}三刑"
                );
            }
            if (clashedSources.Length > 0) {
                details.Add(
                    $"流日地支{day.Zhi.ToZhiString()}與{string.Join("、", clashedSources.Select(source => $"{source.Label}{source.Branch.ToZhiString()}"))}形成 {clashedSources.Length} 組相沖"
                );
            }
            if (CourseRuleCatalog.TravelBranches.Contains(day.Zhi)) {
                details.Add($"流日本身為驛馬，期間驛馬由 {background.TravelBranchCount} 個增為 {background.TravelBranchCount + 1} 個");
            }

            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.TravelSafety,
                isHighAttention ? DailySignalLevel.HighAttention : DailySignalLevel.Attention,
                isHighAttention ? "交通與移動高注意日" : "交通與移動注意日",
                string.Join("；", details) + "。",
                "不要疲勞駕駛、酒駕或趕路；檢查輪胎、煞車、燈具並替長途、雨夜與陌生路線預留緩衝。",
                [CreateTenGodFactor(
                    info,
                    context,
                    day.Date.Year,
                    "流日地支",
                    day.Zhi.ToShiShen(info.DayZhu.Gan).ToCombined()
                )]
            ));
        }

        private void AddMovingSignal(
            BaZiInfo info,
            LiuRi day,
            IReadOnlyCollection<DiZhi>? householdYearBranches,
            ICollection<DailyFortuneSignal> signals
        ) {
            var branches = householdYearBranches is { Count: > 0 }
                ? householdYearBranches
                : [info.YearZhu.Zhi];
            var clashedBranches = branches.Where(branch => IsClash(day.Zhi, branch)).Distinct().ToArray();
            if (clashedBranches.Length > 0) {
                signals.Add(new DailyFortuneSignal(
                    DailyFortuneTopic.Moving,
                    DailySignalLevel.Attention,
                    "搬家命盤初篩未通過",
                    $"流日地支沖到同住者年支：{string.Join("、", clashedBranches.Select(branch => branch.ToZhiString()))}。",
                    "改選未沖到所有同住者年支的日期，再確認通書標示宜入宅或宜遷移。"
                ));
                return;
            }

            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.Moving,
                DailySignalLevel.VerificationRequired,
                "搬家命盤初篩通過",
                "流日地支未沖到已選取的同住者年支。",
                "這只是命盤初篩；仍須確認通書標示宜入宅或宜遷移，再核對交屋、搬運、天候與安全條件。"
            ));
        }

        private IReadOnlyList<DailyTravelSafetyPeriodBackground> CreateTravelSafetyBackgrounds(
            BaZiInfo info,
            IReadOnlyList<DailyFortuneResult> results
        ) {
            if (results.Count == 0) {
                return [];
            }

            var backgrounds = new List<DailyTravelSafetyPeriodBackground>();
            var startIndex = 0;
            while (startIndex < results.Count) {
                var endIndex = startIndex;
                while (endIndex + 1 < results.Count
                    && HasSameTravelSafetyContext(results[startIndex].Context, results[endIndex + 1].Context)) {
                    endIndex++;
                }

                backgrounds.Add(CreateTravelSafetyBackground(
                    info,
                    results[startIndex].Context,
                    results[startIndex].Day.Date,
                    results[endIndex].Day.Date
                ));
                startIndex = endIndex + 1;
            }

            return backgrounds;
        }

        private DailyTravelSafetyPeriodBackground CreateTravelSafetyBackground(
            BaZiInfo info,
            DailyPeriodContext context,
            DateTime startDate,
            DateTime endDate
        ) {
            var sources = new List<DailyTravelSafetyBranchSource> {
                CreateTravelSafetySource("本命年支", info.YearZhu.Zhi),
                CreateTravelSafetySource("本命月支", info.MonthZhu.Zhi),
                CreateTravelSafetySource("本命日支", info.DayZhu.Zhi)
            };
            if (info.IsBirthTimeAccurate) {
                sources.Add(CreateTravelSafetySource("本命時支", info.HourZhu.Zhi));
            }
            if (context.DaYun is not null) {
                sources.Add(CreateTravelSafetySource("大運支", context.DaYun.Zhi));
            }
            sources.Add(CreateTravelSafetySource("流年支", context.YearZhi));
            sources.Add(CreateTravelSafetySource("流月支", context.MonthZhi));

            var clashes = new List<DailyTravelSafetyClash>();
            for (var firstIndex = 0; firstIndex < sources.Count; firstIndex++) {
                for (var secondIndex = firstIndex + 1; secondIndex < sources.Count; secondIndex++) {
                    if (IsClash(sources[firstIndex].Branch, sources[secondIndex].Branch)) {
                        clashes.Add(new DailyTravelSafetyClash(sources[firstIndex], sources[secondIndex]));
                    }
                }
            }

            var branches = sources.Select(source => source.Branch).ToArray();
            var punishments = BaZiDefine.ThreeXing
                .Where(group => group.All(branches.Contains))
                .Select(group => new DailyTravelSafetyPunishment([.. group]))
                .ToArray();
            var hasRepeatedClash = clashes
                .GroupBy(clash => GetClashKey(clash.First.Branch, clash.Second.Branch))
                .Any(group => group.Count() >= 2);
            var travelBranchCount = sources.Count(source => source.IsTravelBranch);
            DailySignalLevel? level = punishments.Length > 0 || hasRepeatedClash
                ? DailySignalLevel.HighAttention
                : (clashes.Count > 0 || travelBranchCount >= 2 ? DailySignalLevel.Attention : null);

            return new DailyTravelSafetyPeriodBackground(
                startDate,
                endDate,
                level,
                sources,
                clashes,
                punishments,
                hasRepeatedClash,
                info.IsBirthTimeAccurate
            );
        }

        private static DailyTravelSafetyBranchSource CreateTravelSafetySource(string label, DiZhi branch) {
            return new DailyTravelSafetyBranchSource(label, branch, CourseRuleCatalog.TravelBranches.Contains(branch));
        }

        private static bool HasSameTravelSafetyContext(DailyPeriodContext first, DailyPeriodContext second) {
            return first.YearZhi == second.YearZhi
                && first.MonthZhi == second.MonthZhi
                && first.DaYun?.Zhi == second.DaYun?.Zhi;
        }

        private static (int First, int Second) GetClashKey(DiZhi first, DiZhi second) {
            var firstValue = (int)first;
            var secondValue = (int)second;
            return firstValue <= secondValue
                ? (firstValue, secondValue)
                : (secondValue, firstValue);
        }

        private CapacityAssessment EvaluateOpportunityCapacity(
            BaZiInfo info,
            int year,
            DailyPeriodContext context
        ) {
            if (context.DaYun is null) {
                return new CapacityAssessment(
                    DailySignalLevel.VerificationRequired,
                    false,
                    $"本命為{info.StrengthStatus.ToGeJuString()}，但目前查無涵蓋 {year} 年的大運，尚不能把事件窗口解讀成可承接的結果。"
                );
            }

            DaYunFavorabilityContext daYunContext = _periodFavorabilityService.EvaluateDaYun(
                info,
                context.DaYun,
                year
            );
            var daYunGroup = daYunContext.PrimaryTenGod;
            var daYunIsFavorable = daYunContext.PrimaryIsFavorable;
            var yearGroups = GetYearTenGodGroups(info, context);
            var favorableYearGroups = yearGroups
                .Where(group => _periodFavorabilityService.IsPeriodFavorable(info, daYunContext, group))
                .ToArray();
            var unfavorableYearGroups = yearGroups
                .Where(group => !_periodFavorabilityService.IsPeriodFavorable(info, daYunContext, group))
                .ToArray();
            var periodDescription = DescribePeriodContext(
                year,
                context.DaYun,
                daYunContext,
                daYunGroup,
                favorableYearGroups,
                unfavorableYearGroups
            );

            if (info.RequiresStrengthVerification) {
                var direction = daYunIsFavorable && favorableYearGroups.Length > 0
                    ? "目前大運與流年仍見順勢訊號"
                    : "目前大運或流年帶有逆勢、破格訊號";
                return new CapacityAssessment(
                    DailySignalLevel.VerificationRequired,
                    false,
                    $"本命屬疑似{info.StrengthStatus.ToGeJuString()}，須用過往年份回驗後才能定格；{direction}。{periodDescription}"
                );
            }

            if (daYunIsFavorable) {
                var yearAdvice = (favorableYearGroups.Length, unfavorableYearGroups.Length) switch {
                    ( > 0, 0) => "流年也在大運調整後的當期喜用方向，承接條件相對有支撐",
                    ( > 0, > 0) => "流年有加分與耗洩並存，可把握窗口但要控制負荷",
                    _ => "流年偏原局忌神方向，事件耗洩較強；因大運先提供支撐，仍可承接但應控制規模"
                };
                return new CapacityAssessment(
                    DailySignalLevel.Opportunity,
                    true,
                    $"本命為{info.StrengthStatus.ToGeJuString()}，大運主作用在喜用方向；{yearAdvice}。{periodDescription}"
                );
            }

            if (unfavorableYearGroups.Length == 0) {
                return new CapacityAssessment(
                    DailySignalLevel.Attention,
                    false,
                    $"本命為{info.StrengthStatus.ToGeJuString()}，流年有短期補強，但大運主背景仍不在喜用方向，只宜視為暫時緩衝。{periodDescription}"
                );
            }

            return new CapacityAssessment(
                DailySignalLevel.Attention,
                false,
                $"本命為{info.StrengthStatus.ToGeJuString()}，目前大運不在喜用方向，流年也仍有耗洩或失衡訊號，宜保守承接。{periodDescription}"
            );
        }

        private static HealthBackgroundAssessment EvaluateHealthBackground(
            BaZiInfo info,
            int year,
            DailyPeriodContext context,
            WuXing weakElement
        ) {
            if (context.DaYun is null) {
                return new HealthBackgroundAssessment(false, $"目前查無涵蓋 {year} 年的大運，長期健康背景需另行確認。");
            }

            var daYunElement = GetActiveDaYunElement(info, context.DaYun, year);
            var yearElements = GetYearElements(context);
            var daYunSupports = Supports(daYunElement, weakElement);
            var daYunChallenges = Challenges(daYunElement, weakElement);
            var yearSupports = yearElements.Any(element => Supports(element, weakElement));
            var yearChallenges = yearElements.Any(element => Challenges(element, weakElement));
            var phase = GetDaYunPhaseLabel(context.DaYun, year);

            if (daYunChallenges && yearChallenges) {
                return new HealthBackgroundAssessment(
                    true,
                    $"大運{phase}主作用與流年都對此弱項帶有剋、洩或耗，形成跨期間疊加；這是提高保養優先級的訊號，不代表必然發病。"
                );
            }

            if (daYunSupports && yearChallenges) {
                return new HealthBackgroundAssessment(
                    false,
                    $"大運{phase}對此弱項有長期支撐，流年則有短期剋、洩或耗；保留大運支撐，但該年仍宜多留意。"
                );
            }

            if (daYunChallenges && yearSupports) {
                return new HealthBackgroundAssessment(
                    false,
                    $"大運{phase}對此弱項較不利，流年有短期補強與緩衝；宜把握時間檢查、休養或處理既有症狀。"
                );
            }

            if (daYunSupports || yearSupports) {
                return new HealthBackgroundAssessment(false, "大運或流年對此弱項仍見補強，可視為相對支撐，但不免除保養與就醫。");
            }

            return new HealthBackgroundAssessment(false, "大運與流年沒有明顯直接補強；維持保守作息，並以實際症狀及檢查結果為準。");
        }

        private HealthBackgroundAssessment EvaluateCongBackground(
            BaZiInfo info,
            int year,
            DailyPeriodContext context
        ) {
            if (context.DaYun is null) {
                return new HealthBackgroundAssessment(false, $"目前查無涵蓋 {year} 年的大運，從格背景需另行確認。");
            }

            DaYunFavorabilityContext daYunContext = _periodFavorabilityService.EvaluateDaYun(
                info,
                context.DaYun,
                year
            );
            var daYunGroup = daYunContext.PrimaryTenGod;
            var daYunBreaks = !daYunContext.PrimaryIsFavorable;
            var yearGroups = GetYearTenGodGroups(info, context);
            var yearBreaks = yearGroups.Any(group =>
                !_periodFavorabilityService.IsPeriodFavorable(info, daYunContext, group)
            );
            var yearFollows = yearGroups.Any(group =>
                _periodFavorabilityService.IsPeriodFavorable(info, daYunContext, group)
            );
            var phase = GetDaYunPhaseLabel(context.DaYun, year);

            if (daYunBreaks && yearBreaks) {
                return new HealthBackgroundAssessment(
                    true,
                    $"疑似從格尚待回驗；大運{phase}主作用與流年都見逆勢訊號，形成跨期間疊加。"
                );
            }

            if (daYunBreaks && yearFollows) {
                return new HealthBackgroundAssessment(false, $"疑似從格尚待回驗；大運{phase}偏逆勢，流年仍有部分順勢緩衝。");
            }

            return new HealthBackgroundAssessment(false, $"疑似從格尚待回驗；大運{phase}或流年仍見順勢支撐，本日逆勢訊號偏短期。");
        }

        private PeerBackgroundAssessment EvaluatePeerBackground(
            BaZiInfo info,
            int year,
            DailyPeriodContext context
        ) {
            var peerIsFavorable = _periodFavorabilityService.IsNatalFavorable(info, ShiShen.BiJie);
            if (context.DaYun is null) {
                var missingSummary = peerIsFavorable
                    ? "比劫在長期喜忌上可幫扶日主，但短期仍可能同時帶來分財與同儕競爭；目前查無大運可供疊加判斷。"
                    : "比劫不在本命喜用方向，且短期仍可能帶來分財與同儕競爭；目前查無大運可供疊加判斷。";
                return new PeerBackgroundAssessment(false, missingSummary);
            }

            DaYunFavorabilityContext daYunContext = _periodFavorabilityService.EvaluateDaYun(
                info,
                context.DaYun,
                year
            );
            var daYunGroup = daYunContext.PrimaryTenGod;
            peerIsFavorable = _periodFavorabilityService.IsPeriodFavorable(
                info,
                daYunContext,
                ShiShen.BiJie
            );
            var yearGroups = GetYearTenGodGroups(info, context);
            var peerIsStacked = daYunGroup == ShiShen.BiJie || yearGroups.Contains(ShiShen.BiJie);
            var phase = GetDaYunPhaseLabel(context.DaYun, year);
            var summary = peerIsFavorable
                ? "比劫在長期喜忌上可幫扶或順勢，但短期的分財、競爭與權責問題仍可同時成立。"
                : "比劫不在本命喜用方向，除了短期競爭，也可能讓整體格局更失衡。";
            if (peerIsStacked) {
                summary += $"大運{phase}主作用或流年也見比劫，作用較集中。";
            }

            return new PeerBackgroundAssessment(!peerIsFavorable && peerIsStacked, summary);
        }

        private IReadOnlyList<DailyFortuneTenGodFactor> GetTenGodFactors(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            IReadOnlyCollection<ShiShen> targetGroups
        ) {
            var factors = new List<DailyFortuneTenGodFactor>();
            DaYunFavorabilityContext? daYunContext = context.DaYun is null
                ? null
                : _periodFavorabilityService.EvaluateDaYun(info, context.DaYun, day.Date.Year);
            string reason = daYunContext is null
                ? "目前查無大運，暫依本命格局"
                : _periodFavorabilityService.GetPeriodReason(info, daYunContext);
            var ganGroup = day.Gan.ToShiShen(info.DayZhu.Gan).ToCombined();
            if (targetGroups.Contains(ganGroup)) {
                factors.Add(CreateTenGodFactor(info, daYunContext, reason, "天干", ganGroup));
            }

            foreach (var group in day.Zhi.ToFullWuXing()
                         .Keys
                         .Select(element => element.ToShiShen(info.RiZhu).ToCombined())
                         .Where(targetGroups.Contains)
                         .Distinct()) {
                factors.Add(CreateTenGodFactor(info, daYunContext, reason, "地支藏干", group));
            }

            return factors;
        }

        private static IReadOnlyList<ShiShen> GetYearTenGodGroups(BaZiInfo info, DailyPeriodContext context) {
            return [
                context.YearGan.ToShiShen(info.DayZhu.Gan).ToCombined(),
                .. context.YearZhi.ToFullWuXing()
                    .Keys
                    .Select(element => element.ToShiShen(info.RiZhu).ToCombined())
                    .Distinct()
            ];
        }

        private static IReadOnlyList<WuXing> GetYearElements(DailyPeriodContext context) {
            return [context.YearGan.ToWuXing(), .. context.YearZhi.ToFullWuXing().Keys];
        }

        private static WuXing GetActiveDaYunElement(BaZiInfo info, DaYun daYun, int year) {
            var group = daYun.GetPrimaryTenGod(info.DayZhu.Gan, year).ToCombined();
            return TenGodElementResolver.Resolve(info.RiZhu, group);
        }

        private DailyFortuneTenGodFactor CreateTenGodFactor(
            BaZiInfo info,
            DailyPeriodContext context,
            int year,
            string source,
            ShiShen tenGod
        ) {
            DaYunFavorabilityContext? daYunContext = context.DaYun is null
                ? null
                : _periodFavorabilityService.EvaluateDaYun(info, context.DaYun, year);
            string reason = daYunContext is null
                ? "目前查無大運，暫依本命格局"
                : _periodFavorabilityService.GetPeriodReason(info, daYunContext);
            return CreateTenGodFactor(info, daYunContext, reason, source, tenGod);
        }

        private DailyFortuneTenGodFactor CreateTenGodFactor(
            BaZiInfo info,
            DaYunFavorabilityContext? daYunContext,
            string reason,
            string source,
            ShiShen tenGod
        ) {
            bool isFavorable = daYunContext is null
                ? _periodFavorabilityService.IsNatalFavorable(info, tenGod)
                : _periodFavorabilityService.IsPeriodFavorable(info, daYunContext, tenGod);
            return new DailyFortuneTenGodFactor(source, tenGod, isFavorable, reason);
        }

        private static bool Supports(WuXing source, WuXing target) {
            return source == target || BaZiDefine.Generation[source] == target;
        }

        private static bool Challenges(WuXing source, WuXing target) {
            return source == BaZiDefine.RestrictBy[target]
                || source == BaZiDefine.Generation[target]
                || source == BaZiDefine.Restricting[target];
        }

        private static string DescribePeriodContext(
            int year,
            DaYun daYun,
            DaYunFavorabilityContext daYunContext,
            ShiShen daYunGroup,
            IReadOnlyCollection<ShiShen> favorableYearGroups,
            IReadOnlyCollection<ShiShen> unfavorableYearGroups
        ) {
            var phase = GetDaYunPhaseLabel(daYun, year);
            var daYunDirection = daYunContext.PrimaryIsFavorable ? "喜用" : "忌神";
            var yearDescription = (favorableYearGroups.Count, unfavorableYearGroups.Count) switch {
                ( > 0, 0) => $"流年見{FormatTenGodGroups(favorableYearGroups)}，皆在喜用方向",
                (0, > 0) => $"流年見{FormatTenGodGroups(unfavorableYearGroups)}，偏忌神方向",
                _ => $"流年同見喜用的{FormatTenGodGroups(favorableYearGroups)}與需留意的{FormatTenGodGroups(unfavorableYearGroups)}"
            };
            return $"大運{phase}主作用為{daYunGroup.ToShenString()}（{daYunDirection}）；{yearDescription}。";
        }

        private static string GetDaYunPhaseLabel(DaYun daYun, int year) {
            return daYun.GetPhase(year) == DaYunPhase.FirstFiveYears
                ? "前五年天干"
                : "後五年地支";
        }

        private static string FormatTenGodGroups(IEnumerable<ShiShen> groups) {
            return string.Join("、", groups.Distinct().Select(group => group.ToShenString()));
        }

        private sealed record CapacityAssessment(DailySignalLevel Level, bool CanAct, string Summary);

        private sealed record HealthBackgroundAssessment(bool IsStackedRisk, string Summary);

        private sealed record PeerBackgroundAssessment(bool IsStackedRisk, string Summary);

        private static IReadOnlyList<WuXing> GetWeakElements(BaZiInfo info) {
            var counts = BaZiDefine.WuXingList.ToDictionary(element => element, _ => 0);
            foreach (var pillar in new[] { info.YearZhu, info.MonthZhu, info.DayZhu, info.HourZhu }) {
                counts[pillar.GanWuXing]++;
                counts[pillar.ZhiWuXing]++;
            }
            var minimum = counts.Values.Min();
            return counts.Where(pair => pair.Value == minimum).Select(pair => pair.Key).ToArray();
        }

        private static DaYun? FindDaYun(BaZiInfo info, int year) {
            return info.DaYunList.FirstOrDefault(daYun => year >= daYun.StartYear && year <= daYun.EndYear);
        }

        private bool IsClash(DiZhi first, DiZhi second) {
            return _relationshipEngine.HasRelationship(first, second, BranchRelationshipType.SixClash);
        }

        private static string GetDayKey(LiuRi day) => $"{day.Gan.ToGanString()}{day.Zhi.ToZhiString()}";

        private static HashSet<string> Days(params string[] days) => [.. days];
    }
}

using BaZi.Models;

namespace BaZi.Services {

    /// <summary>依筆記中的明確規則提供流日主題分析。</summary>
    public sealed class DailyFortuneService {
        private static readonly IReadOnlyDictionary<WuXing, DiZhi[]> WealthBranches =
            new Dictionary<WuXing, DiZhi[]> {
                [WuXing.Jin] = [DiZhi.Chen, DiZhi.Yin, DiZhi.Mao, DiZhi.Hai, DiZhi.Zi, DiZhi.Wei],
                [WuXing.Mu] = [DiZhi.Yin, DiZhi.Wu, DiZhi.Xu, DiZhi.Wei, DiZhi.Si],
                [WuXing.Shui] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Xu],
                [WuXing.Huo] = [DiZhi.Chou, DiZhi.Chen, DiZhi.Shen, DiZhi.You],
                [WuXing.Tu] = [DiZhi.Shen, DiZhi.You, DiZhi.Zi, DiZhi.Chou, DiZhi.Hai, DiZhi.Chen]
            };

        private static readonly IReadOnlyDictionary<WuXing, DiZhi[]> CareerBranches =
            new Dictionary<WuXing, DiZhi[]> {
                [WuXing.Jin] = [DiZhi.Yin, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Xu],
                [WuXing.Mu] = [DiZhi.Chou, DiZhi.Si, DiZhi.Shen, DiZhi.You, DiZhi.Xu, DiZhi.Chen],
                [WuXing.Shui] = [DiZhi.Chou, DiZhi.Yin, DiZhi.Chen, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Shen, DiZhi.Xu],
                [WuXing.Huo] = [DiZhi.Zi, DiZhi.Chou, DiZhi.Chen, DiZhi.Shen, DiZhi.Hai],
                [WuXing.Tu] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen, DiZhi.Wei, DiZhi.Hai]
            };

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

        private static readonly IReadOnlyDictionary<WuXing, string> HealthParts =
            new Dictionary<WuXing, string> {
                [WuXing.Mu] = "肝膽、四肢與筋骨",
                [WuXing.Huo] = "心血管與眼睛",
                [WuXing.Tu] = "脾胃、腸胃與消化",
                [WuXing.Jin] = "呼吸道、肺、大腸與皮膚",
                [WuXing.Shui] = "腎臟、膀胱、泌尿與循環"
            };

        private static readonly DiZhi[] TravelBranches = [DiZhi.Yin, DiZhi.Shen, DiZhi.Si, DiZhi.Hai];

        /// <summary>分析指定國曆月份的每一天。</summary>
        public IReadOnlyList<DailyFortuneResult> AnalyzeMonth(
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
            return results;
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

        private static void AddWealthSignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var wealth = BaZiDefine.Restricting[info.RiZhu];
            var output = BaZiDefine.Generation[info.RiZhu];
            var ganElement = day.Gan.ToWuXing();
            var branchElements = day.Zhi.ToFullWuXing();
            var acceptedBranch = WealthBranches[info.RiZhu].Contains(day.Zhi);
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
                GetTenGodFactors(info, day, [ShiShen.Cai, ShiShen.ShihShang])
            ));
        }

        private static void AddCareerSignal(
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
            var isPrimary = CareerBranches[info.RiZhu].Contains(day.Zhi);
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
                GetTenGodFactors(info, day, [ShiShen.GuanSha, ShiShen.Cai])
            ));
        }

        private static void AddRomanceSignal(
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
                    GetTenGodFactors(info, day, [ShiShen.Cai, ShiShen.ShihShang])
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
                GetTenGodFactors(info, day, [ShiShen.GuanSha, ShiShen.Cai])
            ));
        }

        private static void AddHealthSignals(
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
                    $"日主同五行能量集中，較需留意{HealthParts[affected]}的負擔。{background.Summary}",
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
                    $"命盤最弱的{weakElement.ToWuXingString()}受到強勢五行牽制，較需留意{HealthParts[weakElement]}。{background.Summary}",
                    "提前調整飲食、作息與運動強度；日期只用來安排保守作息，不作疾病診斷。"
                ));
            }
        }

        private static void AddCongHealthSignal(
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

        private static void AddInterpersonalSignal(
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
                GetTenGodFactors(info, day, [ShiShen.BiJie])
            ));
        }

        private static void AddTravelSafetySignal(
            BaZiInfo info,
            LiuRi day,
            DailyPeriodContext context,
            ICollection<DailyFortuneSignal> signals
        ) {
            var branches = new List<DiZhi> {
                info.YearZhu.Zhi,
                info.MonthZhu.Zhi,
                info.DayZhu.Zhi
            };
            if (info.IsBirthTimeAccurate) {
                branches.Add(info.HourZhu.Zhi);
            }
            if (context.DaYun is not null) {
                branches.Add(context.DaYun.Zhi);
            }
            branches.Add(context.YearZhi);
            branches.Add(context.MonthZhi);
            branches.Add(day.Zhi);

            var punishment = BaZiDefine.ThreeXing.FirstOrDefault(group => group.All(branches.Contains));
            var dayClashes = branches.Take(branches.Count - 1).Count(branch => IsClash(day.Zhi, branch));
            var travelCount = branches.Count(TravelBranches.Contains);
            if (punishment is null && dayClashes == 0 && travelCount < 3) {
                return;
            }

            var isHighAttention = punishment is not null || dayClashes >= 2;
            var details = new List<string>();
            if (punishment is not null) {
                details.Add($"跨本命與運勢層湊成{string.Join(string.Empty, punishment.Select(branch => branch.ToZhiString()))}三刑");
            }
            if (dayClashes > 0) {
                details.Add($"流日地支形成 {dayClashes} 組相沖");
            }
            if (travelCount >= 3) {
                details.Add($"各層共有 {travelCount} 個驛馬地支");
            }

            signals.Add(new DailyFortuneSignal(
                DailyFortuneTopic.TravelSafety,
                isHighAttention ? DailySignalLevel.HighAttention : DailySignalLevel.Attention,
                isHighAttention ? "交通與移動高注意日" : "交通與移動注意日",
                string.Join("；", details) + "。",
                "不要疲勞駕駛、酒駕或趕路；檢查輪胎、煞車、燈具並替長途、雨夜與陌生路線預留緩衝。"
            ));
        }

        private static void AddMovingSignal(
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

        private static CapacityAssessment EvaluateOpportunityCapacity(
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

            var daYunGroup = context.DaYun.GetPrimaryTenGod(info.DayZhu.Gan, year).ToCombined();
            var daYunIsFavorable = IsFavorable(info, daYunGroup);
            var yearGroups = GetYearTenGodGroups(info, context);
            var favorableYearGroups = yearGroups.Where(group => IsFavorable(info, group)).ToArray();
            var unfavorableYearGroups = yearGroups.Where(group => !IsFavorable(info, group)).ToArray();
            var periodDescription = DescribePeriodContext(
                info,
                year,
                context.DaYun,
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
                    ( > 0, 0) => "流年也在本命喜用方向，承接條件相對有支撐",
                    ( > 0, > 0) => "流年有加分與耗洩並存，可把握窗口但要控制負荷",
                    _ => "流年偏原局忌神方向，事件耗洩較強；因大運先提供支撐，仍可承接但應控制規模"
                };
                return new CapacityAssessment(
                    DailySignalLevel.Opportunity,
                    true,
                    $"本命為{info.StrengthStatus.ToGeJuString()}，大運在喜用方向；{yearAdvice}。{periodDescription}"
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

        private static HealthBackgroundAssessment EvaluateCongBackground(
            BaZiInfo info,
            int year,
            DailyPeriodContext context
        ) {
            if (context.DaYun is null) {
                return new HealthBackgroundAssessment(false, $"目前查無涵蓋 {year} 年的大運，從格背景需另行確認。");
            }

            var daYunGroup = context.DaYun.GetPrimaryTenGod(info.DayZhu.Gan, year).ToCombined();
            var daYunBreaks = !IsFavorable(info, daYunGroup);
            var yearGroups = GetYearTenGodGroups(info, context);
            var yearBreaks = yearGroups.Any(group => !IsFavorable(info, group));
            var yearFollows = yearGroups.Any(group => IsFavorable(info, group));
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

        private static PeerBackgroundAssessment EvaluatePeerBackground(
            BaZiInfo info,
            int year,
            DailyPeriodContext context
        ) {
            var peerIsFavorable = info.LikeWuXing.Contains(info.RiZhu);
            if (context.DaYun is null) {
                var missingSummary = peerIsFavorable
                    ? "比劫在長期喜忌上可幫扶日主，但短期仍可能同時帶來分財與同儕競爭；目前查無大運可供疊加判斷。"
                    : "比劫不在本命喜用方向，且短期仍可能帶來分財與同儕競爭；目前查無大運可供疊加判斷。";
                return new PeerBackgroundAssessment(false, missingSummary);
            }

            var daYunGroup = context.DaYun.GetPrimaryTenGod(info.DayZhu.Gan, year).ToCombined();
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

        private static IReadOnlyList<DailyFortuneTenGodFactor> GetTenGodFactors(
            BaZiInfo info,
            LiuRi day,
            IReadOnlyCollection<ShiShen> targetGroups
        ) {
            var factors = new List<DailyFortuneTenGodFactor>();
            var ganGroup = day.Gan.ToShiShen(info.DayZhu.Gan).ToCombined();
            if (targetGroups.Contains(ganGroup)) {
                factors.Add(new DailyFortuneTenGodFactor("天干", ganGroup));
            }

            foreach (var group in day.Zhi.ToFullWuXing()
                         .Keys
                         .Select(element => element.ToShiShen(info.RiZhu).ToCombined())
                         .Where(targetGroups.Contains)
                         .Distinct()) {
                factors.Add(new DailyFortuneTenGodFactor("地支藏干", group));
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

        private static bool IsFavorable(BaZiInfo info, ShiShen group) {
            var element = TenGodElementResolver.Resolve(info.RiZhu, group);
            return info.LikeWuXing.Contains(element);
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
            BaZiInfo info,
            int year,
            DaYun daYun,
            ShiShen daYunGroup,
            IReadOnlyCollection<ShiShen> favorableYearGroups,
            IReadOnlyCollection<ShiShen> unfavorableYearGroups
        ) {
            var phase = GetDaYunPhaseLabel(daYun, year);
            var daYunDirection = IsFavorable(info, daYunGroup) ? "喜用" : "忌神";
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

        private static bool IsClash(DiZhi first, DiZhi second) {
            return first != second && BaZiDefine.Chong.Any(pair => pair.Contains(first) && pair.Contains(second));
        }

        private static string GetDayKey(LiuRi day) => $"{day.Gan.ToGanString()}{day.Zhi.ToZhiString()}";

        private static HashSet<string> Days(params string[] days) => [.. days];
    }
}

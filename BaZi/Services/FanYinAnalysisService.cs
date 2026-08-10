using BaZi.Models;

namespace BaZi.Services {

    /// <summary>依原命四柱中天干相剋、地支相剋相沖判斷反吟。</summary>
    public sealed class FanYinAnalysisService {

        /// <summary>分析原命盤內的反吟，不納入大運、流年或流月。</summary>
        /// <param name="info">八字命盤。</param>
        /// <returns>反吟分析結果。</returns>
        public FanYinAnalysisResult AnalyzeNatal(BaZiInfo info) {
            ArgumentNullException.ThrowIfNull(info);

            IReadOnlyList<Zhu> pillars = info.IsBirthTimeAccurate
                ? [info.YearZhu, info.MonthZhu, info.DayZhu, info.HourZhu]
                : [info.YearZhu, info.MonthZhu, info.DayZhu];

            return AnalyzeNatalPillars(pillars, info.IsBirthTimeAccurate);
        }

        /// <summary>分析指定的原命柱，供無完整命盤的情境與測試使用。</summary>
        /// <param name="pillars">按年、月、日、時順序排列的原命柱。</param>
        /// <param name="includesHourPillar">是否已納入準確時柱。</param>
        /// <returns>反吟分析結果。</returns>
        public FanYinAnalysisResult AnalyzeNatalPillars(
            IReadOnlyList<Zhu> pillars,
            bool includesHourPillar
        ) {
            ArgumentNullException.ThrowIfNull(pillars);

            var matches = new List<FanYinMatch>();
            for (var i = 0; i < pillars.Count; i++) {
                for (var j = i + 1; j < pillars.Count; j++) {
                    if (IsFanYinPair(pillars[i], pillars[j])) {
                        Zhu[] matchPillars = [pillars[i], pillars[j]];
                        matches.Add(
                            new FanYinMatch(
                                matchPillars,
                                GetFanYinNatalSituation(matchPillars, includesHourPillar)
                            )
                        );
                    }
                }
            }

            return new FanYinAnalysisResult(matches, includesHourPillar);
        }

        /// <summary>分析大運、流年或流月與原命及較早運柱形成的反吟，並檢查是否合走既有反吟。</summary>
        /// <param name="info">八字命盤。</param>
        /// <param name="periodPillar">要分析的大運、流年或流月。</param>
        /// <param name="earlierFortunePillars">發生於指定期間之前、要一併比較的運柱。</param>
        /// <returns>指定期間的反吟與合走分析結果。</returns>
        public PeriodFanYinAnalysisResult AnalyzePeriod(
            BaZiInfo info,
            IGanZhi periodPillar,
            IReadOnlyList<IGanZhi>? earlierFortunePillars = null
        ) {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(periodPillar);

            var sourcePillars = new List<IGanZhi> {
                info.YearZhu,
                info.MonthZhu,
                info.DayZhu
            };
            if (info.IsBirthTimeAccurate) {
                sourcePillars.Add(info.HourZhu);
            }
            if (earlierFortunePillars is not null) {
                sourcePillars.AddRange(earlierFortunePillars);
            }

            return AnalyzePeriodPillars(sourcePillars, periodPillar, info.IsBirthTimeAccurate);
        }

        /// <summary>分析指定期間與較早命運柱形成的反吟及合走，供測試與組合運柱使用。</summary>
        /// <param name="sourcePillars">原命柱與發生於指定期間之前的運柱。</param>
        /// <param name="periodPillar">要分析的大運、流年或流月。</param>
        /// <param name="includesHourPillar">是否已納入準確時柱。</param>
        /// <returns>指定期間的反吟與合走分析結果。</returns>
        public PeriodFanYinAnalysisResult AnalyzePeriodPillars(
            IReadOnlyList<IGanZhi> sourcePillars,
            IGanZhi periodPillar,
            bool includesHourPillar
        ) {
            ArgumentNullException.ThrowIfNull(sourcePillars);
            ArgumentNullException.ThrowIfNull(periodPillar);

            var matches = sourcePillars
                .Where(source => IsFanYinPair(source, periodPillar))
                .Select(source => new PeriodFanYinMatch(
                    source,
                    periodPillar,
                    GetFanYinPeriodSituation(source.Id, periodPillar.Id)
                ))
                .ToList();
            var mitigations = FindMitigations(sourcePillars, periodPillar);

            return new PeriodFanYinAnalysisResult(matches, mitigations, includesHourPillar);
        }

        /// <summary>取得反吟對應的狀況說明</summary>
        /// <param name="pillars">按年、月、日、時順序排列的原命柱。</param>
        /// <param name="includesHourPillar">是否已納入準確時柱。</param>
        /// <returns>反吟狀況。</returns>
        public static string GetFanYinNatalSituation(IEnumerable<Zhu> pillars, bool includesHourPillar) {
            if (pillars.Any(p => p.Id == "年柱") && pillars.Any(p => p.Id == "月柱")) {
                return "早年環境多變，父母長輩緣薄或意見不合；青少年時期容易搬遷、生活起伏較大";
            } else if (pillars.Any(p => p.Id == "月柱") && pillars.Any(p => p.Id == "日柱")) {
                return "自己與配偶、家庭衝突多；內心常處於矛盾、焦慮或動盪狀態，事業與居住地易變動";
            } else if (includesHourPillar && pillars.Any(p => p.Id == "時柱") && pillars.Any(p => p.Id == "日柱")) {
                return "中晚年運勢震盪，與子女緣分較淺、聚少離多；須注意自身健康與老年勞碌";
            } else {
                return "人生容易動盪、心神不寧、內心衝突";
            }
        }

        /// <summary>天干是否有反吟的跡象，兩天干相剋</summary>
        /// <param name="a">第一個天干</param>
        /// <param name="b">第二個天干</param>
        /// <returns>此兩干是否有反吟的跡象</returns>
        public static bool IsFanYinGan(TianGan a, TianGan b) {
            var wxa = a.ToWuXing();
            var wxb = b.ToWuXing();
            return BaZiDefine.Restricting[wxa] == wxb;
        }

        /// <summary>地支是否有反吟的跡象，兩地支相剋或相沖</summary>
        /// <param name="a">第一個地支</param>
        /// <param name="b">第二個地支</param>
        /// <returns>此地支是否有反吟的跡象</returns>
        public static bool IsFanYinZhi(DiZhi a, DiZhi b) {
            return a switch {
                DiZhi.Zi => b is DiZhi.Si or DiZhi.Wu,
                DiZhi.Chou => b is DiZhi.Hai or DiZhi.Zi or DiZhi.Wei,
                DiZhi.Yin => b is DiZhi.Chen or DiZhi.Xu or DiZhi.Chou or DiZhi.Wei or DiZhi.Shen,
                DiZhi.Mao => b is DiZhi.Chen or DiZhi.Xu or DiZhi.Chou or DiZhi.Wei or DiZhi.You,
                DiZhi.Chen => b is DiZhi.Hai or DiZhi.Zi or DiZhi.Xu,
                DiZhi.Si => b is DiZhi.Shen or DiZhi.You or DiZhi.Hai,
                DiZhi.Wu => b is DiZhi.Shen or DiZhi.You or DiZhi.Zi,
                DiZhi.Wei => b is DiZhi.Hai or DiZhi.Zi or DiZhi.Chou,
                DiZhi.Shen => b is DiZhi.Yin or DiZhi.Mao,
                DiZhi.You => b is DiZhi.Yin or DiZhi.Mao,
                DiZhi.Xu => b is DiZhi.Hai or DiZhi.Zi or DiZhi.Chen,
                DiZhi.Hai => b is DiZhi.Si or DiZhi.Wu,
                _ => false
            };
        }

        private static IReadOnlyList<FanYinMitigation> FindMitigations(
            IReadOnlyList<IGanZhi> sourcePillars,
            IGanZhi periodPillar
        ) {
            var mitigations = new List<FanYinMitigation>();
            for (var firstIndex = 0; firstIndex < sourcePillars.Count; firstIndex++) {
                for (var secondIndex = firstIndex + 1; secondIndex < sourcePillars.Count; secondIndex++) {
                    var first = sourcePillars[firstIndex];
                    var second = sourcePillars[secondIndex];
                    if (!IsFanYinPair(first, second)) {
                        continue;
                    }

                    if (IsFullyCombined(first, periodPillar)) {
                        mitigations.Add(new FanYinMitigation(first, second, first, periodPillar));
                    } else if (IsFullyCombined(second, periodPillar)) {
                        mitigations.Add(new FanYinMitigation(first, second, second, periodPillar));
                    }
                }
            }

            return mitigations;
        }

        private static bool IsFanYinPair(IGanZhi first, IGanZhi second) {
            bool firstRestrictsSecond = IsFanYinGan(first.Gan, second.Gan)
                && IsFanYinZhi(first.Zhi, second.Zhi);
            bool secondRestrictsFirst = IsFanYinGan(second.Gan, first.Gan)
                && IsFanYinZhi(second.Zhi, first.Zhi);
            return firstRestrictsSecond || secondRestrictsFirst;
        }

        private static bool IsFullyCombined(IGanZhi first, IGanZhi second) {
            bool ganCombined = BaZiDefine.FiveHe.Any(pair => pair.Contains(first.Gan) && pair.Contains(second.Gan));
            bool zhiCombined = BaZiDefine.SixHe.Values.Any(pair => pair.Contains(first.Zhi) && pair.Contains(second.Zhi));
            return ganCombined && zhiCombined;
        }

        private static string GetFanYinPeriodSituation(string sourcePillarId, string periodPillarId) {
            var subject = sourcePillarId switch {
                "年柱" => "家庭根基、長輩或早年環境",
                "月柱" => "職場、責任、父母或長輩關係",
                "日柱" => "自我狀態、家庭與親密關係",
                "時柱" => "子女、晚輩、未來規劃或晚年安排",
                "大運" => "此十年大運的主要方向",
                "流年" => "本年度已展開的安排",
                _ => "受沖剋干支所對應的課題"
            };
            var duration = periodPillarId switch {
                "大運" => "這段大運期間",
                "流年" => "這個流年內",
                "流月" => "這個流月內",
                _ => "這段期間"
            };

            return $"{duration}{subject}較容易出現拉扯、變動或重新安排；反吟描述的是變動訊號，不代表結果必然不利，仍須配合喜忌、宮位與其他合沖判讀";
        }
    }
}

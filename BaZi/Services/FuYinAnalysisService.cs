using BaZi.Models;

namespace BaZi.Services {

    /// <summary>依原命四柱中完整重複的干支判斷伏吟。</summary>
    public sealed class FuYinAnalysisService {

        /// <summary>分析原命盤內的伏吟，不納入大運、流年或流月。</summary>
        /// <param name="info">八字命盤。</param>
        /// <returns>伏吟分析結果。</returns>
        public FuYinAnalysisResult AnalyzeNatal(BaZiInfo info) {
            ArgumentNullException.ThrowIfNull(info);

            IReadOnlyList<Zhu> pillars = info.IsBirthTimeAccurate
                ? [info.YearZhu, info.MonthZhu, info.DayZhu, info.HourZhu]
                : [info.YearZhu, info.MonthZhu, info.DayZhu];

            return AnalyzeNatalPillars(pillars, info.IsBirthTimeAccurate);
        }

        /// <summary>分析指定的原命柱，供無完整命盤的情境與測試使用。</summary>
        /// <param name="pillars">按年、月、日、時順序排列的原命柱。</param>
        /// <param name="includesHourPillar">是否已納入準確時柱。</param>
        /// <returns>伏吟分析結果。</returns>
        public FuYinAnalysisResult AnalyzeNatalPillars(
            IReadOnlyList<Zhu> pillars,
            bool includesHourPillar
        ) {
            ArgumentNullException.ThrowIfNull(pillars);

            var matches = pillars
                .GroupBy(pillar => (pillar.Gan, pillar.Zhi))
                .Where(group => group.Count() >= 2)
                .Select(group => new FuYinMatch(
                    group.Key.Gan,
                    group.Key.Zhi,
                    [.. group.Select(pillar => pillar.Id)],
                    GetFuYinNatalSituation(group, includesHourPillar)
                ))
                .ToList();

            return new FuYinAnalysisResult(matches, includesHourPillar);
        }

        /// <summary>分析大運、流年或流月與原命及較早運柱形成的伏吟。</summary>
        /// <param name="info">八字命盤。</param>
        /// <param name="periodPillar">要分析的大運、流年或流月。</param>
        /// <param name="earlierFortunePillars">發生於指定期間之前、要一併比較的運柱。</param>
        /// <returns>指定期間的伏吟分析結果。</returns>
        public PeriodFuYinAnalysisResult AnalyzePeriod(
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

        /// <summary>分析指定期間與較早命運柱形成的伏吟，供測試與組合運柱使用。</summary>
        /// <param name="sourcePillars">原命柱與發生於指定期間之前的運柱。</param>
        /// <param name="periodPillar">要分析的大運、流年或流月。</param>
        /// <param name="includesHourPillar">是否已納入準確時柱。</param>
        /// <returns>指定期間的伏吟分析結果。</returns>
        public PeriodFuYinAnalysisResult AnalyzePeriodPillars(
            IReadOnlyList<IGanZhi> sourcePillars,
            IGanZhi periodPillar,
            bool includesHourPillar
        ) {
            ArgumentNullException.ThrowIfNull(sourcePillars);
            ArgumentNullException.ThrowIfNull(periodPillar);

            var matches = sourcePillars
                .Where(source => source.Gan == periodPillar.Gan && source.Zhi == periodPillar.Zhi)
                .Select(source => new PeriodFuYinMatch(
                    source,
                    periodPillar,
                    GetFuYinPeriodSituation(source.Id, periodPillar.Id)
                ))
                .ToList();

            return new PeriodFuYinAnalysisResult(matches, includesHourPillar);
        }

        /// <summary>取得伏吟對應的狀況說明</summary>
        /// <param name="pillars">按年、月、日、時順序排列的原命柱。</param>
        /// <param name="includesHourPillar">是否已納入準確時柱。</param>
        /// <returns>伏吟狀況。</returns>
        public static string GetFuYinNatalSituation(IGrouping<(TianGan Gan, DiZhi Zhi), Zhu> pillars, bool includesHourPillar) {
            if (pillars.Any(p => p.Id == "年柱") && pillars.Any(p => p.Id == "日柱")) {
                return "根基不穩，可能離鄉背井或與長輩緣薄、意見多；對自己的出身或過去的事情常感到放不下";
            } else if (pillars.Any(p => p.Id == "月柱") && pillars.Any(p => p.Id == "日柱")) {
                return "職場環境或工作內容容易重複更動、面臨瓶頸；內心常在工作選擇上反覆猶豫，自我衝突大";
            } else if (includesHourPillar && pillars.Any(p => p.Id == "時柱") && pillars.Any(p => p.Id == "日柱")) {
                return "對子女或晚輩操心特別多；晚年容易感到孤獨、胡思亂想，或為健康問題煩心";
            } else {
                return "心志反覆、自我糾結，容易產生波折、牽掛與內心壓力";
            }
        }

        private static string GetFuYinPeriodSituation(string sourcePillarId, string periodPillarId) {
            var subject = sourcePillarId switch {
                "年柱" => "家庭根基、長輩或早年形成的模式",
                "月柱" => "職場、責任、父母或長輩相關的既有課題",
                "日柱" => "自我狀態、家庭與親密關係的既有課題",
                "時柱" => "子女、晚輩、未來規劃或晚年相關的課題",
                "大運" => "此十年大運的主題與節奏",
                "流年" => "本年度已浮現的主題與節奏",
                _ => "相同干支所對應的課題"
            };
            var duration = periodPillarId switch {
                "大運" => "這段大運期間",
                "流年" => "這個流年內",
                "流月" => "這個流月內",
                _ => "這段期間"
            };

            return $"{duration}{subject}容易反覆出現或進展較慢，適合回頭整理舊問題；伏吟本身不直接等同吉凶，仍須配合喜忌與整體命局判讀";
        }
    }
}

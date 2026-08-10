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
    }
}

using BaZi.Models;

namespace BaZi.Services {

    /// <summary>集中保存筆記明列的五行順序、流運查表與健康對應。</summary>
    public static class CourseRuleCatalog {
        public static IReadOnlyList<WuXing> ElementOrder { get; } = [
            WuXing.Mu,
            WuXing.Huo,
            WuXing.Tu,
            WuXing.Jin,
            WuXing.Shui
        ];

        /// <summary>筆記 3-2 用於大運、流年與流月的財星地支表。</summary>
        public static IReadOnlyDictionary<WuXing, DiZhi[]> PeriodWealthBranches { get; } =
            new Dictionary<WuXing, DiZhi[]> {
                [WuXing.Jin] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen, DiZhi.Wei, DiZhi.Hai],
                [WuXing.Mu] = [DiZhi.Yin, DiZhi.Chen, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Shen, DiZhi.Xu],
                [WuXing.Shui] = [DiZhi.Yin, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Xu],
                [WuXing.Huo] = [DiZhi.Chou, DiZhi.Si, DiZhi.Shen, DiZhi.You, DiZhi.Xu],
                [WuXing.Tu] = [DiZhi.Zi, DiZhi.Chou, DiZhi.Chen, DiZhi.Shen, DiZhi.Hai]
            };

        /// <summary>筆記 5-2 用於流日選日的財星地支表；其課程口徑與 3-2 不同。</summary>
        public static IReadOnlyDictionary<WuXing, DiZhi[]> DailyWealthBranches { get; } =
            new Dictionary<WuXing, DiZhi[]> {
                [WuXing.Jin] = [DiZhi.Chen, DiZhi.Yin, DiZhi.Mao, DiZhi.Hai, DiZhi.Zi, DiZhi.Wei],
                [WuXing.Mu] = [DiZhi.Yin, DiZhi.Wu, DiZhi.Xu, DiZhi.Wei, DiZhi.Si],
                [WuXing.Shui] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Xu],
                [WuXing.Huo] = [DiZhi.Chou, DiZhi.Chen, DiZhi.Shen, DiZhi.You],
                [WuXing.Tu] = [DiZhi.Shen, DiZhi.You, DiZhi.Zi, DiZhi.Chou, DiZhi.Hai, DiZhi.Chen]
            };

        /// <summary>筆記 3-4 與流日頁共用的五日主官殺地支表。</summary>
        public static IReadOnlyDictionary<WuXing, DiZhi[]> CareerBranches { get; } =
            new Dictionary<WuXing, DiZhi[]> {
                [WuXing.Jin] = [DiZhi.Yin, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Xu],
                [WuXing.Mu] = [DiZhi.Chou, DiZhi.Si, DiZhi.Shen, DiZhi.You, DiZhi.Xu, DiZhi.Chen],
                [WuXing.Shui] = [DiZhi.Chou, DiZhi.Yin, DiZhi.Chen, DiZhi.Si, DiZhi.Wu, DiZhi.Wei, DiZhi.Shen, DiZhi.Xu],
                [WuXing.Huo] = [DiZhi.Zi, DiZhi.Chou, DiZhi.Chen, DiZhi.Shen, DiZhi.Hai],
                [WuXing.Tu] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen, DiZhi.Wei, DiZhi.Hai]
            };

        public static IReadOnlyDictionary<WuXing, string> HealthParts { get; } =
            new Dictionary<WuXing, string> {
                [WuXing.Mu] = "肝、膽、四肢與筋骨",
                [WuXing.Huo] = "心臟、心血管與眼睛",
                [WuXing.Tu] = "脾胃、腸胃與消化吸收",
                [WuXing.Jin] = "呼吸道、肺、支氣管、大腸與皮膚",
                [WuXing.Shui] = "腎臟、膀胱、泌尿與循環系統"
            };

        public static IReadOnlySet<DiZhi> TravelBranches { get; } =
            new HashSet<DiZhi> { DiZhi.Yin, DiZhi.Shen, DiZhi.Si, DiZhi.Hai };
    }
}

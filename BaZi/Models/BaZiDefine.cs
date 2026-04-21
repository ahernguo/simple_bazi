using System.Collections.Concurrent;

namespace BaZi.Models {

    public static class BaZiDefine {

        /// <summary>取得五行的相生關係，即 '我生'</summary>
        public static IDictionary<WuXing, WuXing> Generation { get; }
            = new ConcurrentDictionary<WuXing, WuXing> {
                [WuXing.Huo] = WuXing.Tu,
                [WuXing.Tu] = WuXing.Jin,
                [WuXing.Jin] = WuXing.Shui,
                [WuXing.Shui] = WuXing.Mu,
                [WuXing.Mu] = WuXing.Huo
            };

        /// <summary>取得五行的被生關係，即 '生我'</summary>
        public static IDictionary<WuXing, WuXing> GenerateBy { get; }
            = new ConcurrentDictionary<WuXing, WuXing> {
                [WuXing.Huo] = WuXing.Mu,
                [WuXing.Tu] = WuXing.Huo,
                [WuXing.Jin] = WuXing.Tu,
                [WuXing.Shui] = WuXing.Jin,
                [WuXing.Mu] = WuXing.Shui
            };

        /// <summary>取得五行的相剋關係，即 '我剋'</summary>
        public static IDictionary<WuXing, WuXing> Restricting { get; }
            = new ConcurrentDictionary<WuXing, WuXing> {
                [WuXing.Huo] = WuXing.Jin,
                [WuXing.Jin] = WuXing.Mu,
                [WuXing.Mu] = WuXing.Tu,
                [WuXing.Tu] = WuXing.Shui,
                [WuXing.Shui] = WuXing.Huo
            };

        /// <summary>取得五行的被剋關係，即 '剋我'</summary>
        public static IDictionary<WuXing, WuXing> RestrictBy { get; }
            = new ConcurrentDictionary<WuXing, WuXing> {
                [WuXing.Huo] = WuXing.Shui,
                [WuXing.Jin] = WuXing.Huo,
                [WuXing.Mu] = WuXing.Jin,
                [WuXing.Tu] = WuXing.Mu,
                [WuXing.Shui] = WuXing.Tu
            };

        /// <summary>取得地支六合</summary>
        public static IDictionary<WuXing, DiZhi[]> LiuHe { get; }
            = new ConcurrentDictionary<WuXing, DiZhi[]> {
                [WuXing.Huo | WuXing.Tu] = [DiZhi.Wu, DiZhi.Wei],   // 午未 = 陽火/陰土
                [WuXing.Shui] = [DiZhi.Si, DiZhi.Shen],             // 巳申 = 陰水
                [WuXing.Jin] = [DiZhi.Chen, DiZhi.You],             // 辰酉 = 陽金
                [WuXing.Huo] = [DiZhi.Mao, DiZhi.Xu],               // 卯戌 = 陰火
                [WuXing.Mu] = [DiZhi.Yin, DiZhi.Hai],               // 寅亥 = 陽木
                [WuXing.Tu] = [DiZhi.Zi, DiZhi.Chou]                // 子丑 = 陰土
            };

        /// <summary>取得地支三會</summary>
        public static IDictionary<WuXing, DiZhi[]> SanHui { get; }
            = new ConcurrentDictionary<WuXing, DiZhi[]> {
                [WuXing.Mu] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen],   // 寅卯辰 = 三會木局
                [WuXing.Huo] = [DiZhi.Si, DiZhi.Wu, DiZhi.Wei],     // 巳午未 = 三會火局
                [WuXing.Jin] = [DiZhi.Shen, DiZhi.You, DiZhi.Xu],   // 申酉戌 = 三會金局
                [WuXing.Shui] = [DiZhi.Hai, DiZhi.Zi, DiZhi.Chou]   // 亥子丑 = 三會水局
            };

        /// <summary>取得地支三合</summary>
        public static IDictionary<WuXing, DiZhi[]> SanHe { get; }
            = new ConcurrentDictionary<WuXing, DiZhi[]> {
                [WuXing.Shui] = [DiZhi.Shen, DiZhi.Zi, DiZhi.Chen], // 申子辰 = 三合水局
                [WuXing.Mu] = [DiZhi.Hai, DiZhi.Mao, DiZhi.Wei],    // 亥卯未 = 三合木局
                [WuXing.Huo] = [DiZhi.Yin, DiZhi.Wu, DiZhi.Xu],     // 寅午戌 = 三合火/土局
                [WuXing.Tu] = [DiZhi.Yin, DiZhi.Wu, DiZhi.Xu],      // 寅午戌 = 三合火/土局
                [WuXing.Jin] = [DiZhi.Si, DiZhi.You, DiZhi.Chou]    // 巳酉丑 = 三合金局
            };
    }

}

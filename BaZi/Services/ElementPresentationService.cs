using BaZi.Models;

namespace BaZi.Services {

    /// <summary>集中管理五行在畫面上的樣式。</summary>
    public static class ElementPresentationService {

        /// <summary>取得五行對應的 CSS 類別。</summary>
        public static string GetCssClass(WuXing element) {
            return element switch {
                WuXing.Mu => "element-wood",
                WuXing.Huo => "element-fire",
                WuXing.Tu => "element-earth",
                WuXing.Jin => "element-metal",
                WuXing.Shui => "element-water",
                _ => string.Empty
            };
        }
    }
}

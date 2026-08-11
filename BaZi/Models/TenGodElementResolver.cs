namespace BaZi.Models {

    /// <summary>解析十神對應的五行。</summary>
    public static class TenGodElementResolver {

        /// <summary>取得指定十神對日主所代表的五行。</summary>
        /// <param name="dayMasterElement">日主五行</param>
        /// <param name="tenGod">十神</param>
        /// <returns>十神五行</returns>
        public static WuXing Resolve(WuXing dayMasterElement, ShiShen tenGod) {
            return tenGod.ToCombined() switch {
                ShiShen.Cai => BaZiDefine.Restricting[dayMasterElement],
                ShiShen.GuanSha => BaZiDefine.RestrictBy[dayMasterElement],
                ShiShen.ShihShang => BaZiDefine.Generation[dayMasterElement],
                ShiShen.Yin => BaZiDefine.GenerateBy[dayMasterElement],
                ShiShen.BiJie => dayMasterElement,
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(
                    nameof(tenGod),
                    (int)tenGod,
                    typeof(ShiShen)
                )
            };
        }
    }
}

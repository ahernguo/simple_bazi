using System.ComponentModel;

namespace BaZi.Models {
    internal static class BaZiConvert {

        #region 天干
        /// <summary>將天干文字轉為對應的 <see cref="TianGan"/></summary>
        /// <param name="gan">欲轉換的天干文字</param>
        /// <returns>天干</returns>
        /// <exception cref="ArgumentException">無法解析的文字</exception>
        public static TianGan ToTianGan(this string gan) {
            return gan switch {
                "甲" => TianGan.Jia,
                "乙" => TianGan.Yi,
                "丙" => TianGan.Bing,
                "丁" => TianGan.Ding,
                "戊" => TianGan.Wu,
                "己" => TianGan.Ji,
                "庚" => TianGan.Geng,
                "辛" => TianGan.Xin,
                "壬" => TianGan.Ren,
                "癸" => TianGan.Gui,
                _ => throw new ArgumentException($"Cannot parse '{gan}'")
            };
        }

        /// <summary>將 <see cref="TianGan"/> 轉為對應的文字</summary>
        /// <param name="gan">欲轉換的天干</param>
        /// <returns>天干文字</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的天干</exception>
        public static string ToGanString(this TianGan gan) {
            return gan switch {
                TianGan.Jia => "甲",
                TianGan.Yi => "乙",
                TianGan.Bing => "丙",
                TianGan.Ding => "丁",
                TianGan.Wu => "戊",
                TianGan.Ji => "己",
                TianGan.Geng => "庚",
                TianGan.Xin => "辛",
                TianGan.Ren => "壬",
                TianGan.Gui => "癸",
                _ => throw new InvalidEnumArgumentException(nameof(gan), (int)gan, typeof(TianGan))
            };
        }

        /// <summary>取得 <see cref="TianGan"/> 對應的 <see cref="WuXing"/></summary>
        /// <param name="gan">欲轉換的天干</param>
        /// <returns>五行</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的天干</exception>
        public static WuXing ToWuXing(this TianGan gan) {
            return gan switch {
                TianGan.Jia => WuXing.Mu,
                TianGan.Yi => WuXing.Mu,
                TianGan.Bing => WuXing.Huo,
                TianGan.Ding => WuXing.Huo,
                TianGan.Wu => WuXing.Tu,
                TianGan.Ji => WuXing.Tu,
                TianGan.Geng => WuXing.Jin,
                TianGan.Xin => WuXing.Jin,
                TianGan.Ren => WuXing.Shui,
                TianGan.Gui => WuXing.Shui,
                _ => throw new InvalidEnumArgumentException(nameof(gan), (int)gan, typeof(TianGan))
            };
        }
        #endregion

        #region 地支
        /// <summary>將地支文字轉為對應的 <see cref="DiZhi"/></summary>
        /// <param name="zhi">欲轉換的地支文字</param>
        /// <returns>地支</returns>
        /// <exception cref="ArgumentException">無法解析的文字</exception>
        public static DiZhi ToDiZhi(this string zhi) {
            return zhi switch {
                "子" => DiZhi.Zi,
                "丑" => DiZhi.Chou,
                "寅" => DiZhi.Yin,
                "卯" => DiZhi.Mao,
                "辰" => DiZhi.Chen,
                "巳" => DiZhi.Si,
                "午" => DiZhi.Wu,
                "未" => DiZhi.Wei,
                "申" => DiZhi.Shen,
                "酉" => DiZhi.You,
                "戌" => DiZhi.Xu,
                "亥" => DiZhi.Hai,
                _ => throw new ArgumentException($"Cannot parse '{zhi}'")
            };
        }

        /// <summary>將 <see cref="DiZhi"/> 轉為對應的文字</summary>
        /// <param name="zhi">欲轉換的地支</param>
        /// <returns>地支文字</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的地支</exception>
        public static string ToZhiString(this DiZhi zhi) {
            return zhi switch {
                DiZhi.Zi => "子",
                DiZhi.Chou => "丑",
                DiZhi.Yin => "寅",
                DiZhi.Mao => "卯",
                DiZhi.Chen => "辰",
                DiZhi.Si => "巳",
                DiZhi.Wu => "午",
                DiZhi.Wei => "未",
                DiZhi.Shen => "申",
                DiZhi.You => "酉",
                DiZhi.Xu => "戌",
                DiZhi.Hai => "亥",
                _ => throw new InvalidEnumArgumentException(nameof(zhi), (int)zhi, typeof(DiZhi))
            };
        }

        /// <summary>取得 <see cref="DiZhi"/> 對應的 <see cref="WuXing"/></summary>
        /// <param name="zhi">欲轉換的地支</param>
        /// <returns>五行</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的地支</exception>
        public static WuXing ToWuXing(this DiZhi zhi) {
            return zhi switch {
                DiZhi.Zi => WuXing.Shui,
                DiZhi.Chou => WuXing.Tu,
                DiZhi.Yin => WuXing.Mu,
                DiZhi.Mao => WuXing.Mu,
                DiZhi.Chen => WuXing.Tu,
                DiZhi.Si => WuXing.Huo,
                DiZhi.Wu => WuXing.Huo,
                DiZhi.Wei => WuXing.Tu,
                DiZhi.Shen => WuXing.Jin,
                DiZhi.You => WuXing.Jin,
                DiZhi.Xu => WuXing.Tu,
                DiZhi.Hai => WuXing.Shui,
                _ => throw new InvalidEnumArgumentException(nameof(zhi), (int)zhi, typeof(DiZhi))
            };
        }
        #endregion

        #region 五行
        /// <summary>將五行文字轉為對應的 <see cref="WuXing"/></summary>
        /// <param name="wx">欲轉換的五行文字</param>
        /// <returns>五行</returns>
        /// <exception cref="ArgumentException">無法解析的文字</exception>
        public static WuXing ToWuXing(this string wx) {
            return wx switch {
                "木" => WuXing.Mu,
                "火" => WuXing.Huo,
                "土" => WuXing.Tu,
                "金" => WuXing.Jin,
                "水" => WuXing.Shui,
                _ => throw new ArgumentException($"Cannot parse '{wx}'")
            };
        }

        /// <summary>將 <see cref="WuXing"/> 轉為對應的文字</summary>
        /// <param name="wx">欲轉換的五行</param>
        /// <returns>五行文字</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的五行</exception>
        public static string ToWuXingString(this WuXing wx) {
            return wx switch {
                WuXing.Mu => "木",
                WuXing.Huo => "火",
                WuXing.Tu => "土",
                WuXing.Jin => "金",
                WuXing.Shui => "水",
                _ => throw new InvalidEnumArgumentException(nameof(wx), (int)wx, typeof(WuXing))
            };
        }
        #endregion

        #region 極性
        /// <summary>將極性文字轉為對應的 <see cref="JiXing"/></summary>
        /// <param name="ji">欲轉換的極性文字</param>
        /// <returns>極性</returns>
        /// <exception cref="ArgumentException">無法解析的文字</exception>
        public static JiXing ToJiXing(this string ji) {
            return ji switch {
                "陰" => JiXing.Yin,
                "陽" => JiXing.Yang,
                _ => throw new ArgumentException($"Cannot parse '{ji}'")
            };
        }

        /// <summary>將 <see cref="JiXing"/> 轉為對應的文字</summary>
        /// <param name="jx">欲轉換的極性</param>
        /// <returns>極性文字</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的極性</exception>
        public static string ToJiXingString(this JiXing jx) {
            return jx switch {
                JiXing.Yin => "陰",
                JiXing.Yang => "陽",
                _ => throw new InvalidEnumArgumentException(nameof(jx), (int)jx, typeof(JiXing))
            };
        }
        #endregion

        #region 十神
        /// <summary>將十神文字轉為對應的 <see cref="ShiShen"/></summary>
        /// <param name="shen">欲轉換的十神文字</param>
        /// <returns>十神</returns>
        /// <exception cref="ArgumentException">無法解析的文字</exception>
        public static ShiShen ToShiShen(this string shen) {
            return shen switch {
                "比肩" => ShiShen.BiJian,
                "劫財" => ShiShen.JieCai,
                "食神" => ShiShen.ShihShen,
                "傷官" => ShiShen.ShangGuan,
                "偏財" => ShiShen.PianCai,
                "正財" => ShiShen.ZhengCai,
                "七殺" => ShiShen.QiSha,
                "正官" => ShiShen.ZhengGuan,
                "偏印" => ShiShen.PianYin,
                "正印" => ShiShen.ZhengYin,
                "日主" => ShiShen.RiZhu,
                _ => throw new ArgumentException($"Cannot parse '{shen}'")
            };
        }

        /// <summary>將 <see cref="ShiShen"/> 轉為對應的文字</summary>
        /// <param name="shen">欲轉換的十神</param>
        /// <returns>十神文字</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的十神</exception>
        public static string ToShenString(this ShiShen shen) {
            return shen switch {
                ShiShen.RiZhu => "日主",
                ShiShen.BiJian => "比肩",
                ShiShen.JieCai => "劫財",
                ShiShen.ShihShen => "食神",
                ShiShen.ShangGuan => "傷官",
                ShiShen.PianCai => "偏財",
                ShiShen.ZhengCai => "正財",
                ShiShen.QiSha => "七殺",
                ShiShen.ZhengGuan => "正官",
                ShiShen.PianYin => "偏印",
                ShiShen.ZhengYin => "正印",
                ShiShen.ZhengGuan | ShiShen.QiSha => "官殺",
                ShiShen.ShihShen | ShiShen.ShangGuan => "食傷",
                ShiShen.ZhengCai | ShiShen.PianCai => "財星",
                ShiShen.ZhengYin | ShiShen.PianYin => "印星",
                ShiShen.BiJian | ShiShen.JieCai => "比劫",
                _ => throw new InvalidEnumArgumentException(nameof(shen), (int)shen, typeof(ShiShen))
            };
        }
        #endregion

        #region 性別
        /// <summary>將 <see cref="Sex"/> 轉為對應的文字</summary>
        /// <param name="sex">欲轉換的性別</param>
        /// <returns>性別文字</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的性別</exception>
        public static string ToSexString(this Sex sex) {
            return sex switch {
                Sex.Male => "男",
                Sex.Female => "女",
                _ => throw new InvalidEnumArgumentException(nameof(sex), (int)sex, typeof(Sex))
            };
        }
        #endregion

        #region 格局
        /// <summary>將 <see cref="GeJu"/> 轉為對應的文字</summary>
        /// <param name="geJu">欲轉換的格局</param>
        /// <returns>格局文字</returns>
        /// <exception cref="InvalidEnumArgumentException">無法解析的格局</exception>
        public static string ToGeJuString(this GeJu geJu) {
            return geJu switch {
                GeJu.ShenQiang => "身強",
                GeJu.ShenRuo => "身弱",
                GeJu.CongQiang => "從強格",
                GeJu.CongRuo => "從弱格",
                _ => throw new InvalidEnumArgumentException(nameof(geJu), (int)geJu, typeof(GeJu))
            };
        }
        #endregion

    }
}

using System.ComponentModel;

namespace BaZi.Models;
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

    /// <summary>取得 <see cref="TianGan"/> 對應的 <see cref="JiXing"/></summary>
    /// <param name="gan">欲轉換的天干</param>
    /// <returns>極性</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的天干</exception>
    public static JiXing ToJiXing(this TianGan gan) {
        return gan switch {
            TianGan.Jia => JiXing.Yang,
            TianGan.Yi => JiXing.Yin,
            TianGan.Bing => JiXing.Yang,
            TianGan.Ding => JiXing.Yin,
            TianGan.Wu => JiXing.Yang,
            TianGan.Ji => JiXing.Yin,
            TianGan.Geng => JiXing.Yang,
            TianGan.Xin => JiXing.Yin,
            TianGan.Ren => JiXing.Yang,
            TianGan.Gui => JiXing.Yin,
            _ => throw new InvalidEnumArgumentException(nameof(gan), (int)gan, typeof(TianGan))
        };
    }

    /// <summary>取得 <see cref="TianGan"/> 對應的 <see cref="ShiShen"/></summary>
    /// <param name="gan">欲轉換的天干</param>
    /// <param name="riZhuGan">日柱天干</param>
    /// <returns>十神</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的天干</exception>
    public static ShiShen ToShiShen(this TianGan gan, TianGan riZhuGan) {
        /* 先取得日主的極性與五行 */
        var srcJiXing = riZhuGan.ToJiXing();
        var srcWuXing = riZhuGan.ToWuXing();
        /* 取得要判斷的天干極性與五行 */
        var tarJiXing = gan.ToJiXing();
        var tarWuXing = gan.ToWuXing();
        /* 依照五行相生相剋，再搭配極性來判斷 */
        if (tarWuXing == BaZiDefine.Restricting[srcWuXing]) {
            // 我剋。 同性=偏財, 異性=正財
            return (srcJiXing == tarJiXing) ? ShiShen.PianCai : ShiShen.ZhengCai;
        } else if (tarWuXing == BaZiDefine.RestrictBy[srcWuXing]) {
            // 剋我。 同性=七殺, 異性=正官
            return (srcJiXing == tarJiXing) ? ShiShen.QiSha : ShiShen.ZhengGuan;
        } else if (tarWuXing == BaZiDefine.Generation[srcWuXing]) {
            // 我生。 同性=食神, 異性=傷官
            return (srcJiXing == tarJiXing) ? ShiShen.ShihShen : ShiShen.ShangGuan;
        } else if (tarWuXing == BaZiDefine.GenerateBy[srcWuXing]) {
            // 生我。 同性=偏印, 異性=正印
            return (srcJiXing == tarJiXing) ? ShiShen.PianYin : ShiShen.ZhengYin;
        } else if (tarWuXing == srcWuXing) {
            // 同我。 同性=比肩, 異性=劫財
            return (srcJiXing == tarJiXing) ? ShiShen.BiJian : ShiShen.JieCai;
        } else {
            throw new ArgumentException("無法判斷的極性與五行");
        }
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

    /// <summary>取得 <see cref="DiZhi"/> 對應的所有 <see cref="WuXing"/></summary>
    /// <param name="zhi">欲轉換的地支</param>
    /// <returns>五行</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的地支</exception>
    public static IDictionary<WuXing, int> ToFullWuXing(this DiZhi zhi) {
        return zhi switch {
            DiZhi.Zi => new Dictionary<WuXing, int>() { { WuXing.Shui, 100 } },
            DiZhi.Chou => new Dictionary<WuXing, int>() { { WuXing.Tu, 33 }, { WuXing.Shui, 33 }, { WuXing.Jin, 33 } },
            DiZhi.Yin => new Dictionary<WuXing, int>() { { WuXing.Mu, 50 }, { WuXing.Huo, 40 }, { WuXing.Tu, 10 } },
            DiZhi.Mao => new Dictionary<WuXing, int>() { { WuXing.Mu, 100 } },
            DiZhi.Chen => new Dictionary<WuXing, int>() { { WuXing.Tu, 33 }, { WuXing.Mu, 33 }, { WuXing.Shui, 33 } },
            DiZhi.Si => new Dictionary<WuXing, int>() { { WuXing.Huo, 50 }, { WuXing.Tu, 40 }, { WuXing.Jin, 10 } },
            DiZhi.Wu => new Dictionary<WuXing, int>() { { WuXing.Huo, 50 }, { WuXing.Tu, 50 } },
            DiZhi.Wei => new Dictionary<WuXing, int>() { { WuXing.Tu, 40 }, { WuXing.Huo, 40 }, { WuXing.Mu, 20 } },
            DiZhi.Shen => new Dictionary<WuXing, int>() { { WuXing.Jin, 50 }, { WuXing.Shui, 40 }, { WuXing.Tu, 10 } },
            DiZhi.You => new Dictionary<WuXing, int>() { { WuXing.Jin, 100 } },
            DiZhi.Xu => new Dictionary<WuXing, int>() { { WuXing.Tu, 45 }, { WuXing.Huo, 45 }, { WuXing.Jin, 10 } },
            DiZhi.Hai => new Dictionary<WuXing, int>() { { WuXing.Shui, 60 }, { WuXing.Mu, 40 } },
            _ => throw new InvalidEnumArgumentException(nameof(zhi), (int)zhi, typeof(DiZhi))
        };
    }

    /// <summary>取得 <see cref="DiZhi"/> 對應的 <see cref="JiXing"/></summary>
    /// <param name="zhi">欲轉換的地支</param>
    /// <returns>極性</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的地支</exception>
    public static JiXing ToJiXing(this DiZhi zhi) {
        return zhi switch {
            DiZhi.Zi => JiXing.Yang,
            DiZhi.Chou => JiXing.Yin,
            DiZhi.Yin => JiXing.Yang,
            DiZhi.Mao => JiXing.Yin,
            DiZhi.Chen => JiXing.Yang,
            DiZhi.Si => JiXing.Yin,
            DiZhi.Wu => JiXing.Yang,
            DiZhi.Wei => JiXing.Yin,
            DiZhi.Shen => JiXing.Yang,
            DiZhi.You => JiXing.Yin,
            DiZhi.Xu => JiXing.Yang,
            DiZhi.Hai => JiXing.Yin,
            _ => throw new InvalidEnumArgumentException(nameof(zhi), (int)zhi, typeof(DiZhi))
        };
    }

    /// <summary>取得 <see cref="DiZhi"/> 對應的 <see cref="TianGan"/> (支藏干)</summary>
    /// <param name="zhi">欲轉換的地支</param>
    /// <returns>支藏干</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的地支</exception>
    public static TianGan ToTianGan(this DiZhi zhi) {
        return zhi switch {
            DiZhi.Zi => TianGan.Gui,
            DiZhi.Chou => TianGan.Ji,
            DiZhi.Yin => TianGan.Jia,
            DiZhi.Mao => TianGan.Yi,
            DiZhi.Chen => TianGan.Wu,
            DiZhi.Si => TianGan.Bing,
            DiZhi.Wu => TianGan.Ding,
            DiZhi.Wei => TianGan.Ji,
            DiZhi.Shen => TianGan.Geng,
            DiZhi.You => TianGan.Xin,
            DiZhi.Xu => TianGan.Wu,
            DiZhi.Hai => TianGan.Ren,
            _ => throw new InvalidEnumArgumentException(nameof(zhi), (int)zhi, typeof(DiZhi))
        };
    }

    /// <summary>取得 <see cref="DiZhi"/> 對應的 <see cref="ShiShen"/></summary>
    /// <param name="zhi">欲轉換的地支</param>
    /// <param name="riZhuGan">日柱天干</param>
    /// <returns>十神</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的地支</exception>
    public static ShiShen ToShiShen(this DiZhi zhi, TianGan riZhuGan) {
        /* 取得要判斷的支藏干 */
        var hideGan = zhi.ToTianGan();
        /* 以支藏干來跟日主做判斷 */
        return hideGan.ToShiShen(riZhuGan);
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

    /// <summary>將 <see cref="WuXing"/> 轉為對應的 <see cref="ShiShen"/></summary>
    /// <param name="wx">欲轉換的五行</param>
    /// <returns>十神</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的五行</exception>
    public static ShiShen ToShiShen(this WuXing wx, WuXing riZhu) {
        if (BaZiDefine.Restricting[riZhu] == wx) {
            return ShiShen.ZhengCai | ShiShen.PianCai;
        } else if (BaZiDefine.RestrictBy[riZhu] == wx) {
            return ShiShen.ZhengGuan | ShiShen.QiSha;
        } else if (BaZiDefine.Generation[riZhu] == wx) {
            return ShiShen.ShangGuan | ShiShen.ShihShen;
        } else if (BaZiDefine.GenerateBy[riZhu] == wx) {
            return ShiShen.ZhengYin | ShiShen.PianYin;
        } else {
            return ShiShen.JieCai | ShiShen.BiJian;
        }
    }

    /// <summary>取得天干地支對應的五行</summary>
    /// <param name="gan">天干</param>
    /// <param name="zhi">地支</param>
    /// <returns>五行</returns>
    public static WuXing[] GetWuXing(TianGan gan, DiZhi zhi) {
        /* 取得天干的五行 */
        var ganWuXing = gan.ToWuXing();
        /* 取得地支的完整五行 */
        var zhiWuXing = zhi.ToFullWuXing();
        /* 如果天干的五行有剋地支的五行，對應的地支要扣分 */
        var meKe = BaZiDefine.Restricting[ganWuXing];
        if (zhiWuXing.ContainsKey(meKe)) {
            zhiWuXing[meKe] -= 20;
        }
        /* 如果天干的五行有生地支的五行，對應的地支要加分 */
        var meShen = BaZiDefine.Generation[ganWuXing];
        if (zhiWuXing.ContainsKey(meShen)) {
            zhiWuXing[meShen] += 20;
        }
        /* 開始合併 */
        if (zhiWuXing.ContainsKey(ganWuXing)) {
            zhiWuXing[ganWuXing] += 100;
        } else {
            zhiWuXing.Add(ganWuXing, 100);
        }
        /* 取出大於 40 分的最多三個 */
        return zhiWuXing
            .Where(kvp => kvp.Value > 40)
            .OrderByDescending(kvp => kvp.Value)
            .Select(kvp => kvp.Key)
            .Take(3).ToArray();
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
            "劫財" or "劫财" => ShiShen.JieCai,
            "食神" => ShiShen.ShihShen,
            "傷官" or "伤官" => ShiShen.ShangGuan,
            "偏財" or "偏财" => ShiShen.PianCai,
            "正財" or "正财" => ShiShen.ZhengCai,
            "七殺" or "七杀" => ShiShen.QiSha,
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
            ShiShen.BiJie => "比劫",
            ShiShen.ShihShang => "食傷",
            ShiShen.Cai => "財星",
            ShiShen.GuanSha => "官殺",
            ShiShen.Yin => "印星",
            _ => throw new InvalidEnumArgumentException(nameof(shen), (int)shen, typeof(ShiShen))
        };
    }

    /// <summary>將 <see cref="ShiShen"/> 轉為對應的運名稱</summary>
    /// <param name="shen">欲轉換的十神</param>
    /// <returns>運</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的十神</exception>
    public static string ToYunString(this ShiShen shen) {
        return shen switch {
            ShiShen.BiJian or ShiShen.JieCai or ShiShen.BiJie => "比劫運",
            ShiShen.ShihShen or ShiShen.ShangGuan or ShiShen.ShihShang => "食傷運",
            ShiShen.PianCai or ShiShen.ZhengCai or ShiShen.Cai => "財運",
            ShiShen.QiSha or ShiShen.ZhengGuan or ShiShen.GuanSha => "官殺運",
            ShiShen.PianYin or ShiShen.ZhengYin or ShiShen.Yin => "印運",
            _ => throw new InvalidEnumArgumentException(nameof(shen), (int)shen, typeof(ShiShen))
        };
    }

    /// <summary>將 <see cref="ShiShen"/> 轉為對應的兩神物件</summary>
    /// <param name="shen">欲轉換的十神</param>
    /// <returns>兩神</returns>
    /// <exception cref="InvalidEnumArgumentException">無法解析的十神</exception>
    public static ShiShen ToCombined(this ShiShen shen) {
        return shen switch {
            ShiShen.BiJian or ShiShen.JieCai or ShiShen.BiJie => ShiShen.BiJie,
            ShiShen.ShihShen or ShiShen.ShangGuan or ShiShen.ShihShang => ShiShen.ShihShang,
            ShiShen.PianCai or ShiShen.ZhengCai or ShiShen.Cai => ShiShen.Cai,
            ShiShen.QiSha or ShiShen.ZhengGuan or ShiShen.GuanSha => ShiShen.GuanSha,
            ShiShen.PianYin or ShiShen.ZhengYin or ShiShen.Yin => ShiShen.Yin,
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

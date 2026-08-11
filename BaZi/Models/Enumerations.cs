namespace BaZi.Models {

    /// <summary>五行</summary>
    [Flags]
    public enum WuXing : int {
        /// <summary>木</summary>
        Mu = 0x010000,
        /// <summary>火</summary>
        Huo = 0x020000,
        /// <summary>土</summary>
        Tu = 0x040000,
        /// <summary>金</summary>
        Jin = 0x080000,
        /// <summary>水</summary>
        Shui = 0x100000
    }

    /// <summary>極性</summary>
    [Flags]
    public enum JiXing : int {
        /// <summary>陰</summary>
        Yin = 0x200000,
        /// <summary>陽</summary>
        Yang = 0x400000
    }

    /// <summary>天干</summary>
    [Flags]
    public enum TianGan : int {
        /// <summary>甲(木)</summary>
        Jia = 0x410001,
        /// <summary>乙(木)</summary>
        Yi = 0x210002,
        /// <summary>丙(火)</summary>
        Bing = 0x420001,
        /// <summary>丁(火)</summary>
        Ding = 0x220002,
        /// <summary>戊(土)</summary>
        Wu = 0x440001,
        /// <summary>己(土)</summary>
        Ji = 0x240002,
        /// <summary>庚(金)</summary>
        Geng = 0x480001,
        /// <summary>辛(金)</summary>
        Xin = 0x280002,
        /// <summary>壬(水)</summary>
        Ren = 0x500001,
        /// <summary>癸(水)</summary>
        Gui = 0x300002
    }

    /// <summary>地支</summary>
    [Flags]
    public enum DiZhi : int {
        /// <summary>子(鼠)；100% 癸水, 23~01 時, 11月(冬)</summary>
        Zi = 0x100001,
        /// <summary>丑(牛)；(1/3)己土 (1/3)癸水 (1/3)辛金, 01~03 時, 12月(冬)</summary>
        Chou = 0x180002,
        /// <summary>寅(虎)；(50%)甲木 (40%)丙火 (10%)戊土, 03~05 時, 1 月(春)</summary>
        Yin = 0x030004,
        /// <summary>卯(兔)；100% 乙木, 05~07 時, 2 月(春)</summary>
        Mao = 0x010008,
        /// <summary>辰(龍)；(1/3)戊土 (1/3)乙木 (1/3)癸水, 07~09 時, 3 月(春)</summary>
        Chen = 0x110010,
        /// <summary>巳(蛇)；(50%)丙火 (40%)戊土 (10%)庚金, 09~11 時, 4 月(夏)</summary>
        Si = 0x060020,
        /// <summary>午(馬)；(50%)丁火 (50%)己土, 11~13 時, 5 月(夏)</summary>
        Wu = 0x060040,
        /// <summary>未(羊)；(40%)己土 (40%)丁火 (20%)乙木, 13~15 時, 6 月(夏)</summary>
        Wei = 0x070080,
        /// <summary>申(猴)；(50%)庚金 (40%)壬水 (10%)戊土, 15~17 時, 7 月(秋)</summary>
        Shen = 0x180100,
        /// <summary>酉(雞)；(100%)辛金, 17~19 時, 8 月(秋)</summary>
        You = 0x080200,
        /// <summary>戌(狗)；(45%)戊土 (45%)丁火 (10%)辛金, 19~21 時, 9 月(秋)</summary>
        Xu = 0x060400,
        /// <summary>亥(豬)；(60%)壬水 (40%)甲木, 21~23 時, 10 月(冬)</summary>
        Hai = 0x110800
    }

    /// <summary>十神</summary>
    [Flags]
    public enum ShiShen : int {
        /// <summary>日主</summary>
        RiZhu = 0,
        /// <summary>比肩</summary>
        BiJian = 0x0001,
        /// <summary>劫財</summary>
        JieCai = 0x0002,
        /// <summary>食神</summary>
        ShihShen = 0x0004,
        /// <summary>傷官</summary>
        ShangGuan = 0x0008,
        /// <summary>偏財</summary>
        PianCai = 0x0010,
        /// <summary>正財</summary>
        ZhengCai = 0x0020,
        /// <summary>七殺</summary>
        QiSha = 0x0040,
        /// <summary>正官</summary>
        ZhengGuan = 0x0080,
        /// <summary>偏印</summary>
        PianYin = 0x0100,
        /// <summary>正印</summary>
        ZhengYin = 0x0200,
        /// <summary>比劫</summary>
        BiJie = 0x0003,
        /// <summary>食傷</summary>
        ShihShang = 0x000C,
        /// <summary>財</summary>
        Cai = 0x0030,
        /// <summary>官殺</summary>
        GuanSha = 0x00C0,
        /// <summary>印</summary>
        Yin = 0x0300
    }

    /// <summary>性別</summary>
    public enum Sex {
        /// <summary>男</summary>
        Male = 1,
        /// <summary>女</summary>
        Female = 2
    }

    /// <summary>格局</summary>
    [Flags]
    public enum GeJu {
        /// <summary>一般格局</summary>
        General = 0x10,
        /// <summary>從格</summary>
        CongGe = 0x20,
        /// <summary>身強</summary>
        ShenQiang = 0x11,
        /// <summary>身弱</summary>
        ShenRuo = 0x12,
        /// <summary>從強</summary>
        CongQiang = 0x21,
        /// <summary>從弱</summary>
        CongRuo = 0x22
    }

    /// <summary>大運十年中的主作用階段。</summary>
    public enum DaYunPhase {
        /// <summary>前五年以天干為主，地支仍有作用。</summary>
        FirstFiveYears = 1,
        /// <summary>後五年以地支為主，天干作用較弱。</summary>
        LastFiveYears = 2
    }

    /// <summary>合、會結果</summary>
    public enum HeHui {
        /// <summary>地支三合</summary>
        ThreeHe = 1,
        /// <summary>地支三會</summary>
        ThreeHui = 2,
        /// <summary>天干五合</summary>
        FiveHe = 3,
        /// <summary>地支六合</summary>
        SixHe = 4
    }
}

namespace BaZi.Models {

    /// <summary>描述包含天干、地支之類別</summary>
    public interface IGanZhi {

        #region Properties
        /// <summary>取得名稱</summary>
        public string Id { get; }
        /// <summary>取得天干</summary>
        public TianGan Gan { get; }
        /// <summary>取得地支</summary>
        public DiZhi Zhi { get; }
        #endregion

    }

    /// <summary>柱</summary>
    public class Zhu : IGanZhi {

        #region Properties
        /// <summary>取得此柱的名稱</summary>
        public string Id { get; }
        /// <summary>取得天干</summary>
        public TianGan Gan { get; }
        /// <summary>取得地支</summary>
        public DiZhi Zhi { get; }
        /// <summary>取得主星</summary>
        public ShiShen ZhuXing { get; }
        /// <summary>取得副星(支藏干)</summary>
        public IReadOnlyList<ShiShen> FuXing { get; }
        /// <summary>取得天干對應的五行</summary>
        public WuXing GanWuXing { get; }
        /// <summary>取得地支對應的五行</summary>
        public WuXing ZhiWuXing { get; }
        /// <summary>取得或設定在計算格局時，天干是否為加分項</summary>
        public bool IsGanBonus { get; set; }
        /// <summary>取得或設定在計算格局時，地支是否為加分項</summary>
        public bool IsZhiBonus { get; set; }
        #endregion

        #region Constructor
        /// <summary>建構 '柱' 資訊</summary>
        /// <param name="id">此柱的名稱</param>
        /// <param name="gan">天干文字</param>
        /// <param name="zhi">地支文字</param>
        /// <param name="zhuXing">主星</param>
        /// <param name="fuXing">副星</param>
        public Zhu(string id, string gan, string zhi, string zhuXing, IList<string> fuXing) {
            Id = id;
            Gan = gan.ToTianGan();
            GanWuXing = Gan.ToWuXing();
            Zhi = zhi.ToDiZhi();
            ZhiWuXing = Zhi.ToWuXing();
            ZhuXing = zhuXing.ToShiShen();
            FuXing = fuXing.Select(s => s.ToShiShen()).ToList();
            IsGanBonus = false;
            IsZhiBonus = false;
        }
        #endregion

        #region Overrides
        public override string ToString() {
            return $"{Gan.ToGanString()}{Zhi.ToZhiString()}, {ZhuXing.ToShenString()}, {(string.Join("|", FuXing.Select(fx => fx.ToShenString())))}";
        }
        #endregion
    }

    /// <summary>流月</summary>
    public class LiuYue : IGanZhi {

        #region Properties
        /// <summary>取得此流月對應的名稱</summary>
        public string Id { get; } = "流月";
        /// <summary>取得天干</summary>
        public TianGan Gan { get; }
        /// <summary>取得地支</summary>
        public DiZhi Zhi { get; }
        /// <summary>取得流月序號，範圍為 0 到 11</summary>
        public int Index { get; }
        /// <summary>取得農曆月份中文名稱</summary>
        public string MonthInChinese { get; }
        #endregion

        #region Constructor
        /// <summary>從 <see cref="Lunar.EightChar.LiuYue"/> 建構資訊</summary>
        /// <param name="liuYue">欲建構的流月來源</param>
        public LiuYue(Lunar.EightChar.LiuYue liuYue) {
            Gan = liuYue.GanZhi.Substring(0, 1).ToTianGan();
            Zhi = liuYue.GanZhi.Substring(1, 1).ToDiZhi();
            Index = liuYue.Index;
            MonthInChinese = liuYue.MonthInChinese;
        }
        #endregion

    }

    /// <summary>流年</summary>
    public class LiuNian : IGanZhi {

        #region Properties
        /// <summary>取得此流年對應的名稱</summary>
        public string Id { get; } = "流年";
        /// <summary>取得天干</summary>
        public TianGan Gan { get; }
        /// <summary>取得地支</summary>
        public DiZhi Zhi { get; }
        /// <summary>取得此流年的年紀</summary>
        public int Age { get; }
        /// <summary>取得此流年的年份(西元)</summary>
        public int Year { get; }
        /// <summary>取得此流年對應的十神</summary>
        public ShiShen Shen { get; }
        /// <summary>取得此流年的流月</summary>
        public IReadOnlyList<LiuYue> LiuYueList { get; }
        #endregion

        #region Constructor
        /// <summary>從 <see cref="Lunar.EightChar.LiuNian"/> 建構資訊</summary>
        /// <param name="liuNian">欲建構的流年來源</param>
        public LiuNian(Lunar.EightChar.LiuNian liuNian) {
            Gan = liuNian.GanZhi.Substring(0, 1).ToTianGan();
            Zhi = liuNian.GanZhi.Substring(1, 1).ToDiZhi();
            Age = liuNian.Age;
            Year = liuNian.Year;
            LiuYueList = [.. liuNian.GetLiuYue().Select(ly => new LiuYue(ly))];
        }
        #endregion

    }

    /// <summary>大運</summary>
    public class DaYun : IGanZhi {

        #region Properties
        /// <summary>取得此大運對應的名稱</summary>
        public string Id { get; } = "大運";
        /// <summary>取得天干</summary>
        public TianGan Gan { get; }
        /// <summary>取得地支</summary>
        public DiZhi Zhi { get; }
        /// <summary>取得此運的年紀</summary>
        public int StartAge { get; }
        /// <summary>取得此運的年份(西元)</summary>
        public int StartYear { get; }
        /// <summary>取得此運對應的十神</summary>
        public ShiShen Shen { get; }
        /// <summary>取得此運的流年</summary>
        public IReadOnlyList<LiuNian> LiuNianList { get; }
        /// <summary>取得或設定是否為好運 (僅供註記)</summary>
        public bool IsGoodYun { get; set; }
        #endregion

        #region Constructor
        public DaYun(Lunar.EightChar.DaYun daYun) {
            Gan = daYun.GanZhi.Substring(0, 1).ToTianGan();
            Zhi = daYun.GanZhi.Substring(1, 1).ToDiZhi();
            StartAge = daYun.StartAge;
            StartYear = daYun.StartYear;
            LiuNianList = [.. daYun.GetLiuNian().Select(ln => new LiuNian(ln))];
            IsGoodYun = false;
        }
        #endregion

    }

    /// <summary>
    /// 八字完整命盤資料模型
    /// </summary>
    public class BaZiInfo {

        #region Log
        private static readonly log4net.ILog LOG4N = log4net.LogManager.GetLogger("bazi");
        #endregion

        #region Properties
        /// <summary>取得年柱</summary>
        public Zhu YearZhu { get; }
        /// <summary>取得月柱</summary>
        public Zhu MonthZhu { get; }
        /// <summary>取得日柱</summary>
        public Zhu DayZhu { get; }
        /// <summary>取得時柱</summary>
        public Zhu HourZhu { get; }
        /// <summary>取得使用者是否提供準確的生辰時、分</summary>
        public bool IsBirthTimeAccurate { get; }
        /// <summary>取得日主五行</summary>
        public WuXing RiZhu { get; }
        /// <summary>取得日主極性(陰/陽)</summary>
        public JiXing RiZhuJiXing { get; }
        /// <summary>取得性別</summary>
        public Sex Gender { get; }
        /// <summary>取得陽曆生日</summary>
        public DateTime SolarDate { get; }
        /// <summary>取得陰曆生日</summary>
        public DateTime LunarDate { get; }
        /// <summary>取得生肖</summary>
        public string ShengXiao { get; }
        /// <summary>取得身格得分</summary>
        public int StrengthScore { get; }
        /// <summary>取得身格</summary>
        public GeJu StrengthStatus { get; }
        /// <summary>取得大運清單</summary>
        public IReadOnlyList<DaYun> DaYunList { get; }
        /// <summary>取得當前的大運</summary>
        public DaYun? CurrentDaYun { get; }
        /// <summary>取得喜用神</summary>
        public IReadOnlyList<WuXing> LikeWuXing { get; }
        /// <summary>取得忌神</summary>
        public IReadOnlyList<WuXing> UnlikeWuXing { get; }
        #endregion

        #region Constructor
        /// <summary>建構八字資訊</summary>
        /// <param name="birthdate">生辰</param>
        /// <param name="gender">性別 (1)女 (2)男</param>
        /// <param name="isBirthTimeAccurate">是否具有準確的生辰時、分</param>
        public BaZiInfo(DateTime birthdate, int gender, bool isBirthTimeAccurate = true) {
            LOG4N.Info($"生辰 '{birthdate}' 性別 '{gender}' 生辰時間準確 '{isBirthTimeAccurate}'");
            SolarDate = birthdate;
            Gender = (gender == 1) ? Sex.Female : Sex.Male;
            IsBirthTimeAccurate = isBirthTimeAccurate;
            var solar = Lunar.Solar.FromDate(birthdate);
            var lunar = solar.Lunar;
            LOG4N.Info($"對應的農曆為 '{lunar}'");
            var eightChar = lunar.EightChar;
            LunarDate = new DateTime(lunar.Year, lunar.Month, lunar.Day);
            ShengXiao = lunar.YearShengXiao;
            YearZhu = new Zhu(
                "年柱",
                eightChar.YearGan,
                eightChar.YearZhi,
                eightChar.YearShiShenGan,
                eightChar.YearShiShenZhi
            );
            LOG4N.Info($"年柱為 '{YearZhu}'");
            MonthZhu = new Zhu(
                "月柱",
                eightChar.MonthGan,
                eightChar.MonthZhi,
                eightChar.MonthShiShenGan,
                eightChar.MonthShiShenZhi
            );
            LOG4N.Info($"月柱為 '{MonthZhu}'");
            DayZhu = new Zhu(
                "日柱",
                eightChar.DayGan,
                eightChar.DayZhi,
                eightChar.DayShiShenGan,
                eightChar.DayShiShenZhi
            );
            LOG4N.Info($"日柱為 '{DayZhu}'");
            HourZhu = new Zhu(
                "時柱",
                eightChar.TimeGan,
                eightChar.TimeZhi,
                eightChar.TimeShiShenGan,
                eightChar.TimeShiShenZhi
            );
            LOG4N.Info($"時柱為 '{HourZhu}'");
            RiZhu = DayZhu.GanWuXing;
            RiZhuJiXing = (JiXing)((int)DayZhu.Gan & 0x600000);
            LOG4N.Info($"日主為 {RiZhuJiXing}{RiZhu}");
            LOG4N.Info($"日主為 '{DayZhu.Gan.ToGanString()}({RiZhuJiXing.ToJiXingString()}{RiZhu.ToWuXingString()})'");
            var lunarGender = (Gender == Sex.Male) ? 1 : 0;
            var yun = eightChar.GetYun(lunarGender, 2);
            var daYuns = yun.GetDaYun();
            var daYunList = new List<DaYun>();
            var curYear = DateTime.Now.Year;
            foreach (var daYun in daYuns) {
                /* 排除大運之前，從 1 開始 */
                if (daYun.Index == 0)
                    continue;
                var y = new DaYun(daYun);
                daYunList.Add(y);
                if ((y.StartYear <= curYear) && (curYear <= (y.StartYear + 10))) {
                    CurrentDaYun = y;
                }
            }
            DaYunList = daYunList;
            var score = CalculateGeJu();
            StrengthScore = score;
            if (80 <= score) {
                StrengthStatus = GeJu.CongQiang;
                LOG4N.Info($"80 ≦ 分數，從強");
            } else if ((45 <= score) && (score < 80)) {
                StrengthStatus = GeJu.ShenQiang;
                LOG4N.Info($"45 ≦ 分數 < 80，身強");
            } else if ((20 < score) && (score < 45)) {
                StrengthStatus = GeJu.ShenRuo;
                LOG4N.Info($"20 < 分數 < 45，身弱");
            } else if (20 <= score) {
                StrengthStatus = GeJu.CongRuo;
                LOG4N.Info($"分數 ≦ 20，從弱");
            }
            LikeWuXing = StrengthStatus switch {
                GeJu.ShenQiang or GeJu.CongRuo => [BaZiDefine.Restricting[RiZhu], BaZiDefine.RestrictBy[RiZhu], BaZiDefine.Generation[RiZhu]],
                GeJu.ShenRuo or GeJu.CongQiang => [BaZiDefine.GenerateBy[RiZhu], RiZhu],
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(StrengthStatus), (int)StrengthStatus, typeof(GeJu))
            };
            UnlikeWuXing = StrengthStatus switch {
                GeJu.ShenQiang or GeJu.CongRuo => [BaZiDefine.GenerateBy[RiZhu], RiZhu],
                GeJu.ShenRuo or GeJu.CongQiang => [BaZiDefine.Restricting[RiZhu], BaZiDefine.RestrictBy[RiZhu], BaZiDefine.Generation[RiZhu]],
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(nameof(StrengthStatus), (int)StrengthStatus, typeof(GeJu))
            };
        }
        #endregion

        #region Methods
        private int CalculateGeJu() {
            LOG4N.Info($"開始計算格局...");
            /* 先取得日柱五行 */
            var points = new Dictionary<string, bool> {
            { "YearGan", false }, { "YearZhi", false },
            { "MonthGan", false }, { "MonthZhi", false },
            { "DayZhi", false },
            { "HourGan", false }, { "HourZhi", false }
        };
            /* ----- Step1. 計算四柱是否有幫扶的加分項 ----- */
            //年柱
            points["YearGan"] = SameOrGenerate(RiZhu, YearZhu.GanWuXing);
            LOG4N.Info($"  年柱天干 '{YearZhu.Gan.ToGanString()}{YearZhu.GanWuXing.ToWuXingString()}' → {(points["YearGan"] ? "得分" : "未得分")}");
            points["YearZhi"] = SameOrGenerate(RiZhu, YearZhu.ZhiWuXing);
            LOG4N.Info($"  年柱地支 '{YearZhu.Zhi.ToZhiString()}{YearZhu.ZhiWuXing.ToWuXingString()}' → {(points["YearZhi"] ? "得分" : "未得分")}");
            //月柱
            points["MonthGan"] = SameOrGenerate(RiZhu, MonthZhu.GanWuXing);
            LOG4N.Info($"  月柱天干 '{MonthZhu.Gan.ToGanString()}{MonthZhu.GanWuXing.ToWuXingString()}' → {(points["MonthGan"] ? "得分" : "未得分")}");
            points["MonthZhi"] = SameOrGenerate(RiZhu, MonthZhu.ZhiWuXing);
            LOG4N.Info($"  月柱地支 '{MonthZhu.Zhi.ToZhiString()}{MonthZhu.ZhiWuXing.ToWuXingString()}' → {(points["MonthZhi"] ? "得分" : "未得分")}");
            //日柱
            points["DayZhi"] = SameOrGenerate(RiZhu, DayZhu.ZhiWuXing);
            LOG4N.Info($"  日柱地支 '{DayZhu.Zhi.ToZhiString()}{DayZhu.ZhiWuXing.ToWuXingString()}' → {(points["DayZhi"] ? "得分" : "未得分")}");
            //時柱
            points["HourGan"] = SameOrGenerate(RiZhu, HourZhu.GanWuXing);
            LOG4N.Info($"  時柱天干 '{HourZhu.Gan.ToGanString()}{HourZhu.GanWuXing.ToWuXingString()}' → {(points["HourGan"] ? "得分" : "未得分")}");
            points["HourZhi"] = SameOrGenerate(RiZhu, HourZhu.ZhiWuXing);
            LOG4N.Info($"  時柱地支 '{HourZhu.Zhi.ToZhiString()}{HourZhu.ZhiWuXing.ToWuXingString()}' → {(points["HourZhi"] ? "得分" : "未得分")}");
            /* ----- Step2. 貪生忘剋。但要注意相鄰的兩干如果已經被合走，則貪生忘剋不會生效! (2026/01/06 直播說的) ----- */
            LOG4N.Info($"  檢查貪生忘剋...");
            // [月支] 生 [月干] 生 [日主]
            if (!points["MonthZhi"]) {
                var a = IsGenerate(MonthZhu.ZhiWuXing, MonthZhu.GanWuXing);
                var b = IsGenerate(MonthZhu.GanWuXing, RiZhu);
                if (a && b) {
                    points["MonthZhi"] = true;
                    LOG4N.Info($"    [月支] 生 [月干] 生 [日主] 成立，月柱地支得分");
                }
            }
            // [時支] 生 [時干] 生 [日主]
            if (!points["HourZhi"]) {
                var a = IsGenerate(HourZhu.ZhiWuXing, HourZhu.GanWuXing);
                var b = IsGenerate(HourZhu.GanWuXing, RiZhu);
                if (a && b) {
                    points["HourZhi"] = true;
                    LOG4N.Info($"    [時支] 生 [時干] 生 [日主] 成立，時柱地支得分");
                }
            }
            // [年干] 生 [月干] 生 [日主]
            if (!points["YearGan"]) {
                var a = IsGenerate(YearZhu.GanWuXing, MonthZhu.GanWuXing);
                var b = IsGenerate(MonthZhu.GanWuXing, RiZhu);
                if (a && b) {
                    points["YearGan"] = true;
                    LOG4N.Info($"    [年干] 生 [月干] 生 [日主] 成立，年柱天干得分");
                }
            }
            // [年支] 生 [年干] 生 [月干] 生 [日主]
            if (!points["YearZhi"]) {
                var a = IsGenerate(YearZhu.ZhiWuXing, YearZhu.GanWuXing);
                var b = IsGenerate(YearZhu.GanWuXing, MonthZhu.GanWuXing);
                var c = IsGenerate(MonthZhu.GanWuXing, RiZhu);
                if (a && b && c) {
                    points["YearZhi"] = true;
                    LOG4N.Info($"    [年支] 生 [年干] 生 [月干] 生 [日主] 成立，年柱地支得分");
                }
            }
            /* ----- Step3. 四庫。若有引發庫，有可能原本加分的變成扣分，所以這邊用 `=` 複寫 ----- */
            LOG4N.Info($"  檢查年柱是否觸發庫...");
            if (CheckKu(RiZhu, YearZhu, MonthZhu, null, out var ku)) {
                points["YearZhi"] = SameOrGenerate(RiZhu, ku);
            }
            LOG4N.Info($"  檢查月柱是否觸發庫...");
            if (CheckKu(RiZhu, MonthZhu, DayZhu, YearZhu, out ku)) {
                points["MonthZhi"] = SameOrGenerate(RiZhu, ku);
            }
            LOG4N.Info($"  檢查日柱是否觸發庫...");
            if (CheckKu(RiZhu, DayZhu, HourZhu, MonthZhu, out ku)) {
                points["DayZhi"] = SameOrGenerate(RiZhu, ku);
            }
            LOG4N.Info($"  檢查時柱是否觸發庫...");
            if (CheckKu(RiZhu, HourZhu, null, DayZhu, out ku)) {
                points["HourZhi"] = SameOrGenerate(RiZhu, ku);
            }
            /* ----- Step4. 三會。三會的能量強，一但形成就會改變地支的五行，要用新的五行來算 ----- */
            (string key, string tip)[] zhiList = [("YearZhi", "年柱地支"), ("MonthZhi", "月柱地支"), ("DayZhi", "日柱地支"), ("HourZhi", "時柱地支")];
            LOG4N.Info($"  檢查是否有三會...");
            foreach (var kvp in BaZiDefine.ThreeHui) {
                if (CheckHui(kvp.Value, kvp.Key, out var hui)) {
                    LOG4N.Info($"    觸發 '{string.Join("", kvp.Value.Select(v => v.ToZhiString()))}' 三會{kvp.Key.ToWuXingString()}局");
                    //當此地支是三會之一時，則此地支強迫變成目標五行，重新計算是否達成
                    var newStt = SameOrGenerate(RiZhu, kvp.Key);
                    if (hui[0] > 0) {
                        points["YearZhi"] = newStt;
                        LOG4N.Info($"      年柱地支 '{YearZhu.Zhi.ToZhiString()}{YearZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，{(newStt ? "得分" : "不得分")}");
                    }
                    if (hui[1] > 0) {
                        points["MonthZhi"] = newStt;
                        LOG4N.Info($"      月柱地支 '{MonthZhu.Zhi.ToZhiString()}{MonthZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，{(newStt ? "得分" : "不得分")}");
                    }
                    if (hui[2] > 0) {
                        points["DayZhi"] = newStt;
                        LOG4N.Info($"      日柱地支 '{DayZhu.Zhi.ToZhiString()}{DayZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，{(newStt ? "得分" : "不得分")}");
                    }
                    if (hui[3] > 0) {
                        points["HourZhi"] = newStt;
                        LOG4N.Info($"      時柱地支 '{HourZhu.Zhi.ToZhiString()}{HourZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，{(newStt ? "得分" : "不得分")}");
                    }
                }
            }
            /* ----- Step5. 三合/半合/暗拱。能量不如三會，如果改變的新五行沒有加分，那就維持原來的，不強制複寫 ----- */
            LOG4N.Info($"  檢查是否有三合/半合/暗拱...");
            foreach (var kvp in BaZiDefine.ThreeHe) {
                if (SameOrGenerate(RiZhu, kvp.Key) && CheckHe(kvp.Value, kvp.Key, out var he)) {
                    LOG4N.Info($"    觸發 '{string.Join("", kvp.Value.Select(v => v.ToZhiString()))}' {kvp.Key.ToWuXingString()}局");
                    if (he[0] > 0) {
                        points["YearZhi"] = true;
                        LOG4N.Info($"      年柱地支 '{YearZhu.Zhi.ToZhiString()}{YearZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，得分");
                    }
                    if (he[1] > 0) {
                        points["MonthZhi"] = true;
                        LOG4N.Info($"      月柱地支 '{MonthZhu.Zhi.ToZhiString()}{MonthZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，得分");
                    }
                    if (he[2] > 0) {
                        points["DayZhi"] = true;
                        LOG4N.Info($"      日柱地支 '{DayZhu.Zhi.ToZhiString()}{DayZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，得分");
                    }
                    if (he[3] > 0) {
                        points["HourZhi"] = true;
                        LOG4N.Info($"      時柱地支 '{HourZhu.Zhi.ToZhiString()}{HourZhu.ZhiWuXing.ToWuXingString()}' 更改為 '{kvp.Key.ToWuXingString()}'，得分");
                    }
                }
            }
            /* ----- Step6. 計算最終分數 ----- */
            var score = 0;
            LOG4N.Info($"  計算最終分數...");
            if (points["YearGan"]) {
                score += 5;
                YearZhu.IsGanBonus = true;
                LOG4N.Info($"    年柱天干 +5");
            } else {
                LOG4N.Info($"    年柱天干 +0");
            }
            if (points["YearZhi"]) {
                score += 20;
                YearZhu.IsZhiBonus = true;
                LOG4N.Info($"    年柱地支 +20");
            } else {
                LOG4N.Info($"    年柱地支 +0");
            }
            if (points["MonthGan"]) {
                score += 5;
                MonthZhu.IsGanBonus = true;
                LOG4N.Info($"    月柱天干 +5");
            } else {
                LOG4N.Info($"    月柱天干 +0");
            }
            if (points["MonthZhi"]) {
                score += 35;
                MonthZhu.IsZhiBonus = true;
                LOG4N.Info($"    月柱地支 +35");
            } else {
                LOG4N.Info($"    月柱地支 +0");
            }
            if (points["DayZhi"]) {
                score += 20;
                DayZhu.IsZhiBonus = true;
                LOG4N.Info($"    日柱地支 +20");
            } else {
                LOG4N.Info($"    日柱地支 +0");
            }
            if (points["HourGan"]) {
                score += 5;
                HourZhu.IsGanBonus = true;
                LOG4N.Info($"    時柱天干 +5");
            } else {
                LOG4N.Info($"    時柱天干 +0");
            }
            if (points["HourZhi"]) {
                score += 10;
                HourZhu.IsZhiBonus = true;
                LOG4N.Info($"    時柱地支 +10");
            } else {
                LOG4N.Info($"    時柱地支 +0");
            }
            LOG4N.Info($"  總得分 {score}");
            return score;
        }

        /// <summary>檢查五行是否同日主，或是生日主</summary>
        /// <param name="riZhu">日主五行</param>
        /// <param name="tar">欲比較的五行</param>
        /// <returns>(true)同日主或生日主 (false)不成立</returns>
        private static bool SameOrGenerate(WuXing riZhu, WuXing tar) {
            return (tar == riZhu)  //同我
                || (BaZiDefine.Generation[tar] == riZhu);  //生我
        }

        /// <summary>檢查 a 是否生 b</summary>
        /// <param name="a">欲比較的五行</param>
        /// <param name="b">被比較的五行</param>
        /// <returns>(true)a生b (false)不成立</returns>
        private static bool IsGenerate(WuXing a, WuXing b) {
            return BaZiDefine.Generation[a] == b;
        }

        /// <summary>檢查 (日+月) 或 (日+時) 是否有三刑/相刑</summary>
        /// <returns>(true)有刑 (false)</returns>
        private bool CheckXing() {
            /* 先列出所有的刑，輪詢判斷是否有成立 */
            var xing = new List<DiZhi[]> {
            new DiZhi[] { DiZhi.Yin, DiZhi.Si, DiZhi.Shen },    //寅巳申 = 無恩之刑
            new DiZhi[] { DiZhi.Chou, DiZhi.Xu, DiZhi.Wei },    //丑戌未 = 恃勢之刑
            new DiZhi[] { DiZhi.Zi, DiZhi.Mao },                //子卯 = 恩愛之刑
            new DiZhi[] { DiZhi.Chen, DiZhi.Chen },             //辰辰 = 自刑
            new DiZhi[] { DiZhi.Wu, DiZhi.Wu },                 //午午 = 自刑
            new DiZhi[] { DiZhi.You, DiZhi.You },               //酉酉 = 自刑
            new DiZhi[] { DiZhi.Xu, DiZhi.Xu },                 //戌戌 = 自刑
        };
            /* 通常以 (日+月) 或 (日+時) 為主，根本身有關
                * 若是 (年+日) 或 (月+時) 則力量較小，可忽略
                * 若是 (年+月) 表示是長輩那邊有狀況，根本身較無關係，可忽略 */
            // 判斷 日柱+月柱
            var dmZhi = new DiZhi[] { DayZhu.Zhi, MonthZhu.Zhi };
            if (xing.Any(c => dmZhi.All(z => c.Contains(z)))) {
                return true;
            }
            // 判斷 日柱+時柱
            var dhZhi = new DiZhi[] { DayZhu.Zhi, HourZhu.Zhi };
            if (xing.Any(c => dhZhi.All(z => c.Contains(z)))) {
                return true;
            }
            return false;
        }

        /// <summary>檢查是否相鄰的兩干為六合之一</summary>
        /// <param name="neibZhi">相鄰的兩干</param>
        /// <param name="wuXing">若為六合，回傳對應的五行</param>
        /// <returns>(true)六合之一 (false)無</returns>
        private static bool CheckLiuHe(DiZhi[] neibZhi, out WuXing wuXing) {
            var match = BaZiDefine.SixHe.FirstOrDefault(kvp => neibZhi.All(z => kvp.Value.Contains(z)));
            if ((match.Value != null) && (match.Value.Length > 0)) {
                wuXing = match.Key;
                return true;
            }
            wuXing = 0;
            return false;
        }

        /// <summary>檢查是否引發四庫，讓該地支變更五行</summary>
        /// <param name="riZhu">日主五行</param>
        /// <param name="zhu">欲檢查的柱</param>
        /// <param name="leftZhu">左邊的柱</param>
        /// <param name="rightZhu">右邊的柱</param>
        /// <param name="newZhiWuXing">更改後的新五行</param>
        /// <returns>(true)有引發庫 (false)無引發</returns>
        private static bool CheckKu(WuXing riZhu, Zhu zhu, Zhu? leftZhu, Zhu? rightZhu, out WuXing newZhiWuXing) {
            var zhuMap = new Dictionary<string, (TianGan gan, WuXing wuXing)> {
            { "同柱", (zhu.Gan, zhu.GanWuXing) },
            { "左柱", (leftZhu is null) ? (0, 0) : (leftZhu.Gan, leftZhu.GanWuXing) },
            { "右柱", (rightZhu is null) ? (0, 0) : (rightZhu.Gan, rightZhu.GanWuXing) }
        };
            /* 辰 = 水庫 */
            if (zhu.Zhi == DiZhi.Chen) {
                zhuMap.Remove("同柱");    //判斷水庫時，因為優先判斷同柱是否為土木水了，只剩隔壁柱的判斷
                var w = zhuMap.FirstOrDefault(kvp => kvp.Value.wuXing == WuXing.Shui);  //下方可能會用到，先留起來放
                if (zhu.GanWuXing == WuXing.Shui) {
                    /* 辰 = (1/3)戊土 (1/3)乙木 (1/3)癸水
                        * 若該柱的天干是(土|木|水)，則直接變成對應的五行
                        * 變化是一定要變，如果原本是土日主，辰土同我有加分，現在變成水，但土剋水，原本有得分就變成 0 了 */
                    newZhiWuXing = WuXing.Shui;
                    LOG4N.Info($"    同柱天干為 '{zhu.Gan.ToGanString()}水'，'辰' 由 '土' 變 '水' (強迫變化)");
                    return true;
                } else if (zhu.GanWuXing == WuXing.Mu) {
                    newZhiWuXing = WuXing.Mu; //同上
                    LOG4N.Info($"    同柱天干為 '{zhu.Gan.ToGanString()}木'，'辰' 由 '土' 變 '木' (強迫變化)");
                    return true;
                } else if (zhu.GanWuXing == WuXing.Tu) {
                    newZhiWuXing = WuXing.Tu; //同上。雖然原本就是辰土，但為了方便下方判斷四庫(不用再看同柱)，所以乾脆卡一個 if，下方就不用再判斷五行
                    LOG4N.Info($"    同柱天干為 '{zhu.Gan.ToGanString()}土'，'辰' 維持 '土'");
                    return false;
                } else if (!string.IsNullOrEmpty(w.Key) && (w.Value.gan > 0)) {
                    /* 水庫觸發條件(經 '身強身弱逐步解說' 測試)
                        * 1. 同柱或隔壁柱干有水。但因為上面已經把同柱干的水給判斷掉了(優先程度高、強迫變化)，故這邊只能判斷隔壁柱有沒有水
                        * 2. 還是要檢查同日主或生日主 */
                    newZhiWuXing = WuXing.Shui;
                    if (SameOrGenerate(riZhu, WuXing.Shui)) {
                        LOG4N.Info($"    {w.Key}天干為 '{w.Value.gan.ToGanString()}水'，觸發水庫候選。'水' 對{riZhu.ToWuXingString()}日主加分，'辰' 由 '土' 變 '水'");
                        return true;
                    } else {
                        LOG4N.Info($"    {w.Key}天干為 '{w.Value.gan.ToGanString()}水'，觸發水庫候選。'水' 對{riZhu.ToWuXingString()}日主不加分，'辰' 維持 '土'");
                        return false;
                    }
                } else {
                    LOG4N.Info($"    雖有 '辰'，但水庫沒有觸發，維持 '土'");
                    newZhiWuXing = 0;
                    return false;
                }
            }
            /* 未 = 木庫 */
            if (zhu.Zhi == DiZhi.Wei) {
                var m = zhuMap.FirstOrDefault(kvp => kvp.Value.wuXing == WuXing.Mu);    //下方可能會用到，先留起來放
                if (riZhu == WuXing.Huo) {
                    /* 未 = (40%)土 (40%)火 (20%)木
                        * 如果日主是火，則未可以視為火
                        * (2026/01/06 直播時說要先判斷火日主，再判斷有無木庫) */
                    newZhiWuXing = WuXing.Huo;
                    LOG4N.Info($"    日主 '火'，'未' 由 '土' 變 '火' (強迫變化)");
                    return true;
                } else if (riZhu == WuXing.Tu) {
                    newZhiWuXing = WuXing.Tu;   //同上。原本就是未土，這邊卡一個寫 Log。且未土若是加分項，原本就會加分不會進這裡，順便直接離開
                    LOG4N.Info($"    日主 '土'，'未' 維持 '土' (強迫變化)");
                    return false;
                } else if (!string.IsNullOrEmpty(m.Key) && (m.Value.gan > 0)) {
                    newZhiWuXing = WuXing.Mu;
                    if (SameOrGenerate(riZhu, WuXing.Mu)) {
                        LOG4N.Info($"    {m.Key}天干為 '{m.Value.gan.ToGanString()}木'，觸發木庫候選。'木' 對{riZhu.ToWuXingString()}日主加分，'未' 由 '土' 變 '木'");
                        return true;
                    } else {
                        LOG4N.Info($"    {m.Key}天干為 '{m.Value.gan.ToGanString()}木'，觸發木庫候選。'木' 對{riZhu.ToWuXingString()}日主不加分，'未' 維持 '土'");
                        return false;
                    }
                } else {
                    LOG4N.Info($"    雖有 '未'，但木庫沒有觸發，維持 '土'");
                    newZhiWuXing = 0;
                    return false;
                }
            }
            /* 戌 = 火/土庫 */
            if (zhu.Zhi == DiZhi.Xu) {
                var h = zhuMap.FirstOrDefault(kvp => kvp.Value.wuXing == WuXing.Huo);    //下方可能會用到，先留起來放
                if (riZhu == WuXing.Huo) {
                    /* 戌 = (45%)土 (45%)火 (10%)金
                        * 如果日主是火，則戌可以視為火
                        * (2026/01/06 直播時說要先判斷火日主，再判斷有無庫) */
                    newZhiWuXing = WuXing.Huo;
                    LOG4N.Info($"    日主 '火'，'戌' 由 '土' 變 '火' (強迫變化)");
                    return true;
                } else if (riZhu == WuXing.Tu) {
                    newZhiWuXing = WuXing.Tu;   //同上。原本就是戌土，這邊卡一個寫 Log。且戌土若是加分項，原本就會加分不會進這裡，順便直接離開
                    LOG4N.Info($"    日主 '土'，'戌' 維持 '土' (強迫變化)");
                    return false;
                } else if (!string.IsNullOrEmpty(h.Key) && (h.Value.gan > 0)) {
                    newZhiWuXing = WuXing.Huo;
                    if (SameOrGenerate(riZhu, WuXing.Huo)) {
                        LOG4N.Info($"    {h.Key}天干為 '{h.Value.gan.ToGanString()}火'，觸發火庫候選。'火' 對{riZhu.ToWuXingString()}日主加分，'戌' 由 '土' 變 '火'");
                        return true;
                    } else {
                        LOG4N.Info($"    {h.Key}天干為 '{h.Value.gan.ToGanString()}火'，觸發火庫候選。'火' 對{riZhu.ToWuXingString()}日主不加分，'戌' 維持 '土'");
                        return false;
                    }
                } else {
                    LOG4N.Info($"    雖有 '戌'，但火庫沒有觸發，維持 '土'");
                    newZhiWuXing = 0;
                    return false;
                }
            }
            /* 丑 = 金庫 */
            if (zhu.Zhi == DiZhi.Chou) {
                zhuMap.Remove("同柱");    //判斷金庫時，因為優先判斷同柱是否為土水金了，只剩隔壁柱的判斷
                var j = zhuMap.FirstOrDefault(kvp => kvp.Value.wuXing == WuXing.Jin);  //下方可能會用到，先留起來放
                if (zhu.GanWuXing == WuXing.Shui) {
                    /* 丑 = (1/3)己土 (1/3)癸水 (1/3)辛金
                        * 若該柱的天干是(土|水|金)，則直接變成對應的五行
                        * 變化是一定要變，如果原本是土日主，辰土同我有加分，現在變成水，但土剋水，原本有得分就變成 0 了 */
                    newZhiWuXing = WuXing.Shui;
                    LOG4N.Info($"    同柱天干為 '{zhu.Gan.ToGanString()}水'，'丑' 由 '土' 變 '水' (強迫變化)");
                    return true;
                } else if (zhu.GanWuXing == WuXing.Jin) {
                    newZhiWuXing = WuXing.Jin; //同上
                    LOG4N.Info($"    同柱天干為 '{zhu.Gan.ToGanString()}金'，'丑' 由 '土' 變 '金' (強迫變化)");
                    return false;
                } else if (zhu.GanWuXing == WuXing.Tu) {
                    newZhiWuXing = WuXing.Tu; //同上。雖然原本就是丑土，但為了方便下方判斷四庫(不用再看同柱)，所以乾脆卡一個 if，下方就不用再判斷五行
                    LOG4N.Info($"    同柱天干為 '{zhu.Gan.ToGanString()}土'，'丑' 維持 '土'");
                    return true;
                } else if (!string.IsNullOrEmpty(j.Key) && (j.Value.gan > 0)) {
                    /* 金庫觸發條件(經 '身強身弱逐步解說' 測試)
                        * 1. 同柱或隔壁柱干有金。但因為上面已經把同柱干的金給判斷掉了(優先程度高、強迫變化)，故這邊只能判斷隔壁柱有沒有金
                        * 2. 還是要檢查同日主或生日主 */
                    newZhiWuXing = WuXing.Jin;
                    if (SameOrGenerate(riZhu, WuXing.Jin)) {
                        LOG4N.Info($"    {j.Key}天干為 '{j.Value.gan.ToGanString()}金'，觸發金庫候選。'金' 對{riZhu.ToWuXingString()}日主加分，'丑' 由 '土' 變 '金'");
                        return true;
                    } else {
                        LOG4N.Info($"    {j.Key}天干為 '{j.Value.gan.ToGanString()}金'，觸發金庫候選。'金' 對{riZhu.ToWuXingString()}日主不加分，'丑' 維持 '土'");
                        return false;
                    }
                } else {
                    LOG4N.Info($"    雖有 '丑'，但金庫沒有觸發，維持 '土'");
                    newZhiWuXing = 0;
                    return false;
                }
            }
            LOG4N.Info($"    沒有 '辰未戌丑'，不會觸發庫");
            newZhiWuXing = 0;
            return false;
        }

        /// <summary>檢查是否有三會</summary>
        /// <param name="chkZhi">欲檢查的三會地支</param>
        /// <param name="replaceWuXing">對應的三會五行</param>
        /// <param name="newWuXing">當回傳為 true 時，紀錄對應、有被取代的地支屬性。依序為 [0]年支 [1]月支 [2]日支 [3]時支</param>
        /// <returns>(true)有三會 (false)無</returns>
        private bool CheckHui(DiZhi[] chkZhi, WuXing replaceWuXing, out WuXing[] newWuXing) {
            var chk = new (bool trig, int idx)[4];
            /* 年支 */
            chk[0] = chkZhi.Contains(YearZhu.Zhi) ? (true, chkZhi.IndexOf(YearZhu.Zhi)) : (false, -1);
            /* 月支 */
            chk[1] = chkZhi.Contains(MonthZhu.Zhi) ? (true, chkZhi.IndexOf(MonthZhu.Zhi)) : (false, -1);
            /* 日支 */
            chk[2] = chkZhi.Contains(DayZhu.Zhi) ? (true, chkZhi.IndexOf(DayZhu.Zhi)) : (false, -1);
            /* 時支 */
            chk[3] = chkZhi.Contains(HourZhu.Zhi) ? (true, chkZhi.IndexOf(HourZhu.Zhi)) : (false, -1);
            /* 計算是否有三個。不可以單純用 trig 判斷，有可能是三個都相同的，改用 Group 判斷是否有三個群組 */
            var grp = chk.Where(tp => tp.trig).GroupBy(tp => tp.idx);
            if (grp.Count() >= 3) {
                /* 將對應的位置替換成新的五行 */
                newWuXing = [.. chk.Select(tp => tp.trig ? replaceWuXing : 0)];
                return true;
            }
            newWuXing = [];
            return false;
        }

        /// <summary>檢查是否有三合/半合/暗拱</summary>
        /// <param name="chkZhi">欲檢查的三合地支</param>
        /// <param name="replaceWuXing">對應的三合五行</param>
        /// <param name="newWuXing">當回傳為 true 時，紀錄對應、有被取代的地支屬性。依序為 [0]年支 [1]月支 [2]日支 [3]時支</param>
        /// <returns>(true)有三合/半合/暗拱 (false)無</returns>
        private bool CheckHe(DiZhi[] chkZhi, WuXing replaceWuXing, out WuXing[] newWuXing) {
            var chk = new (bool trig, int idx)[4];
            /* 年支 */
            chk[0] = chkZhi.Contains(YearZhu.Zhi) ? (true, chkZhi.IndexOf(YearZhu.Zhi)) : (false, -1);
            /* 月支 */
            chk[1] = chkZhi.Contains(MonthZhu.Zhi) ? (true, chkZhi.IndexOf(MonthZhu.Zhi)) : (false, -1);
            /* 日支 */
            chk[2] = chkZhi.Contains(DayZhu.Zhi) ? (true, chkZhi.IndexOf(DayZhu.Zhi)) : (false, -1);
            /* 時支 */
            chk[3] = chkZhi.Contains(HourZhu.Zhi) ? (true, chkZhi.IndexOf(HourZhu.Zhi)) : (false, -1);
            /* 計算是否有三個。不可以單純用 trig 判斷，有可能是三個都相同的，改用 Group 判斷是否有三個群組 */
            var grp = chk.Where(tp => tp.trig).GroupBy(tp => tp.idx);
            if (grp.Count() >= 2) {
                /* 兩個的有可能是半合(chkZhi[0]+chkZhi[1], chkZhi[1]+chkZhi[2])或暗拱(chkZhi[0]+chkZhi[2])
                    * 三個以上就是完整的三合。不論哪種都是變成對應的五行，再去判斷是否可以加分 */
                newWuXing = [.. chk.Select(tp => tp.trig ? replaceWuXing : 0)];
                return true;
            }
            newWuXing = [];
            return false;
        }
        #endregion
    }
}

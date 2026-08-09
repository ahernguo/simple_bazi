using System.Collections.Concurrent;

namespace BaZi.Models {

    public static class BaZiDefine {

        /// <summary>取得五行清單</summary>
        public static IList<WuXing> WuXingList { get; }
            = [WuXing.Jin, WuXing.Mu, WuXing.Shui, WuXing.Huo, WuXing.Tu];

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

        /// <summary>取得天干五合</summary>
        public static IList<IList<TianGan>> FiveHe { get; }
            = [
                [TianGan.Jia, TianGan.Ji],      // 甲己
            [TianGan.Yi, TianGan.Geng],     // 乙庚
            [TianGan.Bing, TianGan.Xin],    // 丙辛
            [TianGan.Ding, TianGan.Ren],    // 丁壬
            [TianGan.Wu, TianGan.Gui]       // 戊癸
            ];

        /// <summary>取得地支六合</summary>
        public static IDictionary<WuXing, DiZhi[]> SixHe { get; }
            = new ConcurrentDictionary<WuXing, DiZhi[]> {
                [WuXing.Huo | WuXing.Tu] = [DiZhi.Wu, DiZhi.Wei],   // 午未 = 陽火/陰土
                [WuXing.Shui] = [DiZhi.Si, DiZhi.Shen],             // 巳申 = 陰水
                [WuXing.Jin] = [DiZhi.Chen, DiZhi.You],             // 辰酉 = 陽金
                [WuXing.Huo] = [DiZhi.Mao, DiZhi.Xu],               // 卯戌 = 陰火
                [WuXing.Mu] = [DiZhi.Yin, DiZhi.Hai],               // 寅亥 = 陽木
                [WuXing.Tu] = [DiZhi.Zi, DiZhi.Chou]                // 子丑 = 陰土
            };

        /// <summary>取得地支三會</summary>
        public static IDictionary<WuXing, DiZhi[]> ThreeHui { get; }
            = new ConcurrentDictionary<WuXing, DiZhi[]> {
                [WuXing.Mu] = [DiZhi.Yin, DiZhi.Mao, DiZhi.Chen],   // 寅卯辰 = 三會木局
                [WuXing.Huo] = [DiZhi.Si, DiZhi.Wu, DiZhi.Wei],     // 巳午未 = 三會火局
                [WuXing.Jin] = [DiZhi.Shen, DiZhi.You, DiZhi.Xu],   // 申酉戌 = 三會金局
                [WuXing.Shui] = [DiZhi.Hai, DiZhi.Zi, DiZhi.Chou]   // 亥子丑 = 三會水局
            };

        /// <summary>取得地支三合</summary>
        public static IDictionary<WuXing, DiZhi[]> ThreeHe { get; }
            = new ConcurrentDictionary<WuXing, DiZhi[]> {
                [WuXing.Shui] = [DiZhi.Shen, DiZhi.Zi, DiZhi.Chen], // 申子辰 = 三合水局
                [WuXing.Mu] = [DiZhi.Hai, DiZhi.Mao, DiZhi.Wei],    // 亥卯未 = 三合木局
                [WuXing.Huo] = [DiZhi.Yin, DiZhi.Wu, DiZhi.Xu],     // 寅午戌 = 三合火/土局
                [WuXing.Tu] = [DiZhi.Yin, DiZhi.Wu, DiZhi.Xu],      // 寅午戌 = 三合火/土局
                [WuXing.Jin] = [DiZhi.Si, DiZhi.You, DiZhi.Chou]    // 巳酉丑 = 三合金局
            };

        /// <summary>取得十神對應的描述</summary>
        public static IDictionary<ShiShen, (string desc, string positive, string negative, string advice)> ShiShenDetails { get; }
            = new ConcurrentDictionary<ShiShen, (string desc, string positive, string negative, string advice)> {
                [ShiShen.ZhengGuan] = (
                    "追求穩定、被社會認可；名聲、被信任、紀律",
                    "品行端正、光明磊落、有責任心、管理能力、知禮守法",
                    "墨守成規、不知變通、一板一眼、優柔寡斷、太在乎別人看法而綁手綁腳",
                    "適時放下包袱，勿過度謹慎。跳脫性思考、放輕鬆"
                ),
                [ShiShen.QiSha] = (
                    "挑戰、突破自己；個性威嚴正直、勇敢果決、嫉惡如仇",
                    "好挑戰、膽識高、使命必達、不怕困難、責任感、行動力強、運籌帷幄、領導",
                    "弱同理心、有距離感、倔強強勢、不通情理、急躁、偏激霸道、掌控慾強、獨斷獨行",
                    "團隊協作，記得照顧夥伴情緒；適時放鬆"
                ),
                [ShiShen.ShihShen] = (
                    "享受身活、及時行樂、活在當下",
                    "生活樂趣、興趣多元、喜學習嘗試新事物、熱忱耐心、藝術感知立、溫和、有度量",
                    "好奇心強、易分心、三分鐘熱度、不夠理性憑感覺、過度安逸",
                    "排輕重緩急、避免過度樂觀、理性判斷"
                ),
                [ShiShen.ShangGuan] = (
                    "追求贏、自由、精彩人生",
                    "反應快、聰明、爆發力強、突破框架",
                    "為反而反、挑戰權力、易被誤解、得理不饒人、思考太跳躍(別人跟不上)",
                    "三思後行、放慢說話速度、別強出頭、練習控制脾氣情緒"
                ),
                [ShiShen.ZhengCai] = (
                    "紀律、信用、追求穩定",
                    "勤儉務實、任勞任怨、善理財、儲蓄觀念強、重視家庭、社會道德、正派、保守、講信用",
                    "固執、過於保守、重視眼前錯失長遠、斤斤計較、缺乏勇氣與魄力",
                    "跨出思維小風險嘗試、勇敢追求夢想喜好、練習放手與授權、多跟思維創新的人討論"
                ),
                [ShiShen.PianCai] = (
                    "享受玩樂、賺錢花錢、財富大進大出",
                    "會賺也會花、不拘小節慷慨、投資眼光與膽識好、處事圓滑、人際手腕靈活、拿得起放得下",
                    "奢侈浪費、物慾太高、過度自信、易分心投機、愛面子",
                    "謹慎理財、風險管理、提升專注力、剋制慾望"
                ),
                [ShiShen.ZhengYin] = (
                    "私領域、安全感、忠厚老實、淡泊名利、安靜、怕衝突",
                    "道德感強、待人寬容、為他人著想、享福、獲貴人提拔、不爭權奪利、不強出頭",
                    "太自我保護、太依賴被動、不懂察言觀色與表達、缺乏獨立思考",
                    "積極勇敢、追求自我實現、多與身邊貴人溝通交流"
                ),
                [ShiShen.PianYin] = (
                    "自我界線、獨來獨往、不愛社交、喜怒不形於色、與玄學有緣",
                    "觀察力強、領悟力與直覺強、看事通透淡然、處事不爭不搶",
                    "負面思考、冷淡、固執己見",
                    "多正向思考、結識朋友與交流、降敏、避鑽牛角尖、多出門"
                ),
                [ShiShen.BiJian] = (
                    "重視友誼、講義氣、好勝不服輸、自尊自信強烈",
                    "敢冒險、講義氣、人際關係好、為朋友兩肋插刀、行動力強",
                    "意氣用事、因財惹禍、自主意識高難被說服",
                    "謹慎評估合夥勿過重義氣、慎選朋友、學習獨處"
                ),
                [ShiShen.JieCai] = (
                    "人際關係、存在感",
                    "性格圓融、樂於助人、重情重義、廣結善緣、人緣好具號召力、行動力高、應變能力強",
                    "太愛面子、孤注一擲、衝動魯莽、有勇無謀、易受環境跟朋友影響、朋友大於自己跟家人、不擅處理錢財",
                    "多替自己著想、慎選朋友、置產獲強迫儲蓄、三思後行、避免衝動行事"
                )
            };

        /// <summary>取得十神對應的意義(雙組合、大於等於三個時)</summary>
        public static IDictionary<ShiShen, string> ShiShenCombineMeans { get; }
            = new ConcurrentDictionary<ShiShen, string> {
                [ShiShen.ZhengGuan | ShiShen.QiSha] = "約束、責任、名譽、權威、壓力",
                [ShiShen.ShihShen | ShiShen.ShangGuan] = "才華、表達、創意、自由、感性",
                [ShiShen.ZhengCai | ShiShen.PianCai] = "務實、金錢、掌控、現實、情慾",
                [ShiShen.ZhengYin | ShiShen.PianYin] = "保護、學問、思想、名譽、依賴",
                [ShiShen.BiJian | ShiShen.JieCai] = "自我、競爭、意志、朋友、獨立"
            };

        /// <summary>取得十神對應的描述(雙組合、大於等於三個時)</summary>
        public static IDictionary<ShiShen, (string desc, string positive, string negative, string advice)> ShiShenCombineGte3Details { get; }
            = new ConcurrentDictionary<ShiShen, (string desc, string positive, string negative, string advice)> {
                [ShiShen.ZhengGuan | ShiShen.QiSha] = (
                    "守法循規，有責任感，具備領導潛質",
                    "自律性強，重信譽，做事有始有終",
                    "較為嚴肅，有時生活過於規律顯得乏味",
                    "保持現有的責任感，適時放鬆心情"
                ),
                [ShiShen.ShihShen | ShiShen.ShangGuan] = (
                    "思維敏捷，口才佳，具備專業技術與美感",
                    "聰明伶俐，擅長解決問題，生活有情調",
                    "有時流於空談，想法多而實踐力稍弱",
                    "將創意轉化為實際的作品，專注於一項專長"
                ),
                [ShiShen.ZhengCai | ShiShen.PianCai] = (
                    "勤奮務實，金錢觀念正確，擅長資源分配",
                    "目標導向，具備商業頭腦，生活踏實",
                    "較為現實，凡事以價值衡量，可能缺乏感性",
                    "繼續保持財務規劃，偶爾享受感性生活"
                ),
                [ShiShen.ZhengYin | ShiShen.PianYin] = (
                    "溫和慈祥，愛好學習，生活安定有長輩緣",
                    "學習力強，具包容力，能逢凶化吉",
                    "行動較慢，有時過於保守，依賴心稍強",
                    "保持學習習慣，多嘗試主動出擊"
                ),
                [ShiShen.BiJian | ShiShen.JieCai] = (
                    "意志堅定，具備自信，能與人合作",
                    "獨立自主，抗壓性強，能白手起家",
                    "有時固執，不輕易接受他人建議",
                    "保持自信的同時，多傾聽專業意見"
                )
            };

        /// <summary>取得十神對應的描述(雙組合、小於三個時)</summary>
        public static IDictionary<ShiShen, (string desc, string positive, string negative, string advice)> ShiShenCombineLs3Details { get; }
            = new ConcurrentDictionary<ShiShen, (string desc, string positive, string negative, string advice)> {
                [ShiShen.ZhengGuan | ShiShen.QiSha] = (
                    "壓力重重，易受外界環境制約，性格膽小或極端極致",
                    "意志力驚人(若身強)，具備危機意識",
                    "易犯小人，精神焦慮，過於壓抑，健康易受損",
                    "學會減壓。 尋求正印/偏印(學習)來化解壓力，避免過度承擔責任"
                ),
                [ShiShen.ShihShen | ShiShen.ShangGuan] = (
                    "持才傲物，情緒起伏大，追求極致自由",
                    "極具創造力，在藝術、研發領域有天賦",
                    "容易恃才傲物，人際關係易摩擦，做事三分鐘熱度",
                    "收斂鋒芒。 多聽少說，學習正財/偏財(目標管理)將點子落實"
                ),
                [ShiShen.ZhengCai | ShiShen.PianCai] = (
                    "貪多嚼不爛，物欲重，易為財或情困擾",
                    "資源多、機會多，人脈廣闊",
                    "處事虛浮，易有「財多身弱」之感，生活勞碌心累",
                    "懂得止損。 不要同時追逐過多目標，專注於核心事業"
                ),
                [ShiShen.ZhengYin | ShiShen.PianYin] = (
                    "思想包袱重，行動力極差，容易多愁善感",
                    "思考深邃，具備宗教或哲學慧根",
                    "容易空想、鑽牛角尖，性格孤僻且容易錯失良機",
                    "付諸行動。 強制自己減少思考時間，多與外界互動、運動"
                ),
                [ShiShen.BiJian | ShiShen.JieCai] = (
                    "固執己見，好勝心強，易財來財去",
                    "意志力極其強悍，不懼競爭，講義氣",
                    "易破財，人際關係易爭執，不聽勸",
                    "學習謙卑。 練習理財與財務分配，避免與人發生金錢糾紛"
                )
            };

        /// <summary>取得相刑清單</summary>
        public static IList<IList<DiZhi>> TwoXing { get; }
            = [
                [DiZhi.Yin, DiZhi.Si, DiZhi.Shen],  // 無恩之刑 = 寅巳申(外在, 力度強)
			[DiZhi.Chou, DiZhi.Xu, DiZhi.Wei],  // 恃勢之刑 = 丑戌未(外在, 力度強)
            [DiZhi.Zi, DiZhi.Mao],              // 恩愛之刑 = 子卯
            ];

        /// <summary>取得三刑清單</summary>
        public static IList<IList<DiZhi>> ThreeXing { get; }
            = [
                [DiZhi.Yin, DiZhi.Si, DiZhi.Shen],  // 無恩之刑 = 寅巳申(外在, 力度強)
			[DiZhi.Chou, DiZhi.Xu, DiZhi.Wei],  // 恃勢之刑 = 丑戌未(外在, 力度強)
            ];

        /// <summary>取得自刑清單</summary>
        public static IList<DiZhi> SelfXing { get; } = [DiZhi.Chen, DiZhi.Wu, DiZhi.You, DiZhi.Hai];   // 辰辰、午午、酉酉、亥亥

        /// <summary>取得相沖清單</summary>
        public static IList<IList<DiZhi>> Chong { get; }
            = [
                [DiZhi.Zi, DiZhi.Wu],     // 子午
            [DiZhi.Chou, DiZhi.Wei],  // 丑未
            [DiZhi.Yin, DiZhi.Shen],  // 寅申
            [DiZhi.Mao, DiZhi.You],   // 卯酉
            [DiZhi.Chen, DiZhi.Xu],   // 辰戌
            [DiZhi.Si, DiZhi.Hai]     // 巳亥
            ];

        /// <summary>取得破清單</summary>
        public static IList<IList<DiZhi>> Po { get; }
            = [
                [DiZhi.Yin, DiZhi.Hai],     // 寅亥
            [DiZhi.Si, DiZhi.Shen],     // 巳申
            [DiZhi.Zi, DiZhi.You],      // 子酉
            [DiZhi.Wu, DiZhi.Mao],      // 午卯
            [DiZhi.Xu, DiZhi.Wei],      // 戌未
            [DiZhi.Chou, DiZhi.Chen]    // 丑辰
            ];

        /// <summary>取得害清單</summary>
        public static IList<IList<DiZhi>> Hai { get; }
            = [
                [DiZhi.Mao, DiZhi.Chen],    // 卯辰
            [DiZhi.Yin, DiZhi.Si],      // 寅巳
            [DiZhi.Wu, DiZhi.Chou],     // 午丑
            [DiZhi.Zi, DiZhi.Wei],      // 子未
            [DiZhi.You, DiZhi.Xu],      // 酉戌
            [DiZhi.Shen, DiZhi.Hai]     // 申亥
            ];
    }
}

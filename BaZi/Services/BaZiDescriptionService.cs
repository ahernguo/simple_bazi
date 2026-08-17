using BaZi.Models;

namespace BaZi.Services {

    /// <summary>提供命盤分析頁共用的日主文字資料。</summary>
    public sealed class BaZiDescriptionService {

        /// <summary>取得天干在課程中的代表物與描述。</summary>
        public DayMasterProfile GetDayMasterProfile(TianGan dayMaster) {
            return dayMaster switch {
                TianGan.Jia => new DayMasterProfile("大樹", "堅強、有原則、領導開創者、格局大"),
                TianGan.Yi => new DayMasterProfile("藤蔓", "柔軟靈活、協調者、適應力強"),
                TianGan.Bing => new DayMasterProfile("太陽", "開朗、熱情、照顧他人、情緒鮮明、好勝、被注目、不服輸"),
                TianGan.Ding => new DayMasterProfile("燭火", "溫暖細膩、專注、內斂持久、觀察力佳、照顧者"),
                TianGan.Wu => new DayMasterProfile("高山", "可靠、包容、固執、壓迫感"),
                TianGan.Ji => new DayMasterProfile("田土", "細膩、培育滋養、適應力強、溫厚務實"),
                TianGan.Geng => new DayMasterProfile("鋼鐵", "果決、果斷、敢衝敢拚、正義感強、情緒直接"),
                TianGan.Xin => new DayMasterProfile("珠寶", "品味高雅、細節、追求完美、處事彈性、有自知之明、借力使力"),
                TianGan.Ren => new DayMasterProfile("大海", "心胸開闊、智慧包容、善於溝通、大鳴大放、冒險家、做啥大家都知道"),
                TianGan.Gui => new DayMasterProfile("雨露", "默默堅持、長跑、敏感體貼、觀察入微、見解精闢、支持者／顧問／知心朋友"),
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(
                    nameof(dayMaster),
                    (int)dayMaster,
                    typeof(TianGan)
                )
            };
        }

        /// <summary>取得「60 日柱系列」影片已介紹之日柱補充描述；尚未介紹時傳回 null。</summary>
        public string? GetDayPillarDescription(TianGan dayMaster, DiZhi dayBranch) {
            return (dayMaster, dayBranch) switch {
                (TianGan.Jia, DiZhi.Zi) => "重視外表與他人評價，外型亮眼且有自己的品味；需穩定自信並加強資金與資產管理，避免錢財隨性進出。",
                (TianGan.Jia, DiZhi.Yin) => "體能與人緣佳、行動能量強，透過會流汗的戶外運動較能保持專注；合夥時宜親自參與，不能只出資而不經營。",
                (TianGan.Jia, DiZhi.Chen) => "高大挺拔，財運與長輩貴人運佳，尤其容易獲得女性長輩協助；情緒低落時可多接觸陽光與戶外環境。",
                (TianGan.Jia, DiZhi.Wu) => "聰明、有規劃、重效率，能先想好步驟再行動；真正的課題是聚焦，把時間與心力集中在最想投入的方向。",
                (TianGan.Jia, DiZhi.Shen) => "主管、前輩與高資源客戶是重要貴人，事業需要親自拜訪、奔走與承接資源；只靠電話或被動等待較難發揮優勢。",
                (TianGan.Jia, DiZhi.Xu) => "財務基礎與伴侶助力較佳，可與伴侶共同學習商業、投資及資產配置；富足後透過分享與助人形成正向循環。",
                (TianGan.Yi, DiZhi.Mao) => "外柔內剛、沉穩可靠，重視精神交流，卻容易壓抑或含糊表達；把心意、需求與界線說清楚，可減少關係中的誤解。",
                (TianGan.Yi, DiZhi.Chou) => "自律守信，工作、主管與伴侶助力佳，適合在制度完整的環境累積能力；有原則之餘，也要保留彈性與自主判斷。",
                (TianGan.Yi, DiZhi.Si) => "聰明、善溝通且行動力強，適合管理、創業或把興趣發展成副業；主動開口能整合資源，但需管理衝動與情緒。",
                (TianGan.Yi, DiZhi.Wei) => "穩健務實、重視資產帶來的安全感，理財細心且感情忠誠；好惡過於鮮明時，需避免偏愛造成識人或投資盲點。",
                (TianGan.Yi, DiZhi.You) => "邏輯、自律、執行與資源整合能力佳，容易獲得主管提拔並成為重要執行者；感情上也容易遇到能力佳、能互相支持的伴侶。",
                (TianGan.Yi, DiZhi.Hai) => "直覺敏銳、有靈感與宏觀思考，重視精神層面與雙贏；課題是把想法落地，可搭配執行力強、能協助管理財務的夥伴。",
                (TianGan.Bing, DiZhi.Yin) => "有力量感、魄力與大格局，對朋友家人慷慨，情緒來得快也去得快；快速決策與豪爽消費前仍宜保留檢核空間。",
                (TianGan.Bing, DiZhi.Zi) => "外型高挑有品味，專業、責任感與長官緣佳，承諾的事會努力完成；太在意他人觀感時，可能因謹慎而錯過嘗試。",
                (TianGan.Bing, DiZhi.Xu) => "喜歡深度知識，耐力、研究能力與專業見解佳，也常是可靠的意見提供者；價值觀堅定，一旦認同便會長期支持。",
                (TianGan.Bing, DiZhi.Shen) => "事業、客戶與財運表現佳，邏輯、反應與解題效率強，是天生的問題解決者；發展過程可能較忙碌奔波。",
                (TianGan.Bing, DiZhi.Wu) => "外型鮮明、擅長交際並有舞台魅力，適合發言、公關或代表性角色；熱心照顧他人，也享受被看見與獲得掌聲。",
                (TianGan.Bing, DiZhi.Chen) => "工作運佳卻低調，有原則、有主見，不會人云亦云；氣質沉穩而帶書卷味，即使不刻意表現仍有安定的存在感。",
                (TianGan.Ding, DiZhi.Mao) => "親和討喜、長輩緣佳，隨遇而安且不愛競爭；感情上嚮往無壓力、能包容自己並提供安全感的關係。",
                (TianGan.Ding, DiZhi.Chou) => "低調內斂、說話直接而出發點善良，重視理財與家庭財務規劃；適合與尊重生活步調的伴侶穩健經營未來。",
                (TianGan.Ding, DiZhi.Hai) => "正派、自律、重規範與承諾，做事條理分明且值得信任；過度謹慎時容易綁手綁腳，需要練習放鬆與保留創新彈性。",
                (TianGan.Ding, DiZhi.You) => "聰明細緻、反應靈活，在工作與理財上具有優勢；感情重視知識與思考交流，欣賞能共同規劃高品質生活的伴侶。",
                (TianGan.Ding, DiZhi.Wei) => "安靜內斂，觀察、研究、文字與幕後創作能力佳；感情被動卻深情，重視不必多言也能安心相處的靈魂默契。",
                (TianGan.Ding, DiZhi.Si) => "外向活潑、表達直接，熱愛戶外、旅遊與朋友，重視工作生活平衡；感情需要能一起探索與享受生活的合拍玩伴。",
                (TianGan.Wu, DiZhi.Zi) => "規劃、風險意識與財務掌控力強，會替工作與關係預先安排未來；容易思慮過度，感情中最需要信任與分享脆弱。",
                (TianGan.Wu, DiZhi.Yin) => "果斷有效率，責任感、管理力與人脈信用佳，容易獲得主管託付；感情保護慾強，是遇事願意扛責任的伴侶。",
                (TianGan.Wu, DiZhi.Chen) => "有原則、重信用，靠耐力累積事業、資產與長期關係；沉穩可靠但也可能固執，一旦認定便以實際行動長久守護。",
                (TianGan.Wu, DiZhi.Wu) => "陽光有魅力、行動快且能帶動氣氛，喜歡體驗與挑戰；自尊與原則感強，感情欣賞有能力、自信且能並肩同行的對象。",
                (TianGan.Wu, DiZhi.Shen) => "學習快、點子多，擅長連結資訊、商機與跨領域人脈，適合多元發展；需替過多機會排序，感情也重視新鮮感與共同成長。",
                (TianGan.Wu, DiZhi.Xu) => "低調重情、守信守密，會用行動照顧家人朋友，事業屬長期累積型；需建立借貸與人情界線，避免心軟到犧牲自己。",
                _ => null
            };
        }
    }
}

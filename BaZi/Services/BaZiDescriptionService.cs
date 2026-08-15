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
    }
}

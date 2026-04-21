using BaZi.Models;

namespace BaZi.Services {

    /// <summary>提供八字排盤相關服務，封裝 lunar-csharp 呼叫</summary>
    public class BaZiService {

        #region Log
        private static readonly log4net.ILog LOG4N = log4net.LogManager.GetLogger(nameof(BaZiService));
        #endregion

        #region 排盤
        /// <summary>根據國曆生日與性別進行排盤</summary>
        /// <param name="birthDate">國曆生日</param>
        /// <param name="gender">性別 (1:女, 2:男)</param>
        /// <returns>八字命盤資料</returns>
        public BaZiInfo GetBaZiInfo(DateTime birthDate, int gender) {
            LOG4N.Info($"開始排盤: {birthDate:yyyy-MM-dd HH:mm}, 性別: {(gender == 1 ? "女" : "男")}");
            try {
                var info = new BaZiInfo(birthDate, gender);
                LOG4N.Info($"排盤完成: {info.DayZhu.Gan.ToGanString()}{info.DayZhu.Zhi.ToZhiString()} 日主");
                return info;
            } catch (Exception ex) {
                LOG4N.Error("排盤過程發生錯誤", ex);
                throw;
            }
        }
        #endregion
    }
}

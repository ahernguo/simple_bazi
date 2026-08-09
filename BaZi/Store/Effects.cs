using BaZi.Services;
using Fluxor;

namespace BaZi.Store {

    /// <summary>提供開始進行排盤之功能</summary>
    public class BaZiEffects {

        #region Fields
        private readonly BaZiService mSvc;
        #endregion

        #region Constructor
        /// <summary>建構排盤功能</summary>
        /// <param name="baZiService">來源的八字服務</param>
        public BaZiEffects(BaZiService baZiService) {
            mSvc = baZiService;
        }
        #endregion

        #region Effects
        /// <summary>開始進行排盤</summary>
        /// <param name="action">呼叫 Effect 的 Action</param>
        /// <param name="dispatcher">派工器</param>
        /// <returns>非同步處理結果</returns>
        [EffectMethod]
        public Task OnCalculate(CalculateBaZiAction action, Fluxor.IDispatcher dispatcher) {
            try {
                var info = mSvc.GetBaZiInfo(action.BirthDate, action.Gender, action.IsBirthTimeAccurate);
                dispatcher.Dispatch(new CalculateBaZiSuccessAction(info));
            } catch (Exception ex) {
                dispatcher.Dispatch(new CalculateBaZiFailureAction(ex.Message));
            }

            return Task.CompletedTask;
        }
        #endregion
    }
}

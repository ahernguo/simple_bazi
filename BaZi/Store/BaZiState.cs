using BaZi.Models;
using Fluxor;

namespace BaZi.Store {

    /// <summary>提供使用者輸入與八字排盤結果之紀錄</summary>
    [FeatureState]
    public class BaZiState {

        #region Properties
        /// <summary>取得是否已載入服務</summary>
        public bool IsLoading { get; }
        /// <summary>取得是否已進行排盤</summary>
        public bool IsCalculated { get; }
        /// <summary>取得是否有發生錯誤</summary>
        public string? Error { get; }
        /// <summary>取得使用者輸入的生辰</summary>
        public DateTime? BirthDate { get; }
        /// <summary>取得使用者選擇的性別。 (1)女 (2)男</summary>
        public int Gender { get; } // 1:女, 2:男
        /// <summary>取得排盤結果</summary>
        public BaZiInfo? Info { get; }
        #endregion

        #region Constructor
        /// <summary>建立空白的服務</summary>
        public BaZiState() {
            IsLoading = false;
            IsCalculated = false;
            Error = null;
            BirthDate = null;
            Gender = 2; // 預設男
            Info = null;
        }

        /// <summary>建立已處理的服務</summary>
        /// <param name="isLoading">是否已載入服務</param>
        /// <param name="isCalculated">是否已進行排盤</param>
        /// <param name="error">是否有錯誤</param>
        /// <param name="birthDate">使用者輸入的生辰</param>
        /// <param name="gender">使用者輸入的性別</param>
        /// <param name="info">排盤結果</param>
        public BaZiState(bool isLoading, bool isCalculated, string? error, DateTime? birthDate, int gender, BaZiInfo? info) {
            IsLoading = isLoading;
            IsCalculated = isCalculated;
            Error = error;
            BirthDate = birthDate;
            Gender = gender;
            Info = info;
        }
        #endregion
    }
}

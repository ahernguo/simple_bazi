namespace BaZi.Models {

    /// <summary>依幫身分數套用格局數值邊界。</summary>
    public static class StrengthClassifier {

        /// <summary>取得幫身分數對應的格局。</summary>
        /// <param name="score">範圍為 0 到 100 的幫身分數</param>
        /// <returns>格局</returns>
        public static GeJu Classify(int score) {
            ArgumentOutOfRangeException.ThrowIfLessThan(score, 0);
            ArgumentOutOfRangeException.ThrowIfGreaterThan(score, 100);

            if (score > 80) {
                return GeJu.CongQiang;
            }

            if (score >= 45) {
                return GeJu.ShenQiang;
            }

            if (score >= 20) {
                return GeJu.ShenRuo;
            }

            return GeJu.CongRuo;
        }

        /// <summary>取得需要歷史事件複核的疑似從格。</summary>
        /// <param name="score">範圍為 0 到 100 的幫身分數</param>
        /// <returns>疑似從格；一般格局時為 null</returns>
        public static GeJu? GetCandidate(int score) {
            var status = Classify(score);
            return status is GeJu.CongQiang or GeJu.CongRuo ? status : null;
        }
    }
}

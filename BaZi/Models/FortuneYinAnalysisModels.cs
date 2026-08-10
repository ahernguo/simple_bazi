namespace BaZi.Models {

    /// <summary>單一大運、流年或流月的伏吟、反吟分析結果。</summary>
    public sealed record FortuneYinAnalysisResult(
        string PeriodName,
        PeriodFuYinAnalysisResult FuYin,
        PeriodFanYinAnalysisResult FanYin
    ) {
        /// <summary>取得此期間是否有需要顯示的伏吟、反吟或合走內容。</summary>
        public bool HasContent => FuYin.HasFuYin || FanYin.HasContent;
    }
}

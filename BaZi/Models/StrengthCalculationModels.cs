namespace BaZi.Models {

    /// <summary>單一柱位在格局計算中的結果。</summary>
    public sealed record StrengthPositionResult(
        string Key,
        string Label,
        WuXing OriginalElement,
        WuXing EffectiveElement,
        int Weight,
        bool IsSupportive,
        IReadOnlyList<string> Reasons
    ) {
        /// <summary>取得此柱位實際計入的分數。</summary>
        public int Score => IsSupportive ? Weight : 0;
    }

    /// <summary>格局分數與各柱位判定明細。</summary>
    public sealed record StrengthCalculationResult(
        int TotalScore,
        IReadOnlyList<StrengthPositionResult> Positions
    );
}

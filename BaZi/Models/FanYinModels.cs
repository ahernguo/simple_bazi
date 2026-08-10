namespace BaZi.Models {

    /// <summary>原命盤中某兩柱干與干相剋，支與支是相剋相沖的反吟柱。</summary>
    public sealed record FanYinMatch(
        IReadOnlyList<Zhu> Pillars,
        string Situation
    );

    /// <summary>原命盤反吟分析結果。</summary>
    public sealed record FanYinAnalysisResult(
        IReadOnlyList<FanYinMatch> Matches,
        bool IncludesHourPillar
    ) {
        /// <summary>取得原命盤是否有反吟。</summary>
        public bool HasFanYin => Matches.Count > 0;
    }
}

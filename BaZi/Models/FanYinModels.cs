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

    /// <summary>大運、流年或流月與較早命運柱形成的反吟。</summary>
    public sealed record PeriodFanYinMatch(
        IGanZhi SourcePillar,
        IGanZhi PeriodPillar,
        string Situation
    );

    /// <summary>指定運柱同時以天干五合、地支六合連結既有反吟其中一柱的結果。</summary>
    public sealed record FanYinCombination(
        IGanZhi FirstFanYinPillar,
        IGanZhi SecondFanYinPillar,
        IGanZhi CombinedPillar,
        IGanZhi CombiningPillar
    );

    /// <summary>指定大運、流年或流月的反吟與合絆並存分析結果。</summary>
    public sealed record PeriodFanYinAnalysisResult(
        IReadOnlyList<PeriodFanYinMatch> Matches,
        IReadOnlyList<FanYinCombination> Combinations,
        bool IncludesHourPillar
    ) {
        /// <summary>取得指定期間是否形成反吟。</summary>
        public bool HasFanYin => Matches.Count > 0;

        /// <summary>取得指定期間是否有反吟或與既有反吟並存的合絆。</summary>
        public bool HasContent => HasFanYin || Combinations.Count > 0;
    }
}

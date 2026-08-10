namespace BaZi.Models {

    /// <summary>原命盤中一組天干、地支完全相同的伏吟柱。</summary>
    public sealed record FuYinMatch(
        TianGan Gan,
        DiZhi Zhi,
        IReadOnlyList<string> PillarNames,
        string Situation
    );

    /// <summary>原命盤伏吟分析結果。</summary>
    public sealed record FuYinAnalysisResult(
        IReadOnlyList<FuYinMatch> Matches,
        bool IncludesHourPillar
    ) {
        /// <summary>取得原命盤是否有伏吟。</summary>
        public bool HasFuYin => Matches.Count > 0;
    }

    /// <summary>大運、流年或流月與較早命運柱形成的伏吟。</summary>
    public sealed record PeriodFuYinMatch(
        IGanZhi SourcePillar,
        IGanZhi PeriodPillar,
        string Situation
    );

    /// <summary>指定大運、流年或流月的伏吟分析結果。</summary>
    public sealed record PeriodFuYinAnalysisResult(
        IReadOnlyList<PeriodFuYinMatch> Matches,
        bool IncludesHourPillar
    ) {
        /// <summary>取得指定期間是否有伏吟。</summary>
        public bool HasFuYin => Matches.Count > 0;
    }
}

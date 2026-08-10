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
}

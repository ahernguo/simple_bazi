namespace BaZi.Models {

    /// <summary>本命地支與流年地支的太歲互動類型。</summary>
    public enum TaiSuiInteractionType {
        SameBranch,
        SixClash,
        Punishment,
        SixHarm,
        SixBreak
    }

    /// <summary>命盤單一宮位與流年地支的間接犯太歲互動。</summary>
    public sealed record TaiSuiPillarInteraction(
        string PillarName,
        DiZhi NatalBranch,
        IReadOnlyList<TaiSuiInteractionType> Interactions
    );

    /// <summary>流年同支使原局既有互動再次集中的來源。</summary>
    public sealed record TaiSuiReinforcedInteraction(
        string RepeatedPillarName,
        DiZhi RepeatedBranch,
        string RelatedPillarName,
        DiZhi RelatedBranch,
        IReadOnlyList<TaiSuiInteractionType> Interactions
    );

    /// <summary>指定流年與本命四柱的犯太歲及生肖相沖分析。</summary>
    public sealed record TaiSuiAnalysisResult(
        int Year,
        TianGan AnnualStem,
        DiZhi AnnualBranch,
        string AnnualZodiac,
        string NatalZodiac,
        DiZhi NatalYearBranch,
        IReadOnlyList<TaiSuiInteractionType> DirectInteractions,
        IReadOnlyList<TaiSuiPillarInteraction> IndirectInteractions,
        bool IsHourPillarIncluded,
        IReadOnlyList<TaiSuiReinforcedInteraction>? ReinforcedInteractions = null
    ) {
        /// <summary>取得本命生肖是否與流年生肖形成六沖。</summary>
        public bool IsZodiacClash => DirectInteractions.Contains(TaiSuiInteractionType.SixClash);

        /// <summary>取得出生年支是否形成任一種直接犯太歲關係。</summary>
        public bool HasDirectTaiSui => DirectInteractions.Count > 0;

        /// <summary>取得月、日、時支是否形成間接犯太歲關係。</summary>
        public bool HasIndirectTaiSui => IndirectInteractions.Count > 0;

        /// <summary>取得原局既有互動是否因流年同支而再次集中。</summary>
        public bool HasReinforcedInteraction => ReinforcedInteractions is { Count: > 0 };
    }
}

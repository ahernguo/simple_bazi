namespace BaZi.Models {

    /// <summary>地支互動規則類型。</summary>
    public enum BranchRelationshipType {
        SixCombination,
        SixClash,
        SixHarm,
        Punishment,
        ThreeMeeting
    }

    /// <summary>地支互動命中的命盤範圍。</summary>
    public enum BranchRelationshipScope {
        NatalA,
        NatalB,
        CrossChart
    }

    /// <summary>地支互動組合的完成狀態。</summary>
    public enum BranchRelationshipCompletion {
        Pair,
        Partial,
        Complete,
        Self,
        Candidate
    }

    /// <summary>地支互動資料的信心等級。</summary>
    public enum BranchRelationshipConfidence {
        Confirmed,
        Hypothetical
    }

    /// <summary>地支來源所屬命盤。</summary>
    public enum BranchRelationshipParticipant {
        A,
        B
    }

    /// <summary>地支來源的四柱位置。</summary>
    public enum BranchRelationshipPillarPosition {
        Year,
        Month,
        Day,
        Hour
    }

    /// <summary>不含柱位的固定地支規則命中。</summary>
    public sealed record BranchRelationshipRuleMatch(
        BranchRelationshipType RelationType,
        IReadOnlyList<DiZhi> Members,
        BranchRelationshipCompletion Completion,
        WuXing? TransformElement,
        IReadOnlyList<string> InterpretationKeys
    );

    /// <summary>單一命中成員的命盤與柱位來源。</summary>
    public sealed record BranchRelationshipSource(
        BranchRelationshipParticipant Participant,
        BranchRelationshipPillarPosition Position,
        DiZhi Branch,
        BranchRelationshipConfidence Confidence
    ) {
        /// <summary>取得可追溯至命盤與柱位的識別字串。</summary>
        public string SourcePosition => $"{Participant}.{Position}";
    }

    /// <summary>一組實際柱位構成的地支互動。</summary>
    public sealed record BranchRelationshipOccurrence(
        IReadOnlyList<BranchRelationshipSource> Sources
    );

    /// <summary>依規則、成員與範圍聚合後的地支互動命中。</summary>
    public sealed record BranchRelationshipHit(
        BranchRelationshipType RelationType,
        IReadOnlyList<DiZhi> Members,
        BranchRelationshipScope Scope,
        IReadOnlyList<BranchRelationshipOccurrence> Occurrences,
        BranchRelationshipCompletion Completion,
        WuXing? TransformElement,
        BranchRelationshipConfidence Confidence,
        IReadOnlyList<string> InterpretationKeys
    ) {
        /// <summary>取得實際柱位組合的命中次數。</summary>
        public int OccurrenceCount => Occurrences.Count;

        /// <summary>取得參與此命中的命盤。</summary>
        public IReadOnlyList<BranchRelationshipParticipant> Participants => [
            .. Occurrences
                .SelectMany(occurrence => occurrence.Sources)
                .Select(source => source.Participant)
                .Distinct()
                .OrderBy(participant => participant)
        ];

        /// <summary>取得所有實際命中的命盤與柱位。</summary>
        public IReadOnlyList<string> SourcePositions => [
            .. Occurrences
                .SelectMany(occurrence => occurrence.Sources)
                .Select(source => source.SourcePosition)
                .Distinct()
        ];
    }

    /// <summary>兩張命盤的完整地支互動分析。</summary>
    public sealed record BranchRelationshipAnalysis(
        IReadOnlyList<BranchRelationshipHit> Hits,
        bool UsesThreePillarsForA,
        bool UsesThreePillarsForB
    );
}

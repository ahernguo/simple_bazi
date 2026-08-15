using BaZi.Models;

namespace BaZi.Services {

    /// <summary>以既有四柱與地支資料統一判定六合、六沖、六害、六破、相刑、三合及三會候選。</summary>
    public sealed class EarthlyBranchRelationshipEngine {
        private static readonly IReadOnlyList<DiZhi> BranchOrder = [
            DiZhi.Zi,
            DiZhi.Chou,
            DiZhi.Yin,
            DiZhi.Mao,
            DiZhi.Chen,
            DiZhi.Si,
            DiZhi.Wu,
            DiZhi.Wei,
            DiZhi.Shen,
            DiZhi.You,
            DiZhi.Xu,
            DiZhi.Hai
        ];

        /// <summary>判定兩個地支直接形成的固定規則。</summary>
        public IReadOnlyList<BranchRelationshipRuleMatch> MatchPair(DiZhi first, DiZhi second) {
            var matches = new List<BranchRelationshipRuleMatch>();
            var members = NormalizeMembers(first, second);

            if (first == second) {
                if (BaZiDefine.SelfXing.Contains(first)) {
                    matches.Add(new BranchRelationshipRuleMatch(
                        BranchRelationshipType.Punishment,
                        members,
                        BranchRelationshipCompletion.Self,
                        null,
                        GetInterpretationKeys(BranchRelationshipType.Punishment)
                    ));
                }

                return matches;
            }

            foreach (KeyValuePair<WuXing, DiZhi[]> rule in BaZiDefine.SixHe) {
                if (ContainsPair(rule.Value, first, second)) {
                    matches.Add(new BranchRelationshipRuleMatch(
                        BranchRelationshipType.SixCombination,
                        members,
                        BranchRelationshipCompletion.Candidate,
                        NormalizeCombinationElement(rule.Key),
                        GetInterpretationKeys(BranchRelationshipType.SixCombination)
                    ));
                }
            }

            if (BaZiDefine.Chong.Any(rule => ContainsPair(rule, first, second))) {
                matches.Add(new BranchRelationshipRuleMatch(
                    BranchRelationshipType.SixClash,
                    members,
                    BranchRelationshipCompletion.Pair,
                    null,
                    GetInterpretationKeys(BranchRelationshipType.SixClash)
                ));
            }

            if (BaZiDefine.Hai.Any(rule => ContainsPair(rule, first, second))) {
                matches.Add(new BranchRelationshipRuleMatch(
                    BranchRelationshipType.SixHarm,
                    members,
                    BranchRelationshipCompletion.Pair,
                    null,
                    GetInterpretationKeys(BranchRelationshipType.SixHarm)
                ));
            }

            if (BaZiDefine.Po.Any(rule => ContainsPair(rule, first, second))) {
                matches.Add(new BranchRelationshipRuleMatch(
                    BranchRelationshipType.SixBreak,
                    members,
                    BranchRelationshipCompletion.Pair,
                    null,
                    GetInterpretationKeys(BranchRelationshipType.SixBreak)
                ));
            }

            IList<DiZhi>? punishmentGroup = BaZiDefine.TwoXing
                .FirstOrDefault(rule => ContainsPair(rule, first, second));
            if (punishmentGroup is not null) {
                matches.Add(new BranchRelationshipRuleMatch(
                    BranchRelationshipType.Punishment,
                    members,
                    punishmentGroup.Count == 2
                        ? BranchRelationshipCompletion.Pair
                        : BranchRelationshipCompletion.Partial,
                    null,
                    GetInterpretationKeys(BranchRelationshipType.Punishment)
                ));
            }

            return matches;
        }

        /// <summary>判定兩個地支是否形成指定的固定規則；同時保留可能重疊的其他規則。</summary>
        public bool HasRelationship(DiZhi first, DiZhi second, BranchRelationshipType relationType) {
            return MatchPair(first, second).Any(match => match.RelationType == relationType);
        }

        /// <summary>分析兩張命盤；出生時辰不準確者只使用年、月、日三柱。</summary>
        public BranchRelationshipAnalysis Analyze(BaZiInfo chartA, BaZiInfo chartB) {
            ArgumentNullException.ThrowIfNull(chartA);
            ArgumentNullException.ThrowIfNull(chartB);

            return AnalyzeCore(chartA, chartB, null);
        }

        /// <summary>建立獨立的假設時柱情境，不改變基準命盤分析。</summary>
        public BranchRelationshipAnalysis AnalyzeHypotheticalHour(
            BaZiInfo chartA,
            BaZiInfo chartB,
            BranchRelationshipParticipant participant
        ) {
            ArgumentNullException.ThrowIfNull(chartA);
            ArgumentNullException.ThrowIfNull(chartB);

            var hypotheticalChart = participant == BranchRelationshipParticipant.A ? chartA : chartB;
            if (hypotheticalChart.IsBirthTimeAccurate) {
                throw new InvalidOperationException("只有出生時辰不準確的命盤可建立假設時柱情境。");
            }

            return AnalyzeCore(chartA, chartB, participant);
        }

        private BranchRelationshipAnalysis AnalyzeCore(
            BaZiInfo chartA,
            BaZiInfo chartB,
            BranchRelationshipParticipant? hypotheticalParticipant
        ) {
            IReadOnlyList<BranchRelationshipSource> sourcesA = GetSources(
                chartA,
                BranchRelationshipParticipant.A,
                hypotheticalParticipant == BranchRelationshipParticipant.A
            );
            IReadOnlyList<BranchRelationshipSource> sourcesB = GetSources(
                chartB,
                BranchRelationshipParticipant.B,
                hypotheticalParticipant == BranchRelationshipParticipant.B
            );
            var candidates = new List<HitCandidate>();

            AddNatalPairCandidates(sourcesA, BranchRelationshipScope.NatalA, candidates);
            AddNatalPairCandidates(sourcesB, BranchRelationshipScope.NatalB, candidates);
            AddCrossPairCandidates(sourcesA, sourcesB, candidates);

            AddGroupCandidates(sourcesA, BranchRelationshipScope.NatalA, candidates);
            AddGroupCandidates(sourcesB, BranchRelationshipScope.NatalB, candidates);
            AddGroupCandidates(
                [.. sourcesA, .. sourcesB],
                BranchRelationshipScope.CrossChart,
                candidates
            );

            return new BranchRelationshipAnalysis(
                Aggregate(candidates),
                !chartA.IsBirthTimeAccurate && hypotheticalParticipant != BranchRelationshipParticipant.A,
                !chartB.IsBirthTimeAccurate && hypotheticalParticipant != BranchRelationshipParticipant.B
            );
        }

        private void AddNatalPairCandidates(
            IReadOnlyList<BranchRelationshipSource> sources,
            BranchRelationshipScope scope,
            ICollection<HitCandidate> candidates
        ) {
            for (var firstIndex = 0; firstIndex < sources.Count; firstIndex++) {
                for (var secondIndex = firstIndex + 1; secondIndex < sources.Count; secondIndex++) {
                    AddPairCandidates(sources[firstIndex], sources[secondIndex], scope, candidates);
                }
            }
        }

        private void AddCrossPairCandidates(
            IReadOnlyList<BranchRelationshipSource> sourcesA,
            IReadOnlyList<BranchRelationshipSource> sourcesB,
            ICollection<HitCandidate> candidates
        ) {
            foreach (BranchRelationshipSource sourceA in sourcesA) {
                foreach (BranchRelationshipSource sourceB in sourcesB) {
                    AddPairCandidates(
                        sourceA,
                        sourceB,
                        BranchRelationshipScope.CrossChart,
                        candidates
                    );
                }
            }
        }

        private void AddPairCandidates(
            BranchRelationshipSource first,
            BranchRelationshipSource second,
            BranchRelationshipScope scope,
            ICollection<HitCandidate> candidates
        ) {
            IReadOnlyList<BranchRelationshipRuleMatch> matches = MatchPair(first.Branch, second.Branch);
            foreach (BranchRelationshipRuleMatch match in matches) {
                candidates.Add(CreateCandidate(
                    match.RelationType,
                    match.Members,
                    scope,
                    match.Completion,
                    match.TransformElement,
                    match.InterpretationKeys,
                    [first, second]
                ));
            }
        }

        private static void AddGroupCandidates(
            IReadOnlyList<BranchRelationshipSource> sources,
            BranchRelationshipScope scope,
            ICollection<HitCandidate> candidates
        ) {
            foreach (IList<DiZhi> group in BaZiDefine.ThreeXing) {
                var availableMembers = group.Where(member => sources.Any(source => source.Branch == member)).ToArray();
                if (availableMembers.Length != group.Count) {
                    continue;
                }

                AddSelections(
                    BranchRelationshipType.Punishment,
                    availableMembers,
                    sources,
                    scope,
                    BranchRelationshipCompletion.Complete,
                    null,
                    GetInterpretationKeys(BranchRelationshipType.Punishment),
                    candidates
                );
            }

            var threeCombinationGroups = BaZiDefine.ThreeHe
                .GroupBy(rule => string.Join(",", rule.Value.Select(branch => (int)branch)))
                .Select(group => new {
                    Members = group.First().Value,
                    TransformElement = group.Count() == 1 ? group.First().Key : (WuXing?)null
                });
            foreach (var group in threeCombinationGroups) {
                var availableMembers = group.Members
                    .Where(member => sources.Any(source => source.Branch == member))
                    .ToArray();
                if (availableMembers.Length < 2) {
                    continue;
                }

                AddSelections(
                    BranchRelationshipType.ThreeCombination,
                    availableMembers,
                    sources,
                    scope,
                    availableMembers.Length == group.Members.Length
                        ? BranchRelationshipCompletion.Complete
                        : BranchRelationshipCompletion.Partial,
                    group.TransformElement,
                    GetInterpretationKeys(BranchRelationshipType.ThreeCombination),
                    candidates
                );
            }

            foreach (KeyValuePair<WuXing, DiZhi[]> group in BaZiDefine.ThreeHui) {
                if (group.Value.All(member => sources.Any(source => source.Branch == member))) {
                    AddSelections(
                        BranchRelationshipType.ThreeMeeting,
                        group.Value,
                        sources,
                        scope,
                        BranchRelationshipCompletion.Candidate,
                        group.Key,
                        GetInterpretationKeys(BranchRelationshipType.ThreeMeeting),
                        candidates
                    );
                }
            }
        }

        private static void AddSelections(
            BranchRelationshipType relationType,
            IReadOnlyList<DiZhi> members,
            IReadOnlyList<BranchRelationshipSource> sources,
            BranchRelationshipScope scope,
            BranchRelationshipCompletion completion,
            WuXing? transformElement,
            IReadOnlyList<string> interpretationKeys,
            ICollection<HitCandidate> candidates
        ) {
            var choices = members
                .Select(member => sources.Where(source => source.Branch == member).ToArray())
                .ToArray();
            foreach (IReadOnlyList<BranchRelationshipSource> selection in SelectOneFromEach(choices)) {
                if (scope == BranchRelationshipScope.CrossChart
                    && selection.Select(source => source.Participant).Distinct().Count() < 2) {
                    continue;
                }

                candidates.Add(CreateCandidate(
                    relationType,
                    members,
                    scope,
                    completion,
                    transformElement,
                    interpretationKeys,
                    selection
                ));
            }
        }

        private static IReadOnlyList<IReadOnlyList<BranchRelationshipSource>> SelectOneFromEach(
            IReadOnlyList<BranchRelationshipSource[]> choices
        ) {
            var results = new List<IReadOnlyList<BranchRelationshipSource>>();
            SelectOneFromEach(choices, 0, [], results);
            return results;
        }

        private static void SelectOneFromEach(
            IReadOnlyList<BranchRelationshipSource[]> choices,
            int index,
            IReadOnlyList<BranchRelationshipSource> current,
            ICollection<IReadOnlyList<BranchRelationshipSource>> results
        ) {
            if (index == choices.Count) {
                results.Add(current);
                return;
            }

            foreach (BranchRelationshipSource source in choices[index]) {
                SelectOneFromEach(choices, index + 1, [.. current, source], results);
            }
        }

        private static IReadOnlyList<BranchRelationshipSource> GetSources(
            BaZiInfo chart,
            BranchRelationshipParticipant participant,
            bool includeHypotheticalHour
        ) {
            var sources = new List<BranchRelationshipSource> {
                new(participant, BranchRelationshipPillarPosition.Year, chart.YearZhu.Zhi, BranchRelationshipConfidence.Confirmed),
                new(participant, BranchRelationshipPillarPosition.Month, chart.MonthZhu.Zhi, BranchRelationshipConfidence.Confirmed),
                new(participant, BranchRelationshipPillarPosition.Day, chart.DayZhu.Zhi, BranchRelationshipConfidence.Confirmed)
            };

            if (chart.IsBirthTimeAccurate || includeHypotheticalHour) {
                sources.Add(new BranchRelationshipSource(
                    participant,
                    BranchRelationshipPillarPosition.Hour,
                    chart.HourZhu.Zhi,
                    chart.IsBirthTimeAccurate
                        ? BranchRelationshipConfidence.Confirmed
                        : BranchRelationshipConfidence.Hypothetical
                ));
            }

            return sources;
        }

        private static HitCandidate CreateCandidate(
            BranchRelationshipType relationType,
            IReadOnlyList<DiZhi> members,
            BranchRelationshipScope scope,
            BranchRelationshipCompletion completion,
            WuXing? transformElement,
            IReadOnlyList<string> interpretationKeys,
            IReadOnlyList<BranchRelationshipSource> sources
        ) {
            var confidence = sources.Any(source => source.Confidence == BranchRelationshipConfidence.Hypothetical)
                ? BranchRelationshipConfidence.Hypothetical
                : BranchRelationshipConfidence.Confirmed;
            return new HitCandidate(
                relationType,
                members,
                scope,
                new BranchRelationshipOccurrence(sources),
                completion,
                transformElement,
                confidence,
                interpretationKeys
            );
        }

        private static IReadOnlyList<BranchRelationshipHit> Aggregate(
            IReadOnlyList<HitCandidate> candidates
        ) {
            return [.. candidates
                .GroupBy(candidate => new {
                    candidate.RelationType,
                    Members = string.Join(",", candidate.Members.Select(member => (int)member)),
                    candidate.Scope,
                    candidate.Completion,
                    candidate.TransformElement
                })
                .Select(group => new BranchRelationshipHit(
                    group.Key.RelationType,
                    group.First().Members,
                    group.Key.Scope,
                    [.. group.Select(candidate => candidate.Occurrence)],
                    group.Key.Completion,
                    group.Key.TransformElement,
                    group.Any(candidate => candidate.Confidence == BranchRelationshipConfidence.Hypothetical)
                        ? BranchRelationshipConfidence.Hypothetical
                        : BranchRelationshipConfidence.Confirmed,
                    group.First().InterpretationKeys
                ))
                .OrderBy(hit => hit.RelationType)
                .ThenBy(hit => hit.Scope)
                .ThenBy(hit => GetBranchIndex(hit.Members[0]))
                .ThenBy(hit => hit.Members.Count)];
        }

        private static IReadOnlyList<DiZhi> NormalizeMembers(DiZhi first, DiZhi second) {
            return GetBranchIndex(first) <= GetBranchIndex(second)
                ? [first, second]
                : [second, first];
        }

        private static bool ContainsPair(IEnumerable<DiZhi> members, DiZhi first, DiZhi second) {
            return members.Contains(first) && members.Contains(second);
        }

        private static int GetBranchIndex(DiZhi branch) {
            for (var index = 0; index < BranchOrder.Count; index++) {
                if (BranchOrder[index] == branch) {
                    return index;
                }
            }

            throw new ArgumentOutOfRangeException(nameof(branch), branch, null);
        }

        private static WuXing NormalizeCombinationElement(WuXing element) {
            // 既有六合表以「火 | 土」區分午未與子丑兩個土合；規則輸出仍依規格標準化為土候選。
            return element == (WuXing.Huo | WuXing.Tu) ? WuXing.Tu : element;
        }

        private static IReadOnlyList<string> GetInterpretationKeys(BranchRelationshipType relationType) {
            return relationType switch {
                BranchRelationshipType.SixCombination => ["attraction", "familiarity", "coordination"],
                BranchRelationshipType.SixClash => ["difference", "change", "push-pull"],
                BranchRelationshipType.SixHarm => ["implicit-discomfort", "misunderstanding", "trust-sensitivity"],
                BranchRelationshipType.SixBreak => ["disruption", "instability", "repair-needed"],
                BranchRelationshipType.Punishment => ["reactivity", "defensiveness", "accumulated-pressure"],
                BranchRelationshipType.ThreeCombination => ["shared-direction", "coordination", "transformation-candidate"],
                BranchRelationshipType.ThreeMeeting => ["shared-direction", "concentration", "transformation-candidate"],
                _ => throw new ArgumentOutOfRangeException(nameof(relationType), relationType, null)
            };
        }

        private sealed record HitCandidate(
            BranchRelationshipType RelationType,
            IReadOnlyList<DiZhi> Members,
            BranchRelationshipScope Scope,
            BranchRelationshipOccurrence Occurrence,
            BranchRelationshipCompletion Completion,
            WuXing? TransformElement,
            BranchRelationshipConfidence Confidence,
            IReadOnlyList<string> InterpretationKeys
        );
    }
}

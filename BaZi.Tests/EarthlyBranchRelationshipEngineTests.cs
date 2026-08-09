using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class EarthlyBranchRelationshipEngineTests {
        private readonly BaZiService _baZiService = new();
        private readonly EarthlyBranchRelationshipEngine _engine = new();

        [Theory]
        [InlineData(DiZhi.Zi, DiZhi.Chou, WuXing.Tu)]
        [InlineData(DiZhi.Yin, DiZhi.Hai, WuXing.Mu)]
        [InlineData(DiZhi.Mao, DiZhi.Xu, WuXing.Huo)]
        [InlineData(DiZhi.Chen, DiZhi.You, WuXing.Jin)]
        [InlineData(DiZhi.Si, DiZhi.Shen, WuXing.Shui)]
        [InlineData(DiZhi.Wu, DiZhi.Wei, WuXing.Tu)]
        public void MatchPair_AllSixCombinations_ReturnCandidate(
            DiZhi first,
            DiZhi second,
            WuXing expectedElement
        ) {
            IReadOnlyList<BranchRelationshipRuleMatch> matches = _engine.MatchPair(first, second);

            var match = Assert.Single(matches, item =>
                item.RelationType == BranchRelationshipType.SixCombination);
            Assert.Equal(BranchRelationshipCompletion.Candidate, match.Completion);
            Assert.Equal(expectedElement, match.TransformElement);
        }

        [Fact]
        public void MatchPair_ReversedCombination_ReturnsSameRule() {
            var forward = Assert.Single(_engine.MatchPair(DiZhi.Si, DiZhi.Shen), item =>
                item.RelationType == BranchRelationshipType.SixCombination);
            var reverse = Assert.Single(_engine.MatchPair(DiZhi.Shen, DiZhi.Si), item =>
                item.RelationType == BranchRelationshipType.SixCombination);

            Assert.Equal(forward.RelationType, reverse.RelationType);
            Assert.True(forward.Members.SequenceEqual(reverse.Members));
            Assert.Equal(forward.Completion, reverse.Completion);
            Assert.Equal(forward.TransformElement, reverse.TransformElement);
        }

        [Theory]
        [InlineData(DiZhi.Zi, DiZhi.Wu)]
        [InlineData(DiZhi.Chou, DiZhi.Wei)]
        [InlineData(DiZhi.Yin, DiZhi.Shen)]
        [InlineData(DiZhi.Mao, DiZhi.You)]
        [InlineData(DiZhi.Chen, DiZhi.Xu)]
        [InlineData(DiZhi.Si, DiZhi.Hai)]
        public void MatchPair_AllSixClashes_ReturnPair(DiZhi first, DiZhi second) {
            IReadOnlyList<BranchRelationshipRuleMatch> matches = _engine.MatchPair(first, second);

            var match = Assert.Single(matches, item =>
                item.RelationType == BranchRelationshipType.SixClash);
            Assert.Equal(BranchRelationshipCompletion.Pair, match.Completion);
        }

        [Fact]
        public void MatchPair_UnrelatedBranches_ReturnsNoRule() {
            IReadOnlyList<BranchRelationshipRuleMatch> matches = _engine.MatchPair(DiZhi.Zi, DiZhi.Yin);

            Assert.Empty(matches);
        }

        [Fact]
        public void Analyze_CombinationAndClashExist_PreservesBoth() {
            BranchRelationshipAnalysis analysis = _engine.Analyze(CreateChartA(), CreateChartB());

            Assert.Contains(analysis.Hits, hit =>
                hit.RelationType == BranchRelationshipType.SixCombination);
            Assert.Contains(analysis.Hits, hit =>
                hit.RelationType == BranchRelationshipType.SixClash);
        }

        [Fact]
        public void Analyze_UnknownHour_ExcludesHourFromEveryHit() {
            BranchRelationshipAnalysis analysis = _engine.Analyze(CreateChartA(), CreateChartB());

            Assert.True(analysis.UsesThreePillarsForB);
            Assert.DoesNotContain(
                analysis.Hits.SelectMany(hit => hit.Occurrences)
                    .SelectMany(occurrence => occurrence.Sources),
                source => source.Participant == BranchRelationshipParticipant.B
                    && source.Position == BranchRelationshipPillarPosition.Hour
            );
        }

        [Fact]
        public void AnalyzeHypotheticalHour_OnlyScenarioContainsHypotheticalHour() {
            BaZiInfo chartA = CreateChartA();
            BaZiInfo chartB = CreateChartB();

            BranchRelationshipAnalysis baseline = _engine.Analyze(chartA, chartB);
            BranchRelationshipAnalysis scenario = _engine.AnalyzeHypotheticalHour(
                chartA,
                chartB,
                BranchRelationshipParticipant.B
            );

            Assert.DoesNotContain(baseline.Hits, hit =>
                hit.Confidence == BranchRelationshipConfidence.Hypothetical);
            Assert.Contains(scenario.Hits, hit =>
                hit.Confidence == BranchRelationshipConfidence.Hypothetical);
            Assert.Contains(
                scenario.Hits.SelectMany(hit => hit.Occurrences)
                    .SelectMany(occurrence => occurrence.Sources),
                source => source.Participant == BranchRelationshipParticipant.B
                    && source.Position == BranchRelationshipPillarPosition.Hour
                    && source.Confidence == BranchRelationshipConfidence.Hypothetical
            );
            Assert.Contains(scenario.Hits, hit =>
                hit.RelationType == BranchRelationshipType.ThreeMeeting
                && hit.Members.SequenceEqual([DiZhi.Hai, DiZhi.Zi, DiZhi.Chou])
                && hit.Confidence == BranchRelationshipConfidence.Hypothetical);
        }

        [Fact]
        public void Analyze_RepeatedBranch_PreservesPositionsAndAggregatesCount() {
            BranchRelationshipAnalysis analysis = _engine.Analyze(CreateChartA(), CreateChartB());

            var hit = Assert.Single(analysis.Hits, item =>
                item.Scope == BranchRelationshipScope.CrossChart
                && item.RelationType == BranchRelationshipType.SixCombination
                && item.Members.SequenceEqual([DiZhi.Si, DiZhi.Shen]));
            Assert.Equal(2, hit.OccurrenceCount);
            Assert.Contains(hit.SourcePositions, position => position == "A.Year");
            Assert.Contains(hit.SourcePositions, position => position == "A.Hour");
            Assert.Contains(hit.SourcePositions, position => position == "B.Year");
        }

        [Fact]
        public void Analyze_ThreePunishment_DistinguishesCompleteAndPartial() {
            BranchRelationshipAnalysis analysis = _engine.Analyze(CreateChartA(), CreateChartB());

            var complete = Assert.Single(analysis.Hits, hit =>
                hit.Scope == BranchRelationshipScope.CrossChart
                && hit.RelationType == BranchRelationshipType.Punishment
                && hit.Members.SequenceEqual([DiZhi.Yin, DiZhi.Si, DiZhi.Shen]));
            var partial = Assert.Single(analysis.Hits, hit =>
                hit.Scope == BranchRelationshipScope.NatalB
                && hit.RelationType == BranchRelationshipType.Punishment
                && hit.Members.SequenceEqual([DiZhi.Yin, DiZhi.Shen]));
            Assert.Equal(BranchRelationshipCompletion.Complete, complete.Completion);
            Assert.Equal(BranchRelationshipCompletion.Partial, partial.Completion);
        }

        [Fact]
        public void Analyze_SpecificationCase_ReturnsExpectedMainHits() {
            BranchRelationshipAnalysis analysis = _engine.Analyze(CreateChartA(), CreateChartB());

            AssertHit(analysis, BranchRelationshipScope.NatalB, BranchRelationshipType.SixClash, 1, DiZhi.Yin, DiZhi.Shen);
            AssertHit(analysis, BranchRelationshipScope.NatalB, BranchRelationshipType.SixHarm, 1, DiZhi.Shen, DiZhi.Hai);
            AssertHit(analysis, BranchRelationshipScope.NatalB, BranchRelationshipType.SixCombination, 1, DiZhi.Yin, DiZhi.Hai);
            AssertHit(analysis, BranchRelationshipScope.CrossChart, BranchRelationshipType.SixCombination, 2, DiZhi.Si, DiZhi.Shen);
            AssertHit(analysis, BranchRelationshipScope.CrossChart, BranchRelationshipType.SixClash, 2, DiZhi.Si, DiZhi.Hai);
            AssertHit(analysis, BranchRelationshipScope.CrossChart, BranchRelationshipType.SixHarm, 2, DiZhi.Yin, DiZhi.Si);
            Assert.DoesNotContain(analysis.Hits, hit =>
                hit.Scope == BranchRelationshipScope.CrossChart
                && hit.Members.Contains(DiZhi.Chou)
                && hit.Members.Contains(DiZhi.Hai)
                && hit.RelationType is BranchRelationshipType.SixCombination
                    or BranchRelationshipType.SixClash
                    or BranchRelationshipType.SixHarm);
        }

        private BaZiInfo CreateChartA() {
            return _baZiService.GetBaZiInfo(new DateTime(1990, 1, 12, 9, 0, 0), 2);
        }

        private BaZiInfo CreateChartB(int gender = 1) {
            return _baZiService.GetBaZiInfo(
                new DateTime(1992, 2, 17, 0, 0, 0),
                gender,
                false
            );
        }

        private static void AssertHit(
            BranchRelationshipAnalysis analysis,
            BranchRelationshipScope scope,
            BranchRelationshipType relationType,
            int occurrenceCount,
            params DiZhi[] members
        ) {
            var hit = Assert.Single(analysis.Hits, item =>
                item.Scope == scope
                && item.RelationType == relationType
                && item.Members.SequenceEqual(members));
            Assert.Equal(occurrenceCount, hit.OccurrenceCount);
        }
    }
}

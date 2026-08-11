using BaZi.Models;
using Xunit;

namespace BaZi.Tests {

    public sealed class StrengthClassifierTests {
        [Theory]
        [InlineData(0, GeJu.CongRuo)]
        [InlineData(19, GeJu.CongRuo)]
        [InlineData(20, GeJu.ShenRuo)]
        [InlineData(44, GeJu.ShenRuo)]
        [InlineData(45, GeJu.ShenQiang)]
        [InlineData(80, GeJu.ShenQiang)]
        [InlineData(81, GeJu.CongQiang)]
        [InlineData(100, GeJu.CongQiang)]
        public void Classify_BoundaryScore_ReturnsExpectedStatus(int score, GeJu expected) {
            var actual = StrengthClassifier.Classify(score);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(19, GeJu.CongRuo)]
        [InlineData(20, null)]
        [InlineData(80, null)]
        [InlineData(81, GeJu.CongQiang)]
        public void GetCandidate_BoundaryScore_OnlyMarksExtremeScores(int score, GeJu? expected) {
            var actual = StrengthClassifier.GetCandidate(score);

            Assert.Equal(expected, actual);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(101)]
        public void Classify_InvalidScore_Throws(int score) {
            Assert.Throws<ArgumentOutOfRangeException>(() => StrengthClassifier.Classify(score));
        }
    }
}

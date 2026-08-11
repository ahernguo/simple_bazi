using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class BaZiServiceTests {
        private readonly BaZiService _service = new();

        [Fact]
        public void GetBaZiInfo_UncertainBirthTime_PreservesChartAndStrengthCalculation() {
            DateTime birthDate = new(1990, 1, 1, 0, 0, 0);

            var accurateInfo = _service.GetBaZiInfo(birthDate, 2);
            var uncertainInfo = _service.GetBaZiInfo(birthDate, 2, false);

            Assert.True(accurateInfo.IsBirthTimeAccurate);
            Assert.False(uncertainInfo.IsBirthTimeAccurate);
            Assert.Equal(accurateInfo.HourZhu.Gan, uncertainInfo.HourZhu.Gan);
            Assert.Equal(accurateInfo.HourZhu.Zhi, uncertainInfo.HourZhu.Zhi);
            Assert.Equal(accurateInfo.StrengthScore, uncertainInfo.StrengthScore);
            Assert.Equal(accurateInfo.StrengthStatus, uncertainInfo.StrengthStatus);
            Assert.Equal(accurateInfo.LikeWuXing, uncertainInfo.LikeWuXing);
            Assert.Equal(accurateInfo.UnlikeWuXing, uncertainInfo.UnlikeWuXing);
        }

        [Fact]
        public void GetBaZiInfo_StrengthCalculation_PositionsSumToTotalScore() {
            var info = _service.GetBaZiInfo(new DateTime(1990, 1, 1, 12, 0, 0), 2);

            Assert.Equal(7, info.StrengthCalculation.Positions.Count);
            Assert.Equal(
                info.StrengthScore,
                info.StrengthCalculation.Positions.Sum(position => position.Score)
            );
            Assert.All(info.StrengthCalculation.Positions, position => Assert.NotEmpty(position.Reasons));
        }

        [Fact]
        public void GetBaZiInfo_JiaShenJiSiXinMaoRenChen_BindingBlocksMonthBranchGreedyGeneration() {
            var startDate = new DateTime(2004, 5, 5, 8, 0, 0);
            var info = Enumerable.Range(0, 32)
                .Select(offset => _service.GetBaZiInfo(startDate.AddDays(offset), 2))
                .First(chart => chart.DayZhu.Gan == TianGan.Xin && chart.DayZhu.Zhi == DiZhi.Mao);

            Assert.Equal(TianGan.Jia, info.YearZhu.Gan);
            Assert.Equal(DiZhi.Shen, info.YearZhu.Zhi);
            Assert.Equal(TianGan.Ji, info.MonthZhu.Gan);
            Assert.Equal(DiZhi.Si, info.MonthZhu.Zhi);
            Assert.Equal(TianGan.Ren, info.HourZhu.Gan);
            Assert.Equal(DiZhi.Chen, info.HourZhu.Zhi);
            var monthBranch = Assert.Single(
                info.StrengthCalculation.Positions.Where(position => position.Key == "MonthZhi")
            );
            Assert.False(monthBranch.IsSupportive);
            Assert.Contains(monthBranch.Reasons, reason => reason.Contains("合絆", StringComparison.Ordinal));
        }
    }
}

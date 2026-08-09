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
    }
}

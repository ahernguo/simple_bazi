using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class BaZiDescriptionServiceTests {
        private readonly BaZiDescriptionService _descriptionService = new();

        [Theory]
        [InlineData(TianGan.Jia, DiZhi.Zi)]
        [InlineData(TianGan.Jia, DiZhi.Yin)]
        [InlineData(TianGan.Jia, DiZhi.Chen)]
        [InlineData(TianGan.Jia, DiZhi.Wu)]
        [InlineData(TianGan.Jia, DiZhi.Shen)]
        [InlineData(TianGan.Jia, DiZhi.Xu)]
        [InlineData(TianGan.Yi, DiZhi.Mao)]
        [InlineData(TianGan.Yi, DiZhi.Chou)]
        [InlineData(TianGan.Yi, DiZhi.Si)]
        [InlineData(TianGan.Yi, DiZhi.Wei)]
        [InlineData(TianGan.Yi, DiZhi.You)]
        [InlineData(TianGan.Yi, DiZhi.Hai)]
        [InlineData(TianGan.Bing, DiZhi.Yin)]
        [InlineData(TianGan.Bing, DiZhi.Zi)]
        [InlineData(TianGan.Bing, DiZhi.Xu)]
        [InlineData(TianGan.Bing, DiZhi.Shen)]
        [InlineData(TianGan.Bing, DiZhi.Wu)]
        [InlineData(TianGan.Bing, DiZhi.Chen)]
        [InlineData(TianGan.Ding, DiZhi.Mao)]
        [InlineData(TianGan.Ding, DiZhi.Chou)]
        [InlineData(TianGan.Ding, DiZhi.Hai)]
        [InlineData(TianGan.Ding, DiZhi.You)]
        [InlineData(TianGan.Ding, DiZhi.Wei)]
        [InlineData(TianGan.Ding, DiZhi.Si)]
        [InlineData(TianGan.Wu, DiZhi.Zi)]
        [InlineData(TianGan.Wu, DiZhi.Yin)]
        [InlineData(TianGan.Wu, DiZhi.Chen)]
        [InlineData(TianGan.Wu, DiZhi.Wu)]
        [InlineData(TianGan.Wu, DiZhi.Shen)]
        [InlineData(TianGan.Wu, DiZhi.Xu)]
        public void GetDayPillarDescription_VideoCoveredDayPillar_ReturnsDescription(
            TianGan dayMaster,
            DiZhi dayBranch
        ) {
            string? description = _descriptionService.GetDayPillarDescription(dayMaster, dayBranch);

            Assert.False(string.IsNullOrWhiteSpace(description));
        }

        [Theory]
        [InlineData(TianGan.Ji, DiZhi.Chou)]
        [InlineData(TianGan.Geng, DiZhi.Zi)]
        [InlineData(TianGan.Xin, DiZhi.Chou)]
        [InlineData(TianGan.Ren, DiZhi.Yin)]
        [InlineData(TianGan.Gui, DiZhi.Mao)]
        public void GetDayPillarDescription_UncoveredDayMaster_ReturnsNull(
            TianGan dayMaster,
            DiZhi dayBranch
        ) {
            string? description = _descriptionService.GetDayPillarDescription(dayMaster, dayBranch);

            Assert.Null(description);
        }

        [Fact]
        public void GetDayPillarDescription_UnlistedCombination_ReturnsNull() {
            string? description = _descriptionService.GetDayPillarDescription(TianGan.Jia, DiZhi.Chou);

            Assert.Null(description);
        }
    }
}

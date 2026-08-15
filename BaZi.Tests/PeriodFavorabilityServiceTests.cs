using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests {

    public sealed class PeriodFavorabilityServiceTests {
        private readonly PeriodFavorabilityService _service = new();

        [Fact]
        public void IsPeriodFavorable_WeakChart_ChangesAfterSupportiveDaYun() {
            var info = new BaZiInfo(new DateTime(1970, 1, 15, 12, 0, 0), 2);
            var contexts = info.DaYunList
                .SelectMany(daYun => new[] { daYun.StartYear, daYun.StartYear + 5 }
                    .Select(year => _service.EvaluateDaYun(info, daYun, year)))
                .ToArray();
            DaYunFavorabilityContext supportive = contexts.First(context => context.PrimaryIsFavorable);
            DaYunFavorabilityContext unsupported = contexts.First(context => !context.PrimaryIsFavorable);

            Assert.Equal(GeJu.ShenRuo, info.StrengthStatus);
            Assert.True(_service.IsPeriodFavorable(info, supportive, ShiShen.Cai));
            Assert.False(_service.IsPeriodFavorable(info, unsupported, ShiShen.Cai));
            Assert.False(_service.IsPeriodFavorable(info, supportive, ShiShen.BiJie));
            Assert.True(_service.IsPeriodFavorable(info, unsupported, ShiShen.BiJie));
        }
    }
}

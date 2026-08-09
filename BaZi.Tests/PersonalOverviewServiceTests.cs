using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests;

public sealed class PersonalOverviewServiceTests {
    private readonly BaZiService _baZiService = new();
    private readonly PersonalOverviewService _service = new(new TenGodAnalysisService());

    [Fact]
    public void Analyze_ReturnsFourPersonalTopicCardsInRequestedOrder() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 1, 12, 0, 0), 2);

        var result = _service.Analyze(info);

        Assert.Equal(
            ["財富與事業", "感情姻緣", "健康", "家人緣分"],
            result.Cards.Select(card => card.Title)
        );
        Assert.All(result.Cards, card => {
            Assert.NotEmpty(card.Summary);
            Assert.NotEmpty(card.Sections);
            Assert.NotEmpty(card.Notice);
        });
    }

    [Fact]
    public void Analyze_HealthSummaryCountsEightSurfaceCharacters() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1988, 8, 8, 8, 0, 0), 1);
        var expectedCounts = new[] {
            info.YearZhu.GanWuXing, info.YearZhu.ZhiWuXing,
            info.MonthZhu.GanWuXing, info.MonthZhu.ZhiWuXing,
            info.DayZhu.GanWuXing, info.DayZhu.ZhiWuXing,
            info.HourZhu.GanWuXing, info.HourZhu.ZhiWuXing
        }.GroupBy(element => element).ToDictionary(group => group.Key, group => group.Count());

        var result = _service.Analyze(info);

        Assert.Equal(8, expectedCounts.Values.Sum());
        foreach (var element in BaZiDefine.WuXingList) {
            int count = expectedCounts.GetValueOrDefault(element);
            Assert.Contains($"{GetElementName(element)} {count}", result.Health.Summary);
        }
    }

    [Fact]
    public void Analyze_RelationshipUsesSpouseStarAndPreservesSafetyBoundary() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1992, 6, 15, 9, 30, 0), 1);
        var spouseElement = BaZiDefine.RestrictBy[info.RiZhu];

        var result = _service.Analyze(info);

        Assert.Contains($"夫妻星為{GetElementName(spouseElement)}", result.Relationship.Summary);
        Assert.Contains(result.Relationship.Sections, section => section.Title == "夫妻星與對象傾向");
        Assert.Contains("不保證", result.Relationship.Notice);
        Assert.Contains("人身安全優先", result.Relationship.Notice);
    }

    [Fact]
    public void Analyze_FamilyDoesNotConvertChildStarsIntoBirthCount() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 1, 12, 0, 0), 2);

        var result = _service.Analyze(info);

        Assert.Contains("子女緣分", result.Family.Summary);
        Assert.Contains("子息星數量不等於胎數", result.Family.Notice);
        Assert.Contains(result.Family.Sections, section => section.Title == "子息星分布");
        Assert.Contains(result.Family.Sections, section => section.Title == "子息宮與親子距離");
    }

    [Fact]
    public void Analyze_ContainsOnlyNatalSections() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 1, 12, 0, 0), 2);

        var result = _service.Analyze(info);

        Assert.Equal(
            ["夫妻星與對象傾向", "夫妻宮與原局互動", "你的關係需求"],
            result.Relationship.Sections.Select(section => section.Title)
        );
        Assert.Equal(
            ["本命弱項", "三刑安全提醒"],
            result.Health.Sections.Select(section => section.Title)
        );
        Assert.Equal(
            ["子息星分布", "子息宮與親子距離"],
            result.Family.Sections.Select(section => section.Title)
        );
    }

    [Fact]
    public void Analyze_HealthWithoutNatalThreeXingUsesConciseSafetyReminder() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 1, 12, 0, 0), 2);

        var result = _service.Analyze(info);
        var section = Assert.Single(
            result.Health.Sections,
            item => item.Title == "三刑安全提醒"
        );

        Assert.Equal(
            "本命沒有湊成寅巳申或丑戌未三刑，無重大危害，但不代表會永遠健康，仍請維持健康檢查與管理。",
            Assert.Single(section.Details)
        );
    }

    private static string GetElementName(WuXing element) {
        return element switch {
            WuXing.Mu => "木",
            WuXing.Huo => "火",
            WuXing.Tu => "土",
            WuXing.Jin => "金",
            WuXing.Shui => "水",
            _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
        };
    }
}

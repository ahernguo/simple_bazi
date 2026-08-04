using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests;

public sealed class CompatibilityServiceTests {
    private readonly BaZiService _baZiService = new();
    private readonly TenGodAnalysisService _tenGodService = new();

    [Fact]
    public void GetStatistics_ReturnsCompleteDistributionWithoutDayMaster() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 1, 12, 0, 0), 2);

        var statistics = _tenGodService.GetStatistics(info);

        Assert.NotEmpty(statistics);
        Assert.DoesNotContain(statistics, item => item.Xing == ShiShen.RiZhu);
        Assert.Equal(1, statistics.Sum(item => item.Percentage), 10);
        Assert.Equal(_tenGodService.GetAllStars(info).Count, statistics.Sum(item => item.Count));
    }

    [Fact]
    public void GetGroupStatistics_ReturnsFiveGroupsWithSameTotal() {
        var info = _baZiService.GetBaZiInfo(new DateTime(1988, 8, 8, 8, 0, 0), 1);

        var groups = _tenGodService.GetGroupStatistics(info);

        Assert.Equal(5, groups.Count);
        Assert.Equal(_tenGodService.GetAllStars(info).Count, groups.Sum(group => group.Count));
    }

    [Theory]
    [InlineData(CompatibilityRelationship.Romance, "生肖初篩")]
    [InlineData(CompatibilityRelationship.Parent, "對方的主要十神")]
    [InlineData(CompatibilityRelationship.Child, "對方的主要十神")]
    [InlineData(CompatibilityRelationship.Sibling, "手足訊號")]
    [InlineData(CompatibilityRelationship.Friend, "朋友五行互補")]
    [InlineData(CompatibilityRelationship.Colleague, "對方的主要十神")]
    public void Analyze_EachRelationship_ReturnsExpectedSection(
        CompatibilityRelationship relationship,
        string expectedTitle
    ) {
        var service = CreateService();
        var self = _baZiService.GetBaZiInfo(new DateTime(1990, 1, 1, 12, 0, 0), 2);
        var other = _baZiService.GetBaZiInfo(new DateTime(1992, 6, 15, 9, 30, 0), 1);

        var result = service.Analyze(self, other, relationship);

        Assert.Equal(relationship, result.Relationship);
        Assert.Contains(result.Sections, section => section.Title == expectedTitle);
        Assert.NotEmpty(result.Limitations);
    }

    [Theory]
    [InlineData("romance", CompatibilityRelationship.Romance)]
    [InlineData("parent", CompatibilityRelationship.Parent)]
    [InlineData("child", CompatibilityRelationship.Child)]
    [InlineData("sibling", CompatibilityRelationship.Sibling)]
    [InlineData("friend", CompatibilityRelationship.Friend)]
    [InlineData("colleague", CompatibilityRelationship.Colleague)]
    public void TryParse_KnownRoute_ReturnsRelationship(
        string route,
        CompatibilityRelationship expected
    ) {
        var parsed = CompatibilityRelationshipCatalog.TryParse(route, out var relationship);

        Assert.True(parsed);
        Assert.Equal(expected, relationship);
    }

    [Fact]
    public void ToDateTime_CombinesSelectedDateAndTime() {
        var input = new CompatibilityBirthInput {
            BirthDate = new DateTime(2000, 2, 3),
            BirthTime = new DateTime(2026, 8, 4, 14, 25, 0)
        };

        var result = input.ToDateTime();

        Assert.Equal(new DateTime(2000, 2, 3, 14, 25, 0), result);
    }

    [Fact]
    public void AnalyzeRomance_SameZodiac_DoesNotReportSixClash() {
        var service = CreateService();
        var self = _baZiService.GetBaZiInfo(new DateTime(1989, 6, 1, 12, 0, 0), 2);

        var result = service.Analyze(self, self, CompatibilityRelationship.Romance);
        var zodiacSection = Assert.Single(result.Sections, section => section.Title == "生肖初篩");

        Assert.DoesNotContain("形成六沖", zodiacSection.Summary);
        Assert.Contains("沒有六沖的狀況", zodiacSection.Summary);
    }

    [Fact]
    public void AnalyzeRomance_NoTrineOrClash_UsesNeutralSummary() {
        var service = CreateService();
        var self = _baZiService.GetBaZiInfo(new DateTime(1989, 6, 1, 12, 0, 0), 2);
        var other = _baZiService.GetBaZiInfo(new DateTime(1990, 6, 1, 12, 0, 0), 1);

        var result = service.Analyze(self, other, CompatibilityRelationship.Romance);
        var zodiacSection = Assert.Single(result.Sections, section => section.Title == "生肖初篩");

        Assert.Contains("沒有三合、六沖的狀況", zodiacSection.Summary);
        Assert.Contains("無特別不合的狀況", zodiacSection.Summary);
    }

    private CompatibilityService CreateService() {
        return new CompatibilityService(_baZiService, _tenGodService);
    }
}

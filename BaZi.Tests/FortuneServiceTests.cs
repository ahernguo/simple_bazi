using BaZi.Models;
using BaZi.Services;
using Xunit;

namespace BaZi.Tests;

public class FortuneServiceTests {
    private static readonly DateTime BirthDate = new(1990, 1, 1, 12, 0, 0);

    [Fact]
    public void GetLiuYueMonths_ExistingYear_ReturnsTwelveOrderedMonths() {
        var info = new BaZiInfo(BirthDate, 2);
        var service = new FortuneService();

        var months = service.GetLiuYueMonths(info, 2026);

        Assert.Equal(12, months.Count);
        Assert.Equal(Enumerable.Range(0, 12), months.Select(month => month.Index));
        Assert.Equal("正", months[0].MonthInChinese);
        Assert.Equal(TianGan.Geng, months[0].Gan);
        Assert.Equal(DiZhi.Yin, months[0].Zhi);
        Assert.Equal("臘", months[11].MonthInChinese);
        Assert.Equal(TianGan.Xin, months[11].Gan);
        Assert.Equal(DiZhi.Chou, months[11].Zhi);
    }

    [Fact]
    public void GetLiuYueMonths_MissingYear_ReturnsEmptyList() {
        var info = new BaZiInfo(BirthDate, 2);
        var service = new FortuneService();

        var months = service.GetLiuYueMonths(info, 1800);

        Assert.Empty(months);
    }

    [Fact]
    public void SelfXing_UsesHaiInsteadOfXu() {
        Assert.Contains(DiZhi.Hai, BaZiDefine.SelfXing);
        Assert.DoesNotContain(DiZhi.Xu, BaZiDefine.SelfXing);
    }

    [Fact]
    public void LiuYueAnalysis_ExistingMonth_UsesLiuYueWording() {
        var info = new BaZiInfo(BirthDate, 2);
        var service = new FortuneService();

        var html = service.LiuYueAnalysis(info, 2026, 0).Value;

        Assert.Contains("2026 正月流月分析", html);
        Assert.Contains("庚", html);
        Assert.Contains("寅", html);
        Assert.Contains("節氣月", html);
        Assert.DoesNotContain("今年流年運勢", html);
        Assert.DoesNotContain("這一年", html);
    }

    [Fact]
    public void LiuYueAnalysis_LiuNianAndLiuYueChong_ListsActualParticipants() {
        var info = new BaZiInfo(new DateTime(1980, 1, 15, 10, 0, 0), 2);
        var service = new FortuneService();

        var html = service.LiuYueAnalysis(info, 2026, 10).Value;

        Assert.Contains("流年 <span class=\"border-bottom-dash\">午</span> 與流月 <span class=\"border-bottom-dash\">子</span> 形成", html);
        Assert.Contains("相沖", html);
    }

    [Fact]
    public void LiuNianAnalysis_ExistingYear_PreservesLiuNianOutput() {
        var info = new BaZiInfo(BirthDate, 2);
        var service = new FortuneService();

        var html = service.LiuNianAnalysis(info, 2026).Value;

        Assert.Contains("2026 流年分析", html);
        Assert.Contains("丙", html);
        Assert.Contains("午", html);
    }
}

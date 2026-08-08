using BaZi.Models;
using Xunit;

namespace BaZi.Tests;

public sealed class BaZiConvertTests {
    [Theory]
    [InlineData("劫财", ShiShen.JieCai, "劫財")]
    [InlineData("伤官", ShiShen.ShangGuan, "傷官")]
    [InlineData("偏财", ShiShen.PianCai, "偏財")]
    [InlineData("正财", ShiShen.ZhengCai, "正財")]
    [InlineData("七杀", ShiShen.QiSha, "七殺")]
    public void Zhu_SimplifiedTenGod_ParsesAndDisplaysTraditional(
        string simplified,
        ShiShen expected,
        string traditional) {
        var zhu = new Zhu("測試柱", "甲", "子", simplified, [simplified]);

        Assert.Equal(expected, zhu.ZhuXing);
        Assert.Equal([expected], zhu.FuXing);
        Assert.Contains(traditional, zhu.ToString());
    }
}

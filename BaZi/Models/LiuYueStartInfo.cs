namespace BaZi.Models;

/// <summary>描述流月及其起訖節氣時間。</summary>
/// <param name="Month">流月資料。</param>
/// <param name="JieQiName">起始節氣名稱。</param>
/// <param name="StartDate">起始日期時間。</param>
/// <param name="EndJieQiName">結束節氣名稱。</param>
/// <param name="EndDate">結束日期時間。</param>
public sealed record LiuYueStartInfo(
    LiuYue Month,
    string JieQiName,
    DateTime StartDate,
    string EndJieQiName,
    DateTime EndDate
);

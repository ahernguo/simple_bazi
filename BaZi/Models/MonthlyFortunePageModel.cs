namespace BaZi.Models;

/// <summary>流月頁面的查詢狀態</summary>
public sealed class MonthlyFortunePageModel {
    public int? SelectedYear { get; set; } = DateTime.Now.Year;

    public int? SelectedMonthIndex { get; set; }

    public int? QueriedYear { get; set; }

    public int? QueriedMonthIndex { get; set; }
}

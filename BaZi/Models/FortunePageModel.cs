namespace BaZi.Models;

/// <summary>大運與流年頁面的查詢狀態</summary>
public sealed class FortunePageModel {
    public int? SelectedLiuNianYear { get; set; } = DateTime.Now.Year;

    public int? QueriedLiuNianYear { get; set; }
}

namespace BaZi.Models {

    /// <summary>大運主次作用與本命喜忌的判定結果。</summary>
    public sealed record DaYunFavorabilityContext(
        DaYunPhase Phase,
        ShiShen PrimaryTenGod,
        ShiShen SecondaryTenGod,
        bool PrimaryIsFavorable,
        bool SecondaryIsFavorable
    );

    /// <summary>可由共用元件或舊版 HTML 報表呈現的十神資訊。</summary>
    public sealed record TenGodPresentation(
        string Text,
        string CssClass,
        string? Tooltip,
        WuXing? Element,
        bool? IsFavorable
    );

    /// <summary>日干在課程中的代表物與性格描述。</summary>
    public sealed record DayMasterProfile(string Representative, string Description);
}

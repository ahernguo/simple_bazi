namespace BaZi.Models;

/// <summary>個人概述區塊的提示層級。</summary>
public enum PersonalOverviewTone {
    Information,
    Positive,
    Caution
}

/// <summary>個人概述文字片段的語意種類。</summary>
public enum PersonalOverviewTextKind {
    Plain,
    Element,
    TenGod
}

/// <summary>供個人概述以安全 Razor 標記呈現的文字片段。</summary>
public sealed record PersonalOverviewTextSegment(
    string Text,
    PersonalOverviewTextKind Kind = PersonalOverviewTextKind.Plain,
    WuXing? Element = null
);

/// <summary>個人概述 Card 內的一個分析區塊。</summary>
public sealed record PersonalOverviewSection(
    string Title,
    string Summary,
    IReadOnlyList<string> Details,
    PersonalOverviewTone Tone = PersonalOverviewTone.Information
);

/// <summary>個人概述的一張主題 Card。</summary>
public sealed record PersonalOverviewCard(
    string Title,
    string Icon,
    string Summary,
    IReadOnlyList<PersonalOverviewSection> Sections,
    string Notice
);

/// <summary>財富事業、感情、健康與家人緣分的個人分析結果。</summary>
public sealed record PersonalOverviewResult(
    PersonalOverviewCard WealthCareer,
    PersonalOverviewCard Relationship,
    PersonalOverviewCard Health,
    PersonalOverviewCard Family
) {
    public IReadOnlyList<PersonalOverviewCard> Cards => [
        WealthCareer,
        Relationship,
        Health,
        Family
    ];
}

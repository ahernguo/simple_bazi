using System.ComponentModel.DataAnnotations;

namespace BaZi.Models {

    /// <summary>合盤關係類型。</summary>
    public enum CompatibilityRelationship {
        Romance,
        Parent,
        Child,
        Sibling,
        Friend,
        Colleague
    }

    /// <summary>合盤分析區塊的提示層級。</summary>
    public enum CompatibilityTone {
        Information,
        Notice,
        Positive,
        Caution
    }

    /// <summary>合盤文字中的十神所屬命盤。</summary>
    public enum CompatibilityTenGodSubject {
        None,
        Self,
        Other
    }

    /// <summary>合盤對象的出生資料。</summary>
    public sealed class CompatibilityBirthInput : IValidatableObject {
        [Required(ErrorMessage = "請輸入出生日期。")]
        public DateTime BirthDate { get; set; } = new(1990, 1, 1);

        [Required(ErrorMessage = "請輸入出生時間。")]
        public DateTime BirthTime { get; set; } = DateTime.Today.AddHours(12);

        [Range(1, 2, ErrorMessage = "請選擇性別。")]
        public int Gender { get; set; } = 2;

        public bool IsBirthTimeAccurate { get; set; } = true;

        public DateTime ToDateTime() {
            return BirthDate.Date.Add(BirthTime.TimeOfDay);
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) {
            var birthDateTime = ToDateTime();
            if (birthDateTime.Year < 1900) {
                yield return new ValidationResult("出生年份不可早於 1900 年。", [nameof(BirthDate)]);
            }

            if (birthDateTime > DateTime.Now) {
                yield return new ValidationResult("出生時間不可晚於現在。", [nameof(BirthDate), nameof(BirthTime)]);
            }
        }
    }

    /// <summary>單一十神的統計結果。</summary>
    public sealed record TenGodStatistic(ShiShen Xing, int Count, double Percentage);

    /// <summary>正偏十神合併後的統計結果。</summary>
    public sealed record TenGodGroupStatistic(
        ShiShen Group,
        int Count,
        IReadOnlyList<ShiShen> LeadingStars
    );

    /// <summary>合盤分析的一個可呈現區塊。</summary>
    public sealed record CompatibilitySection(
        string Title,
        string Summary,
        IReadOnlyList<string> Details,
        CompatibilityTone Tone = CompatibilityTone.Information,
        CompatibilityTenGodSubject TenGodSubject = CompatibilityTenGodSubject.None,
        IReadOnlyList<string>? Notes = null
    );

    /// <summary>合盤分析結果。</summary>
    public sealed record CompatibilityResult(
        CompatibilityRelationship Relationship,
        BaZiInfo Self,
        BaZiInfo Other,
        IReadOnlyList<CompatibilitySection> Sections,
        IReadOnlyList<string> Limitations,
        BranchRelationshipAnalysis? BranchRelationships,
        IReadOnlyList<CompatibilitySection> InternetSourceSections
    );

    /// <summary>合盤關係的路由與顯示資訊。</summary>
    public static class CompatibilityRelationshipCatalog {
        public static bool TryParse(string? routeValue, out CompatibilityRelationship relationship) {
            relationship = routeValue?.ToLowerInvariant() switch {
                "romance" => CompatibilityRelationship.Romance,
                "parent" => CompatibilityRelationship.Parent,
                "child" => CompatibilityRelationship.Child,
                "sibling" => CompatibilityRelationship.Sibling,
                "friend" => CompatibilityRelationship.Friend,
                "colleague" => CompatibilityRelationship.Colleague,
                _ => default
            };

            return routeValue?.ToLowerInvariant() is
                "romance" or "parent" or "child" or "sibling" or "friend" or "colleague";
        }

        public static string GetDisplayName(CompatibilityRelationship relationship) {
            return relationship switch {
                CompatibilityRelationship.Romance => "感情",
                CompatibilityRelationship.Parent => "父母",
                CompatibilityRelationship.Child => "兒女",
                CompatibilityRelationship.Sibling => "手足",
                CompatibilityRelationship.Friend => "朋友",
                CompatibilityRelationship.Colleague => "同事",
                _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null)
            };
        }

        public static string GetDescription(CompatibilityRelationship relationship) {
            return relationship switch {
                CompatibilityRelationship.Romance => "從生肖、夫妻星、本命五行與日柱結構，整理兩人的互補與溝通提醒。",
                CompatibilityRelationship.Parent => "依父母本人的十神分布，整理較適合的相處與溝通入口。",
                CompatibilityRelationship.Child => "依孩子本人的十神分布，整理陪伴、引導與界線建議。",
                CompatibilityRelationship.Sibling => "先看自己命盤的比劫訊號，再參考手足本人的十神傾向。",
                CompatibilityRelationship.Friend => "依本命格局與五行方向檢查互補，並補上合作風險提醒。",
                CompatibilityRelationship.Colleague => "依同事本人的十神分布，整理平級協作與交付方式。",
                _ => throw new ArgumentOutOfRangeException(nameof(relationship), relationship, null)
            };
        }
    }
}

using BaZi.Components.Common;
using BaZi.Models;
using Microsoft.AspNetCore.Components;

namespace BaZi.Components.Compatibility {

    public partial class CompatibilityResultView {
        [Parameter, EditorRequired]
        public CompatibilityResult Result { get; set; } = default!;

        private static string FormatBirth(BaZiInfo info) {
            return info.IsBirthTimeAccurate
                ? $"{info.SolarDate:yyyy/MM/dd HH:mm}，{info.Gender.ToSexString()}"
                : $"{info.SolarDate:yyyy/MM/dd}，時辰未知，{info.Gender.ToSexString()}";
        }

        private (BaZiInfo? Info, string Label) GetSubjectInfo(CompatibilityTenGodSubject subject) {
            return subject switch {
                CompatibilityTenGodSubject.Self => (Result.Self, "自己"),
                CompatibilityTenGodSubject.Other => (Result.Other, "對方"),
                _ => (null, string.Empty)
            };
        }

        private static TenGodDisplayMode GetTenGodMode(CompatibilitySection section) {
            return section.TenGodShowsFavorability
                ? TenGodDisplayMode.Favorability
                : TenGodDisplayMode.Reference;
        }

        private static string GetToneIcon(CompatibilityTone tone) {
            return tone switch {
                CompatibilityTone.Positive => "fa-circle-check text-success",
                CompatibilityTone.Caution => "fa-triangle-exclamation text-warning",
                CompatibilityTone.Notice => "fa-circle-info text-info",
                _ => "fa-compass"
            };
        }
    }
}

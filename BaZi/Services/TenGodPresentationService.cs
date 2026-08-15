using System.Text.Encodings.Web;
using System.Text.Unicode;
using BaZi.Models;

namespace BaZi.Services {

    /// <summary>集中建立十神文字、喜忌樣式與 Tooltip。</summary>
    public sealed class TenGodPresentationService {
        private static readonly HtmlEncoder SafeHtmlEncoder = HtmlEncoder.Create(UnicodeRanges.All);
        private const string FavorableCssClass = "topic-ten-god topic-ten-god-favorable";
        private const string UnfavorableCssClass = "topic-ten-god topic-ten-god-unfavorable";
        private const string ReferenceCssClass = "topic-ten-god-reference";

        /// <summary>建立不含吉凶判定的十神參考文字。</summary>
        public TenGodPresentation CreateReference(ShiShen tenGod, string? displayText = null) {
            return new TenGodPresentation(
                displayText ?? tenGod.ToShenString(),
                ReferenceCssClass,
                null,
                null,
                null
            );
        }

        /// <summary>依本命或外部提供的當期結果建立喜忌文字。</summary>
        public TenGodPresentation CreateFavorability(
            BaZiInfo info,
            ShiShen tenGod,
            bool isFavorable,
            string reason,
            string? displayText = null,
            bool showFavorabilityLabel = false
        ) {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);

            WuXing element = TenGodElementResolver.Resolve(info.RiZhu, tenGod);
            string tenGodText = tenGod.ToShenString();
            string direction = isFavorable ? "喜用神" : "忌神";
            string effect = isFavorable
                ? "喜用神表示目前方向有助平衡或順勢；十神本身的事件意象仍須另外判讀"
                : "忌神表示目前方向可能加重失衡或破格；不代表事件必然發生";
            string text = displayText
                ?? (showFavorabilityLabel ? $"{tenGodText}（{direction}）" : tenGodText);

            return new TenGodPresentation(
                text,
                isFavorable ? FavorableCssClass : UnfavorableCssClass,
                $"{tenGodText}屬{element.ToWuXingString()}，{reason}列為{direction}\r\n{effect}",
                element,
                isFavorable
            );
        }

        /// <summary>建立喜用或忌神方向標籤。</summary>
        public TenGodPresentation CreateDirection(bool isFavorable, string reason) {
            ArgumentException.ThrowIfNullOrWhiteSpace(reason);
            return new TenGodPresentation(
                isFavorable ? "喜用方向" : "忌神方向",
                isFavorable ? FavorableCssClass : UnfavorableCssClass,
                reason,
                null,
                isFavorable
            );
        }

        /// <summary>將呈現模型轉為舊版報表可用的已編碼 HTML。</summary>
        public string ToHtml(TenGodPresentation presentation) {
            ArgumentNullException.ThrowIfNull(presentation);

            string cssClass = SafeHtmlEncoder.Encode(presentation.CssClass);
            string text = SafeHtmlEncoder.Encode(presentation.Text);
            if (string.IsNullOrWhiteSpace(presentation.Tooltip)) {
                return $"<span class=\"{cssClass}\">{text}</span>";
            }

            string tooltip = SafeHtmlEncoder.Encode(presentation.Tooltip);
            return $"<span class=\"{cssClass}\" title=\"{tooltip}\">{text}</span>";
        }
    }
}

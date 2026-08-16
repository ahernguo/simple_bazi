using BaZi.Models;

namespace BaZi.Services {

    /// <summary>提供命盤十神的共用統計。</summary>
    public sealed class TenGodAnalysisService {
        private static readonly IReadOnlyDictionary<ShiShen, ShiShen[]> GroupMembers =
            new Dictionary<ShiShen, ShiShen[]> {
                [ShiShen.Cai] = [ShiShen.ZhengCai, ShiShen.PianCai],
                [ShiShen.GuanSha] = [ShiShen.ZhengGuan, ShiShen.QiSha],
                [ShiShen.ShihShang] = [ShiShen.ShihShen, ShiShen.ShangGuan],
                [ShiShen.Yin] = [ShiShen.ZhengYin, ShiShen.PianYin],
                [ShiShen.BiJie] = [ShiShen.BiJian, ShiShen.JieCai]
            };

        public IReadOnlyList<ShiShen> GetAllStars(BaZiInfo info) {
            ArgumentNullException.ThrowIfNull(info);

            var stars = new List<ShiShen> {
                info.YearZhu.ZhuXing,
                info.MonthZhu.ZhuXing
            };
            stars.AddRange(info.YearZhu.FuXing);
            stars.AddRange(info.MonthZhu.FuXing);
            stars.AddRange(info.DayZhu.FuXing);
            if (info.IsBirthTimeAccurate) {
                stars.Add(info.HourZhu.ZhuXing);
                stars.AddRange(info.HourZhu.FuXing);
            }

            return stars;
        }

        public IReadOnlyList<ShiShen> GetMainStars(BaZiInfo info) {
            ArgumentNullException.ThrowIfNull(info);
            return info.IsBirthTimeAccurate
                ? [info.YearZhu.ZhuXing, info.MonthZhu.ZhuXing, info.HourZhu.ZhuXing]
                : [info.YearZhu.ZhuXing, info.MonthZhu.ZhuXing];
        }

        public IReadOnlyList<TenGodStatistic> GetStatistics(BaZiInfo info) {
            var allStars = GetAllStars(info);
            var total = allStars.Count;
            if (total == 0) {
                return [];
            }

            return [.. allStars
            .GroupBy(star => star)
            .Select(group => new TenGodStatistic(group.Key, group.Count(), (double)group.Count() / total))];
        }

        public IReadOnlyList<TenGodGroupStatistic> GetGroupStatistics(BaZiInfo info) {
            var allStars = GetAllStars(info);

            return [.. GroupMembers.Select(pair => {
            var counts = pair.Value.ToDictionary(star => star, star => allStars.Count(item => item == star));
            var maxCount = counts.Values.Max();
            var leadingStars = maxCount == 0
                ? Array.Empty<ShiShen>()
                : counts.Where(item => item.Value == maxCount).Select(item => item.Key).ToArray();
            return new TenGodGroupStatistic(pair.Key, counts.Values.Sum(), leadingStars);
        })];
        }

        public IReadOnlyList<TenGodGroupStatistic> GetDominantGroups(BaZiInfo info) {
            var groups = GetGroupStatistics(info);
            var maxCount = groups.Max(group => group.Count);
            return [.. groups.Where(group => group.Count == maxCount)];
        }

        public static string GetViewsOnLove(ShiShen shishen) {
            return shishen switch {
                ShiShen.BiJian => "比肩代表平等、競爭與自我，喜歡如同戰友的另一半，各自獨立不依賴卻能互相扶持。若能打進比肩的朋友圈且獨立自主，會視為平起平坐的摯愛",
                ShiShen.JieCai => "對朋友閨密都很好、不分你我，容易被認為花心。劫財對另一半的愛即是「最核心的自己人」，只要願意陪劫財衝鋒陷陣，低潮時給他溫暖擁抱，劫財會為他赴湯蹈火，給他所有一切",
                ShiShen.ShihShen => "食神代表享受、才華與樂觀，懂得生活的浪漫大師。食神的愛是給另一半小確幸，記得他的嗜好、習慣、興趣，例如飲料甜度、不吃的食物、吃隱藏美食等，只希望每天開心過日",
                ShiShen.ShangGuan => "傷官特質是追求完美、敢說敢衝。傷官追求的是靈魂伴侶、知識上的共鳴，對另一半的愛是炙熱、毫無保留，會為了愛去衝撞全世界。只要能追上傷官的思維，傷官會製造最刻骨銘心、充滿火花的愛情",
                ShiShen.PianCai => "偏財特質是樂於分享、講求效率。偏財在戀愛中是慷慨的，會想要把全世界最好的都捧到另一半面前，讓另一半享受最好的一切",
                ShiShen.ZhengCai => "正財本質是務實跟保守、需要安全感。正財的浪漫不是買花、吃大餐，是默默存下頭期款，提供一個不愁吃穿的避風港",
                ShiShen.QiSha => "七殺特質是挑戰跟驅動。七殺談戀愛是霸道型，除了自己外其他人都不可欺負他，會為另一半解決所有麻煩，天塌下來也會替另一半會頂著",
                ShiShen.ZhengGuan => "正官代表責任、紀律與秩序。正官的愛是守承諾跟負責任，雖不能帶給另一半偶像劇般的激情，但正官的愛就是絕對的忠誠、扛起所有家庭責任",
                ShiShen.PianYin => "偏印是天生的觀察者，對哲學、玄學有興趣，需要極大的獨處空間。偏印的愛是懂另一半的沉默，靈魂的契合不用言語來證明，當偏印在 Me time 充好電後，就會給另一半最深刻的溫柔",
                ShiShen.ZhengYin => "正印代表思想、包容跟母性光輝。正印的愛是靜水深流的付出、無處不在的愛，默默把家事做完、準備好飯菜、無微不至的照顧另一半",
                _ => ""
            };
        }
    }
}

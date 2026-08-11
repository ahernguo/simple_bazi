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
    }
}

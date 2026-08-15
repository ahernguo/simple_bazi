using BaZi.Models;

namespace BaZi.Services {

    /// <summary>依本命格局與大運背景統一判定十神的喜忌方向。</summary>
    public sealed class PeriodFavorabilityService {

        /// <summary>判定十神是否為本命格局的喜用方向。</summary>
        public bool IsNatalFavorable(BaZiInfo info, ShiShen tenGod) {
            ArgumentNullException.ThrowIfNull(info);

            WuXing element = TenGodElementResolver.Resolve(info.RiZhu, tenGod);
            return info.LikeWuXing.Contains(element);
        }

        /// <summary>判定五行是否為本命格局的喜用方向。</summary>
        public bool IsNatalFavorable(BaZiInfo info, WuXing element) {
            ArgumentNullException.ThrowIfNull(info);
            return info.LikeWuXing.Contains(element);
        }

        /// <summary>取得指定年份的大運主次作用與原局喜忌。</summary>
        public DaYunFavorabilityContext EvaluateDaYun(BaZiInfo info, DaYun daYun, int year) {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(daYun);

            DaYunPhase phase = daYun.GetPhase(year);
            ShiShen stemTenGod = daYun.Gan.ToShiShen(info.DayZhu.Gan).ToCombined();
            ShiShen branchTenGod = daYun.Zhi.ToShiShen(info.DayZhu.Gan).ToCombined();
            ShiShen primaryTenGod = phase == DaYunPhase.FirstFiveYears ? stemTenGod : branchTenGod;
            ShiShen secondaryTenGod = phase == DaYunPhase.FirstFiveYears ? branchTenGod : stemTenGod;

            return new DaYunFavorabilityContext(
                phase,
                primaryTenGod,
                secondaryTenGod,
                IsNatalFavorable(info, primaryTenGod),
                IsNatalFavorable(info, secondaryTenGod)
            );
        }

        /// <summary>依大運調整後的有效強弱，判定流年、流月或流日十神方向。</summary>
        public bool IsPeriodFavorable(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext,
            ShiShen tenGod
        ) {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(daYunContext);

            ShiShen group = tenGod.ToCombined();
            bool isExhaustingDirection = group is ShiShen.Cai or ShiShen.GuanSha or ShiShen.ShihShang;
            return UsesExhaustingPeriodDirection(info, daYunContext)
                ? isExhaustingDirection
                : !isExhaustingDirection;
        }

        /// <summary>取得當期喜忌 Tooltip 所使用的判定依據。</summary>
        public string GetPeriodReason(BaZiInfo info, DaYunFavorabilityContext daYunContext) {
            ArgumentNullException.ThrowIfNull(info);
            ArgumentNullException.ThrowIfNull(daYunContext);

            return info.StrengthStatus switch {
                GeJu.ShenRuo when daYunContext.PrimaryIsFavorable => "依身弱且大運主作用已幫扶的當期規則",
                GeJu.ShenRuo => "依身弱且大運主作用尚未幫扶的當期規則",
                GeJu.CongQiang or GeJu.CongRuo => "依從格順勢／破格規則",
                _ => "依本命格局與目前大運主作用"
            };
        }

        private static bool UsesExhaustingPeriodDirection(
            BaZiInfo info,
            DaYunFavorabilityContext daYunContext
        ) {
            return info.StrengthStatus switch {
                GeJu.ShenQiang or GeJu.CongRuo => true,
                GeJu.CongQiang => false,
                GeJu.ShenRuo => daYunContext.PrimaryIsFavorable,
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(
                    nameof(info.StrengthStatus),
                    (int)info.StrengthStatus,
                    typeof(GeJu)
                )
            };
        }
    }
}

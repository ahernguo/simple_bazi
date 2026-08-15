namespace BaZi.Models {

    /// <summary>流日分析主題。</summary>
    public enum DailyFortuneTopic {
        Wealth = 1,
        Career = 2,
        Romance = 3,
        Health = 4,
        Interpersonal = 5,
        TravelSafety = 6,
        Moving = 7
    }

    /// <summary>流日訊號層級。</summary>
    public enum DailySignalLevel {
        Opportunity = 1,
        Attention = 2,
        HighAttention = 3,
        VerificationRequired = 4
    }

    /// <summary>指定日期的流日干支。</summary>
    public sealed record LiuRi(DateTime Date, TianGan Gan, DiZhi Zhi) : IGanZhi {
        public string Id => "流日";

        /// <summary>從國曆日期建立流日。</summary>
        public static LiuRi FromDate(DateTime date) {
            var eightChar = Lunar.Solar.FromDate(date.Date.AddHours(12)).Lunar.EightChar;
            return new LiuRi(date.Date, eightChar.DayGan.ToTianGan(), eightChar.DayZhi.ToDiZhi());
        }
    }

    /// <summary>分析流日時使用的年、月與大運背景。</summary>
    public sealed record DailyPeriodContext(
        TianGan YearGan,
        DiZhi YearZhi,
        TianGan MonthGan,
        DiZhi MonthZhi,
        DaYun? DaYun
    );

    /// <summary>實際觸發流日規則的十神來源。</summary>
    public sealed record DailyFortuneTenGodFactor(
        string Source,
        ShiShen TenGod,
        bool IsFavorable,
        string FavorabilityReason
    );

    /// <summary>單一流日訊號。</summary>
    public sealed record DailyFortuneSignal(
        DailyFortuneTopic Topic,
        DailySignalLevel Level,
        string Title,
        string Summary,
        string Advice,
        IReadOnlyList<DailyFortuneTenGodFactor>? TenGodFactors = null
    );

    /// <summary>單日完整分析結果。</summary>
    public sealed record DailyFortuneResult(
        LiuRi Day,
        DailyPeriodContext Context,
        IReadOnlyList<DailyFortuneSignal> Signals
    ) {
        public bool HasSignals => Signals.Count > 0;
    }

    /// <summary>交通期間背景中的單一地支來源。</summary>
    public sealed record DailyTravelSafetyBranchSource(
        string Label,
        DiZhi Branch,
        bool IsTravelBranch
    );

    /// <summary>交通期間背景中的一組相沖。</summary>
    public sealed record DailyTravelSafetyClash(
        DailyTravelSafetyBranchSource First,
        DailyTravelSafetyBranchSource Second
    );

    /// <summary>交通期間背景中已湊齊的三刑。</summary>
    public sealed record DailyTravelSafetyPunishment(IReadOnlyList<DiZhi> Members);

    /// <summary>國曆月份內一段連續且相同的交通背景。</summary>
    public sealed record DailyTravelSafetyPeriodBackground(
        DateTime StartDate,
        DateTime EndDate,
        DailySignalLevel? Level,
        IReadOnlyList<DailyTravelSafetyBranchSource> Sources,
        IReadOnlyList<DailyTravelSafetyClash> Clashes,
        IReadOnlyList<DailyTravelSafetyPunishment> Punishments,
        bool HasRepeatedClash,
        bool IncludesHourPillar
    ) {
        /// <summary>取得期間背景中的驛馬地支總數。</summary>
        public int TravelBranchCount => Sources.Count(source => source.IsTravelBranch);
    }

    /// <summary>包含每日結果及交通期間背景的月份分析。</summary>
    public sealed record DailyFortuneMonthAnalysis(
        IReadOnlyList<DailyFortuneResult> Results,
        IReadOnlyList<DailyTravelSafetyPeriodBackground> TravelSafetyBackgrounds
    );

    /// <summary>流日頁面的查詢狀態。</summary>
    public sealed class DailyFortunePageModel {
        public int? SelectedYear { get; set; } = DateTime.Now.Year;

        public int SelectedMonth { get; set; } = DateTime.Now.Month;

        public DailyFortuneTopic? SelectedTopic { get; set; } = DailyFortuneTopic.Wealth;

        public int? QueriedYear { get; set; }

        public int? QueriedMonth { get; set; }

        public DailyFortuneTopic? QueriedTopic { get; set; }

        public HashSet<DiZhi> HouseholdYearBranches { get; } = [];
    }
}

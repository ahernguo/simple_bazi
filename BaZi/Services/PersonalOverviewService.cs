using BaZi.Models;

namespace BaZi.Services {

    /// <summary>依指定課程筆記產生本命四大主題的個人概述。</summary>
    public sealed class PersonalOverviewService {
        private static readonly WuXing[] ElementOrder = [
            WuXing.Mu,
        WuXing.Huo,
        WuXing.Tu,
        WuXing.Jin,
        WuXing.Shui
        ];

        private static readonly ShiShen[] GroupOrder = [
            ShiShen.Cai,
        ShiShen.GuanSha,
        ShiShen.ShihShang,
        ShiShen.Yin,
        ShiShen.BiJie
        ];

        private readonly TenGodAnalysisService _tenGodService;

        public PersonalOverviewService(TenGodAnalysisService tenGodService) {
            _tenGodService = tenGodService;
        }

        public PersonalOverviewResult Analyze(BaZiInfo info) {
            ArgumentNullException.ThrowIfNull(info);

            return new PersonalOverviewResult(
                BuildWealthCareerCard(info),
                BuildRelationshipCard(info),
                BuildHealthCard(info),
                BuildFamilyCard(info)
            );
        }

        private PersonalOverviewCard BuildWealthCareerCard(BaZiInfo info) {
            var environmentGroups = GetWorkEnvironmentGroups(info);
            var groupStatistics = _tenGodService.GetGroupStatistics(info);
            var maxCount = groupStatistics.Max(group => group.Count);
            var dominantGroups = groupStatistics
                .Where(group => group.Count == maxCount)
                .OrderBy(group => Array.IndexOf(GroupOrder, group.Group))
                .ToArray();
            var secondaryCount = groupStatistics
                .Where(group => group.Count < maxCount)
                .Select(group => group.Count)
                .DefaultIfEmpty(-1)
                .Max();
            var secondaryGroups = secondaryCount < 0
                ? []
                : groupStatistics
                    .Where(group => group.Count == secondaryCount)
                    .OrderBy(group => Array.IndexOf(GroupOrder, group.Group))
                    .ToArray();

            var environmentDetails = environmentGroups
                .Select(group => $"{group.Name}（{group.Count} 支）：{group.Description}")
                .ToArray();
            var dominantDetails = dominantGroups
                .Select(group => {
                    var rule = PersonalOverviewRules.CareerRules[group.Group];
                    return $"{group.Group.ToShenString()} 共 {group.Count} 個：{rule.Core} {rule.Advice}";
                })
                .ToArray();
            var combinationDetails = GetCareerCombinationDetails(dominantGroups, secondaryGroups);
            var dominantNames = string.Join("、", dominantGroups.Select(group => group.Group.ToShenString()));
            var environmentNames = string.Join("、", environmentGroups.Select(group => group.Name));
            var balanceNote = GetCareerTieNote(info, dominantGroups, secondaryGroups);

            return new PersonalOverviewCard(
                "財富與事業",
                "fa-coins",
                $"工作場景以 {environmentNames} 較明顯；以 {dominantNames} 為主要職能。",
                [
                    new PersonalOverviewSection(
                    "工作環境",
                    "四柱地支用來描述較適合的工作場景。若有並列多個表示對命主都適合",
                    environmentDetails
                ),
                new PersonalOverviewSection(
                    "主要工作能力與得財方式",
                    balanceNote,
                    dominantDetails,
                    PersonalOverviewTone.Positive
                ),
                new PersonalOverviewSection(
                    "第一順位 × 第二順位",
                    combinationDetails.Count == 0
                        ? "目前沒有可分出的第二順位，先以並列主要十神的共同特質理解。"
                        : "以十神主要能力與輔助能力疊加；它是職涯建議，不是職業限制。",
                    combinationDetails
                )
                ],
                "財務與職涯內容屬建議性質，不應單獨作為投資、借貸、創業、轉職或收入保證的依據。"
            );
        }

        private PersonalOverviewCard BuildRelationshipCard(BaZiInfo info) {
            var spouseElement = info.Gender == Sex.Male
                ? BaZiDefine.Restricting[info.RiZhu]
                : BaZiDefine.RestrictBy[info.RiZhu];
            var spouseGroup = info.Gender == Sex.Male ? ShiShen.Cai : ShiShen.GuanSha;
            var candidates = FindSpouseCandidates(info, spouseElement);
            var spouseDetails = candidates.Count == 0
                ? [
                    "命盤未見天干或地支主氣夫妻星；改以夫妻宮與自己的完整日柱作輔助。",
                DescribeDayPillarFallback(info)
                ]
                : candidates.Select(candidate => DescribeSpouseCandidate(candidate)).ToArray();
            var palaceProfile = GetPalaceProfile(info.DayZhu.Zhi);
            var palaceRelations = GetPalaceRelations(info);
            var dominantGroups = _tenGodService.GetDominantGroups(info);
            var relationshipDetails = dominantGroups.Select(group => {
                var rule = PersonalOverviewRules.RelationshipStyleRules[group.Group];
                var leadingStars = group.LeadingStars.Count > 1
                    ? string.Empty
                    : $"；以 {string.Join("、", group.LeadingStars.Select(star => star.ToShenString()))} 較明顯";
                return $"{group.Group.ToShenString()}（{group.Count}）{leadingStars}：重視{rule.Need}。優勢是{rule.Strength}；需留意{rule.Risk}。建議{rule.Advice}。";
            }).ToArray();
            var spouseSummary = candidates.Count == 0
                ? $"依{info.Gender.ToSexString()}命口徑，夫妻星為 {spouseElement.ToWuXingString()}（{spouseGroup.ToShenString()}），本命明顯訊號較少。"
                : $"依{info.Gender.ToSexString()}命口徑，夫妻星為 {spouseElement.ToWuXingString()}（{spouseGroup.ToShenString()}），找到 {candidates.Count} 個所在柱候選。";

            return new PersonalOverviewCard(
                "感情姻緣",
                "fa-heart",
                $"夫妻宮在日支{info.DayZhu.Zhi.ToZhiString()}，屬{palaceProfile.Name}；夫妻星為{spouseElement.ToWuXingString()}。",
                [
                    new PersonalOverviewSection(
                    "夫妻星與對象傾向",
                    spouseSummary,
                    spouseDetails
                ),
                new PersonalOverviewSection(
                    "夫妻宮與原局互動",
                    palaceProfile.Description,
                    palaceRelations,
                    palaceRelations.Any(detail => detail.Contains("相刑", StringComparison.Ordinal)
                        || detail.Contains("相沖", StringComparison.Ordinal))
                        ? PersonalOverviewTone.Caution
                        : PersonalOverviewTone.Information
                ),
                new PersonalOverviewSection(
                    "你的關係需求",
                    "以並列最多的十神為主，提供伴侶理解你的需求與相處方式。",
                    relationshipDetails,
                    PersonalOverviewTone.Positive
                )
                ],
                "夫妻星與夫妻宮只表示吸引力及互動傾向，不保證遇見、結婚、分手或第三者；吸引力強不一定是姻緣，有可能變成麻吉或閨密。任何親密互動都需雙方清楚同意；遇到威脅、跟蹤、暴力或強迫時，以人身安全優先。"
            );
        }

        private PersonalOverviewCard BuildHealthCard(BaZiInfo info) {
            var counts = CountChartElements(info);
            var minimumCount = counts.Values.Min();
            var maximumCount = counts.Values.Max();
            var strongestElements = ElementOrder.Where(element => counts[element] == maximumCount).ToArray();
            var weakElements = ElementOrder
                .Where(element => counts[element] == minimumCount)
                .OrderByDescending(element => strongestElements.Any(strong => BaZiDefine.Restricting[strong] == element))
                .ToArray();
            var weakDetails = weakElements.Select(element => {
                var rule = PersonalOverviewRules.HealthRules[element];
                var isControlled = strongestElements.Any(strong => BaZiDefine.Restricting[strong] == element);
                var countText = counts[element] == 0 ? "八個表層字中未見" : $"八個表層字中有 {counts[element]} 個";
                var controlledText = isControlled
                    ? $"；又受到最多的{string.Join("／", strongestElements.Where(strong => BaZiDefine.Restricting[strong] == element).Select(strong => strong.ToWuXingString()))}直接剋制，列為較優先注意"
                    : string.Empty;
                return $"{element.ToWuXingString()}：{countText}{controlledText}。對應{rule.BodyAreas}。日常可{rule.Advice}。";
            }).ToArray();
            var countSummary = string.Join("、", ElementOrder.Select(element => $"{element.ToWuXingString()} {counts[element]}"));
            var weakNames = string.Join("、", weakElements.Select(element => element.ToWuXingString()));
            var threeXing = GetNatalThreeXing(info);
            var riskDetails = threeXing.Count == 0
                ? ["本命沒有湊成寅巳申或丑戌未三刑，無重大危害，但不代表會永遠健康，仍請維持健康檢查與管理。"]
                : threeXing.Select(group => group == "寅巳申"
                    ? "本命湊成寅巳申三刑；交通、跌倒、外傷與既有心血管風險的加強管理。"
                    : "本命湊成丑戌未三刑；容易有小病、情緒起伏大，建議身體異常即就醫、情緒支持與定期健康管理。")
                    .ToArray();
            var posWeak = new PersonalOverviewSection(
                "本命弱項",
                "健康以四柱天干與地支主氣為主，不考慮身強、身弱與支藏干",
                weakDetails,
                PersonalOverviewTone.Caution
            );
            var posThreeXing = new PersonalOverviewSection(
                "三刑安全提醒",
                "三刑只看本命四柱地支",
                riskDetails,
                PersonalOverviewTone.Caution
            );
            return new PersonalOverviewCard(
                "健康",
                "fa-heart-pulse",
                $"表層五行為 {countSummary}；以 {weakNames} 為較需留意的弱項。",
                [posWeak, posThreeXing],
                "五行與身體部位僅為「表徵」或「意象」，不能診斷疾病或決定治療。有症狀、家族史或檢查異常時，請直接依合格醫療專業處理。"
            );
        }

        private PersonalOverviewCard BuildFamilyCard(BaZiInfo info) {
            var childElement = info.Gender == Sex.Male
                ? BaZiDefine.RestrictBy[info.RiZhu]
                : BaZiDefine.Generation[info.RiZhu];
            var childGroup = info.Gender == Sex.Male ? ShiShen.GuanSha : ShiShen.ShihShang;
            var signals = FindElementSignals(info, childElement);
            var directCount = signals.Sum(signal => signal.StemCount + signal.MainBranchCount);
            var hiddenCount = signals.Sum(signal => signal.HiddenBranchCount);
            var locationDetails = signals
                .Where(signal => signal.TotalCount > 0)
                .Select(signal => {
                    var source = DescribeSignalSources(signal);
                    var group = GetPalaceProfile(signal.Pillar.Zhi);
                    return $"{signal.PillarName}{signal.Pillar.Gan.ToGanString()}{signal.Pillar.Zhi.ToZhiString()}（{source}）：地支屬{group.Name}，{GetChildBranchDescription(signal.Pillar.Zhi)}";
                })
                .ToArray();
            if (locationDetails.Length == 0) {
                locationDetails = ["本命未見子息星；訊號不突出，但不代表沒有孩子、無法懷孕或親子關係不好，僅為緣分較薄。"];
            }

            IReadOnlyList<string> palaceDetails;
            if (!info.IsBirthTimeAccurate) {
                palaceDetails = ["因不確定準確時辰，此部分不進行推論"];
            } else {
                var hourSignal = signals.Single(signal => signal.PillarName == "時柱");
                var palaceRelation = GetPrimaryRelation(info.DayZhu.Zhi, info.HourZhu.Zhi);
                palaceDetails = [
                    $"時柱{info.HourZhu.Gan.ToGanString()}{info.HourZhu.Zhi.ToZhiString()}是子息宮；{(hourSignal.TotalCount > 0 ? $"其中見到子息星來源：{DescribeSignalSources(hourSignal)}，生命中有孩子的緣分；非必定有孩子，但有緣分與機率。" : "未見子息星，生命中較無孩子的緣分；非絕對無子，僅表示緣分少、機率低。")}",
                    palaceRelation switch {
                        "相沖" => $"日支{info.DayZhu.Zhi.ToZhiString()}與時支{info.HourZhu.Zhi.ToZhiString()}相沖；孩子長大後較可能外地求學、工作或聚少離多，關係是否疏離仍視相處狀況而定。",
                        "相刑" => $"日支{info.DayZhu.Zhi.ToZhiString()}與時支{info.HourZhu.Zhi.ToZhiString()}相刑；親子互動需多花心力、較互不理解或有衝突，關係是否失敗仍視相處狀況而定。",
                        _ => $"日支{info.DayZhu.Zhi.ToZhiString()}與時支{info.HourZhu.Zhi.ToZhiString()}未形成相刑或相沖，親子互動無太大問題，但要互相理解、尊重。"
                    }
                ];
            }
            var signalSummary = directCount > 0
                ? $"本命見 {directCount} 個天干／地支主氣子息星訊號{(hiddenCount > 0 ? $"，另有 {hiddenCount} 個非主氣藏干訊號" : string.Empty)}。"
                : hiddenCount > 0
                    ? $"本命只在非主氣藏干見 {hiddenCount} 個子息星訊號，較為間接、緣分較薄。"
                    : "本命未見子息星訊號，較無子女緣。不代表不會有孩子，僅表示緣分較少";

            return new PersonalOverviewCard(
                "家人緣分",
                "fa-people-roof",
                $"子女緣分以子息星與子息宮分開判讀；{info.Gender.ToSexString()}命以 {childElement.ToWuXingString()}（{childGroup.ToShenString()}）為子息星。",
                [
                    new PersonalOverviewSection(
                    "子息星分布",
                    signalSummary,
                    locationDetails
                ),
                new PersonalOverviewSection(
                    "子息宮與親子距離",
                    "子息宮固定在時柱；星與宮分開判讀。",
                    palaceDetails
                )
                ],
                "子息星數量不等於胎數、孩子性別或生育力；生育意願、伴侶共識、健康、醫療、經濟與照顧資源才是現實決策依據。"
            );
        }

        private static IReadOnlyList<string> GetCareerCombinationDetails(
            IReadOnlyList<TenGodGroupStatistic> dominantGroups,
            IReadOnlyList<TenGodGroupStatistic> secondaryGroups
        ) {
            var pairs = new List<(ShiShen First, ShiShen Second)>();
            if (dominantGroups.Count > 1) {
                for (var firstIndex = 0; firstIndex < dominantGroups.Count; firstIndex++) {
                    for (var secondIndex = firstIndex + 1; secondIndex < dominantGroups.Count; secondIndex++) {
                        pairs.Add(CanonicalizePair(dominantGroups[firstIndex].Group, dominantGroups[secondIndex].Group));
                    }
                }
            } else if (dominantGroups.Count == 1) {
                pairs.AddRange(secondaryGroups.Select(group => CanonicalizePair(dominantGroups[0].Group, group.Group)));
            }

            return pairs
                .Distinct()
                .Where(pair => PersonalOverviewRules.CareerPairRules.ContainsKey(pair))
                .Select(pair => $"{pair.First.ToShenString()} × {pair.Second.ToShenString()}：{PersonalOverviewRules.CareerPairRules[pair]}")
                .ToArray();
        }

        private static (ShiShen First, ShiShen Second) CanonicalizePair(ShiShen first, ShiShen second) {
            return Array.IndexOf(GroupOrder, first) <= Array.IndexOf(GroupOrder, second)
                ? (first, second)
                : (second, first);
        }

        private static string GetCareerTieNote(
            BaZiInfo info,
            IReadOnlyList<TenGodGroupStatistic> dominantGroups,
            IReadOnlyList<TenGodGroupStatistic> secondaryGroups
        ) {
            if (dominantGroups.Count == 1 && secondaryGroups.Count <= 1) {
                return "先依十神數量找第一、第二順位，再把主星外顯程度與格局承擔力分開參考。";
            }

            var favoredGroups = info.StrengthStatus is GeJu.ShenQiang or GeJu.CongRuo
                ? new[] { ShiShen.Cai, ShiShen.GuanSha, ShiShen.ShihShang }
                : new[] { ShiShen.Yin, ShiShen.BiJie };
            var tiedGroups = dominantGroups.Count > 1 ? dominantGroups : secondaryGroups;
            var favoredTies = tiedGroups.Where(group => favoredGroups.Contains(group.Group)).ToArray();
            var hint = favoredTies.Length == 0
                ? "但格局喜用沒有特別優先項。"
                : $"以 {string.Join("、", favoredTies.Select(group => group.Group.ToShenString()))} 優先。";
            return $"本命有並列順位，{hint}";
        }

        private static IReadOnlyList<WorkEnvironmentGroup> GetWorkEnvironmentGroups(BaZiInfo info) {
            var branches = GetPillars(info).Select(item => item.Pillar.Zhi).ToArray();
            var groups = new[] {
            new WorkEnvironmentGroup(
                "動態移動型",
                branches.Count(branch => branch is DiZhi.Yin or DiZhi.Shen or DiZhi.Si or DiZhi.Hai),
                "較適合外勤、出差、異地、物流、運輸或內容與地點經常變動的工作。"
            ),
            new WorkEnvironmentGroup(
                "穩定深耕型",
                branches.Count(branch => branch is DiZhi.Chen or DiZhi.Xu or DiZhi.Chou or DiZhi.Wei),
                "較適合辦公室、居家、獨處、長時間研究或專業深耕。"
            ),
            new WorkEnvironmentGroup(
                "舞台曝光型",
                branches.Count(branch => branch is DiZhi.Zi or DiZhi.Wu or DiZhi.Mao or DiZhi.You),
                "較適合對外曝光、形象、品牌、公關、活動、銷售或公開展示專業。"
            )
        };
            var maximum = groups.Max(group => group.Count);
            return groups.Where(group => group.Count == maximum).ToArray();
        }

        private static IReadOnlyList<SpouseCandidate> FindSpouseCandidates(BaZiInfo info, WuXing spouseElement) {
            var directCandidates = GetPillars(info)
                .Select(item => new SpouseCandidate(
                    item.Name,
                    item.Pillar,
                    item.Pillar.GanWuXing == spouseElement,
                    item.Pillar.ZhiWuXing == spouseElement,
                    false
                ))
                .Where(candidate => candidate.IsStem || candidate.IsMainBranch)
                .ToArray();
            if (directCandidates.Length > 0) {
                return directCandidates;
            }

            return GetPillars(info)
                .Where(item => item.Pillar.Zhi.ToFullWuXing().ContainsKey(spouseElement))
                .Select(item => new SpouseCandidate(item.Name, item.Pillar, false, false, true))
                .ToArray();
        }

        private static string DescribeSpouseCandidate(SpouseCandidate candidate) {
            var source = candidate switch {
                { IsStem: true, IsMainBranch: true } => "天干透出＋地支主氣",
                { IsStem: true } => "天干透出",
                { IsMainBranch: true } => "地支主氣",
                _ => "藏干備用"
            };
            var pillarName = $"{candidate.Pillar.Gan.ToGanString()}{candidate.Pillar.Zhi.ToZhiString()}";
            var profile = PersonalOverviewRules.SpousePillarProfiles[pillarName];
            return $"{candidate.PillarName}{pillarName}（{source}）：{profile.Personality}；生活／工作傾向為{profile.Lifestyle}；外貌傾向為{profile.Appearance}。{GetSpouseLocationDescription(candidate.PillarName)}";
        }

        private static string GetSpouseLocationDescription(string pillarName) {
            return pillarName switch {
                "年柱" => "柱位傾向對象年長 10～20 歲，或來自外地、異國與較遠生活圈。",
                "月柱" => "柱位傾向對象年長 2～10 歲。",
                "日柱" => "夫妻星落日支時，柱位傾向年齡相近，約正負 2 歲。",
                "時柱" => "柱位傾向對象年輕 2～10 歲，或緣分較晚穩定。",
                _ => string.Empty
            };
        }

        private static string DescribeDayPillarFallback(BaZiInfo info) {
            var pillarName = $"{info.DayZhu.Gan.ToGanString()}{info.DayZhu.Zhi.ToZhiString()}";
            var profile = PersonalOverviewRules.SpousePillarProfiles[pillarName];
            return $"完整日柱{pillarName}的輔助描述：{profile.Personality}；生活／工作傾向為{profile.Lifestyle}；外貌傾向為{profile.Appearance}。這不代表伴侶必須擁有相同日柱。";
        }

        private static IReadOnlyList<string> GetPalaceRelations(BaZiInfo info) {
            var relations = new List<string>();
            AppendPalaceRelation(relations, "月支", info.MonthZhu.Zhi, info.DayZhu.Zhi);
            if (info.IsBirthTimeAccurate) {
                AppendPalaceRelation(relations, "時支", info.HourZhu.Zhi, info.DayZhu.Zhi);
            }
            if (relations.Count == 0) {
                var neighboringBranches = info.IsBirthTimeAccurate ? "月支、時支" : "月支";
                relations.Add($"夫妻宮與鄰近{neighboringBranches}未形成相刑、相沖、害或破；雖姻緣上較無太大問題，但仍須視實際相處狀況而定。");
            }

            return relations;
        }

        private static void AppendPalaceRelation(
            ICollection<string> relations,
            string otherName,
            DiZhi other,
            DiZhi palace
        ) {
            var relation = GetPrimaryRelation(palace, other);
            if (relation is null)
                return;

            var advice = relation switch {
                "相刑" => "較多口角、壓力與內耗，宜提早建立降溫與修復方式。",
                "相沖" => "變動、距離或聚少離多傾向，適度空間有時比勉強黏在一起更省摩擦。",
                "害" => "相處壓力與心情不悅，宜增加具體溝通。",
                "破" => "影響通常較輕，列為一般磨合提醒即可。",
                _ => string.Empty
            };
            relations.Add($"夫妻宮{palace.ToZhiString()}與{otherName}{other.ToZhiString()}形成{relation}：{advice}");
        }

        private static PalaceProfile GetPalaceProfile(DiZhi branch) {
            return branch switch {
                DiZhi.Zi or DiZhi.Wu or DiZhi.Mao or DiZhi.You => new PalaceProfile(
                    "四正／桃花位",
                    "關係中較重外在呈現、美感、穿搭與被看見；這是吸引力傾向，不是外貌保證。"
                ),
                DiZhi.Yin or DiZhi.Shen or DiZhi.Si or DiZhi.Hai => new PalaceProfile(
                    "四長生／驛馬",
                    "關係步調較動態、直接、重自由與行動，生活可能有較多移動及變化。"
                ),
                _ => new PalaceProfile(
                    "四墓庫",
                    "關係較重穩定、安全與深度，但需求可能藏在心裡，宜建立能說出感受與需要的方式。"
                )
            };
        }

        private static IReadOnlyDictionary<WuXing, int> CountChartElements(BaZiInfo info) {
            var elements = GetPillars(info)
                .SelectMany(item => new[] { item.Pillar.GanWuXing, item.Pillar.ZhiWuXing });
            return ElementOrder.ToDictionary(element => element, element => elements.Count(item => item == element));
        }

        private static IReadOnlyList<string> GetNatalThreeXing(BaZiInfo info) {
            var branches = GetPillars(info)
                .Where(item => info.IsBirthTimeAccurate || !ReferenceEquals(item.Pillar, info.HourZhu))
                .Select(item => item.Pillar.Zhi)
                .Distinct()
                .ToArray();
            var groups = new List<string>();
            if (new[] { DiZhi.Yin, DiZhi.Si, DiZhi.Shen }.All(branches.Contains)) {
                groups.Add("寅巳申");
            }
            if (new[] { DiZhi.Chou, DiZhi.Xu, DiZhi.Wei }.All(branches.Contains)) {
                groups.Add("丑戌未");
            }
            return groups;
        }

        private static IReadOnlyList<ElementSignal> FindElementSignals(BaZiInfo info, WuXing element) {
            return GetPillars(info).Select(item => {
                var stemCount = item.Pillar.GanWuXing == element ? 1 : 0;
                var mainBranchCount = item.Pillar.ZhiWuXing == element ? 1 : 0;
                var hiddenBranchCount = mainBranchCount == 0 && item.Pillar.Zhi.ToFullWuXing().ContainsKey(element) ? 1 : 0;
                return new ElementSignal(
                    item.Name,
                    item.Pillar,
                    stemCount,
                    mainBranchCount,
                    hiddenBranchCount
                );
            }).ToArray();
        }

        private static string DescribeSignalSources(ElementSignal signal) {
            var sources = new List<string>();
            if (signal.StemCount > 0)
                sources.Add("天干透出");
            if (signal.MainBranchCount > 0)
                sources.Add("地支主氣");
            if (signal.HiddenBranchCount > 0)
                sources.Add("非主氣藏干");
            return string.Join("＋", sources);
        }

        private static string GetChildBranchDescription(DiZhi branch) {
            return branch switch {
                DiZhi.Chen or DiZhi.Xu or DiZhi.Chou or DiZhi.Wei => "孩子較安靜內斂、喜靜態興趣；相處時保留安靜空間。",
                DiZhi.Yin or DiZhi.Shen or DiZhi.Si or DiZhi.Hai => "孩子較活潑好動、好奇；相處時可安排安全的動態活動與動中學習。",
                _ => "孩子情緒鮮明、自尊較強；相處時先同理、等情緒下降再說理，避免當眾羞辱。"
            };
        }

        private static string? GetPrimaryRelation(DiZhi first, DiZhi second) {
            if (first == second && BaZiDefine.SelfXing.Contains(first))
                return "相刑";
            if (first != second && BaZiDefine.TwoXing.Any(group => group.Contains(first) && group.Contains(second)))
                return "相刑";
            if (first != second && BaZiDefine.Chong.Any(group => group.Contains(first) && group.Contains(second)))
                return "相沖";
            if (first != second && BaZiDefine.Hai.Any(group => group.Contains(first) && group.Contains(second)))
                return "害";
            if (first != second && BaZiDefine.Po.Any(group => group.Contains(first) && group.Contains(second)))
                return "破";
            return null;
        }

        private static IReadOnlyList<(string Name, Zhu Pillar)> GetPillars(BaZiInfo info) {
            return [
                ("年柱", info.YearZhu),
            ("月柱", info.MonthZhu),
            ("日柱", info.DayZhu),
            ("時柱", info.HourZhu)
            ];
        }

        private sealed record WorkEnvironmentGroup(string Name, int Count, string Description);

        private sealed record PalaceProfile(string Name, string Description);

        private sealed record SpouseCandidate(
            string PillarName,
            Zhu Pillar,
            bool IsStem,
            bool IsMainBranch,
            bool IsHiddenBranch
        );

        private sealed record ElementSignal(
            string PillarName,
            Zhu Pillar,
            int StemCount,
            int MainBranchCount,
            int HiddenBranchCount
        ) {
            public int TotalCount => StemCount + MainBranchCount + HiddenBranchCount;
        }
    }
}

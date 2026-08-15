namespace BaZi.Models {

    /// <summary>提供地支互動類型的共用顯示文字。</summary>
    public static class BranchRelationshipExtensions {

        public static string ToDisplayName(this BranchRelationshipType relationshipType) {
            return relationshipType switch {
                BranchRelationshipType.SixCombination => "六合",
                BranchRelationshipType.SixClash => "六沖",
                BranchRelationshipType.SixHarm => "六害",
                BranchRelationshipType.SixBreak => "六破",
                BranchRelationshipType.Punishment => "相刑",
                BranchRelationshipType.ThreeCombination => "三合",
                BranchRelationshipType.ThreeMeeting => "三會",
                _ => throw new System.ComponentModel.InvalidEnumArgumentException(
                    nameof(relationshipType),
                    (int)relationshipType,
                    typeof(BranchRelationshipType)
                )
            };
        }
    }
}

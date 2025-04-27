using CodeBase.StaticData.Skills;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class BattleSkill
    {
        public string casterId;
        public SkillTypeId typeId;
        public SkillKind kind;
        public TargetType targetType;
        public float maxCooldown;
        
        public bool IsSingleTarget => targetType is TargetType.Ally or TargetType.Enemy or TargetType.Self;
    }
}
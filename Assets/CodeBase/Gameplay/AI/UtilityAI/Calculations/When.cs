using CodeBase.Gameplay.Heroes;
using CodeBase.StaticData.Skills;

namespace CodeBase.Gameplay.AI.UtilityAI.Calculations
{
    public static class When
    {
        public static bool SkillIsDamage(BattleSkill skill, IHero hero)
        {
            return skill.kind == SkillKind.Damage;
        }
    }
}
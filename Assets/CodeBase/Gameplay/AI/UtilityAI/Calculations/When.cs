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
        
        public static bool SkillIsHeal(BattleSkill skill, IHero hero)
        {
            return skill.kind == SkillKind.Heal;
        }
        
        public static bool SkillIsInitiativeBurn(BattleSkill skill, IHero hero)
        {
            return skill.kind == SkillKind.InitiativeBurn;
        }
        
        public static bool SkillIsBasicAttack(BattleSkill skill, IHero hero)
        {
            return skill.kind == SkillKind.Damage && skill.maxCooldown == 0;
        }
    }
}
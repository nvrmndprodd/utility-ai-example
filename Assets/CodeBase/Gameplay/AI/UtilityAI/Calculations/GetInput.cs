using System.Linq;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.Skills;

namespace CodeBase.Gameplay.AI.UtilityAI.Calculations
{
    public static class GetInput
    {
        private const int TRUE = 1;
        private const int FALSE = 0;

        public static float PercentageDamage(BattleSkill skill, IHero target, ISkillSolver skillSolver)
        {
            var damage = SkillValue(skill, target, skillSolver);
            return damage / target.State.MaxHp;
        }

        public static float KillingBlow(BattleSkill skill, IHero target, ISkillSolver skillSolver)
        {
            var damage = SkillValue(skill, target, skillSolver);
            return damage > target.State.MaxHp ? TRUE : FALSE;
        }

        public static float PercentageHp(BattleSkill skill, IHero target, ISkillSolver skillSolver)
        {
            return target.State.HpPercentage;
        }

        public static float PercentageHeal(BattleSkill skill, IHero target, ISkillSolver skillSolver)
        {
            var heal = SkillValue(skill, target, skillSolver);
            return heal;
        }

        public static float PercentageInitiativeBurn(BattleSkill skill, IHero target, ISkillSolver skillSolver)
        {
            var burn = SkillValue(skill, target, skillSolver);
            return burn / target.State.MaxInitiative;
        }

        private static float SkillValue(BattleSkill skill, IHero target, ISkillSolver skillSolver)
        {
            return skillSolver.CalculateSkillValue(skill.casterId, skill.typeId, target.Id);
        }

        public static float TargetUltimateIsReady(BattleSkill skill, IHero target, ISkillSolver skillSolver)
        {
            return target.State.SkillStates.Last().IsReady 
                ? TRUE 
                : FALSE;
        }
    }
}
using CodeBase.Gameplay.Heroes;

namespace CodeBase.Gameplay.AI.UtilityAI.Calculations
{
    public static class GetInput
    {
        public static float PercentageDamage(BattleSkill skill, IHero target)
        {
            return target.State.MaxHp;
        }
    }
}
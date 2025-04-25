using CodeBase.Gameplay.Heroes;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public interface IUtilityFunction
    {
        bool AppliesTo(BattleSkill skill, IHero hero);
        float GetInput(BattleSkill skill, IHero hero);
        float Score(float input, IHero hero);
        string Name { get; }
    }
}
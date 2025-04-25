using System;
using CodeBase.Gameplay.Heroes;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class UtilityFunction : IUtilityFunction
    {
        private readonly Func<BattleSkill, IHero, bool> _appliesTo;
        private readonly Func<BattleSkill, IHero, float> _getInput;
        private readonly Func<float, IHero, float> _score;
        public string Name { get; }

        public UtilityFunction(
            string name,
            Func<BattleSkill, IHero, bool> appliesTo,
            Func<BattleSkill, IHero, float> getInput,
            Func<float, IHero, float> score)
        {
            Name = name;
            _appliesTo = appliesTo;
            _getInput = getInput;
            _score = score;
        }

        public bool AppliesTo(BattleSkill skill, IHero hero)
        {
            return _appliesTo(skill, hero);
        }

        public float GetInput(BattleSkill skill, IHero hero)
        {
            return _getInput(skill, hero);
        }

        public float Score(float input, IHero hero)
        {
            return _score(input, hero);
        }
    }
}
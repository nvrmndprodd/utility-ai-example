using System;
using System.Collections.Generic;
using CodeBase.Gameplay.Heroes;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class Convolutions : List<UtilityFunction>
    {
        public void Add(string name,
            Func<BattleSkill, IHero, bool> appliesTo,
            Func<BattleSkill, IHero, float> getInput,
            Func<float, IHero, float> score)
        {
            Add(new UtilityFunction(name, appliesTo, getInput, score));
        }
    }
}
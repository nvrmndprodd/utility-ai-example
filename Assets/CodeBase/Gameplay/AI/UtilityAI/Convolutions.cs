using System;
using System.Collections.Generic;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.Skills;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class Convolutions : List<UtilityFunction>
    {
        public void Add(string name,
            Func<BattleSkill, IHero, bool> appliesTo,
            Func<BattleSkill, IHero, ISkillSolver, float> getInput,
            Func<float, IHero, float> score)
        {
            Add(new UtilityFunction(name, appliesTo, getInput, score));
        }
    }
}
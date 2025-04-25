using System.Collections.Generic;
using CodeBase.Gameplay.AI.UtilityAI.Calculations;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class Brains
    {
        private Convolutions _convolutions = new Convolutions()
        {
            { "Basic damage", When.SkillIsDamage, GetInput.PercentageDamage, Score.AsIs },
        };
        2 26 18
        public IEnumerable<IUtilityFunction> GetUtilityFunctions()
        {
            return _convolutions;
        }
    }
}
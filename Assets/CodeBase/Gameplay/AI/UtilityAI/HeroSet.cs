using System.Collections.Generic;
using CodeBase.Gameplay.Heroes;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class HeroSet
    {
        public IEnumerable<IHero> targets;

        public HeroSet(IHero hero)
        {
            targets = new[] { hero };
        }

        public HeroSet(IEnumerable<IHero> heroes)
        {
            targets = heroes;
        }
    }
}
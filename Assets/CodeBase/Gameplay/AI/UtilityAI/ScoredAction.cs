using System.Collections.Generic;
using System.Linq;
using CodeBase.Gameplay.Battle;
using CodeBase.Gameplay.Heroes;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class ScoredAction : HeroAction
    {
        public float Score;

        public ScoredAction(IHero readyHero, BattleSkill skill, IEnumerable<IHero> targets, float score)
        {
            Caster = readyHero;
            Skill = skill.typeId;
            SkillKind = skill.kind;
            TargetIds = targets.Select(x => x.Id).ToList();
            Score = score;
        }
    }
}
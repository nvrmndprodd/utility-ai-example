using System.Collections.Generic;
using System.Linq;
using CodeBase.Gameplay.Battle;
using CodeBase.Gameplay.Heroes;
using CodeBase.StaticData.Skills;

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

        public override string ToString()
        {
            string skillCategory = SkillKind switch
            {
                SkillKind.Damage => "damage",
                SkillKind.Heal => "heal",
                SkillKind.InitiativeBurn => "initiative burn",
                _ => "other"
            };

            return $"{skillCategory}: {Skill} targets: {TargetIds.Count} score: {Score:0.00}";
        }
    }
}
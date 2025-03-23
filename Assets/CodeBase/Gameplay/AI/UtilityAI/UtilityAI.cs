using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Extensions;
using CodeBase.Gameplay.Battle;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.Gameplay.Skills.Targeting;
using CodeBase.Infrastructure.StaticData;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class UtilityAI : IArtificialIntelligence
    {
        private readonly IStaticDataService _staticDataService;
        private readonly ITargetPicker _targetPicker;
        private readonly IHeroRegistry _heroRegistry;

        public UtilityAI(IStaticDataService staticDataService, ITargetPicker targetPicker, IHeroRegistry heroRegistry)
        {
            _staticDataService = staticDataService;
            _targetPicker = targetPicker;
            _heroRegistry = heroRegistry;
        }

        public HeroAction MakeBestDecision(IHero readyHero)
        {
            var choices = GetScoredHeroActions(readyHero, GetReadyBattleSkills(readyHero));

            return choices.FindMax(x => x.score);
        }

        private IEnumerable<BattleSkill> GetReadyBattleSkills(IHero readyHero)
        {
            return readyHero.State.SkillStates
                .Where(x => x.IsReady)
                .Select(x => new BattleSkill()
                {
                    casterId = readyHero.Id,
                    typeId = x.TypeId,
                    kind = _staticDataService.HeroSkillFor(x.TypeId, readyHero.TypeId).Kind,
                    targetType = _staticDataService.HeroSkillFor(x.TypeId, readyHero.TypeId).TargetType,
                });
        }

        private IEnumerable<ScoredAction> GetScoredHeroActions(IHero readyHero, IEnumerable<BattleSkill> readySkills)
        {
            foreach (var skill in readySkills)
            {
                foreach (var heroSet in HeroSetsForSkill(skill, readyHero))
                {
                    
                }
            }
        }

        private IEnumerable<HeroSet> HeroSetsForSkill(BattleSkill skill, IHero readyHero)
        {
            var availableTargets = _targetPicker.AvailableTargetsFor(readyHero.Id, skill.targetType);

            if (skill.IsSingleTarget)
            {
                foreach (var targetId in availableTargets)
                {
                    yield return new HeroSet(_heroRegistry.GetHero(targetId));
                }
                
                yield break;
            }

            yield return new HeroSet(availableTargets.Select(_heroRegistry.GetHero));
        }
    }
}
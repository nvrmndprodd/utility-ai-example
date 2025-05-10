using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Extensions;
using CodeBase.Gameplay.Battle;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.Infrastructure.StaticData;
using CodeBase.StaticData.Heroes;
using CodeBase.StaticData.Skills;

namespace CodeBase.Gameplay.AI
{
    public class BehaviourTree : IArtificialIntelligence
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IHeroRegistry _heroRegistry;

        public BehaviourTree(IStaticDataService staticDataService, IHeroRegistry heroRegistry)
        {
            _staticDataService = staticDataService;
            _heroRegistry = heroRegistry;
        }

        public HeroAction MakeBestDecision(IHero readyHero)
        {
            SkillKind chosenSkillKind = ChooseSkillKind(readyHero);
            HeroSkill chosenSkill = ChoseSkill(readyHero, chosenSkillKind);
            List<string> targets = ChoseTargets(
                readyHero.Id,
                chosenSkill.TargetType,
                chosenSkill.Kind
            );
            
            return new HeroAction
            {
                Caster = readyHero,
                Skill = chosenSkill.TypeId,
                SkillKind = chosenSkillKind,
                TargetIds = targets
            };
        }

        private HeroSkill ChoseSkill(IHero readyHero, SkillKind skillKind)
        {
            return readyHero.State.SkillStates
                .Where(x => x.IsReady)
                .Select(x => _staticDataService.HeroSkillFor(x.TypeId, readyHero.TypeId))
                .Last(x => x.Kind == skillKind);
        }

        private SkillKind ChooseSkillKind(IHero readyHero)
        {
            bool haveHeal = readyHero.State.SkillStates
                .Where(x => x.IsReady)
                .Any(x => _staticDataService.HeroSkillFor(x.TypeId, readyHero.TypeId).Kind is SkillKind.Heal);

            if (haveHeal)
            {
                foreach (var ally in _heroRegistry.AlliesOf(readyHero.Id))
                {
                    IHero allyBehaviour = _heroRegistry.GetHero(ally);
                    if (allyBehaviour.State.HpPercentage < 0.7f)
                    {
                        return SkillKind.Heal;
                    }
                }
            }
            
            bool haveInitiativeBurn = readyHero.State.SkillStates
                .Where(x => x.IsReady)
                .Reverse()
                .Any(x => _staticDataService.HeroSkillFor(x.TypeId, readyHero.TypeId).Kind is SkillKind.InitiativeBurn);

            if (haveInitiativeBurn)
            {
                foreach (var enemy in _heroRegistry.EnemiesOf(readyHero.Id))
                {
                    IHero enemyBehaviour = _heroRegistry.GetHero(enemy);
                    if (enemyBehaviour.State.InitiativePercentage > 0.7f)
                    {
                        return SkillKind.InitiativeBurn;
                    }
                }
            }

            return SkillKind.Damage;
        }
        
        private List<string> ChoseTargets(string casterId, 
            TargetType targetType, 
            SkillKind chosenSkillKind)
        {
            switch (targetType)
            {
                case TargetType.Ally:
                    return new List<string> { _heroRegistry.AlliesOf(casterId).FindMin(x => _heroRegistry.GetHero(x).State.HpPercentage) };
                case TargetType.Enemy:
                    return chosenSkillKind switch
                    {
                        SkillKind.Damage => new List<string>
                        {
                            _heroRegistry.EnemiesOf(casterId).FindMin(x => _heroRegistry.GetHero(x).State.HpPercentage)
                        },
                        SkillKind.InitiativeBurn => new List<string>
                        {
                            _heroRegistry.EnemiesOf(casterId).FindMax(x => _heroRegistry.GetHero(x).State.InitiativePercentage)
                        },
                        _ => throw new ArgumentOutOfRangeException(nameof(chosenSkillKind), chosenSkillKind, null)
                    };
                case TargetType.AllAllies:
                    return _heroRegistry.AlliesOf(casterId).ToList();
                case TargetType.AllEnemies:
                    return _heroRegistry.EnemiesOf(casterId).ToList();
                case TargetType.Self:
                    return new List<string> { casterId };
            }

            return new List<string>();
        }
    }
}
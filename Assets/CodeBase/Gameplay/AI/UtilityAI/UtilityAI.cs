using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Extensions;
using CodeBase.Gameplay.AI.MLAgents;
using CodeBase.Gameplay.AI.Reporting;
using CodeBase.Gameplay.Battle;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.Gameplay.Skills;
using CodeBase.Gameplay.Skills.Targeting;
using CodeBase.Infrastructure.StaticData;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class UtilityAI : IArtificialIntelligence
    {
        private readonly IStaticDataService _staticDataService;
        private readonly ITargetPicker _targetPicker;
        private readonly IHeroRegistry _heroRegistry;
        private readonly ISkillSolver _skillSolver;
        private readonly IAIReporter _aiReporter;

        private IEnumerable<IUtilityFunction> _utilityFunctions;

        public UtilityAI(
            IStaticDataService staticDataService, 
            ITargetPicker targetPicker, 
            IHeroRegistry heroRegistry, 
            ISkillSolver skillSolver, 
            IAIReporter aiReporter)
        {
            _staticDataService = staticDataService;
            _targetPicker = targetPicker;
            _heroRegistry = heroRegistry;
            _skillSolver = skillSolver;
            _aiReporter = aiReporter;

            _utilityFunctions = new Brains().GetUtilityFunctions();
            
            BattleAgent.Instance.InitializeDependencies(heroRegistry, staticDataService, skillSolver);
        }

        public HeroAction MakeBestDecision(IHero readyHero)
        {
            var choices = GetScoredHeroActions(readyHero, GetReadyBattleSkills(readyHero)).ToList();
            
            _aiReporter.ReportDecisionScores(readyHero, choices);

            return choices.FindMax(x => x.Score);
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
                    maxCooldown = x.MaxCooldown
                });
        }

        private IEnumerable<ScoredAction> GetScoredHeroActions(IHero readyHero, IEnumerable<BattleSkill> readySkills)
        {
            foreach (var skill in readySkills)
            {
                foreach (var heroSet in HeroSetsForSkill(skill, readyHero))
                {
                    float? score = CalculateScore(skill, heroSet);

                    if (score.HasValue == false)
                    {
                        continue;
                    }

                    yield return new ScoredAction(readyHero, skill, heroSet.targets, score.Value);
                }
            }
        }

        private float? CalculateScore(BattleSkill skill, HeroSet heroSet)
        {
            if (heroSet.targets.IsNullOrEmpty())
            {
                return null;
            }
            
            return heroSet.targets
                .Select(hero => CalculateScore(skill, hero))
                .Where(x => x != null)
                .Sum();
        }

        private float? CalculateScore(BattleSkill skill, IHero hero)
        {
            List<ScoreFactor> scoreFactors = 
                (from utilityFunction in _utilityFunctions
                where utilityFunction.AppliesTo(skill, hero)
                
                let input = utilityFunction.GetInput(skill, hero, _skillSolver)
                let score = utilityFunction.Score(input, hero)
                
                select new ScoreFactor(utilityFunction.Name, score))
                .ToList();
            
            _aiReporter.ReportDecisionDetails(skill, hero, scoreFactors);

            return scoreFactors.Select(x => x.Score).SumOrNull();
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
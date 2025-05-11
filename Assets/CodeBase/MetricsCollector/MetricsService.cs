using System;
using System.Collections.Generic;
using System.Linq;
using CodeBase.Gameplay.Battle;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.Gameplay.Skills;
using CodeBase.StaticData.Skills;
using UnityEngine;

namespace CodeBase.MetricsCollector
{
    public class MetricsService : IMetricsService
    {
        private readonly IHeroRegistry _heroRegistry;
        private readonly ISkillSolver _skillSolver;

        private readonly Metrics _firstTeamMetrics = new();
        private readonly Metrics _secondTeamMetrics = new();

        public MetricsService(IHeroRegistry heroRegistry, ISkillSolver skillSolver)
        {
            _heroRegistry = heroRegistry;
            _skillSolver = skillSolver;

            Time.timeScale = 20;
        }

        public void BattleStarted()
        {
            foreach (var hero in _heroRegistry.FirstTeam)
            {
                _firstTeamMetrics.actionInFight.TryAdd(hero, 0);
            }
            foreach (var hero in _heroRegistry.SecondTeam)
            {
                _secondTeamMetrics.actionInFight.TryAdd(hero, 0);
            }
        }

        public void HeroPerformedAction(HeroAction action)
        {
            string heroId = action.Caster.Id;
            Metrics metrics = GetMetricsForHero(heroId);
            metrics.actionInFight[heroId]++;
            
            AverageValueMetric(action);
        }

        public void HeroDied(string heroId)
        {
            Metrics metrics = GetMetricsForHero(heroId);
            metrics.heroesDiedInFight++;
            int actionsCount = metrics.actionInFight.GetValueOrDefault(heroId, 0);
            metrics.avgActionsToDie = UpdateAverage(metrics.avgActionsToDie, actionsCount, metrics.avgActionsToDieCounter);
            metrics.avgActionsToDieCounter++;
        }

        public void BattleFinished()
        {
            Metrics wonTeamMetrics = _heroRegistry.FirstTeam.Count == 0 ? _secondTeamMetrics : _firstTeamMetrics;
            wonTeamMetrics.fightsWon++;
            
            int actionsCount = _firstTeamMetrics.actionInFight.Values.Sum() + _secondTeamMetrics.actionInFight.Values.Sum();
            wonTeamMetrics.avgActionsToWin = UpdateAverage(wonTeamMetrics.avgActionsToWin, actionsCount, wonTeamMetrics.fightsWon);

            List<string> wonTeam = _heroRegistry.FirstTeam.Count == 0 ? _heroRegistry.SecondTeam : _heroRegistry.FirstTeam;
            
            _firstTeamMetrics.avgActionsPerAgent = UpdateAverage(_firstTeamMetrics.avgActionsPerAgent, _firstTeamMetrics.actionInFight.Values.Select(x => x / 3f).ToList());
            _secondTeamMetrics.avgActionsPerAgent = UpdateAverage(_secondTeamMetrics.avgActionsPerAgent, _secondTeamMetrics.actionInFight.Values.Select(x => x / 3f).ToList());
            
            UpdateAverageValues();
            
            _firstTeamMetrics.Clear();
            _secondTeamMetrics.Clear();

            if (_firstTeamMetrics.fightsWon + _secondTeamMetrics.fightsWon == BattleConductor.FIGHTS)
            {
                Debug.Log($"First team metrics: {_firstTeamMetrics}");
                Debug.Log($"Second team metrics: {_secondTeamMetrics}");
            }
        }

        private void AverageValueMetric(HeroAction action)
        {
            IHero hero = action.Caster;

            Metrics metrics = GetMetricsForHero(hero.Id);
            
            List<float> metricsList = null;

            switch (action.SkillKind)
            {
                case SkillKind.Damage:
                    metricsList = metrics.damagePerTurn;
                    break;
                case SkillKind.Heal:
                    metricsList = metrics.healPerTurn;
                    break;
                case SkillKind.InitiativeBurn:
                    metricsList = metrics.burnPerTurn;
                    break;
            }

            float cumulativeValue = 0;
            
            foreach (var targetId in action.TargetIds)
            {
                float value = _skillSolver.CalculateSkillValue(hero.Id, action.Skill, targetId);
                IHero target = _heroRegistry.GetHero(targetId);

                if (action.SkillKind == SkillKind.Damage)
                {
                    value = Mathf.Min(value, target.State.CurrentHp);
                }
                else if (action.SkillKind == SkillKind.Heal)
                {
                    value = Mathf.Min(value, target.State.MaxHp - target.State.CurrentHp);
                }
                else
                {
                    value = Mathf.Min(value, target.State.CurrentInitiative);
                }
                
                cumulativeValue += value;
            }
            
            metricsList!.Add(cumulativeValue);
        }

        private void UpdateAverageValues()
        {
            _firstTeamMetrics.avgDamage = UpdateAverage(_firstTeamMetrics.avgDamage, _firstTeamMetrics.damagePerTurn);
            _firstTeamMetrics.avgHeal = UpdateAverage(_firstTeamMetrics.avgHeal, _firstTeamMetrics.healPerTurn);
            _firstTeamMetrics.avgBurn = UpdateAverage(_firstTeamMetrics.avgBurn, _firstTeamMetrics.burnPerTurn);
            _firstTeamMetrics.avgHeroesDied = UpdateAverage(_firstTeamMetrics.avgHeroesDied, _firstTeamMetrics.heroesDiedInFight);
            
            _secondTeamMetrics.avgDamage = UpdateAverage(_secondTeamMetrics.avgDamage, _secondTeamMetrics.damagePerTurn);
            _secondTeamMetrics.avgHeal = UpdateAverage(_secondTeamMetrics.avgHeal, _secondTeamMetrics.healPerTurn);
            _secondTeamMetrics.avgBurn = UpdateAverage(_secondTeamMetrics.avgBurn, _secondTeamMetrics.burnPerTurn);
            _secondTeamMetrics.avgHeroesDied = UpdateAverage(_secondTeamMetrics.avgHeroesDied, _secondTeamMetrics.heroesDiedInFight);
            
            int actionsInFight = _firstTeamMetrics.actionInFight.Values.Sum() + _secondTeamMetrics.actionInFight.Values.Sum();
            _firstTeamMetrics.avgActionsPerFight = UpdateAverage(_firstTeamMetrics.avgActionsPerFight, actionsInFight);
            _secondTeamMetrics.avgActionsPerFight = UpdateAverage(_secondTeamMetrics.avgActionsPerFight, actionsInFight);
        }

        private float UpdateAverage(float prevAvg, List<float> values)
        {
            int fightsCount = _firstTeamMetrics.fightsWon + _secondTeamMetrics.fightsWon;

            if (fightsCount == 1)
            {
                return values.Sum();
            }

            if (values.Count == 0)
            {
                return prevAvg * (fightsCount - 1) / fightsCount;
            }

            return prevAvg * (fightsCount - 1) / fightsCount + values.Sum() / fightsCount;
        }

        private float UpdateAverage(float prevAvg, int fightValues, int fightsCount = 0)
        {
            if (fightsCount == 0)
            {
                fightsCount = _firstTeamMetrics.fightsWon + _secondTeamMetrics.fightsWon;
            }
            
            if (fightsCount == 1)
            {
                return fightValues;
            }
            
            return prevAvg * (fightsCount - 1) / fightsCount + (float) fightValues / fightsCount;
        }

        private Metrics GetMetricsForHero(string heroId)
        {
            return _heroRegistry.FirstTeam.Contains(heroId) 
                ? _firstTeamMetrics 
                : _secondTeamMetrics;
        }
    }
}
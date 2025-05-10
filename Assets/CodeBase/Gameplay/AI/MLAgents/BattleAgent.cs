using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CodeBase.Extensions;
using CodeBase.Gameplay.Battle;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.Gameplay.Skills;
using CodeBase.Infrastructure.StaticData;
using CodeBase.StaticData.Skills;
using JetBrains.Annotations;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CodeBase.Gameplay.AI.MLAgents
{
    public class BattleAgent : Agent
    {
        private const int SKILLS_COUNT = 3;
        private const int SKILL_FEATURES = 4; // Kind, TargetType, Value, Cooldown

        private static BattleAgent _instance;
        private static readonly object _lock = new object();

        public static BattleAgent Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = Instantiate(Resources.Load<BattleAgent>("BattleAgent"));
                    DontDestroyOnLoad(_instance.gameObject);
                }

                return _instance;
            }
        }

        private IStaticDataService _staticDataService;
        private IHeroRegistry _heroRegistry;
        private ISkillSolver _skillSolver;

        private IHero _hero;
        private HeroAction _decidedAction;
        private TaskCompletionSource<HeroAction> _taskCompletionSource;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void InitializeDependencies(
            IHeroRegistry heroRegistry,
            IStaticDataService staticDataService,
            ISkillSolver skillSolver)
        {
            _heroRegistry = heroRegistry;
            _staticDataService = staticDataService;
            _skillSolver = skillSolver;
        }

        public void LoadCharacterData(IHero hero)
        {
            lock (_lock)
            {
                _hero = hero;
                _taskCompletionSource = new TaskCompletionSource<HeroAction>();
                RequestDecision();
            }
        }

        public Task<HeroAction> GetAction()
        {
            return _taskCompletionSource.Task;
        }

        public override void CollectObservations(VectorSensor sensor)
        {
            sensor.AddObservation(_hero.State.HpPercentage);
            sensor.AddObservation(_hero.State.Armor);
            sensor.AddObservation(_hero.State.InitiativePercentage);

            var firstTeam = _heroRegistry.FirstTeam;
            var secondTeam = _heroRegistry.SecondTeam;

            if (secondTeam.Contains(_hero.Id))
            {
                (firstTeam, secondTeam) = (secondTeam, firstTeam);
            }
            
            for (int i = 0; i < 3; i++)
            {
                if (firstTeam.Count <= i)
                {
                    sensor.AddObservation(0);
                }
                else
                {
                    var ally = firstTeam[i];
                    if (ally == _hero.Id)
                    {
                        continue;
                    }
                    sensor.AddObservation(_heroRegistry.GetHero(ally).State.HpPercentage);
                }
            }
            
            for (int i = 0; i < 3; i++)
            {
                if (secondTeam.Count <= i)
                {
                    sensor.AddObservation(0);
                    sensor.AddObservation(0);
                }
                else
                {
                    var enemy = secondTeam[i];
                    sensor.AddObservation(_heroRegistry.GetHero(enemy).State.HpPercentage);
                    sensor.AddObservation(_heroRegistry.GetHero(enemy).State.InitiativePercentage);
                }
            }
        
            // Add skill information for each of 3 skills
            foreach (var skillState in _hero.State.SkillStates)
            {
                var skillData = _staticDataService.HeroSkillFor(skillState.TypeId, _hero.TypeId);

                // SkillKind enum as integer
                sensor.AddObservation((int)skillData.Kind / 4f);

                // TargetType enum as integer
                sensor.AddObservation((int)skillData.TargetType / 4f);
            
                sensor.AddObservation(skillState.IsReady ? 1f : 0f);

                // Value and Cooldown normalized
            
                if (skillData.Kind == SkillKind.Damage)
                {
                    sensor.AddObservation(skillData.Value / 100f);
                }
                else
                {
                    sensor.AddObservation(skillData.Value);
                }
                sensor.AddObservation(skillState.Cooldown / 10f);
            }
        }

        public override void OnActionReceived(ActionBuffers actions)
        {
            // Parse discrete actions
            int selectedSkillId = actions.DiscreteActions[0];
            int selectedTargetId = actions.DiscreteActions[1];

            selectedSkillId = Mathf.Clamp(selectedTargetId, 0, 2);
            selectedTargetId = Mathf.Clamp(selectedTargetId, 0, 2);

            // Get skill data
            var skillState = _hero.State.SkillStates[selectedSkillId];
            var skillData = _staticDataService.HeroSkillFor(skillState.TypeId, _hero.TypeId);
            
            Debug.Log(actions.DiscreteActions[0].ToString() + "   " + actions.DiscreteActions[1].ToString());

            // Handle target selection
            List<string> targets = new List<string>();

            switch (skillData.TargetType)
            {
                case TargetType.Enemy:
                    if (selectedTargetId >= _heroRegistry.EnemiesOf(_hero.Id).Count())
                    {
                        AddReward(-5f);
                    }
                    
                    selectedTargetId = Mathf.Clamp(selectedTargetId, 0, _heroRegistry.EnemiesOf(_hero.Id).Count() - 1);
                    targets = new List<string> { _heroRegistry.EnemiesOf(_hero.Id).ToList()[selectedTargetId] };
                    break;
                case TargetType.Ally:
                    if (selectedTargetId >= _heroRegistry.AlliesOf(_hero.Id).Count())
                    {
                        AddReward(-5f);
                    }
                    
                    selectedTargetId = Mathf.Clamp(selectedTargetId, 0, _heroRegistry.AlliesOf(_hero.Id).Count() - 1);
                    targets = new List<string> { _heroRegistry.AlliesOf(_hero.Id).ToList()[selectedTargetId] };
                    break;
                case TargetType.Self:
                    targets = new List<string> { _hero.Id };
                    break;
                case TargetType.AllEnemies:
                    targets = _heroRegistry.EnemiesOf(_hero.Id).ToList();
                    break;
                case TargetType.AllAllies:
                    targets = _heroRegistry.AlliesOf(_hero.Id).ToList();
                    break;
                default:
                    throw new System.ArgumentException("Invalid mass target type");
            }

            _decidedAction = new HeroAction()
            {
                Caster = _hero,
                Skill = skillState.TypeId,
                SkillKind = skillData.Kind,
                TargetIds = targets
            };

            CalculateReward();

            if (skillState.Cooldown > 0)
            {
                skillState = _hero.State.SkillStates[0];
                skillData = _staticDataService.HeroSkillFor(skillState.TypeId, _hero.TypeId);

                // Handle target selection
                targets = new List<string> { _heroRegistry.EnemiesOf(_hero.Id).PickRandom() };

                _decidedAction = new HeroAction()
                {
                    Caster = _hero,
                    Skill = skillState.TypeId,
                    SkillKind = skillData.Kind,
                    TargetIds = targets
                };
            }

            _taskCompletionSource.SetResult(_decidedAction);
        }

        private void CalculateReward()
        {
            // 1. Проверка пропущенного убийства базовой атакой
            var basicAttackSkill = _hero.State.SkillStates[0];
            var basicAttackData = _staticDataService.HeroSkillFor(basicAttackSkill.TypeId, _hero.TypeId);

            bool missedKill = false;
            foreach (var enemyId in _heroRegistry.EnemiesOf(_hero.Id))
            {
                var enemy = _heroRegistry.GetHero(enemyId);
                float potentialDamage = _skillSolver.CalculateSkillValue(_hero.Id, basicAttackData.TypeId, enemyId);

                if (enemy.State.CurrentHp <= potentialDamage &&
                    _decidedAction.Skill != basicAttackData.TypeId && 
                    _decidedAction.TargetIds.Contains(enemy.Id) == false)
                {
                    missedKill = true;
                    break;
                }
            }

            if (missedKill) AddReward(-2.0f);

            // 2. Награда за исцеление при высоком HP
            if (_decidedAction.SkillKind == SkillKind.Heal)
            {
                foreach (var targetId in _decidedAction.TargetIds)
                {
                    var target = _heroRegistry.GetHero(targetId);
                    if (target.State.HpPercentage > 0.5f)
                    {
                        AddReward(-1.5f * (target.State.HpPercentage - 0.5f));
                    }
                }
            }

            // 3. Сжигание инициативы при низком уровне у врагов
            if (_decidedAction.SkillKind == SkillKind.InitiativeBurn)
            {
                foreach (var targetId in _decidedAction.TargetIds)
                {
                    var target = _heroRegistry.GetHero(targetId);
                    if (target.State.InitiativePercentage < 0.25f)
                    {
                        AddReward(-0.8f * (0.25f - target.State.InitiativePercentage));
                    }
                }
            }

            // 4. Использование способности в кулдауне
            var usedSkillState = _hero.State.SkillStates
                .FirstOrDefault(s => s.TypeId == _decidedAction.Skill);

            if (usedSkillState != null && usedSkillState.Cooldown > 0)
            {
                AddReward(-10.0f);
            }

            // 5. Положительная награда за убийство
            if (_decidedAction.SkillKind == SkillKind.Damage)
            {
                foreach (var targetId in _decidedAction.TargetIds)
                {
                    var target = _heroRegistry.GetHero(targetId);
                    if (target.State.CurrentHp <= 0)
                    {
                        AddReward(5.0f);
                    }
                }
            }

            // Существующие награды
            if (_decidedAction.SkillKind == SkillKind.Damage)
            {
                foreach (var targetId in _decidedAction.TargetIds)
                {
                    float damageDealt = _skillSolver.CalculateSkillValue(_hero.Id, _decidedAction.Skill, targetId);
                    AddReward(damageDealt * 0.1f);
                }
            }

            if (_decidedAction.SkillKind == SkillKind.Heal)
            {
                foreach (var targetId in _decidedAction.TargetIds)
                {
                    float healAmount = _skillSolver.CalculateSkillValue(_hero.Id, _decidedAction.Skill, targetId);
                    AddReward(healAmount * 0.2f);
                }
            }

            if (_decidedAction.SkillKind == SkillKind.InitiativeBurn)
            {
                foreach (var targetId in _decidedAction.TargetIds)
                {
                    float initiativeReduction = _skillSolver.CalculateSkillValue(_hero.Id, _decidedAction.Skill, targetId);
                    AddReward(initiativeReduction * 0.15f);
                }
            }
        }

        public override void Heuristic(in ActionBuffers actionsOut)
        {
            var discreteActions = actionsOut.DiscreteActions;

            // Manual skill selection (0-2)
            discreteActions[0] = Random.Range(0, 3);

            // Manual target selection (0-2)
            discreteActions[1] = Random.Range(0, 3);
        }
    }
}
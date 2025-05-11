using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeBase.Gameplay.AI;
using CodeBase.Gameplay.AI.FSM;
using CodeBase.Gameplay.AI.MLAgents;
using CodeBase.Gameplay.AI.Reporting;
using CodeBase.Gameplay.AI.UtilityAI;
using CodeBase.Gameplay.Cooldowns;
using CodeBase.Gameplay.Death;
using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.Gameplay.Initiative;
using CodeBase.Gameplay.Skills;
using CodeBase.Gameplay.Skills.Targeting;
using CodeBase.Infrastructure.StaticData;
using CodeBase.MetricsCollector;
using CodeBase.StaticData.Skills;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

namespace CodeBase.Gameplay.Battle
{
    public class BattleConductor : IBattleConductor, ITickable
    {
        public const int FIGHTS = 500;
        
        private const float TurnTickDuration = 0.05f;

        private readonly IHeroRegistry _heroRegistry;
        private readonly IDeathService _deathService;
        private readonly IInitiativeService _initiativeService;
        private readonly ICooldownService _cooldownService;
        private readonly ISkillSolver _skillSolver;
        private readonly IMetricsService _metrics;
        
        private readonly IArtificialIntelligence _leftTeamAI;
        private readonly IArtificialIntelligence _rightTeamAI;

        private float _untilNextTurnTick;
        private bool _started;
        private bool _finished;

        private bool _turnTimerPaused;

        private int _counter;

        public BattleMode Mode { get; private set; } = BattleMode.Auto;

        public event Action<HeroAction> HeroActionProduced;

        public BattleConductor(
            IHeroRegistry heroRegistry,
            IDeathService deathService,
            IInitiativeService initiativeService,
            ICooldownService cooldownService,
            ISkillSolver skillSolver,
            IMetricsService metrics,
            IStaticDataService staticDataService,
            ITargetPicker targetPicker,
            IAIReporter aiReporter)
        {
            _skillSolver = skillSolver;
            _metrics = metrics;
            _heroRegistry = heroRegistry;
            _deathService = deathService;
            _deathService = deathService;
            _initiativeService = initiativeService;
            _cooldownService = cooldownService;

            _leftTeamAI = new UtilityAI(staticDataService, targetPicker, heroRegistry, skillSolver, aiReporter);
            _rightTeamAI = new BehaviourTree(staticDataService, heroRegistry);
        }

        public async void Tick()
        {
            if (!_started || _finished)
                return;
            
            await UpdateTurnTimer();
            _skillSolver.SkillDelaysTick();
            _deathService.ProcessDeadHeroes();
            CheckBattleEnd();
        }

        public void Start()
        {
            _started = true;
            _metrics.BattleStarted();
        }

        public void Stop()
        {
            _started = false;
        }

        public void ResumeTurnTimer()
        {
            _turnTimerPaused = false;
        }

        public void SetMode(BattleMode mode)
        {
            Mode = mode;
        }

        private void PauseInManualMode()
        {
            if (Mode == BattleMode.Manual)
                _turnTimerPaused = true;
        }

        private void Finish()
        {
            _counter++;
            _metrics.BattleFinished();

            if (_counter < FIGHTS)
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                _finished = true;
            }
        }

        private async Task UpdateTurnTimer()
        {
            if (_turnTimerPaused)
                return;

            _untilNextTurnTick -= Time.deltaTime;
            if (_untilNextTurnTick <= 0)
            {
                await TurnTick();
                _untilNextTurnTick = TurnTickDuration;
            }
        }

        private async Task TurnTick()
        {
            _cooldownService.CooldownTick(TurnTickDuration);
            _initiativeService.ReplenishInitiativeTick();
            await ProcessReadyHeroes();

            if (_initiativeService.HeroIsReadyOnNextTick())
                PauseInManualMode();
        }

        private void CheckBattleEnd()
        {
            if (_heroRegistry.FirstTeam.Count == 0 || _heroRegistry.SecondTeam.Count == 0)
                Finish();
        }

        private async Task ProcessReadyHeroes()
        {
            foreach (var hero in _heroRegistry.AllActiveHeroes())
                if (hero.IsReady)
                {
                    hero.State.CurrentInitiative = 0;
                    await PerformHeroAction(hero);
                }
        }

        public async Task PerformHeroAction(HeroBehaviour readyHero)
        {
            HeroAction heroAction;
            
            /*if (_heroRegistry.SecondTeam.Contains(readyHero.Id))
            {
                BattleAgent.Instance.LoadCharacterData(readyHero);
                heroAction = await BattleAgent.Instance.GetAction();
            }
            else
            {
                heroAction = _leftTeamAI.MakeBestDecision(readyHero);
            }*/
            
            if (_heroRegistry.SecondTeam.Contains(readyHero.Id))
            {
                heroAction = _rightTeamAI.MakeBestDecision(readyHero);
            }
            else
            {
                heroAction = _leftTeamAI.MakeBestDecision(readyHero);
            }

            _metrics.HeroPerformedAction(heroAction);
            
            _skillSolver.ProcessHeroAction(heroAction);

            HeroActionProduced?.Invoke(heroAction);
        }

        private SkillTypeId TempSkill(HeroBehaviour readyHero)
        {
            return readyHero.State.SkillStates[0].TypeId;
        }

        private List<string> TempTargets(HeroBehaviour readyHero)
        {
            return _heroRegistry.EnemiesOf(readyHero.Id).ToList();
        }
    }
}
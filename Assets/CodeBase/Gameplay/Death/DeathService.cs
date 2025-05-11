using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.MetricsCollector;
using UnityEngine;

namespace CodeBase.Gameplay.Death
{
    public class DeathService : IDeathService
    {
        private const float DefaultDestroyTime = 3;
        private readonly IHeroRegistry _heroRegistry;
        private readonly IMetricsService _metrics;

        public DeathService(IHeroRegistry heroRegistry, IMetricsService metrics)
        {
            _heroRegistry = heroRegistry;
            _metrics = metrics;
        }

        public void ProcessDeadHeroes()
        {
            foreach (var id in _heroRegistry.AllIds)
            {
                var hero = _heroRegistry.GetHero(id);
                if (!hero.IsDead)
                    continue;

                _metrics.HeroDied(hero.Id);
                _heroRegistry.Unregister(hero.Id);

                hero.Animator.PlayDeath();
                Object.Destroy(hero.gameObject, DefaultDestroyTime);
            }
        }
    }
}
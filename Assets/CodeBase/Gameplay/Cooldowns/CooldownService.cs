﻿using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using UnityEngine;
using Zenject;

namespace CodeBase.Gameplay.Cooldowns
{
    public class CooldownService : ICooldownService
    {
        private readonly IHeroRegistry _heroRegistry;

        public CooldownService(IHeroRegistry heroRegistry)
        {
            _heroRegistry = heroRegistry;
        }

        public void CooldownTick(float deltaTime)
        {
            foreach (var hero in _heroRegistry.AllActiveHeroes())
            foreach (var skillState in hero.State.SkillStates)
                skillState.TickCooldown(deltaTime);
        }
    }
}
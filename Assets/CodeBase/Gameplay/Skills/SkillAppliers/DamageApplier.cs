using CodeBase.Gameplay.Heroes;
using CodeBase.Gameplay.HeroRegistry;
using CodeBase.Gameplay.UI;
using CodeBase.Infrastructure.StaticData;
using CodeBase.StaticData.Heroes;
using CodeBase.StaticData.Skills;
using UnityEngine;

namespace CodeBase.Gameplay.Skills.SkillAppliers
{
    public class DamageApplier : ISkillApplier
    {
        private readonly IStaticDataService _staticDataService;
        private readonly IHeroRegistry _heroRegistry;
        private readonly IBattleTextPlayer _battleTextPlayer;
        public SkillKind SkillKind => SkillKind.Damage;

        public DamageApplier(IStaticDataService staticDataService, IHeroRegistry heroRegistry,
            IBattleTextPlayer battleTextPlayer)
        {
            _staticDataService = staticDataService;
            _heroRegistry = heroRegistry;
            _battleTextPlayer = battleTextPlayer;
        }

        public void WarmUp()
        {
        }

        public void ApplySkill(ActiveSkill activeSkill)
        {
            foreach (var targetId in activeSkill.TargetIds)
            {
                if (!_heroRegistry.IsAlive(targetId))
                    continue;

                var caster = _heroRegistry.GetHero(activeSkill.CasterId);
                var skill = _staticDataService.HeroSkillFor(activeSkill.Skill, caster.TypeId);

                var target = _heroRegistry.GetHero(targetId);
                target.State.CurrentHp -= skill.Value;

                _battleTextPlayer.PlayText($"{skill.Value}", Color.red, target.transform.position);
                PlayFx(skill.CustomTargetFx, target.transform.position);
            }
        }

        private void PlayFx(GameObject fxPrefab, Vector3 position)
        {
            if (fxPrefab)
                Object.Instantiate(fxPrefab, position, Quaternion.identity);
        }
    }
}
using CodeBase.Gameplay.Battle;

namespace CodeBase.MetricsCollector
{
    public interface IMetricsService
    {
        void HeroPerformedAction(HeroAction action);
        void HeroDied(string heroId);
        void BattleFinished();
        void BattleStarted();
    }
}
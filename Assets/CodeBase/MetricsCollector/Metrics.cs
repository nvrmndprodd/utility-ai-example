using System.Collections.Generic;
using System.Text;
using CodeBase.Gameplay.Battle;

namespace CodeBase.MetricsCollector
{
    public class Metrics
    {
        public int fightsWon;

        public float avgDamage;
        public List<float> damagePerTurn = new();
        public float avgHeal;
        public List<float> healPerTurn = new();
        public float avgBurn;
        public List<float> burnPerTurn = new();

        public float avgHeroesDied;
        public int heroesDiedInFight;

        public float avgActionsPerFight;
        public Dictionary<string, int> actionInFight = new();

        public float avgActionsToWin;
        
        public float avgActionsToDie;
        public int avgActionsToDieCounter;

        public float avgActionsPerAgent;

        public void Clear()
        {
            damagePerTurn.Clear();
            healPerTurn.Clear();
            burnPerTurn.Clear();
            actionInFight.Clear();

            heroesDiedInFight = 0;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Winrate: {(float) fightsWon / BattleConductor.FIGHTS}");
            
            sb.AppendLine($"Average damage per fight: {avgDamage}");
            sb.AppendLine($"Average heal per fight: {avgHeal}");
            sb.AppendLine($"Average burn per fight: {avgBurn}");

            sb.AppendLine($"Average heroes died: {avgHeroesDied}");
            sb.AppendLine($"Average actions per fight: {avgActionsPerFight}");
            sb.AppendLine($"Average actions to win: {avgActionsToWin}");
            sb.AppendLine($"Average actions to die: {avgActionsToDie}");
            sb.AppendLine($"Average actions per agent: {avgActionsPerAgent}");

            return sb.ToString();
        }
    }
}
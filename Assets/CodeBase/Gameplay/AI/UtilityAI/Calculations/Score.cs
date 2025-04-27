using System;
using CodeBase.Gameplay.Heroes;

namespace CodeBase.Gameplay.AI.UtilityAI.Calculations
{
    public static class Score
    {
        public static float AsIs(float input, IHero hero)
        {
            return input;
        }
        
        public static float IfTrueThen(float input, IHero hero)
        {
            return input;
        }

        public static Func<float, IHero, float> IfTrueThen(int ifTrue)
        {
            return (input, _) => input * ifTrue;
        }

        public static Func<float, IHero, float> ScaleBy(int scale)
        {
            return (input, _) => input * scale;
        }

        public static float CullByTargetHp(float healPercentage, IHero target)
        {
            if (target.State.HpPercentage >= 0.7f)
            {
                return -30;
            }

            return 100 * (healPercentage + 3 * (0.7f - target.State.HpPercentage));
        }

        public static Func<float, IHero, float> CullByTargetInitiative(float scaleBy, float cullThreshold)
        {
            return (input, target) => target.State.InitiativePercentage > cullThreshold 
                ? input * scaleBy 
                : 0;
        }

        public static Func<float, IHero, float> NegativeCorrelationScaleBy(float scale)
        {
            return (input, _) => (1 - input) * scale;
        } 
    }
}
using System.Collections.Generic;
using CodeBase.Gameplay.AI.UtilityAI.Calculations;

namespace CodeBase.Gameplay.AI.UtilityAI
{
    public class Brains
    {
        private Convolutions _convolutions = new Convolutions()
        {
            { "Basic Damage", When.SkillIsDamage, GetInput.PercentageDamage, Score.ScaleBy(100) },
            { "Killing Blow", When.SkillIsDamage, GetInput.KillingBlow, Score.IfTrueThen(+150) },
            { "Killing Blow with Basic Attack", When.SkillIsBasicAttack, GetInput.KillingBlow, Score.IfTrueThen(+30) },
            { "Focus Damage", When.SkillIsDamage, GetInput.PercentageHp, Score.NegativeCorrelationScaleBy(50) },
            
            { "Heal", When.SkillIsHeal, GetInput.PercentageHeal, Score.CullByTargetHp },
            
            { "Initiative Burn", When.SkillIsInitiativeBurn, GetInput.PercentageInitiativeBurn, Score.CullByTargetInitiative(scaleBy: 50f, cullThreshold: 0.25f) },
            { "Initiative Burn (Ultimate Is Ready)", When.SkillIsInitiativeBurn, GetInput.TargetUltimateIsReady, Score.IfTrueThen(+30) },
        };
        
        public IEnumerable<IUtilityFunction> GetUtilityFunctions()
        {
            return _convolutions;
        }
    }
}
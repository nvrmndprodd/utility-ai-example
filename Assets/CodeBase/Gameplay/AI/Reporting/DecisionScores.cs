﻿using System.Collections.Generic;

using CodeBase.Gameplay.AI.UtilityAI;

namespace CodeBase.Gameplay.AI.Reporting
{
    public class DecisionScores
    {
        public string HeroName;

        public string FormattedLine;

        public List<ScoredAction> Choices;
    }
}
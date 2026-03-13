using System;
using System.Collections.Generic;
using Speed.Domain;

namespace Speed.Application
{
    public sealed class CpuDecisionService
    {
        private readonly RuleService ruleService;

        public CpuDecisionService(RuleService ruleService)
        {
            this.ruleService = ruleService;
        }

        public CpuDecision Decide(PlayerState cpu, GameState gameState, CpuDifficultySettings settings, Random random)
        {
            var options = new List<CpuDecision>();

            foreach (var card in cpu.Hand.Cards)
            {
                if (ruleService.CanPlace(card, gameState.LeftPile))
                {
                    options.Add(new CpuDecision(card, PileId.Left, true));
                }

                if (ruleService.CanPlace(card, gameState.RightPile))
                {
                    options.Add(new CpuDecision(card, PileId.Right, true));
                }
            }

            if (options.Count == 0)
            {
                return CpuDecision.Pass;
            }

            if (random.NextDouble() < settings.MistakeRate)
            {
                return CpuDecision.Pass;
            }

            return options[random.Next(options.Count)];
        }
    }
}

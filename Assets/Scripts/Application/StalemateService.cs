using Speed.Domain;

namespace Speed.Application
{
    public sealed class StalemateService
    {
        private readonly RuleService ruleService;

        public StalemateService(RuleService ruleService)
        {
            this.ruleService = ruleService;
        }

        public bool HasPlayableCard(PlayerState player, GameState gameState)
        {
            foreach (var card in player.Hand.Cards)
            {
                if (ruleService.CanPlace(card, gameState.LeftPile) || ruleService.CanPlace(card, gameState.RightPile))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

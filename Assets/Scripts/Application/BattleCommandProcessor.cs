using Speed.Domain;

namespace Speed.Application
{
    public sealed class BattleCommandProcessor
    {
        private readonly RuleService ruleService;

        public BattleCommandProcessor(RuleService ruleService)
        {
            this.ruleService = ruleService;
        }

        public PutCardResult TryPutCard(PlayerState player, TablePile pile, int cardId, out Card playedCard)
        {
            playedCard = null;

            if (!player.Hand.TryGet(cardId, out var card))
            {
                return PutCardResult.CardNotInHand;
            }

            if (pile.IsPlayingPutCardAnimation)
            {
                return PutCardResult.BlockedByAnimation;
            }

            if (!ruleService.CanPlace(card, pile))
            {
                return PutCardResult.InvalidRule;
            }

            player.Hand.TryRemove(cardId, out playedCard);
            pile.SetTopCard(playedCard);
            pile.IsPlayingPutCardAnimation = true;
            return PutCardResult.Success;
        }
    }
}

using Speed.Domain;

namespace Speed.Application
{
    public sealed class RuleService
    {
        public bool CanPlace(Card card, TablePile pile)
        {
            if (card == null || pile == null || pile.TopCard == null)
            {
                return false;
            }

            var cardValue = (int)card.Rank;
            var topValue = (int)pile.TopCard.Rank;
            var difference = System.Math.Abs(cardValue - topValue);
            return difference == 1 || difference == 12;
        }
    }
}

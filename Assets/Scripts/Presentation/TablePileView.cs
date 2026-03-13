using Speed.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace Speed.Presentation
{
    [RequireComponent(typeof(Image))]
    public sealed class TablePileView : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private Image background;

        public void SetCard(Card card)
        {
            label.text = card == null ? "--" : GetRankLabel(card.Rank);
        }

        public void SetPreviewCard(Card card)
        {
            if (card != null)
            {
                label.text = GetRankLabel(card.Rank);
            }
        }

        public void SetBusy(bool busy)
        {
            background.color = busy ? new Color(1f, 0.87f, 0.55f, 1f) : Color.white;
        }

        private static string GetRankLabel(Rank rank)
        {
            return rank switch
            {
                Rank.A => "A",
                Rank.J => "J",
                Rank.Q => "Q",
                Rank.K => "K",
                _ => ((int)rank).ToString()
            };
        }
    }
}

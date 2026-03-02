using System;
using System.Collections.Generic;
using SpeedGame.Domain;

namespace SpeedGame.Core
{
    public sealed class SpeedGameModel
    {
        private readonly SpeedRuleSettings _rules;

        private readonly List<CardData> _playerHand = new();
        private readonly List<CardData> _opponentHand = new();
        private readonly Stack<CardData> _playerStock = new();
        private readonly Stack<CardData> _opponentStock = new();
        private readonly CardData[] _tableTop = new CardData[2];

        private readonly Random _random = new();

        public SpeedGameModel(SpeedRuleSettings rules)
        {
            _rules = rules;
        }

        public IReadOnlyList<CardData> PlayerHand => _playerHand;
        public IReadOnlyList<CardData> OpponentHand => _opponentHand;
        public int PlayerStockCount => _playerStock.Count;
        public int OpponentStockCount => _opponentStock.Count;
        public CardData LeftTop => _tableTop[(int)PileLane.Left];
        public CardData RightTop => _tableTop[(int)PileLane.Right];

        public void Setup()
        {
            _playerHand.Clear();
            _opponentHand.Clear();
            _playerStock.Clear();
            _opponentStock.Clear();
            _tableTop[(int)PileLane.Left] = null;
            _tableTop[(int)PileLane.Right] = null;

            var deck = CreateDeck();
            Shuffle(deck);

            var playerPool = new List<CardData>();
            var opponentPool = new List<CardData>();

            for (var i = 0; i < deck.Count; i++)
            {
                if ((i & 1) == 0)
                {
                    playerPool.Add(deck[i]);
                }
                else
                {
                    opponentPool.Add(deck[i]);
                }
            }

            DealInitial(playerPool, _playerHand, _playerStock);
            DealInitial(opponentPool, _opponentHand, _opponentStock);

            _tableTop[(int)PileLane.Left] = PopOrNull(_playerStock);
            _tableTop[(int)PileLane.Right] = PopOrNull(_opponentStock);
        }

        public bool CanApplyCommand(PlayerCommand command)
        {
            if (command.IsDrawRequest)
            {
                return GetStock(command.Side).Count > 0 && GetHand(command.Side).Count < 5;
            }

            var hand = GetHand(command.Side);
            if (command.HandIndex < 0 || command.HandIndex >= hand.Count)
            {
                return false;
            }

            return CanPlace(hand[command.HandIndex], _tableTop[(int)command.Lane]);
        }

        public bool TryApplyCommand(PlayerCommand command, out CardData movedCard)
        {
            movedCard = null;

            if (command.IsDrawRequest)
            {
                if (!CanApplyCommand(command))
                {
                    return false;
                }

                RefillToFive(command.Side);
                return true;
            }

            var hand = GetHand(command.Side);
            if (command.HandIndex < 0 || command.HandIndex >= hand.Count)
            {
                return false;
            }

            var card = hand[command.HandIndex];
            var laneTop = _tableTop[(int)command.Lane];
            if (!CanPlace(card, laneTop))
            {
                return false;
            }

            if (card.IsJoker)
            {
                card.LockedRank = DecideJokerRank(command.Side, command.Lane, card.CardId);
            }

            hand.RemoveAt(command.HandIndex);
            _tableTop[(int)command.Lane] = card;
            movedCard = card;
            RefillToFive(command.Side);
            return true;
        }

        public bool CanAnyMove(PlayerSide side)
        {
            var hand = GetHand(side);
            for (var i = 0; i < hand.Count; i++)
            {
                if (CanPlace(hand[i], _tableTop[(int)PileLane.Left]) || CanPlace(hand[i], _tableTop[(int)PileLane.Right]))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsWin(PlayerSide side)
        {
            var hand = GetHand(side);
            var stock = GetStock(side);

            if (_rules.WinByHandOnly)
            {
                return hand.Count == 0;
            }

            return hand.Count == 0 && stock.Count == 0;
        }

        public bool ResetStuckLanes()
        {
            var changed = false;

            changed |= TryResetLane(PileLane.Left, PlayerSide.Player);
            changed |= TryResetLane(PileLane.Right, PlayerSide.Opponent);

            return changed;
        }

        private bool TryResetLane(PileLane lane, PlayerSide preferredSide)
        {
            var stock = GetStock(preferredSide);
            if (stock.Count > 0)
            {
                _tableTop[(int)lane] = stock.Pop();
                return true;
            }

            var hand = GetHand(preferredSide);
            if (hand.Count == 0)
            {
                return false;
            }

            var card = hand[0];
            hand.RemoveAt(0);
            _tableTop[(int)lane] = card;
            return true;
        }

        private int DecideJokerRank(PlayerSide side, PileLane lane, int jokerCardId)
        {
            var hand = GetHand(side);
            var left = lane == PileLane.Left ? null : _tableTop[(int)PileLane.Left];
            var right = lane == PileLane.Right ? null : _tableTop[(int)PileLane.Right];

            var bestRank = 1;
            var bestScore = int.MinValue;

            for (var rank = 1; rank <= 13; rank++)
            {
                var score = 0;

                foreach (var card in hand)
                {
                    if (card.CardId == jokerCardId)
                    {
                        continue;
                    }

                    if (CanPlaceWithRank(card, left, rank))
                    {
                        score += 2;
                    }

                    if (CanPlaceWithRank(card, right, rank))
                    {
                        score += 2;
                    }
                }

                if (IsAdjacentRank(rank, left?.EffectiveRank ?? -1))
                {
                    score++;
                }

                if (IsAdjacentRank(rank, right?.EffectiveRank ?? -1))
                {
                    score++;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestRank = rank;
                }
            }

            return bestRank;
        }

        private bool CanPlaceWithRank(CardData card, CardData laneTop, int jokerRank)
        {
            if (laneTop == null)
            {
                return true;
            }

            var sourceRank = card.IsJoker ? jokerRank : card.EffectiveRank;
            return CanRanksConnect(sourceRank, laneTop.EffectiveRank);
        }

        private bool CanPlace(CardData card, CardData laneTop)
        {
            if (laneTop == null)
            {
                return true;
            }

            var sourceRank = card.EffectiveRank;
            var targetRank = laneTop.EffectiveRank;
            return CanRanksConnect(sourceRank, targetRank);
        }

        private bool CanRanksConnect(int sourceRank, int targetRank)
        {
            if (_rules.AllowSameRank && sourceRank == targetRank)
            {
                return true;
            }

            if (!_rules.AllowAdjacent)
            {
                return false;
            }

            return IsAdjacentRank(sourceRank, targetRank);
        }

        private bool IsAdjacentRank(int sourceRank, int targetRank)
        {
            if (sourceRank <= 0 || targetRank <= 0)
            {
                return false;
            }

            var diff = Math.Abs(sourceRank - targetRank);
            if (diff == 1)
            {
                return true;
            }

            if (_rules.AllowAceKingWrap)
            {
                return (sourceRank == 1 && targetRank == 13) || (sourceRank == 13 && targetRank == 1);
            }

            return false;
        }

        private void RefillToFive(PlayerSide side)
        {
            var hand = GetHand(side);
            var stock = GetStock(side);

            while (hand.Count < 5 && stock.Count > 0)
            {
                hand.Add(stock.Pop());
            }
        }

        private List<CardData> GetHand(PlayerSide side)
        {
            return side == PlayerSide.Player ? _playerHand : _opponentHand;
        }

        private Stack<CardData> GetStock(PlayerSide side)
        {
            return side == PlayerSide.Player ? _playerStock : _opponentStock;
        }

        private static CardData PopOrNull(Stack<CardData> stack)
        {
            return stack.Count > 0 ? stack.Pop() : null;
        }

        private static void DealInitial(List<CardData> source, List<CardData> hand, Stack<CardData> stock)
        {
            for (var i = 0; i < 5 && source.Count > 0; i++)
            {
                hand.Add(source[0]);
                source.RemoveAt(0);
            }

            for (var i = source.Count - 1; i >= 0; i--)
            {
                stock.Push(source[i]);
            }
        }

        private static List<CardData> CreateDeck()
        {
            var list = new List<CardData>(54);
            var id = 0;

            foreach (CardSuit suit in new[] { CardSuit.Spade, CardSuit.Heart, CardSuit.Diamond, CardSuit.Club })
            {
                for (var rank = 1; rank <= 13; rank++)
                {
                    list.Add(new CardData
                    {
                        CardId = id++,
                        Rank = rank,
                        Suit = suit,
                        IsJoker = false,
                        LockedRank = null
                    });
                }
            }

            for (var j = 0; j < 2; j++)
            {
                list.Add(new CardData
                {
                    CardId = id++,
                    Rank = 0,
                    Suit = CardSuit.Joker,
                    IsJoker = true,
                    LockedRank = null
                });
            }

            return list;
        }

        private void Shuffle(IList<CardData> cards)
        {
            for (var i = cards.Count - 1; i > 0; i--)
            {
                var j = _random.Next(i + 1);
                (cards[i], cards[j]) = (cards[j], cards[i]);
            }
        }
    }
}

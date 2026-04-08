using System;
using System.Collections.Generic;
using UnityEngine;
using Speed.Domain;

namespace Speed.View
{
    [Serializable]
    public class CardPrefabEntry
    {
        public Suit       Suit;
        public int        Rank;
        public GameObject Prefab;
    }

    [CreateAssetMenu(fileName = "CardPrefabRegistry", menuName = "Speed/CardPrefabRegistry")]
    public class CardPrefabRegistry : ScriptableObject
    {
        public CardPrefabEntry[] Entries;

        private Dictionary<string, GameObject> _cache;

        public GameObject GetPrefab(Card card)
        {
            if (_cache == null) BuildCache();
            _cache.TryGetValue(Key(card.Suit, card.Rank), out var prefab);
            return prefab;
        }

        private void BuildCache()
        {
            _cache = new Dictionary<string, GameObject>();
            if (Entries == null) return;
            foreach (var e in Entries)
                if (e?.Prefab != null)
                    _cache[Key(e.Suit, e.Rank)] = e.Prefab;
        }

        private static string Key(Suit suit, int rank) => $"{suit}_{rank}";

        private void OnEnable() => _cache = null;
    }
}

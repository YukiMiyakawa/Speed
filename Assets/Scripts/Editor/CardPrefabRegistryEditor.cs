#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Speed.Domain;
using Speed.View;

namespace Speed.Editor
{
    [CustomEditor(typeof(CardPrefabRegistry))]
    public class CardPrefabRegistryEditor : UnityEditor.Editor
    {
        private const string DeckPath = "Assets/ExternalResource/Asset_PlayingCards/Prefabs/Deck01";

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(8);
            if (GUILayout.Button("Auto-Fill From Deck01 (52 cards)"))
                AutoFill();
        }

        private void AutoFill()
        {
            var registry = (CardPrefabRegistry)target;
            var entries  = new List<CardPrefabEntry>();
            int found    = 0;

            foreach (Suit suit in System.Enum.GetValues(typeof(Suit)))
            {
                for (int rank = 1; rank <= 13; rank++)
                {
                    string rankStr;
                    switch (rank)
                    {
                        case 1:  rankStr = "A";  break;
                        case 11: rankStr = "J";  break;
                        case 12: rankStr = "Q";  break;
                        case 13: rankStr = "K";  break;
                        default: rankStr = rank.ToString(); break;
                    }

                    string path   = $"{DeckPath}/Deck01_{suit}_{rankStr}.prefab";
                    var    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

                    if (prefab != null) found++;
                    else Debug.LogWarning($"[CardPrefabRegistry] Not found: {path}");

                    entries.Add(new CardPrefabEntry { Suit = suit, Rank = rank, Prefab = prefab });
                }
            }

            registry.Entries = entries.ToArray();
            EditorUtility.SetDirty(registry);
            AssetDatabase.SaveAssets();
            Debug.Log($"[CardPrefabRegistry] Auto-filled: {found}/52 prefabs found.");
        }
    }
}
#endif

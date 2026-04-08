using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using DG.Tweening;
using Speed.Domain;
using Speed.Controllers;

namespace Speed.View
{
    public class BattleView : MonoBehaviour
    {
        [Header("References")]
        public GameController    GameController;
        public BattleLayout      Layout;
        public CardPrefabRegistry CardRegistry;
        public ResultDialogView  ResultDialog;

        [Header("Deck UI")]
        public TextMeshProUGUI PlayerDeckCountText;
        public TextMeshProUGUI CpuDeckCountText;
        public GameObject      PlayerDeckVisual;
        public GameObject      CpuDeckVisual;

        [Header("Animation Timing (seconds)")]
        public float CardPlayDuration  = 0.22f;
        public float BounceBackDuration = 0.28f;
        public float FlipDuration      = 0.18f;

        // ---- Card view lists ----
        private readonly List<CardView> _playerHand = new List<CardView>();
        private readonly List<CardView> _cpuHand    = new List<CardView>();
        private CardView _leftPileView;
        private CardView _rightPileView;

        private Transform _cardRoot;

        // ---- Foul indicator ----
        private GameObject   _foulIndicator;
        private TextMeshPro  _foulText;
        private Coroutine    _foulCoroutine;

        // ---- Awake: subscribe early so we catch OnDealComplete from GameController.Start ----
        private void Awake()
        {
            var go = new GameObject("CardRoot");
            _cardRoot = go.transform;

            // Foul indicator (world-space 3D text)
            _foulIndicator = new GameObject("PlayerFoulIndicator");
            _foulIndicator.transform.position = new Vector3(0f, -2.8f, 0f);
            _foulText = _foulIndicator.AddComponent<TextMeshPro>();
            _foulText.fontSize        = 5f;
            _foulText.fontStyle       = FontStyles.Bold;
            _foulText.alignment       = TextAlignmentOptions.Center;
            _foulText.color           = Color.red;
            _foulText.sortingOrder    = 60;
            _foulIndicator.SetActive(false);

            GameController.OnDealComplete      += OnDealComplete;
            GameController.OnCardPlayed        += OnCardPlayed;
            GameController.OnCardPlayFailed    += OnCardPlayFailed;
            GameController.OnFalsePlayAttempt  += OnFalsePlayAttempt;
            GameController.OnFlipStart         += OnFlipStart;
            GameController.OnFlipComplete      += OnFlipComplete;
            GameController.OnGameOver          += OnGameOver;
            GameController.OnFoulStarted       += OnFoulStarted;
            GameController.OnFoulExpired       += OnFoulExpired;
        }

        private void Start()
        {
            ResultDialog.OnRematch += () =>
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            ResultDialog.OnTitle += () =>
                SceneManager.LoadScene("TitleScene");
        }

        private void OnDestroy()
        {
            if (GameController == null) return;
            GameController.OnDealComplete     -= OnDealComplete;
            GameController.OnCardPlayed       -= OnCardPlayed;
            GameController.OnCardPlayFailed   -= OnCardPlayFailed;
            GameController.OnFalsePlayAttempt -= OnFalsePlayAttempt;
            GameController.OnFlipStart        -= OnFlipStart;
            GameController.OnFlipComplete     -= OnFlipComplete;
            GameController.OnGameOver         -= OnGameOver;
            GameController.OnFoulStarted      -= OnFoulStarted;
            GameController.OnFoulExpired      -= OnFoulExpired;
        }

        // ===================================================================
        //  Deal
        // ===================================================================
        private void OnDealComplete(GameState state)
        {
            CreateInitialCards(state);
            RefreshDeckCounts(state);
        }

        private void CreateInitialCards(GameState state)
        {
            for (int i = 0; i < state.PlayerHand.Count; i++)
            {
                var cv = SpawnCardView(state.PlayerHand[i], true);
                cv.SetHomePosition(Layout.GetPlayerHandPosition(i, state.PlayerHand.Count));
                _playerHand.Add(cv);
            }
            for (int i = 0; i < state.CpuHand.Count; i++)
            {
                var cv = SpawnCardView(state.CpuHand[i], false);
                cv.SetHomePosition(Layout.GetCpuHandPosition(i, state.CpuHand.Count));
                _cpuHand.Add(cv);
            }

            _leftPileView  = SpawnCardView(state.LeftPileTop,  true);
            _leftPileView.SetHomePosition(Layout.LeftPileAnchor.position);

            _rightPileView = SpawnCardView(state.RightPileTop, true);
            _rightPileView.SetHomePosition(Layout.RightPileAnchor.position);
        }

        // ===================================================================
        //  Card Play
        // ===================================================================
        private void OnCardPlayed(PlayerId player, int handIndex, PileId pileId)
        {
            if (player == PlayerId.Player)
                StartCoroutine(AnimateCardPlayed(_playerHand, handIndex, pileId, true));
            else
                StartCoroutine(AnimateCardPlayed(_cpuHand, handIndex, pileId, false));
        }

        private IEnumerator AnimateCardPlayed(List<CardView> hand, int handIndex, PileId pileId, bool isPlayer)
        {
            if (handIndex >= hand.Count) yield break;

            var cv      = hand[handIndex];
            hand.RemoveAt(handIndex);

            var pilePos = GetPileWorldPosition(pileId);
            cv.SetSortingOrder(20);

            bool done = false;
            cv.MoveTo(pilePos, CardPlayDuration).OnComplete(() =>
            {
                RefreshPileView(pileId);
                GameController.NotifyPileAnimationComplete(pileId);
                Destroy(cv.gameObject);
                done = true;
            });

            yield return new WaitUntil(() => done);
            yield return RepositionHand(isPlayer);
        }

        private IEnumerator RepositionHand(bool isPlayer)
        {
            var hand   = isPlayer ? _playerHand : _cpuHand;
            int total  = hand.Count;
            for (int i = 0; i < total; i++)
            {
                var pos = isPlayer
                    ? Layout.GetPlayerHandPosition(i, total)
                    : Layout.GetCpuHandPosition(i, total);
                hand[i].SetHomePosition(pos);
                hand[i].MoveTo(pos, 0.15f);
            }
            yield return new WaitForSeconds(0.15f);
            RefreshDeckCounts(GameController.State);
        }

        // ===================================================================
        //  Invalid play / false miss
        // ===================================================================
        private void OnCardPlayFailed(PlayerId player, int handIndex, PileId pileId)
        {
            // Player bounce-back is handled by BattleInputController via AnimateCardBounceBack
        }

        private void OnFalsePlayAttempt(PlayerId player, int handIndex, PileId pileId)
        {
            if (player != PlayerId.Cpu || handIndex >= _cpuHand.Count) return;
            var cv      = _cpuHand[handIndex];
            var pilePos = GetPileWorldPosition(pileId);
            cv.FalsePlayAnimation(pilePos, 0.5f);
        }

        public void AnimateCardBounceBack(CardView cv)
        {
            if (cv != null) cv.BounceBack(BounceBackDuration);
        }

        // ===================================================================
        //  Flip (stalemate)
        // ===================================================================
        private void OnFlipStart()
        {
            StartCoroutine(FlipAnimation());
        }

        private IEnumerator FlipAnimation()
        {
            GameController.ExecuteFlip();

            bool leftDone  = _leftPileView  == null;
            bool rightDone = _rightPileView == null;

            if (_leftPileView != null)
            {
                var bump = Layout.LeftPileAnchor.position + Vector3.up * 0.3f;
                _leftPileView.MoveTo(bump, FlipDuration * 0.5f).OnComplete(() =>
                {
                    RefreshPileView(PileId.Left);
                    _leftPileView?.MoveTo(Layout.LeftPileAnchor.position, FlipDuration * 0.5f)
                        .OnComplete(() => leftDone = true);
                });
            }
            if (_rightPileView != null)
            {
                var bump = Layout.RightPileAnchor.position + Vector3.up * 0.3f;
                _rightPileView.MoveTo(bump, FlipDuration * 0.5f).OnComplete(() =>
                {
                    RefreshPileView(PileId.Right);
                    _rightPileView?.MoveTo(Layout.RightPileAnchor.position, FlipDuration * 0.5f)
                        .OnComplete(() => rightDone = true);
                });
            }

            yield return new WaitUntil(() => leftDone && rightDone);

            RefreshDeckCounts(GameController.State);
            GameController.NotifyFlipAnimationComplete();
        }

        private void OnFlipComplete() { /* Phase already updated in GameController */ }

        // ===================================================================
        //  Foul
        // ===================================================================
        private void OnFoulStarted(PlayerId player)
        {
            if (player != PlayerId.Player) return;
            if (_foulCoroutine != null) StopCoroutine(_foulCoroutine);
            _foulCoroutine = StartCoroutine(FoulCountdown(GameController.FoulDuration));
        }

        private void OnFoulExpired(PlayerId player)
        {
            if (player != PlayerId.Player) return;
            if (_foulCoroutine != null) { StopCoroutine(_foulCoroutine); _foulCoroutine = null; }
            _foulIndicator.SetActive(false);
        }

        private IEnumerator FoulCountdown(float duration)
        {
            _foulIndicator.SetActive(true);
            float remaining = duration;
            while (remaining > 0f)
            {
                _foulText.text = $"お手付き!\n{remaining:F1}s";
                yield return null;
                remaining -= Time.deltaTime;
            }
            _foulIndicator.SetActive(false);
            _foulCoroutine = null;
        }

        // ===================================================================
        //  Game Over
        // ===================================================================
        private void OnGameOver(BattleResult result) => ResultDialog.Show(result);

        // ===================================================================
        //  Helpers
        // ===================================================================
        public int GetPlayerHandIndex(CardView cv) => _playerHand.IndexOf(cv);

        public Vector3 GetPileWorldPosition(PileId pile) =>
            pile == PileId.Left ? Layout.LeftPileAnchor.position : Layout.RightPileAnchor.position;

        private void RefreshPileView(PileId pileId)
        {
            var topCard  = pileId == PileId.Left ? GameController.State.LeftPileTop  : GameController.State.RightPileTop;
            var pileView = pileId == PileId.Left ? _leftPileView : _rightPileView;
            if (pileView == null || topCard == null) return;

            // Replace prefab child
            for (int i = pileView.transform.childCount - 1; i >= 0; i--)
                Destroy(pileView.transform.GetChild(i).gameObject);

            var prefab = CardRegistry != null ? CardRegistry.GetPrefab(topCard) : null;
            if (prefab != null)
            {
                var inst = Instantiate(prefab, pileView.transform);
                inst.transform.localPosition = Vector3.zero;
                pileView.Setup(topCard, inst);
            }
            pileView.SetFaceUp(true);
        }

        private void RefreshDeckCounts(GameState state)
        {
            if (PlayerDeckCountText != null)
                PlayerDeckCountText.text = state.PlayerDeck.Count.ToString();
            if (CpuDeckCountText != null)
                CpuDeckCountText.text = state.CpuDeck.Count.ToString();

            if (PlayerDeckVisual != null)
                PlayerDeckVisual.SetActive(state.PlayerDeck.Count > 0);
            if (CpuDeckVisual != null)
                CpuDeckVisual.SetActive(state.CpuDeck.Count > 0);
        }

        // ===================================================================
        //  Card spawn
        // ===================================================================
        private CardView SpawnCardView(Card card, bool faceUp)
        {
            if (card == null) return null;

            var wrapper = new GameObject($"CV_{card}");
            wrapper.transform.SetParent(_cardRoot);
            var cv = wrapper.AddComponent<CardView>();

            var prefab = CardRegistry != null ? CardRegistry.GetPrefab(card) : null;
            if (prefab != null)
            {
                var inst = Instantiate(prefab, wrapper.transform);
                inst.transform.localPosition = Vector3.zero;
                cv.Setup(card, inst);
            }

            cv.SetFaceUp(faceUp);

            var col = wrapper.AddComponent<BoxCollider2D>();
            col.size = new Vector2(2.0f, 2.5f); // matches card prefab sprite size

            return cv;
        }
    }
}

using System.Collections;
using UnityEngine;
using Speed.Domain;
using Speed.Application;

namespace Speed.Controllers
{
    public class CpuController : MonoBehaviour
    {
        [Header("References")]
        public Speed.View.BattleView BattleView;

        private bool      _thinking;
        private Coroutine _loop;

        private GameController GameController => _gc != null ? _gc : (_gc = GetComponent<GameController>());
        private GameController _gc;

        public void StartThinking()
        {
            _thinking = true;
            if (_loop != null) StopCoroutine(_loop);
            _loop = StartCoroutine(ThinkLoop());
        }

        public void StopThinking()
        {
            _thinking = false;
            if (_loop != null) { StopCoroutine(_loop); _loop = null; }
        }

        private IEnumerator ThinkLoop()
        {
            while (_thinking)
            {
                var settings = GameController.GetCpuSettings();
                yield return new WaitForSeconds(settings.ReactionTimeMs / 1000f);

                if (!_thinking || GameController.Phase != BattlePhase.Playing) continue;

                var decision = CpuDecisionService.Decide(GameController.State, settings);
                yield return StartCoroutine(ExecuteDecision(decision));
            }
        }

        private IEnumerator ExecuteDecision(CpuDecision decision)
        {
            switch (decision.Type)
            {
                case CpuDecisionType.LookAheadMiss:
                    // deliberately do nothing this cycle
                    break;

                case CpuDecisionType.FalseMiss:
                    GameController.NotifyCpuFalsePlay(decision.HandIndex, decision.TargetPile);
                    yield return new WaitForSeconds(0.55f); // wait for false-play animation
                    break;

                case CpuDecisionType.PlayCard:
                    var result = GameController.TryCpuPutCard(decision.HandIndex, decision.TargetPile);
                    if (result.IsSuccess)
                        yield return new WaitForSeconds(0.25f);
                    break;
            }
        }
    }
}

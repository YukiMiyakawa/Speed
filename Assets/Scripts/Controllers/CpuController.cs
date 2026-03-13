using Speed.Application;
using UnityEngine;

namespace Speed.Controllers
{
    public sealed class CpuController : MonoBehaviour
    {
        private const float RetryInterval = 0.08f;

        private GameController gameController;
        private CpuDecisionService decisionService;
        private System.Random random;
        private float thinkTimer;

        public void Initialize(GameController controller, CpuDecisionService service, System.Random sharedRandom)
        {
            gameController = controller;
            decisionService = service;
            random = sharedRandom;
            thinkTimer = controller.CpuDifficultySettings.ReactionSeconds;
        }

        public void Tick(float deltaTime)
        {
            if (gameController == null || !gameController.CanAcceptInput())
            {
                return;
            }

            thinkTimer -= deltaTime;
            if (thinkTimer > 0f)
            {
                return;
            }

            var decision = decisionService.Decide(
                gameController.State.Cpu,
                gameController.State,
                gameController.CpuDifficultySettings,
                random);

            if (decision.ShouldPlay)
            {
                gameController.TryCpuPut(decision);
                thinkTimer = gameController.CpuDifficultySettings.ReactionSeconds;
                return;
            }

            thinkTimer = RetryInterval;
        }
    }
}

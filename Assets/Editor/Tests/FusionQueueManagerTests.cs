using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Game.Manager;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using Cysharp.Threading.Tasks;

namespace Tests
{
    public class FusionQueueManagerTests
    {
        [UnityTest]
        public IEnumerator TestSimulationFlow() => UniTask.ToCoroutine(async () =>
        {
            // 1. Setup
            var go = new GameObject("FusionQueueManager");
            var manager = go.AddComponent<FusionQueueManager>();

            bool completed = false;
            bool pausedOnce = false;
            int successCount = 0;

            manager.OnQueueComplete += () => completed = true;
            manager.OnStepComplete += (step) => successCount++;

            // 2. Auto-Skip on Failure to simulate User Action
            manager.OnQueuePaused += (step, error) =>
            {
                Debug.Log($"[Test] Queue Paused at step {step.step_id}. Simulating Skip...");
                pausedOnce = true;
                // Add a small delay to simulate user reaction time
                UniTask.Void(async () => {
                    await UniTask.Delay(100);
                    manager.SkipCurrentStep();
                });
            };

            // 3. Run Simulation
            manager.RunSimulation();

            // 4. Wait for completion (Timeout after 5 seconds)
            float timeout = 5f;
            float timer = 0f;
            while (!completed && timer < timeout)
            {
                await UniTask.Delay(100);
                timer += 0.1f;
            }

            // 5. Assertions
            Assert.IsTrue(completed, "Queue should complete.");
            Assert.IsTrue(pausedOnce, "Queue should have paused once (Item 2 failure).");
            // Expecting 3 successes (1, 3, 4) and 1 skipped (2)
            // Or maybe 3 successes if logic is correct.
            // Step 1: Success
            // Step 2: Failed -> Skipped
            // Step 3: Success
            // Step 4: Success
            // So 3 successes.
            Assert.AreEqual(3, successCount, "Should have 3 successful items.");

            Object.DestroyImmediate(go);
        });
    }
}

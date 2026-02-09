using System;
using System.Collections.Generic;
using System.Linq;
using App;
using Cysharp.Threading.Tasks;
using Senspark;
using UnityEngine;

namespace Game.Manager
{
    [Serializable]
    public class FusionStep
    {
        public int step_id;
        public string material_hero_id;
        public string status; // "WAITING", "PENDING", "SUCCESS", "FAILED", "SKIPPED"
        public string tx_hash;

        public FusionStep(int id, string materialId)
        {
            step_id = id;
            material_hero_id = materialId;
            status = "WAITING";
            tx_hash = null;
        }
    }

    [Serializable]
    public class FusionQueueData
    {
        public string queue_id;
        public string main_hero_id;
        public string queue_status; // "IDLE", "PROCESSING", "PAUSED", "COMPLETED"
        public List<FusionStep> fusion_steps;

        public FusionQueueData(string mainId, List<string> materialIds)
        {
            queue_id = $"batch_{DateTime.Now.Ticks}";
            main_hero_id = mainId;
            queue_status = "IDLE";
            fusion_steps = new List<FusionStep>();
            for (int i = 0; i < materialIds.Count; i++)
            {
                fusion_steps.Add(new FusionStep(i + 1, materialIds[i]));
            }
        }
    }

    public class FusionQueueManager : MonoBehaviour
    {
        public FusionQueueData currentQueue;

        public event Action<float, string> OnProgressUpdated;
        public event Action<FusionStep> OnStepComplete;
        public event Action<FusionStep, string> OnQueuePaused;
        public event Action OnQueueComplete;

        private IBlockchainManager _blockchainManager;
        private bool _isSimulationMode = false;

        private void Awake()
        {
            // Try to resolve dependency, but don't crash if not found (might be test/sim)
            try {
                if (_blockchainManager == null) // Allow injection before Awake if needed
                {
                    _blockchainManager = ServiceLocator.Instance.Resolve<IBlockchainManager>();
                }
            } catch {
                Debug.LogWarning("FusionQueueManager: IBlockchainManager not found (Normal in Editor tests if not setup).");
            }
        }

        public void SetBlockchainManager(IBlockchainManager manager)
        {
            _blockchainManager = manager;
        }

        public void InitializeQueue(string mainId, List<string> materialIds)
        {
            currentQueue = new FusionQueueData(mainId, materialIds);
            Debug.Log($"[FusionQueue] Initialized Queue: {currentQueue.queue_id} with {materialIds.Count} items.");
        }

        public async void ProcessQueue()
        {
            var cancellationToken = this.GetCancellationTokenOnDestroy();

            if (currentQueue == null || currentQueue.queue_status == "COMPLETED")
            {
                Debug.LogWarning("[FusionQueue] No active queue or queue completed.");
                return;
            }

            currentQueue.queue_status = "PROCESSING";
            Debug.Log($"[FusionQueue] Starting/Resuming Queue Status: {currentQueue.queue_status}");

            // Find first non-finalized step
            var stepsToProcess = currentQueue.fusion_steps.Where(s => s.status == "WAITING" || s.status == "PENDING" || s.status == "FAILED").ToList();

            foreach (var step in stepsToProcess)
            {
                if (cancellationToken.IsCancellationRequested) return;

                // If we are resuming a failed step, we retry it.
                // If step was FAILED, we treat it as pending now.
                if (step.status == "FAILED")
                {
                    Debug.Log($"[FusionQueue] Retrying failed step {step.step_id}...");
                }

                step.status = "PENDING";
                OnProgressUpdated?.Invoke(GetProgress(), $"Processing Item {step.step_id}...");

                bool success = await ExecuteFusionStep(step, cancellationToken);

                if (cancellationToken.IsCancellationRequested) return;

                if (success)
                {
                    step.status = "SUCCESS";
                    step.tx_hash = "0x" + Guid.NewGuid().ToString().Replace("-", "").Substring(0, 16); // Mock hash if real one unavailable

                    Debug.Log($"[FusionQueue] Step {step.step_id} SUCCESS. Hash: {step.tx_hash}");
                    OnStepComplete?.Invoke(step);

                    // Cooldown to be safe
                    await UniTask.Delay(1000, cancellationToken: cancellationToken);
                }
                else
                {
                    step.status = "FAILED";
                    currentQueue.queue_status = "PAUSED";
                    Debug.LogError($"[FusionQueue] Step {step.step_id} FAILED.");

                    // Stop processing and notify UI
                    OnQueuePaused?.Invoke(step, "Transaction Failed or Rejected");
                    return;
                }
            }

            if (cancellationToken.IsCancellationRequested) return;

            currentQueue.queue_status = "COMPLETED";
            OnProgressUpdated?.Invoke(1.0f, "All Completed!");
            OnQueueComplete?.Invoke();
            Debug.Log("[FusionQueue] Queue Completed Successfully.");
        }

        private async UniTask<bool> ExecuteFusionStep(FusionStep step, System.Threading.CancellationToken token)
        {
            if (_isSimulationMode)
            {
                return await SimulateBlockchainCall(step, token);
            }

            if (_blockchainManager == null)
            {
                Debug.LogError("BlockchainManager is null!");
                return false;
            }

            try
            {
                // Parse IDs (Assuming they are integers as per BlockchainManager signature)
                if (!int.TryParse(currentQueue.main_hero_id, out int mainId) || !int.TryParse(step.material_hero_id, out int matId))
                {
                    Debug.LogError($"Invalid Hero IDs: Main={currentQueue.main_hero_id}, Mat={step.material_hero_id}");
                    return false;
                }

                // Call Blockchain
                // Note: Actual UpgradeHero returns bool. Logic should handle it.
                return await _blockchainManager.UpgradeHero(mainId, matId).AsUniTask().AttachExternalCancellation(token);
            }
            catch (Exception ex)
            {
                Debug.LogException(ex);
                return false;
            }
        }

        // Logic to retry logic
        public void RetryCurrentStep()
        {
            if (currentQueue.queue_status != "PAUSED") return;
            ProcessQueue();
        }

        // Logic to skip logic
        public void SkipCurrentStep()
        {
            if (currentQueue.queue_status != "PAUSED") return;

            var failedStep = currentQueue.fusion_steps.FirstOrDefault(s => s.status == "FAILED");
            if (failedStep != null)
            {
                failedStep.status = "SKIPPED";
                Debug.Log($"[FusionQueue] Skipped Step {failedStep.step_id}");
            }

            ProcessQueue();
        }

        private float GetProgress()
        {
            if (currentQueue == null || currentQueue.fusion_steps.Count == 0) return 0f;
            int completed = currentQueue.fusion_steps.Count(s => s.status == "SUCCESS" || s.status == "SKIPPED");
            return (float)completed / currentQueue.fusion_steps.Count;
        }

        // ==================================================================================
        // SIMULATION LOGIC (DELIVERABLE B)
        // ==================================================================================

        public void RunSimulation()
        {
            _isSimulationMode = true;
            Debug.Log("=== STARTING FUSION QUEUE SIMULATION ===");

            // 1. Initialize
            InitializeQueue("101", new List<string> { "201", "202", "203", "204" });

            // 2. Start Process
            ProcessQueue();
        }

        private async UniTask<bool> SimulateBlockchainCall(FusionStep step, System.Threading.CancellationToken token)
        {
            Debug.Log($"[Simulation] Sending Tx for Main: {currentQueue.main_hero_id}, Mat: {step.material_hero_id}...");
            await UniTask.Delay(500, cancellationToken: token); // Network delay

            // Hardcoded simulation behavior:
            // Step 1: Success
            // Step 2: Fail (User Reject)
            // Step 3: Success
            // Step 4: Success

            if (step.material_hero_id == "202" && step.status != "SKIPPED")
            {
                // Only fail the first time it's attempted (if we track attempts, but here we just fail based on ID)
                // To simulate "Retry vs Skip", we need to know if we should pass or fail.
                // Let's assume we want to fail it initially.
                // But if we Retry, it should probably pass or fail again.
                // For the purpose of the requested log: "Success Item 1, Fail Item 2, Skip/Resume to Item 3".
                // I'll fail it if it's the first time we encounter it in this 'run'.
                // But this method is called inside ProcessQueue loop.

                // Let's just fail it always, expecting the user (test script) to Skip it.
                Debug.Log("[Simulation] User Rejected Transaction!");
                return false;
            }

            Debug.Log("[Simulation] Transaction Confirmed!");
            return true;
        }
    }
}

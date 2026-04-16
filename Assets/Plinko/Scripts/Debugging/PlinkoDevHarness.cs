using System.Collections.Generic;
using System.Text;
using Leopotam.EcsLite;
using Plinko.Scripts.Data.Locations;
using Plinko.Scripts.Bootstrap;
using Plinko.Scripts.ECS.Components;
using Plinko.Scripts.Models;
using Plinko.Scripts.ECS.Requests;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Plinko.Scripts.Debugging
{
    public sealed class PlinkoDevHarness : MonoBehaviour
    {
        private const float SnapshotRefreshInterval = 0.1f;

        private GameBootstrapper _bootstrapper;
        private Vector2 _scrollPosition;
        private string _snapshot = "Bootstrap is not ready.";
        private string _lastLoggedSnapshot = string.Empty;
        private string _lastActionLabel = "none";
        private float _nextSnapshotRefreshTime;
        private bool _showOverlay = true;
        private bool _autoRefreshSnapshot = true;
        private bool _autoLogStateChanges;

        public void Initialize(GameBootstrapper bootstrapper)
        {
            _bootstrapper = bootstrapper;
            RefreshSnapshot();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                _showOverlay = !_showOverlay;
            }

            if (!IsReady())
            {
                return;
            }

            if (Input.GetKeyDown(KeyCode.F1))
            {
                QueueStartNewRun();
            }

            if (Input.GetKeyDown(KeyCode.F2))
            {
                QueueContinueRun();
            }

            if (Input.GetKeyDown(KeyCode.F5))
            {
                QueueSaveRun();
            }

            if (Input.GetKeyDown(KeyCode.F6))
            {
                QueueStartLevel(0);
            }

            if (Input.GetKeyDown(KeyCode.F7))
            {
                QueueStartLevel(1);
            }

            if (Input.GetKeyDown(KeyCode.F8))
            {
                QueueStartLevel(2);
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                QueueBuyUnit(0);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                QueueBuyUnit(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                QueueBuyUnit(2);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                QueueRerollUnitShop();
            }

            if (Input.GetKeyDown(KeyCode.T))
            {
                QueueSelectOwnedUnitsForRetraining(1);
            }

            if (Input.GetKeyDown(KeyCode.Y))
            {
                QueueSelectOwnedUnitsForRetraining(2);
            }

            if (Input.GetKeyDown(KeyCode.U))
            {
                QueueConfirmRetraining();
            }

            if (Input.GetKeyDown(KeyCode.P))
            {
                QueueBuyPin(0);
            }

            if (Input.GetKeyDown(KeyCode.O))
            {
                QueueSelectBoardSlot(0);
            }

            if (Input.GetKeyDown(KeyCode.I))
            {
                QueueReplaceBoardPin();
            }

            if (Input.GetKeyDown(KeyCode.G))
            {
                QueueGenerateHand();
            }

            if (Input.GetKeyDown(KeyCode.H))
            {
                QueueClearHand();
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                QueueDeployFirstHandCard();
            }

            if (Input.GetKeyDown(KeyCode.B))
            {
                QueueStartBattle();
            }

            if (Input.GetKeyDown(KeyCode.N))
            {
                QueueAdvanceToNextLevel();
            }

            if (Input.GetKeyDown(KeyCode.M))
            {
                QueueReturnToMenu();
            }

            if (Input.GetKeyDown(KeyCode.F12))
            {
                ForceLogSnapshot("Manual dump");
            }
        }

        private void LateUpdate()
        {
            if (!IsReady() || !_autoRefreshSnapshot && !_autoLogStateChanges)
            {
                return;
            }

            if (Time.unscaledTime < _nextSnapshotRefreshTime)
            {
                return;
            }

            _nextSnapshotRefreshTime = Time.unscaledTime + SnapshotRefreshInterval;
            RefreshSnapshot();
        }

        private void OnGUI()
        {
            if (!_showOverlay)
            {
                return;
            }

            GUI.depth = -1000;
            GUILayout.BeginArea(new Rect(10f, 10f, 560f, Screen.height - 20f), GUI.skin.box);
            GUILayout.Label("Plinko Dev Harness");
            GUILayout.Label("` toggles overlay. F12 logs current snapshot.");

            if (!IsReady())
            {
                GUILayout.Label("Bootstrap is not ready yet.");
                GUILayout.EndArea();
                return;
            }

            GUILayout.Label($"Scene: {SceneManager.GetActiveScene().name}");
            GUILayout.Label($"Last action: {_lastActionLabel}");
            GUILayout.Label($"Save path: {_bootstrapper.Services.RunSaveService.SavePath}");
            GUILayout.Label($"Meta path: {_bootstrapper.Services.MetaSaveService.SavePath}");
            _autoRefreshSnapshot = GUILayout.Toggle(_autoRefreshSnapshot, "Auto refresh snapshot");
            _autoLogStateChanges = GUILayout.Toggle(_autoLogStateChanges, "Auto log state changes");

            GUILayout.Space(6f);
            DrawLocationButtons();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Continue Run"))
            {
                QueueContinueRun();
            }

            if (GUILayout.Button("Save Run"))
            {
                QueueSaveRun();
            }

            if (GUILayout.Button("Log Snapshot"))
            {
                ForceLogSnapshot("Manual dump");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Load Level 0"))
            {
                QueueStartLevel(0);
            }

            if (GUILayout.Button("Load Level 1"))
            {
                QueueStartLevel(1);
            }

            if (GUILayout.Button("Load Level 2"))
            {
                QueueStartLevel(2);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Buy Offer 0"))
            {
                QueueBuyUnit(0);
            }

            if (GUILayout.Button("Buy Offer 1"))
            {
                QueueBuyUnit(1);
            }

            if (GUILayout.Button("Buy Offer 2"))
            {
                QueueBuyUnit(2);
            }

            if (GUILayout.Button("Reroll Shop"))
            {
                QueueRerollUnitShop();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Select 1 Owned"))
            {
                QueueSelectOwnedUnitsForRetraining(1);
            }

            if (GUILayout.Button("Select 2 Owned"))
            {
                QueueSelectOwnedUnitsForRetraining(2);
            }

            if (GUILayout.Button("Confirm Retraining"))
            {
                QueueConfirmRetraining();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Buy Pin 0"))
            {
                QueueBuyPin(0);
            }

            if (GUILayout.Button("Select Slot 0"))
            {
                QueueSelectBoardSlot(0);
            }

            if (GUILayout.Button("Replace Pin"))
            {
                QueueReplaceBoardPin();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Generate Hand"))
            {
                QueueGenerateHand();
            }

            if (GUILayout.Button("Clear Hand"))
            {
                QueueClearHand();
            }

            if (GUILayout.Button("Deploy First Card"))
            {
                QueueDeployFirstHandCard();
            }

            if (GUILayout.Button("Start Battle"))
            {
                QueueStartBattle();
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Advance Next"))
            {
                QueueAdvanceToNextLevel();
            }

            if (GUILayout.Button("Return Menu"))
            {
                QueueReturnToMenu();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(8f);
            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.ExpandHeight(true));
            GUILayout.TextArea(_snapshot, GUILayout.ExpandHeight(true));
            GUILayout.EndScrollView();
            GUILayout.EndArea();
        }

        private bool IsReady()
        {
            return _bootstrapper != null && _bootstrapper.IsReady;
        }

        private void RefreshSnapshot()
        {
            if (!IsReady())
            {
                _snapshot = "Bootstrap is not ready.";
                return;
            }

            _snapshot = BuildSnapshot();
            if (_autoLogStateChanges && _snapshot != _lastLoggedSnapshot)
            {
                Debug.Log(_snapshot);
                _lastLoggedSnapshot = _snapshot;
            }
        }

        private void ForceLogSnapshot(string reason)
        {
            RefreshSnapshot();
            Debug.Log($"{reason}\n{_snapshot}");
            _lastLoggedSnapshot = _snapshot;
        }

        private string BuildSnapshot()
        {
            var world = _bootstrapper.World;
            var services = _bootstrapper.Services;
            var builder = new StringBuilder(2048);

            builder.AppendLine($"Scene: {SceneManager.GetActiveScene().name}");
            builder.AppendLine($"Last Action: {_lastActionLabel}");

            if (!services.RunEntityIndex.TryGetRunEntity(out var runEntity))
            {
                builder.AppendLine("Run: none");
                AppendLocationProgress(services, builder);
                AppendCounts(world, builder);
                return builder.ToString();
            }

            var locationPool = world.GetPool<CurrentLocationComponent>();
            var levelPool = world.GetPool<CurrentLevelComponent>();
            var levelTypePool = world.GetPool<CurrentLevelTypeComponent>();
            var phasePool = world.GetPool<CurrentPhaseComponent>();
            var statusPool = world.GetPool<RunStatusComponent>();
            var goldPool = world.GetPool<CurrentGoldComponent>();
            var playerBasePool = world.GetPool<PlayerBaseHealthComponent>();
            var enemyBasePool = world.GetPool<EnemyBaseHealthComponent>();
            var manaPool = world.GetPool<CurrentManaComponent>();
            var purchasePool = world.GetPool<PurchasePhaseStateComponent>();
            var retrainingPool = world.GetPool<RetrainingPhaseStateComponent>();
            var fieldUpgradePool = world.GetPool<FieldUpgradePhaseStateComponent>();
            var battlePool = world.GetPool<BattleStateComponent>();
            var enemyWavePool = world.GetPool<CurrentEnemyWaveComponent>();

            builder.AppendLine(
                $"Run: location={locationPool.Get(runEntity).LocationId}, level={levelPool.Get(runEntity).LevelIndex}, levelType={levelTypePool.Get(runEntity).Value}, phase={phasePool.Get(runEntity).Value}, status={statusPool.Get(runEntity).Value}");
            builder.AppendLine(
                $"Resources: gold={goldPool.Get(runEntity).Value}, mana={manaPool.Get(runEntity).Value}, playerBase={playerBasePool.Get(runEntity).Value}/{playerBasePool.Get(runEntity).MaxValue}, enemyBase={enemyBasePool.Get(runEntity).Value}/{enemyBasePool.Get(runEntity).MaxValue}");

            if (purchasePool.Has(runEntity))
            {
                var purchaseState = purchasePool.Get(runEntity);
                builder.AppendLine(
                    $"Purchase: rerolls={purchaseState.RerollCount}, activeTraining={purchaseState.ActiveTrainingCount}, canEnterBattle={purchaseState.CanEnterBattle}");
            }

            if (retrainingPool.Has(runEntity))
            {
                var retrainingState = retrainingPool.Get(runEntity);
                builder.AppendLine(
                    $"Retraining: selected={retrainingState.SelectedCount}/{retrainingState.SelectionLimit}, locked={retrainingState.IsSelectionLocked}, activeTraining={retrainingState.ActiveTrainingCount}");
            }

            if (fieldUpgradePool.Has(runEntity))
            {
                var fieldState = fieldUpgradePool.Get(runEntity);
                builder.AppendLine(
                    $"FieldUpgrade: rerolls={fieldState.RerollCount}, selectedSlot={fieldState.SelectedSlotIndex}, highlighted={fieldState.IsPlacementHighlighted}");
            }

            if (battlePool.Has(runEntity))
            {
                var battleState = battlePool.Get(runEntity);
                builder.AppendLine(
                    $"Battle: turn={battleState.CurrentTurn}, resolved={battleState.IsResolved}, active={battleState.IsPlayerTurnActive}, handGenerated={battleState.HasGeneratedHandThisTurn}, nextDeploy={battleState.NextDeploymentOrder}");
            }

            if (enemyWavePool.Has(runEntity))
            {
                var waveState = enemyWavePool.Get(runEntity);
                builder.AppendLine(
                    $"EnemyWave: threshold={waveState.ThresholdPercent}, units={waveState.EnemyCount}, atk={waveState.TotalAttack}, hp={waveState.TotalHealth}");
            }

            builder.AppendLine();
            AppendOwnedUnits(world, builder);
            AppendUnitOffers(world, builder);
            AppendPinOffers(world, builder);
            AppendHand(world, builder);
            AppendDeployedUnits(world, builder);
            AppendSelectedEnemyWave(services.BattleRuntimeService, builder);
            AppendBattleRuntime(services.BattleRuntimeService, builder);
            AppendLocationProgress(services, builder);
            AppendStagedTrainees(world, builder);
            AppendTrainingResults(world, services.PlinkoRuntimeService, builder);
            AppendPlayback(world, builder);
            AppendInstalledPins(world, builder);
            AppendCounts(world, builder);
            return builder.ToString();
        }

        private static void AppendOwnedUnits(EcsWorld world, StringBuilder builder)
        {
            var ownedPool = world.GetPool<OwnedUnitComponent>();
            var unitTypePool = world.GetPool<UnitTypeIdComponent>();
            var unitStatsPool = world.GetPool<UnitStatsComponent>();
            var manaCostPool = world.GetPool<UnitManaCostComponent>();
            var displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            var levelPool = world.GetPool<UnitLevelComponent>();
            var upgradePool = world.GetPool<UpgradeCountComponent>();
            var selectedPool = world.GetPool<SelectedForRetrainingComponent>();
            var ownedUnits = new List<string>();

            foreach (var entity in world.Filter<OwnedUnitComponent>().End())
            {
                var runtimeId = ownedPool.Get(entity).RuntimeId;
                var selectedMark = selectedPool.Has(entity) ? "*" : string.Empty;
                ownedUnits.Add(
                    $"{runtimeId}{selectedMark}: {displayNamePool.Get(entity).Value} ({unitTypePool.Get(entity).Value}) atk={unitStatsPool.Get(entity).Attack} hp={unitStatsPool.Get(entity).Health} mana={manaCostPool.Get(entity).Value} lvl={levelPool.Get(entity).Value} upg={upgradePool.Get(entity).Value}");
            }

            ownedUnits.Sort();
            builder.AppendLine($"OwnedUnits [{ownedUnits.Count}]:");
            if (ownedUnits.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in ownedUnits)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static void AppendUnitOffers(EcsWorld world, StringBuilder builder)
        {
            var offerPool = world.GetPool<UnitShopOfferComponent>();
            var pricePool = world.GetPool<OfferPriceComponent>();
            var unitTypePool = world.GetPool<ShopOfferUnitTypeIdComponent>();
            var offers = new List<string>();

            foreach (var entity in world.Filter<UnitShopOfferComponent>().End())
            {
                offers.Add($"{offerPool.Get(entity).OfferId}: {unitTypePool.Get(entity).Value} price={pricePool.Get(entity).Value}");
            }

            offers.Sort();
            builder.AppendLine($"UnitOffers [{offers.Count}]:");
            if (offers.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in offers)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static void AppendPinOffers(EcsWorld world, StringBuilder builder)
        {
            var offerPool = world.GetPool<PinShopOfferComponent>();
            var pricePool = world.GetPool<OfferPriceComponent>();
            var pinTypePool = world.GetPool<ShopOfferPinTypeIdComponent>();
            var pendingPinPool = world.GetPool<PendingPurchasedPinComponent>();
            var offers = new List<string>();

            foreach (var entity in world.Filter<PinShopOfferComponent>().End())
            {
                offers.Add($"{offerPool.Get(entity).OfferId}: {pinTypePool.Get(entity).Value} price={pricePool.Get(entity).Value}");
            }

            offers.Sort();
            builder.AppendLine($"PinOffers [{offers.Count}]:");
            if (offers.Count == 0)
            {
                builder.AppendLine("  none");
            }
            else
            {
                foreach (var line in offers)
                {
                    builder.AppendLine($"  {line}");
                }
            }

            builder.AppendLine($"PendingPins [{CountEntities<PendingPurchasedPinComponent>(world)}]:");
            foreach (var entity in world.Filter<PendingPurchasedPinComponent>().End())
            {
                var pendingPin = pendingPinPool.Get(entity);
                builder.AppendLine($"  offer={pendingPin.OfferId}, pinType={pendingPin.PinTypeId}");
            }
        }

        private static void AppendHand(EcsWorld world, StringBuilder builder)
        {
            var handStateFilter = world.Filter<RunComponent>().Inc<HandStateComponent>().End();
            var handStatePool = world.GetPool<HandStateComponent>();
            var cardPool = world.GetPool<HandCardComponent>();
            var ownerPool = world.GetPool<HandCardOwnerUnitComponent>();
            var cards = new List<string>();

            foreach (var entity in world.Filter<HandCardComponent>().Inc<HandCardOwnerUnitComponent>().End())
            {
                cards.Add($"{cardPool.Get(entity).HandCardRuntimeId}: owner={ownerPool.Get(entity).OwnedUnitRuntimeId}");
            }

            cards.Sort();
            foreach (var runEntity in handStateFilter)
            {
                var handState = handStatePool.Get(runEntity);
                builder.AppendLine($"HandState: cards={handState.CardCount}, nextId={handState.NextRuntimeId}");
                break;
            }

            builder.AppendLine($"Hand [{cards.Count}]:");
            if (cards.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in cards)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static void AppendDeployedUnits(EcsWorld world, StringBuilder builder)
        {
            var ownerPool = world.GetPool<HandCardOwnerUnitComponent>();
            var deploymentOrderPool = world.GetPool<DeploymentOrderComponent>();
            var deployedUnits = new List<string>();

            foreach (var entity in world.Filter<DeployedForTurnComponent>().Inc<HandCardOwnerUnitComponent>().Inc<DeploymentOrderComponent>().End())
            {
                deployedUnits.Add(
                    $"owner={ownerPool.Get(entity).OwnedUnitRuntimeId} order={deploymentOrderPool.Get(entity).Value}");
            }

            deployedUnits.Sort();
            builder.AppendLine($"Deployed [{deployedUnits.Count}]:");
            if (deployedUnits.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in deployedUnits)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static void AppendSelectedEnemyWave(Services.BattleRuntimeService battleRuntimeService, StringBuilder builder)
        {
            var wave = battleRuntimeService != null ? battleRuntimeService.CurrentEnemyWave : null;
            var enemies = wave != null && wave.Enemies != null ? wave.Enemies : null;
            var count = enemies != null ? enemies.Count : 0;

            builder.AppendLine($"SelectedEnemyWave [{count}]:");
            if (count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var enemy in enemies)
            {
                if (enemy == null)
                {
                    continue;
                }

                builder.AppendLine(
                    $"  {enemy.SpawnId}: {enemy.DisplayName} atk={enemy.Attack} hp={enemy.Health} pos=({enemy.BoardX},{enemy.BoardY}) move={enemy.MoveRange} range={enemy.AttackRange}");
            }
        }

        private static void AppendBattleRuntime(Services.BattleRuntimeService battleRuntimeService, StringBuilder builder)
        {
            var timeline = battleRuntimeService != null ? battleRuntimeService.CurrentTimeline : null;
            var result = battleRuntimeService != null ? battleRuntimeService.CurrentResult : null;

            builder.AppendLine("BattleRuntime:");
            if (timeline == null && result == null)
            {
                builder.AppendLine("  none");
                return;
            }

            if (result != null)
            {
                builder.AppendLine(
                    $"  result: victory={result.IsVictory} defeat={result.IsDefeat} playerBaseAfter={result.PlayerBaseHealthAfter} enemyBaseAfter={result.EnemyBaseHealthAfter}");
            }

            if (timeline != null)
            {
                builder.AppendLine(
                    $"  timeline: ticks={timeline.Ticks.Count} damageToEnemyBase={timeline.SurvivorDamageToEnemyBase} damageToPlayerBase={timeline.SurvivorDamageToPlayerBase}");
                foreach (var tick in timeline.Ticks)
                {
                    if (tick == null)
                    {
                        continue;
                    }

                    var actionSummary = new StringBuilder();
                    if (tick.Actions != null)
                    {
                        for (var actionIndex = 0; actionIndex < tick.Actions.Count; actionIndex++)
                        {
                            var action = tick.Actions[actionIndex];
                            if (action == null)
                            {
                                continue;
                            }

                            if (actionSummary.Length > 0)
                            {
                                actionSummary.Append(" | ");
                            }

                            actionSummary.Append(
                                $"{action.ActionType} src={action.SourceRuntimeId} tgt={action.TargetRuntimeId} val={action.Value} pos=({action.TargetPosition.x},{action.TargetPosition.y})");
                        }
                    }

                    builder.AppendLine($"  tick {tick.TickIndex}: {actionSummary}");
                }
            }
        }

        private static void AppendLocationProgress(GameServicesContext services, StringBuilder builder)
        {
            var locations = services != null && services.LocationConfigService != null
                ? services.LocationConfigService.GetAllLocations()
                : null;
            var count = locations != null ? locations.Count : 0;

            builder.AppendLine($"Locations [{count}]:");
            if (locations == null || locations.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            for (var index = 0; index < locations.Count; index++)
            {
                var location = locations[index];
                if (location == null)
                {
                    continue;
                }

                var isUnlocked = services.UnlocksService == null || services.UnlocksService.IsUnlocked(location.UnlockCondition);
                var isCompleted = services.UnlocksService != null && services.UnlocksService.IsLocationCompleted(location.Id);
                var maxCompletedLevel = services.UnlocksService != null
                    ? services.UnlocksService.GetMaxCompletedLevelIndex(location.Id)
                    : -1;
                builder.AppendLine(
                    $"  {index}: {location.DisplayName} ({location.Id}) unlocked={isUnlocked} completed={isCompleted} maxLevel={maxCompletedLevel}");
            }
        }

        private static void AppendStagedTrainees(EcsWorld world, StringBuilder builder)
        {
            var stagedPool = world.GetPool<StagedTraineeComponent>();
            var unitTypePool = world.GetPool<UnitTypeIdComponent>();
            var displayNamePool = world.GetPool<UnitDisplayNameComponent>();
            var staged = new List<string>();

            foreach (var entity in world.Filter<StagedTraineeComponent>().End())
            {
                var trainee = stagedPool.Get(entity);
                var unitTypeId = unitTypePool.Has(entity) ? unitTypePool.Get(entity).Value : "<missing>";
                var displayName = displayNamePool.Has(entity) ? displayNamePool.Get(entity).Value : "<missing>";
                staged.Add(
                    $"{trainee.RuntimeId}: {displayName} ({unitTypeId}) retraining={trainee.IsRetraining} sourceOffer={trainee.SourceOfferId}");
            }

            staged.Sort();
            builder.AppendLine($"StagedTrainees [{staged.Count}]:");
            if (staged.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in staged)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static void AppendPlayback(EcsWorld world, StringBuilder builder)
        {
            var playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            var entries = new List<string>();

            foreach (var entity in world.Filter<PlinkoTrainingPlaybackComponent>().End())
            {
                var playback = playbackPool.Get(entity);
                entries.Add(
                    $"{playback.RuntimeId}: retraining={playback.IsRetraining} node={playback.CurrentNodeIndex}/{playback.TotalNodeCount} elapsed={playback.Elapsed:0.00}/{playback.Duration:0.00} completed={playback.IsCompleted}");
            }

            entries.Sort();
            builder.AppendLine($"TrainingPlayback [{entries.Count}]:");
            if (entries.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in entries)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static void AppendTrainingResults(EcsWorld world, Services.PlinkoRuntimeService plinkoRuntimeService, StringBuilder builder)
        {
            var playbackByRuntimeId = new Dictionary<int, PlinkoTrainingPlaybackComponent>();
            var playbackPool = world.GetPool<PlinkoTrainingPlaybackComponent>();
            foreach (var entity in world.Filter<PlinkoTrainingPlaybackComponent>().End())
            {
                var playback = playbackPool.Get(entity);
                playbackByRuntimeId[playback.RuntimeId] = playback;
            }

            var entries = new List<string>();
            if (plinkoRuntimeService != null)
            {
                foreach (var pair in plinkoRuntimeService.GetAllResults())
                {
                    var result = pair.Value;
                    if (result == null || result.Result == null)
                    {
                        continue;
                    }

                    playbackByRuntimeId.TryGetValue(pair.Key, out var playback);
                    var progress = playback.TotalNodeCount > 0
                        ? $" playback={Mathf.Clamp(playback.CurrentNodeIndex, 0, playback.TotalNodeCount)}/{playback.TotalNodeCount}"
                        : string.Empty;
                    entries.Add(
                        $"{pair.Key}: basket={result.FinalBasketId} basketMana={result.FinalBasketManaValue} final atk={result.Result.FinalAttack} hp={result.Result.FinalHealth} mana={result.Result.FinalManaCost}{progress} path={BuildPathSummary(result)}");
                }
            }

            entries.Sort();
            builder.AppendLine($"TrainingResults [{entries.Count}]:");
            if (entries.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in entries)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static string BuildPathSummary(PlinkoPathResultModel result)
        {
            if (result == null || result.Nodes == null || result.Nodes.Count == 0)
            {
                return "none";
            }

            var builder = new StringBuilder(128);
            for (var index = 0; index < result.Nodes.Count; index++)
            {
                var node = result.Nodes[index];
                if (index > 0)
                {
                    builder.Append(" -> ");
                }

                builder.Append($"r{node.RowIndex}c{node.ColumnIndex}");
                if (!string.IsNullOrWhiteSpace(node.PinTypeId))
                {
                    builder.Append($"[{node.PinTypeId}]");
                }

                if (node.AttackDelta != 0 || node.HealthDelta != 0 || node.ManaDelta != 0)
                {
                    builder.Append($"({FormatSigned(node.AttackDelta)}/{FormatSigned(node.HealthDelta)}/{FormatSigned(node.ManaDelta)})");
                }
            }

            return builder.ToString();
        }

        private static string FormatSigned(int value)
        {
            return value >= 0 ? $"+{value}" : value.ToString();
        }

        private static void AppendInstalledPins(EcsWorld world, StringBuilder builder)
        {
            var installedPinPool = world.GetPool<InstalledPinComponent>();
            var entries = new List<string>();

            foreach (var entity in world.Filter<InstalledPinComponent>().End())
            {
                var installedPin = installedPinPool.Get(entity);
                entries.Add($"slot={installedPin.SlotIndex}, row={installedPin.RowIndex}, col={installedPin.ColumnIndex}, pin={installedPin.PinTypeId}");
            }

            entries.Sort();
            builder.AppendLine($"InstalledPins [{entries.Count}]:");
            if (entries.Count == 0)
            {
                builder.AppendLine("  none");
                return;
            }

            foreach (var line in entries)
            {
                builder.AppendLine($"  {line}");
            }
        }

        private static void AppendCounts(EcsWorld world, StringBuilder builder)
        {
            builder.AppendLine();
            builder.AppendLine("Counts:");
            builder.AppendLine($"  runs={CountEntities<RunComponent>(world)}");
            builder.AppendLine($"  ownedUnits={CountEntities<OwnedUnitComponent>(world)}");
            builder.AppendLine($"  unitOffers={CountEntities<UnitShopOfferComponent>(world)}");
            builder.AppendLine($"  pinOffers={CountEntities<PinShopOfferComponent>(world)}");
            builder.AppendLine($"  stagedTrainees={CountEntities<StagedTraineeComponent>(world)}");
            builder.AppendLine($"  playback={CountEntities<PlinkoTrainingPlaybackComponent>(world)}");
            builder.AppendLine($"  handCards={CountEntities<HandCardComponent>(world)}");
            builder.AppendLine($"  deployed={CountEntities<DeployedForTurnComponent>(world)}");
            builder.AppendLine($"  installedPins={CountEntities<InstalledPinComponent>(world)}");
        }

        private static int CountEntities<T>(EcsWorld world) where T : struct
        {
            var count = 0;
            foreach (var _ in world.Filter<T>().End())
            {
                count++;
            }

            return count;
        }

        private void QueueStartNewRun()
        {
            var unlockedLocations = _bootstrapper.Services.LocationConfigService.GetUnlockedLocations(_bootstrapper.Services.UnlocksService);
            var firstLocation = unlockedLocations != null && unlockedLocations.Count > 0
                ? unlockedLocations[0]
                : null;
            if (firstLocation == null || string.IsNullOrWhiteSpace(firstLocation.Id))
            {
                _lastActionLabel = "StartNewRun failed: no unlocked location configured";
                RefreshSnapshot();
                return;
            }

            QueueStartNewRun(firstLocation);
        }

        private void QueueStartNewRun(LocationData location)
        {
            if (location == null || string.IsNullOrWhiteSpace(location.Id))
            {
                _lastActionLabel = "StartNewRun failed: invalid location";
                RefreshSnapshot();
                return;
            }

            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<StartNewRunRequest>().Add(entity).LocationId = location.Id;
            _lastActionLabel = $"Queued StartNewRun({location.Id})";
            RefreshSnapshot();
        }

        private void QueueContinueRun()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<ContinueRunRequest>().Add(entity);
            _lastActionLabel = "Queued ContinueRun";
            RefreshSnapshot();
        }

        private void QueueSaveRun()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<SaveRunRequest>().Add(entity);
            _lastActionLabel = "Queued SaveRun";
            RefreshSnapshot();
        }

        private void QueueStartLevel(int levelIndex)
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<StartLevelRequest>().Add(entity).LevelIndex = levelIndex;
            _lastActionLabel = $"Queued StartLevel({levelIndex})";
            RefreshSnapshot();
        }

        private void QueueBuyUnit(int offerId)
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<BuyUnitRequest>().Add(entity).OfferId = offerId;
            _lastActionLabel = $"Queued BuyUnit({offerId})";
            RefreshSnapshot();
        }

        private void QueueRerollUnitShop()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<RerollUnitShopRequest>().Add(entity);
            _lastActionLabel = "Queued RerollUnitShop";
            RefreshSnapshot();
        }

        private void QueueSelectOwnedUnitsForRetraining(int count)
        {
            var runtimeIds = GetFirstOwnedRuntimeIds(count);
            if (runtimeIds.Count == 0)
            {
                _lastActionLabel = "SelectOwnedUnits skipped: no owned units";
                RefreshSnapshot();
                return;
            }

            foreach (var runtimeId in runtimeIds)
            {
                var entity = _bootstrapper.World.NewEntity();
                _bootstrapper.World.GetPool<SelectUnitsForRetrainingRequest>().Add(entity).RuntimeId = runtimeId;
            }

            _lastActionLabel = $"Queued SelectOwnedUnits({string.Join(",", runtimeIds)})";
            RefreshSnapshot();
        }

        private void QueueConfirmRetraining()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<ConfirmRetrainingSelectionRequest>().Add(entity);
            _lastActionLabel = "Queued ConfirmRetraining";
            RefreshSnapshot();
        }

        private void QueueBuyPin(int offerId)
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<BuyPinRequest>().Add(entity).OfferId = offerId;
            _lastActionLabel = $"Queued BuyPin({offerId})";
            RefreshSnapshot();
        }

        private void QueueSelectBoardSlot(int slotIndex)
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<SelectBoardSlotRequest>().Add(entity).SlotIndex = slotIndex;
            _lastActionLabel = $"Queued SelectBoardSlot({slotIndex})";
            RefreshSnapshot();
        }

        private void QueueReplaceBoardPin()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<ReplaceBoardPinRequest>().Add(entity);
            _lastActionLabel = "Queued ReplaceBoardPin";
            RefreshSnapshot();
        }

        private void QueueGenerateHand()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<GenerateHandRequest>().Add(entity);
            _lastActionLabel = "Queued GenerateHand";
            RefreshSnapshot();
        }

        private void QueueClearHand()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<ClearHandRequest>().Add(entity);
            _lastActionLabel = "Queued ClearHand";
            RefreshSnapshot();
        }

        private void QueueDeployFirstHandCard()
        {
            var handCardRuntimeId = GetFirstHandCardRuntimeId();
            if (handCardRuntimeId < 0)
            {
                _lastActionLabel = "Deploy skipped: no hand cards";
                RefreshSnapshot();
                return;
            }

            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<DeployCardRequest>().Add(entity).HandCardRuntimeId = handCardRuntimeId;
            _lastActionLabel = $"Queued DeployCard({handCardRuntimeId})";
            RefreshSnapshot();
        }

        private void QueueStartBattle()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<StartBattleRequest>().Add(entity);
            _lastActionLabel = "Queued StartBattle";
            RefreshSnapshot();
        }

        private void QueueAdvanceToNextLevel()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<AdvanceToNextLevelRequest>().Add(entity);
            _lastActionLabel = "Queued AdvanceToNextLevel";
            RefreshSnapshot();
        }

        private void QueueReturnToMenu()
        {
            var entity = _bootstrapper.World.NewEntity();
            _bootstrapper.World.GetPool<ReturnToMenuRequest>().Add(entity);
            _lastActionLabel = "Queued ReturnToMenu";
            RefreshSnapshot();
        }

        private List<int> GetFirstOwnedRuntimeIds(int count)
        {
            var runtimeIds = new List<int>();
            var ownedPool = _bootstrapper.World.GetPool<OwnedUnitComponent>();
            foreach (var entity in _bootstrapper.World.Filter<OwnedUnitComponent>().End())
            {
                runtimeIds.Add(ownedPool.Get(entity).RuntimeId);
            }

            runtimeIds.Sort();
            if (runtimeIds.Count > count)
            {
                runtimeIds.RemoveRange(count, runtimeIds.Count - count);
            }

            return runtimeIds;
        }

        private int GetFirstHandCardRuntimeId()
        {
            var handCardPool = _bootstrapper.World.GetPool<HandCardComponent>();
            var firstRuntimeId = int.MaxValue;
            var hasAny = false;

            foreach (var entity in _bootstrapper.World.Filter<HandCardComponent>().End())
            {
                var runtimeId = handCardPool.Get(entity).HandCardRuntimeId;
                if (runtimeId < firstRuntimeId)
                {
                    firstRuntimeId = runtimeId;
                    hasAny = true;
                }
            }

            return hasAny ? firstRuntimeId : -1;
        }

        private void DrawLocationButtons()
        {
            var locations = _bootstrapper.Services.LocationConfigService.GetAllLocations();
            if (locations == null || locations.Count == 0)
            {
                return;
            }

            for (var index = 0; index < locations.Count; index += 2)
            {
                GUILayout.BeginHorizontal();
                DrawLocationButton(locations[index], index);
                if (index + 1 < locations.Count)
                {
                    DrawLocationButton(locations[index + 1], index + 1);
                }
                GUILayout.EndHorizontal();
            }
        }

        private void DrawLocationButton(LocationData location, int index)
        {
            if (location == null)
            {
                return;
            }

            var isUnlocked = _bootstrapper.Services.UnlocksService.IsUnlocked(location.UnlockCondition);
            var label = isUnlocked
                ? $"Play {index}: {location.DisplayName}"
                : $"Locked {index}: {location.DisplayName}";
            var previousEnabled = GUI.enabled;
            GUI.enabled = isUnlocked;
            if (GUILayout.Button(label))
            {
                QueueStartNewRun(location);
            }

            GUI.enabled = previousEnabled;
        }
    }
}

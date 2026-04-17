using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Game1.Graph.Runtime.Infrastructure;
using Graph.Core.Runtime;
using Prototype.Business.Bootstrap;
using Prototype.Business.Runtime;
using Sample.Runtime;
using Sample.Runtime.Services;
using Sample.Runtime.UI;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace Prototype.Business.NPC
{
	public class Npcmanager : Interactable
	{
		[FormerlySerializedAs("questGraph")]
		[FormerlySerializedAs("dialogueGraph")]
		public CommonGraph baseDialogueGraph;

		[FormerlySerializedAs("stealGraph")]
		public CommonGraph baseStealGraph;

		public bool allowSteal = true;
		public Transform lookForwardReference;
		public float stealDistance = 1.5f;
		public float stealBackAngle = 120f;
		public float stealMinBackAngle = 40f;
		public string ownerId;
		public GameBootstrap bootstrap;
		public DialogueService dialogueService;
		public ChoiceUiservice choiceUiservice;
		public TradeOfferUiservice tradeOfferUiservice;
		public MapMarkerService mapMarkerService;
		public Transform playerTransform;
		private CommonGraphRunner m_baseRunner;
		private CommonGraph m_currentBaseGraph;

		public override void Interact(Transform player)
		{
			CommonGraph selectedBaseGraph = SelectBaseGraph(player);
			if (selectedBaseGraph == null)
			{
				return;
			}

			StartBaseGraph(selectedBaseGraph);
		}

		public override void Interact()
		{
			Interact(null);
		}

		private CommonGraph SelectBaseGraph(Transform player)
		{
			bool canSteal = allowSteal && baseStealGraph != null && StealContextEvaluator.CanStealFromNpc(player, this);
			if (canSteal)
			{
				return baseStealGraph;
			}

			if (baseDialogueGraph != null)
			{
				return baseDialogueGraph;
			}

			if (baseStealGraph != null)
			{
				return baseStealGraph;
			}

			return null;
		}

		private void StartBaseGraph(CommonGraph graph)
		{
			if (graph == null)
			{
				return;
			}

			if (!HasGraphContent(graph))
			{
				Debug.LogError($"[NPCManager] CommonGraph '{graph.name}' does not contain runtime nodes on '{name}'.",
					this);
				return;
			}

			if (m_baseRunner != null && m_baseRunner.IsRunning)
			{
				return;
			}

			if (m_baseRunner == null || m_currentBaseGraph != graph)
			{
				m_baseRunner = new CommonGraphRunner(GameGraphRuntimeRegistryFactory.Create());
				m_currentBaseGraph = graph;
			}

			IGraphQuestService questService = bootstrap != null && bootstrap.GameServer != null
				? new GraphQuestServiceAdapter(bootstrap.GameServer, this)
				: null;

			var context = new GraphExecutionContext(
				new GraphRuntimeServices(
					dialogueService,
					choiceUiservice,
					null,
					null,
					null,
					questService));
			context.Set(GraphContextKeys.runtimeBootstrap, bootstrap);
			context.Set(GraphContextKeys.runtimeMapMarkerService, mapMarkerService);
			context.Set(GraphContextKeys.runtimePlayerTransform, playerTransform);
			context.ImmediateChoiceAfterDialogue = true;

			_ = m_baseRunner.RunAsync(graph, context);
		}

		private static bool HasGraphContent(CommonGraph graph)
		{
			return graph != null && graph.nodes != null && graph.nodes.Count > 0;
		}


		private sealed class GraphQuestServiceAdapter : IGraphQuestService
		{
			private static readonly BindingFlags s_instancePublicAndNonPublic =
				BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

			private readonly object m_gameServer;
			private readonly Object m_logContext;

			public GraphQuestServiceAdapter(object gameServer, Object logContext)
			{
				m_gameServer = gameServer ?? throw new ArgumentNullException(nameof(gameServer));
				m_logContext = logContext;
			}

			public async UniTask<bool> StartQuestAsync(string questId, CancellationToken cancellationToken)
			{
				if (string.IsNullOrWhiteSpace(questId))
				{
					Debug.LogError("[NPCManager] StartQuestAsync failed because questId is empty.", m_logContext);
					return false;
				}

				object invocationResult = InvokeQuestMethod(new[] { "TryStartQuestAsync", "StartQuestAsync" }, questId,
					cancellationToken);
				if (invocationResult == null)
				{
					Debug.LogError($"[NPCManager] GameServer does not provide a start quest method for quest '{questId}'.",
						m_logContext);
					return false;
				}

				QuestActionOutcome outcome = await ConvertToQuestActionOutcomeAsync(invocationResult);
				if (outcome.success)
				{
					bool snapshotApplied = TryApplyProfileSnapshot(outcome.profileSnapshot);
					if (!snapshotApplied)
					{
						await TryRefreshProfileAsync(cancellationToken);
					}
				}

				return outcome.success;
			}

			public async UniTask<bool> CompleteQuestAsync(string questId, CancellationToken cancellationToken)
			{
				if (string.IsNullOrWhiteSpace(questId))
				{
					Debug.LogError("[NPCManager] CompleteQuestAsync failed because questId is empty.", m_logContext);
					return false;
				}

				object invocationResult = InvokeQuestMethod(new[] { "TryCompleteQuestAsync", "CompleteQuestAsync" },
					questId, cancellationToken);
				if (invocationResult == null)
				{
					Debug.LogError(
						$"[NPCManager] GameServer does not provide a complete quest method for quest '{questId}'.",
						m_logContext);
					return false;
				}

				QuestActionOutcome outcome = await ConvertToQuestActionOutcomeAsync(invocationResult);
				if (outcome.success)
				{
					bool snapshotApplied = TryApplyProfileSnapshot(outcome.profileSnapshot);
					if (!snapshotApplied)
					{
						await TryRefreshProfileAsync(cancellationToken);
					}
				}

				return outcome.success;
			}

			UniTask<Graph.Core.Runtime.Nodes.Server.QuestState> IGraphQuestService.GetQuestStateAsync(
				string questId,
				CancellationToken cancellationToken)
			{
				return GetQuestStateAsyncInternalAsync(questId, cancellationToken);
			}

			private async UniTask<Graph.Core.Runtime.Nodes.Server.QuestState> GetQuestStateAsyncInternalAsync(
				string questId,
				CancellationToken cancellationToken)
			{
				if (string.IsNullOrWhiteSpace(questId))
				{
					Debug.LogError("[NPCManager] GetQuestStateAsync failed because questId is empty.", m_logContext);
					return default;
				}

				object invocationResult =
					InvokeQuestMethod(
						new[] { "GetQuestStateAsync", "TryGetQuestStateAsync", "GetQuestState", "TryGetQuestState" },
						questId, cancellationToken);
				if (invocationResult != null)
				{
					return await ConvertToQuestStateAsync(invocationResult);
				}

				bool isCompleted = await TryQueryQuestFlagAsync(
					new[] { "IsQuestCompletedAsync", "HasCompletedQuestAsync", "IsQuestCompleted", "HasCompletedQuest" },
					questId, cancellationToken);
				if (isCompleted)
				{
					return ParseQuestStateName("Completed");
				}

				bool isActive = await TryQueryQuestFlagAsync(
					new[] { "IsQuestActiveAsync", "HasActiveQuestAsync", "IsQuestActive", "HasActiveQuest" }, questId,
					cancellationToken);
				if (isActive)
				{
					Graph.Core.Runtime.Nodes.Server.QuestState activeState = ParseQuestStateName("Active");
					if (!Equals(activeState, default(Graph.Core.Runtime.Nodes.Server.QuestState)))
					{
						return activeState;
					}

					activeState = ParseQuestStateName("InProgress");
					if (!Equals(activeState, default(Graph.Core.Runtime.Nodes.Server.QuestState)))
					{
						return activeState;
					}

					activeState = ParseQuestStateName("Started");
					if (!Equals(activeState, default(Graph.Core.Runtime.Nodes.Server.QuestState)))
					{
						return activeState;
					}
				}

				if (TryGetQuestStateFromPlayerSync(questId, out Graph.Core.Runtime.Nodes.Server.QuestState syncState))
				{
					return syncState;
				}

				return default;
			}

			private bool TryGetQuestStateFromPlayerSync(string questId, out Graph.Core.Runtime.Nodes.Server.QuestState state)
			{
				state = default;
				if (string.IsNullOrWhiteSpace(questId))
				{
					return false;
				}

				if (m_logContext is not Npcmanager npcManager || npcManager.bootstrap == null)
				{
					return false;
				}

				object playerStateSync = GetMemberValue(npcManager.bootstrap, "PlayerStateSync");
				if (playerStateSync == null)
				{
					return false;
				}

				if (TryGetQuestFlagFromStateSync(playerStateSync, "IsQuestCompleted", questId, out bool isCompleted) &&
				    isCompleted)
				{
					state = ParseQuestStateName("Completed");
					return true;
				}

				if (TryGetQuestFlagFromStateSync(playerStateSync, "IsQuestActive", questId, out bool isActive) && isActive)
				{
					state = ParseQuestStateName("Active");
					return true;
				}

				return false;
			}

			private static bool TryGetQuestFlagFromStateSync(
				object playerStateSync,
				string methodName,
				string questId,
				out bool value)
			{
				value = false;
				if (playerStateSync == null || string.IsNullOrWhiteSpace(methodName))
				{
					return false;
				}

				MethodInfo method = playerStateSync.GetType().GetMethod(methodName, s_instancePublicAndNonPublic, null,
					new[] { typeof(string) }, null);
				if (method == null)
				{
					return false;
				}

				object result = method.Invoke(playerStateSync, new object[] { questId });
				if (result is bool typedBool)
				{
					value = typedBool;
					return true;
				}

				return false;
			}

			private async UniTask<bool> TryQueryQuestFlagAsync(
				string[] methodNames,
				string questId,
				CancellationToken cancellationToken)
			{
				object invocationResult = InvokeQuestMethod(methodNames, questId, cancellationToken);
				if (invocationResult == null)
				{
					return false;
				}

				return await ConvertToBoolAsync(invocationResult);
			}

			private object InvokeQuestMethod(string[] methodNames, string questId, CancellationToken cancellationToken)
			{
				for (var i = 0; i < methodNames.Length; i++)
				{
					MethodInfo method = m_gameServer.GetType().GetMethod(methodNames[i], s_instancePublicAndNonPublic, null,
						new[] { typeof(string), typeof(CancellationToken) }, null);
					if (method != null)
					{
						return method.Invoke(m_gameServer, new object[] { questId, cancellationToken });
					}

					method = m_gameServer.GetType().GetMethod(methodNames[i], s_instancePublicAndNonPublic, null,
						new[] { typeof(string) }, null);
					if (method != null)
					{
						return method.Invoke(m_gameServer, new object[] { questId });
					}
				}

				return null;
			}

			private async UniTask TryRefreshProfileAsync(CancellationToken cancellationToken)
			{
				if (m_logContext is not Npcmanager npcManager || npcManager.bootstrap == null)
				{
					return;
				}

				object bootstrap = npcManager.bootstrap;
				object playerStateSync = GetMemberValue(bootstrap, "PlayerStateSync");

				bool refreshed = await TryInvokeRefreshAsync(playerStateSync, cancellationToken);
				if (refreshed)
				{
					return;
				}

				refreshed = await TryInvokeRefreshAsync(bootstrap, cancellationToken);
				if (refreshed)
				{
				}
			}

			private static object GetMemberValue(object target, string memberName)
			{
				if (target == null || string.IsNullOrWhiteSpace(memberName))
				{
					return null;
				}

				PropertyInfo property = target.GetType().GetProperty(memberName, s_instancePublicAndNonPublic);
				if (property != null)
				{
					return property.GetValue(target);
				}

				FieldInfo field = target.GetType().GetField(memberName, s_instancePublicAndNonPublic);
				if (field != null)
				{
					return field.GetValue(target);
				}

				return null;
			}

			private static async UniTask<bool> TryInvokeRefreshAsync(object target, CancellationToken cancellationToken)
			{
				if (target == null)
				{
					return false;
				}

				string[] methodNames =
				{
					"RefreshAsync",
					"Refresh",
					"RefreshProfileAsync",
					"RefreshProfile",
					"RequestRefreshAsync",
					"RequestRefresh",
					"ReloadAsync",
					"Reload",
					"SyncAsync",
					"Sync"
				};

				for (var i = 0; i < methodNames.Length; i++)
				{
					MethodInfo method = target.GetType().GetMethod(methodNames[i], s_instancePublicAndNonPublic, null,
						new[] { typeof(CancellationToken) }, null);
					if (method != null)
					{
						object result = method.Invoke(target, new object[] { cancellationToken });
						await AwaitNonGenericAsync(result);
						return true;
					}

					method = target.GetType().GetMethod(methodNames[i], s_instancePublicAndNonPublic, null, Type.EmptyTypes,
						null);
					if (method != null)
					{
						object result = method.Invoke(target, null);
						await AwaitNonGenericAsync(result);
						return true;
					}
				}

				return false;
			}

			private static async UniTask AwaitNonGenericAsync(object result)
			{
				if (result == null)
				{
					return;
				}

				if (result is UniTask uniTask)
				{
					await uniTask;
					return;
				}

				if (result is Task task)
				{
					await task;
					return;
				}

				if (result is ValueTask valueTask)
				{
					await valueTask;
				}
			}

			private bool TryApplyProfileSnapshot(object profileSnapshot)
			{
				if (profileSnapshot == null)
				{
					return false;
				}

				if (m_logContext is not Npcmanager npcManager || npcManager.bootstrap == null)
				{
					return false;
				}

				object bootstrap = npcManager.bootstrap;
				object profileSyncService = GetMemberValue(bootstrap, "ProfileSyncService");
				if (TryInvokeApplySnapshot(profileSyncService, profileSnapshot))
				{
					return true;
				}

				object playerStateSync = GetMemberValue(bootstrap, "PlayerStateSync");
				if (TryInvokeApplySnapshot(playerStateSync, profileSnapshot))
				{
					return true;
				}

				return false;
			}

			private static bool TryInvokeApplySnapshot(object target, object snapshot)
			{
				if (target == null || snapshot == null)
				{
					return false;
				}

				Type snapshotType = snapshot.GetType();
				MethodInfo method = target.GetType().GetMethod("ApplySnapshot", s_instancePublicAndNonPublic, null,
					new[] { snapshotType }, null);
				if (method != null)
				{
					method.Invoke(target, new[] { snapshot });
					return true;
				}

				MethodInfo[] methods = target.GetType().GetMethods(s_instancePublicAndNonPublic);
				for (var i = 0; i < methods.Length; i++)
				{
					MethodInfo candidate = methods[i];
					if (!string.Equals(candidate.Name, "ApplySnapshot", StringComparison.Ordinal))
					{
						continue;
					}

					ParameterInfo[] parameters = candidate.GetParameters();
					if (parameters.Length != 1 || !parameters[0].ParameterType.IsInstanceOfType(snapshot))
					{
						continue;
					}

					candidate.Invoke(target, new[] { snapshot });
					return true;
				}

				return false;
			}

			private static async UniTask<QuestActionOutcome> ConvertToQuestActionOutcomeAsync(object result)
			{
				if (result == null)
				{
					return QuestActionOutcome.failed;
				}

				if (result is bool directBool)
				{
					return directBool ? QuestActionOutcome.successWithoutSnapshot : QuestActionOutcome.failed;
				}

				if (result is UniTask<bool> uniTaskBool)
				{
					bool success = await uniTaskBool;
					return success ? QuestActionOutcome.successWithoutSnapshot : QuestActionOutcome.failed;
				}

				if (result is Task<bool> taskBool)
				{
					bool success = await taskBool;
					return success ? QuestActionOutcome.successWithoutSnapshot : QuestActionOutcome.failed;
				}

				if (result is ValueTask<bool> valueTaskBool)
				{
					bool success = await valueTaskBool;
					return success ? QuestActionOutcome.successWithoutSnapshot : QuestActionOutcome.failed;
				}

				if (result is UniTask uniTask)
				{
					await uniTask;
					return QuestActionOutcome.failed;
				}

				if (result is Task task)
				{
					await task;

					PropertyInfo resultProperty = task.GetType().GetProperty("Result", s_instancePublicAndNonPublic);
					if (resultProperty != null)
					{
						object taskResult = resultProperty.GetValue(task);
						return await ConvertToQuestActionOutcomeAsync(taskResult);
					}

					return QuestActionOutcome.failed;
				}

				if (result is ValueTask valueTask)
				{
					await valueTask;
					return QuestActionOutcome.failed;
				}

				if (TryExtractSuccessAndSnapshot(result, out bool successValue, out object snapshot))
				{
					return successValue
						? new QuestActionOutcome(true, snapshot)
						: QuestActionOutcome.failed;
				}

				return QuestActionOutcome.failed;
			}

			private static async UniTask<bool> ConvertToBoolAsync(object result)
			{
				QuestActionOutcome outcome = await ConvertToQuestActionOutcomeAsync(result);
				return outcome.success;
			}

			private static bool TryExtractSuccessAndSnapshot(object result, out bool success, out object profileSnapshot)
			{
				success = false;
				profileSnapshot = null;

				if (result == null)
				{
					return false;
				}

				Type resultType = result.GetType();
				PropertyInfo successProperty = resultType.GetProperty("Success", s_instancePublicAndNonPublic);
				if (successProperty == null || successProperty.PropertyType != typeof(bool))
				{
					return false;
				}

				object successObject = successProperty.GetValue(result);
				if (successObject is not bool typedSuccess)
				{
					return false;
				}

				success = typedSuccess;

				PropertyInfo snapshotProperty = resultType.GetProperty("ProfileSnapshot", s_instancePublicAndNonPublic);
				if (snapshotProperty != null)
				{
					profileSnapshot = snapshotProperty.GetValue(result);
				}

				return true;
			}

			private static async UniTask<Graph.Core.Runtime.Nodes.Server.QuestState> ConvertToQuestStateAsync(object result)
			{
				if (result == null)
				{
					return default;
				}

				if (result is Graph.Core.Runtime.Nodes.Server.QuestState directState)
				{
					return directState;
				}

				if (result is UniTask<Graph.Core.Runtime.Nodes.Server.QuestState> uniTaskState)
				{
					return await uniTaskState;
				}

				if (result is Task<Graph.Core.Runtime.Nodes.Server.QuestState> taskState)
				{
					return await taskState;
				}

				if (result is ValueTask<Graph.Core.Runtime.Nodes.Server.QuestState> valueTaskState)
				{
					return await valueTaskState;
				}

				Type resultType = result.GetType();
				if (resultType.IsEnum)
				{
					return ParseQuestStateName(result.ToString());
				}

				return default;
			}

			private static Graph.Core.Runtime.Nodes.Server.QuestState ParseQuestStateName(string stateName)
			{
				if (string.IsNullOrWhiteSpace(stateName))
				{
					return default;
				}

				return Enum.TryParse(stateName, true, out Graph.Core.Runtime.Nodes.Server.QuestState parsedState)
					? parsedState
					: default;
			}

			private readonly struct QuestActionOutcome
			{
				public static readonly QuestActionOutcome failed = new(false, null);
				public static readonly QuestActionOutcome successWithoutSnapshot = new(true, null);

				public readonly bool success;
				public readonly object profileSnapshot;

				public QuestActionOutcome(bool success, object profileSnapshot)
				{
					this.success = success;
					this.profileSnapshot = profileSnapshot;
				}
			}
		}
	}
}
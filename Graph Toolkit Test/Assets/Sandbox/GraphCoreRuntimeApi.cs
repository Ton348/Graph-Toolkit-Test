using System;
using UnityEngine;

namespace Graph.Core.Runtime
{
    public class Graph : global::BusinessQuestGraph
    {
    }

    [Serializable]
    public abstract class GraphNode : global::BaseGraphNode
    {
    }

    public class GraphRunner : global::BusinessQuestGraphRunner
    {
        public GraphRunner(
            global::BusinessQuestGraph graph,
            global::GameBootstrap bootstrap,
            global::IGameServer gameServer,
            global::DialogueService dialogueService,
            global::ChoiceUIService choiceUIService,
            global::TradeOfferUIService tradeOfferUIService,
            global::MapMarkerService mapMarkerService,
            Transform playerTransform,
            global::GraphProgressService graphProgressService)
            : base(graph, bootstrap, gameServer, dialogueService, choiceUIService, tradeOfferUIService, mapMarkerService, playerTransform, graphProgressService)
        {
        }
    }

    public sealed class GraphExecutionContext
    {
        private readonly global::GraphExecutionContext m_context;

        public GraphExecutionContext(global::IGraphRuntimeServices services = null)
        {
            m_context = new global::GraphExecutionContext(services);
        }

        public GraphExecutionContext(global::GraphExecutionContext context)
        {
            m_context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public global::GraphExecutionContext RuntimeContext => m_context;

        public global::IGraphRuntimeServices Services => m_context.Services;

        public global::IGraphDialogueService DialogueService => m_context.DialogueService;

        public global::IGraphChoiceService ChoiceService => m_context.ChoiceService;

        public global::IGraphMapMarkerService MapMarkerService => m_context.MapMarkerService;

        public global::IGraphCutsceneService CutsceneService => m_context.CutsceneService;

        public global::IGraphCheckpointService CheckpointService => m_context.CheckpointService;

        public global::IGraphQuestService QuestService => m_context.QuestService;

        public void Set<T>(GraphContextKey<T> key, T value)
        {
            if (key == null)
            {
                return;
            }

            m_context.Set(new global::GraphContextKey<T>(key.Id), value);
        }

        public bool TryGet<T>(GraphContextKey<T> key, out T value)
        {
            if (key == null)
            {
                value = default;
                return false;
            }

            return m_context.TryGet(new global::GraphContextKey<T>(key.Id), out value);
        }

        public bool Has<T>(GraphContextKey<T> key)
        {
            return key != null && m_context.Contains(new global::GraphContextKey<T>(key.Id));
        }

        public bool Remove<T>(GraphContextKey<T> key)
        {
            return key != null && m_context.Remove(new global::GraphContextKey<T>(key.Id));
        }

        public void Clear()
        {
            m_context.Clear();
        }
    }

    public class InteractionContext : global::InteractionContext
    {
    }

    public enum InteractionContextType
    {
        Normal = global::InteractionContextType.Normal,
        Steal = global::InteractionContextType.Steal
    }

    public class GraphContextKey : global::GraphContextKey
    {
        public GraphContextKey(string id, Type valueType) : base(id, valueType)
        {
        }
    }

    public sealed class GraphContextKey<T> : GraphContextKey
    {
        public GraphContextKey(string id) : base(id, typeof(T))
        {
        }
    }

    public static class GraphContextKeys
    {
        public static readonly GraphContextKey<int> ChoiceLastIndex = new GraphContextKey<int>(global::GraphContextKeys.ChoiceLastIndex.Id);
        public static readonly GraphContextKey<string> ChoiceLastLabel = new GraphContextKey<string>(global::GraphContextKeys.ChoiceLastLabel.Id);
        public static readonly GraphContextKey<bool> ConditionLastResult = new GraphContextKey<bool>(global::GraphContextKeys.ConditionLastResult.Id);
        public static readonly GraphContextKey<global::ServerActionResult> ServerLastResult = new GraphContextKey<global::ServerActionResult>(global::GraphContextKeys.ServerLastResult.Id);
        public static readonly GraphContextKey<string> BuildingLastRequestedId = new GraphContextKey<string>(global::GraphContextKeys.BuildingLastRequestedId.Id);
        public static readonly GraphContextKey<string> QuestLastRequestedId = new GraphContextKey<string>(global::GraphContextKeys.QuestLastRequestedId.Id);
    }
}

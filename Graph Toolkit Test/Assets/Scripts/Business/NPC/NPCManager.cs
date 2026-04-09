using UnityEngine;
using UnityEngine.Serialization;

public class NPCManager : Interactable
{
    [FormerlySerializedAs("questGraph")]
    public BusinessQuestGraph dialogueGraph;
    public BusinessQuestGraph stealGraph;
    public BaseGraph baseDialogueGraph;
    public BaseGraph baseStealGraph;
    public bool allowSteal = true;
    public Transform lookForwardReference;
    public float stealDistance = 1.5f;
    public float stealBackAngle = 120f;
    public float stealMinBackAngle = 40f;
    public string ownerId;
    public GameBootstrap bootstrap;
    public DialogueService dialogueService;
    public ChoiceUIService choiceUIService;
    public TradeOfferUIService tradeOfferUIService;
    public MapMarkerService mapMarkerService;
    public Transform playerTransform;
    private BusinessQuestGraphRunner runner;
    private BusinessQuestGraph currentGraph;
    private BaseGraphRunner baseRunner;
    private BaseGraph currentBaseGraph;

    public override void Interact(Transform player)
    {
        var selectedBaseGraph = SelectBaseGraph(player);
        if (selectedBaseGraph != null)
        {
            StartBaseGraph(selectedBaseGraph);
            return;
        }

        var selectedGraph = SelectGraph(player);
        if (selectedGraph == null)
        {
            return;
        }

        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (bootstrap == null)
        {
            return;
        }

        if (playerTransform == null && player != null)
        {
            playerTransform = player;
        }

        if (tradeOfferUIService == null)
        {
            tradeOfferUIService = FindObjectOfType<TradeOfferUIService>();
        }

        if (runner != null && runner.IsRunning)
        {
            Debug.Log($"[NPCManager] Interact ignored because graph is already running on '{name}'.");
            return;
        }

        bool shouldRecreateRunner = runner == null || currentGraph != selectedGraph || (tradeOfferUIService != null && !runner.HasTradeOfferUI);

        if (shouldRecreateRunner)
        {
            if (playerTransform == null)
            {
                var playerMovement = FindObjectOfType<PlayerMovement>();
                if (playerMovement != null)
                {
                    playerTransform = playerMovement.transform;
                }
            }

            runner = new BusinessQuestGraphRunner(
                selectedGraph,
                bootstrap,
                bootstrap.GameServer,
                dialogueService,
                choiceUIService,
                tradeOfferUIService,
                mapMarkerService,
                playerTransform,
                bootstrap.GraphProgressService
            );
            currentGraph = selectedGraph;
        }

        runner.Start(BuildInteractionContext(player));
    }

    public override void Interact()
    {
        Interact(null);
    }

    private BusinessQuestGraph SelectGraph(Transform player)
    {
        bool canSteal = allowSteal && stealGraph != null && StealContextEvaluator.CanStealFromNpc(player, this);

        if (canSteal)
        {
            return stealGraph;
        }

        if (dialogueGraph != null)
        {
            return dialogueGraph;
        }

        if (stealGraph != null)
        {
            return stealGraph;
        }

        return null;
    }

    private BaseGraph SelectBaseGraph(Transform player)
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

    private void StartBaseGraph(BaseGraph graph)
    {
        if (graph == null)
        {
            return;
        }

        if (!HasGraphContent(graph))
        {
            Debug.LogError($"[NPCManager] BaseGraph '{graph.name}' does not contain runtime nodes on '{name}'.", this);
            return;
        }

        if (baseRunner != null && baseRunner.IsRunning)
        {
            Debug.Log($"[NPCManager] BaseGraph interact ignored because graph is already running on '{name}'.");
            return;
        }

        if (baseRunner == null || currentBaseGraph != graph)
        {
            baseRunner = new BaseGraphRunner(BaseGraphRuntimeComposition.CreateDefaultRegistry());
            currentBaseGraph = graph;
        }

        GraphExecutionContext context = new GraphExecutionContext(
            new GraphRuntimeServices(
                dialogueService,
                choiceUIService,
                null,
                null,
                null,
                null));

        _ = baseRunner.RunAsync(graph, context);
    }

    private static bool HasGraphContent(BaseGraph graph)
    {
        return graph != null && graph.nodes != null && graph.nodes.Count > 0;
    }

    private InteractionContext BuildInteractionContext(Transform player)
    {
        if (allowSteal && stealGraph != null && StealContextEvaluator.CanStealFromNpc(player, this))
        {
            return new InteractionContext
            {
                contextType = InteractionContextType.Steal,
                sourceNpc = this
            };
        }

        return new InteractionContext
        {
            contextType = InteractionContextType.Normal,
            sourceNpc = this
        };
    }

    private void Update()
    {
        if (runner != null && runner.IsRunning)
        {
            runner.Tick();
        }
    }
}

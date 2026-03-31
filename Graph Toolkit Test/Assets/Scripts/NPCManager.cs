using UnityEngine;
using UnityEngine.Serialization;

public class NPCManager : Interactable
{
    [FormerlySerializedAs("questGraph")]
    public BusinessQuestGraph dialogueGraph;
    public BusinessQuestGraph stealGraph;
    public bool allowSteal = true;
    public Transform lookForwardReference;
    public float stealDistance = 1.5f;
    public float stealBackAngle = 120f;
    public float stealMinBackAngle = 40f;
    public string ownerId;
    public GameBootstrap bootstrap;
    public DialogueService dialogueService;
    public ChoiceUIService choiceUIService;
    public MapMarkerService mapMarkerService;
    public Transform playerTransform;
    private BusinessQuestGraphRunner runner;
    private BusinessQuestGraph currentGraph;

    public override void Interact(Transform player)
    {
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

        if (runner != null && runner.IsRunning)
        {
            Debug.Log($"[NPCManager] Interact ignored because graph is already running on '{name}'.");
            return;
        }

        if (runner == null || currentGraph != selectedGraph)
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

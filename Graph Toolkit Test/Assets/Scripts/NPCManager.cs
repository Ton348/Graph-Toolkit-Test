using UnityEngine;

public class NPCManager : Interactable
{
    public BusinessQuestGraph questGraph;
    public GameBootstrap bootstrap;
    public DialogueService dialogueService;
    public ChoiceUIService choiceUIService;
    public MapMarkerService mapMarkerService;
    public Transform playerTransform;
    private BusinessQuestGraphRunner runner;

    public override void Interact()
    {
        if (questGraph == null)
        {
            Debug.LogWarning("BusinessQuestGraph не назначен");
            return;
        }

        if (bootstrap == null)
        {
            bootstrap = FindObjectOfType<GameBootstrap>();
        }

        if (bootstrap == null || bootstrap.QuestService == null || bootstrap.EventBus == null)
        {
            Debug.LogWarning("GameBootstrap не готов");
            return;
        }

        if (runner == null || !runner.IsRunning)
        {
            if (playerTransform == null)
            {
                var player = FindObjectOfType<PlayerMovement>();
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            runner = new BusinessQuestGraphRunner(
                questGraph,
                bootstrap.RuntimeState,
                bootstrap.QuestService,
                bootstrap.PlayerService,
                bootstrap.RuntimeState.Player,
                bootstrap.EventBus,
                dialogueService,
                choiceUIService,
                mapMarkerService,
                playerTransform
            );
        }

        runner.Start();
    }

    private void Update()
    {
        if (runner != null && runner.IsRunning)
        {
            runner.Tick();
        }
    }
}

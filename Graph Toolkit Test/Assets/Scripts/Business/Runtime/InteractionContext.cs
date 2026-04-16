public enum InteractionContextType
{
    Normal,
    Steal
}

public class InteractionContext
{
    public InteractionContextType contextType;
    public Npcmanager sourceNpc;
}

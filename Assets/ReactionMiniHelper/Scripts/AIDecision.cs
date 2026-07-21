using System;

public enum HelperReactionType
{
    Success,
    FocusError,
    Critical,
    ResourceWarning
}

[Serializable]
public class AIModelResult
{
    public bool esEficaz;
    public bool esEficiente;
}

public class AIDecision
{
    public HelperReactionType ReactionType { get; private set; }
    public string DialogueText { get; private set; }

    public AIDecision(HelperReactionType reactionType, string dialogueText)
    {
        ReactionType = reactionType;
        DialogueText = dialogueText;
    }
}
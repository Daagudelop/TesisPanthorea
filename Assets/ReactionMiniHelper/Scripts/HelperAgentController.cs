using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

public class HelperAgentController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TextMeshProUGUI dialogueText;

    [Header("Dialogue")]
    [SerializeField] private float characterDelay = 0.025f;
    [SerializeField] private float afterReactionDelay = 3f;

    [Header("Animation")]
    [SerializeField] private string idleStateName = "IdleBase";
    [SerializeField] private int reactionRepeatCount = 3;

    private readonly List<AIDecision> pendingReactions = new();

    private Coroutine reactionRunner;

    private bool animationFinished;
    private bool typingFinished;

    private int completedReactionLoops = 0;
    private bool isPlayingReaction = false;

    public bool IsBusy
    {
        get
        {
            return reactionRunner != null || isPlayingReaction;
        }
    }

    private void OnEnable()
    {
        AIGameEvents.OnDecisionGenerated += HandleDecisionGenerated;
    }

    private void OnDisable()
    {
        AIGameEvents.OnDecisionGenerated -= HandleDecisionGenerated;
    }

    private void HandleDecisionGenerated(AIDecision decision)
    {
        pendingReactions.Add(decision);

        if (reactionRunner == null)
        {
            reactionRunner = StartCoroutine(ProcessReactions());
        }
    }

    private IEnumerator ProcessReactions()
    {
        while (pendingReactions.Count > 0)
        {
            AIDecision decision = pendingReactions[0];
            pendingReactions.RemoveAt(0);

            yield return PlayReaction(decision);
        }

        reactionRunner = null;
    }

    private IEnumerator PlayReaction(AIDecision decision)
    {
        dialogueBox.SetActive(true);
        dialogueText.text = "";

        animationFinished = false;
        typingFinished = false;

        completedReactionLoops = 0;
        isPlayingReaction = true;

        string animationTrigger = GetAnimationTrigger(decision.ReactionType);

        ResetReactionTriggers();

        animator.SetTrigger(animationTrigger);

        StartCoroutine(TypeDialogue(decision.DialogueText));

        while (!animationFinished || !typingFinished)
        {
            yield return null;
        }

        yield return new WaitForSeconds(afterReactionDelay);

        isPlayingReaction = false;

        animator.CrossFade(idleStateName, 0.15f);

        dialogueText.text = "";
        dialogueBox.SetActive(false);
    }

    private IEnumerator TypeDialogue(string text)
    {
        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < text.Length; i++)
        {
            char character = text[i];

            if (character == '<')
            {
                int closingIndex = text.IndexOf('>', i);

                if (closingIndex != -1)
                {
                    string richTextTag = text.Substring(i, closingIndex - i + 1);
                    builder.Append(richTextTag);

                    i = closingIndex;
                    dialogueText.text = builder.ToString();

                    continue;
                }
            }

            builder.Append(character);
            dialogueText.text = builder.ToString();

            yield return new WaitForSeconds(characterDelay);
        }

        typingFinished = true;
    }

    public void OnReactionAnimationLoopCompleted()
    {
        if (!isPlayingReaction || animationFinished)
        {
            return;
        }

        completedReactionLoops++;

        Debug.Log("Ciclo de animación completado: " + completedReactionLoops);

        if (completedReactionLoops >= reactionRepeatCount)
        {
            animationFinished = true;
        }
    }

    private string GetAnimationTrigger(HelperReactionType reactionType)
    {
        switch (reactionType)
        {
            case HelperReactionType.Success:
                return "Success";

            case HelperReactionType.FocusError:
                return "FocusError";

            case HelperReactionType.Critical:
                return "Critical";

            case HelperReactionType.ResourceWarning:
                return "ResourceWarning";

            default:
                return "Success";
        }
    }

    private void ResetReactionTriggers()
    {
        animator.ResetTrigger("Success");
        animator.ResetTrigger("FocusError");
        animator.ResetTrigger("Critical");
        animator.ResetTrigger("ResourceWarning");
    }
}
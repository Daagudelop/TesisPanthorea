using System;

public static class AIGameEvents
{
    public static event Action<AIDecision> OnDecisionGenerated;

    public static void RaiseDecisionGenerated(AIDecision decision)
    {
        OnDecisionGenerated?.Invoke(decision);
    }

    public static void RaiseReactionFromEvaluation(bool esEficaz, bool esEficiente)
    {
        AIModelResult result = new AIModelResult
        {
            esEficaz = esEficaz,
            esEficiente = esEficiente
        };

        AIDecision decision = AIResponseEvaluator.Evaluate(result);

        RaiseDecisionGenerated(decision);
    }
}
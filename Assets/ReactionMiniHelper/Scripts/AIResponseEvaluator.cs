public static class AIResponseEvaluator
{
    public static AIDecision Evaluate(AIModelResult result)
    {
        bool esEficaz = result.esEficaz;
        bool esEficiente = result.esEficiente;

        if (esEficaz && esEficiente)
        {
            return new AIDecision(
                HelperReactionType.Success,
                "<color=#04910B>Éxito total</color>, se archiva."
            );
        }

        if (!esEficaz && esEficiente)
        {
            return new AIDecision(
                HelperReactionType.FocusError,
                "Error de enfoque, debe <color=#B8860B>reevaluarlo</color> en las siguientes fases."
            );
        }

        if (!esEficaz && !esEficiente)
        {
            return new AIDecision(
                HelperReactionType.Critical,
                "Alerta crítica. ¡¡Pasa <color=#FF0000>INMEDIATAMENTE</color> a las siguientes fases!!"
            );
        }

        return new AIDecision(
            HelperReactionType.ResourceWarning,
            "<color=#C48600>Costosa</color> pero sirvió... te recomiendo reevaluar en la siguiente fase."
        );
    }
}
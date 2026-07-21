using UnityEngine;
using UnityEngine.InputSystem;

public class LocalAIResponseHandler : MonoBehaviour
{
    [Header("Testing")]
    [SerializeField] private HelperAgentController helperAgent;
    [SerializeField] private bool ignoreTestInputWhileHelperBusy = true;

    public void ReceiveModelResponse(string jsonResponse)
    {
        AIModelResult result = JsonUtility.FromJson<AIModelResult>(jsonResponse);

        AIDecision decision = AIResponseEvaluator.Evaluate(result);

        AIGameEvents.RaiseDecisionGenerated(decision);
    }

    private void Update()
    {
        /*if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.digit1Key.wasPressedThisFrame)
        {
            SendTestResponse("{\"esEficaz\":true,\"esEficiente\":true}");
        }

        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            SendTestResponse("{\"esEficaz\":false,\"esEficiente\":true}");
        }

        if (Keyboard.current.digit3Key.wasPressedThisFrame)
        {
            SendTestResponse("{\"esEficaz\":false,\"esEficiente\":false}");
        }

        if (Keyboard.current.digit4Key.wasPressedThisFrame)
        {
            SendTestResponse("{\"esEficaz\":true,\"esEficiente\":false}");
        }*/
    }

    public void SendTestResponse(string jsonResponse)
    {
        if (ignoreTestInputWhileHelperBusy && helperAgent != null && helperAgent.IsBusy)
        {
            Debug.Log("Helper ocupado. Input de prueba ignorado.");
            return;
        }

        ReceiveModelResponse(jsonResponse);
    }
}
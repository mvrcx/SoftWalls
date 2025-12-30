using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    public GameObject stopButton;
    public GameObject nextButton;

    public ExperimentManager experimentManager;

    // Todos os scripts de movimento que devem ser bloqueados ao clicar Stop
    public MonoBehaviour[] movementScripts;

    void Start()
    {
        stopButton.SetActive(true);
        nextButton.SetActive(false);
    }

    public void OnStopClicked()
    {
        // Bloqueia movimento
        foreach (var script in movementScripts)
        {
            if (script != null)
                script.enabled = false;
        }

        stopButton.SetActive(false);
        nextButton.SetActive(true);
    }

    public void OnNextClicked()
    {
        // Libera movimento
        foreach (var script in movementScripts)
        {
            if (script != null)
                script.enabled = true;
        }

        nextButton.SetActive(false);

        if (experimentManager != null)
            experimentManager.GoToNextRoom();

        stopButton.SetActive(true);
    }
}

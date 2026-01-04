using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    public GameObject stopButton;
    public GameObject nextButton;

    public ExperimentManager experimentManager;
    public MonoBehaviour[] movementScripts;

    void Start()
    {
        stopButton.SetActive(true);
        nextButton.SetActive(false);
    }

    public void OnStopClicked()
    {
        if (experimentManager != null && experimentManager.experimentFinished)
            return;

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
        if (experimentManager != null && experimentManager.experimentFinished)
            return;

        foreach (var script in movementScripts)
        {
            if (script != null)
                script.enabled = true;
        }

        nextButton.SetActive(false);

        experimentManager.GoToNextRoom();

        if (!experimentManager.experimentFinished)
            stopButton.SetActive(true);
    }
}

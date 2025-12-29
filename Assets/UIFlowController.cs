using TMPro;
using UnityEngine;

public class UIFlowController : MonoBehaviour
{
    public GameObject stopButton;
    public GameObject nextButton;
    public TextMeshProUGUI nextLabel;

    public ExperimentManager experimentManager;
    public GameObject locomotionSystem;

    void Start()
    {
        stopButton.SetActive(true);
        nextButton.SetActive(false);
    }

    public void OnStopClicked()
    {
        locomotionSystem.SetActive(false);
        stopButton.SetActive(false);

        nextLabel.text = "Next: " + experimentManager.GetNextRoomName();
        nextButton.SetActive(true);
    }

    public void OnNextClicked()
    {
        nextButton.SetActive(false);
        locomotionSystem.SetActive(true);

        experimentManager.GoToNextRoom();
        stopButton.SetActive(true);
    }
}

using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExperimentManager : MonoBehaviour
{
    [Header("Participant")]
    public string participantId;

    [Header("XR")]
    public Transform xrOrigin;

    [Header("Rooms")]
    public RoomController testingRoom;
    public List<RoomController> experimentalRooms;

    [Header("Logger")]
    public ObjectiveMetricsLogger logger; // apenas um logger compartilhado

    private List<RoomController> randomizedRooms;
    private int currentIndex = -1;
    private bool experimentFinished = false;

    [Header("UI")]
    public GameObject stopButton;
    public GameObject nextRoomButton;
    public TextMeshProUGUI roomNameText;

    [Header("End Message")]
    public Canvas endMessageCanvas;
    public TextMeshProUGUI endMessageText;

    void Start()
    {
        randomizedRooms = new List<RoomController>(experimentalRooms);
        Shuffle(randomizedRooms);

        EnterTestingRoom();

        stopButton.SetActive(true);
        nextRoomButton.SetActive(false);

        if (roomNameText != null)
            roomNameText.text = testingRoom.roomName;

        if (endMessageCanvas != null)
            endMessageCanvas.gameObject.SetActive(false);
    }

    void Shuffle(List<RoomController> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int r = Random.Range(i, list.Count);
            (list[i], list[r]) = (list[r], list[i]);
        }
    }

    void EnterTestingRoom()
    {
        testingRoom.Activate(xrOrigin);
    }

    public void GoToNextRoom()
    {
        if (experimentFinished)
            return;

        // Para o logger do quarto atual
        if (currentIndex >= 0 && currentIndex < randomizedRooms.Count)
            logger.StopRecordingAndSave();

        // Desativa o quarto atual
        if (currentIndex >= 0)
            randomizedRooms[currentIndex].Deactivate();
        else
            testingRoom.Deactivate();

        currentIndex++;

        if (currentIndex < randomizedRooms.Count)
        {
            RoomController room = randomizedRooms[currentIndex];
            room.Activate(xrOrigin);

            if (!string.IsNullOrEmpty(participantId) && logger != null)
            {
                logger.SetRoom(room.gameObject, room.roomName);
                logger.StartRecording(participantId);
            }

            stopButton.SetActive(true);
            nextRoomButton.SetActive(false);

            if (roomNameText != null)
                roomNameText.text = room.roomName;

            Debug.Log("Going to room: " + room.roomName);
        }
        else
        {
            // Fim do experimento
            experimentFinished = true;

            testingRoom.Activate(xrOrigin);

            stopButton.SetActive(false);
            nextRoomButton.SetActive(false);

            if (roomNameText != null)
                roomNameText.gameObject.SetActive(false);

            if (endMessageCanvas != null && endMessageText != null)
            {
                endMessageCanvas.gameObject.SetActive(true);
                endMessageText.text = "The end of the experiment! Thank you!";
            }

            Debug.Log("Experiment finished");
        }
    }

    public void StopRoom()
    {
        if (experimentFinished)
            return;

        // Se não houver quartos para Stop
        if (currentIndex < 0 || currentIndex >= randomizedRooms.Count)
            return;

        logger.StopRecordingAndSave();

        // Se for último quarto
        if (currentIndex == randomizedRooms.Count - 1)
        {
            stopButton.SetActive(false);
            nextRoomButton.SetActive(false);
        }
        else
        {
            stopButton.SetActive(false);
            nextRoomButton.SetActive(true);
        }
    }
}

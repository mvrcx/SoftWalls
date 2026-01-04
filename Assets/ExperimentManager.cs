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
    public ObjectiveMetricsLogger logger;

    private List<RoomController> randomizedRooms;
    private int currentIndex = -1;

    public bool experimentFinished { get; private set; } = false;

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

        experimentFinished = false;
        currentIndex = -1;

        EnterTestingRoom();

        stopButton.SetActive(true);
        nextRoomButton.SetActive(false);

        if (roomNameText != null)
        {
            roomNameText.text = testingRoom.roomName;
            roomNameText.gameObject.SetActive(true);
        }

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

        if (currentIndex >= 0 && currentIndex < randomizedRooms.Count)
            logger?.StopRecordingAndSave();

        if (currentIndex == -1)
            testingRoom.Deactivate();
        else if (currentIndex < randomizedRooms.Count)
            randomizedRooms[currentIndex].Deactivate();

        currentIndex++;

        if (currentIndex < randomizedRooms.Count)
        {
            RoomController room = randomizedRooms[currentIndex];
            room.Activate(xrOrigin);

            if (logger != null && !string.IsNullOrEmpty(participantId))
            {
                logger.SetRoom(room.gameObject, room.roomName);
                logger.StartRecording(participantId);
            }

            stopButton.SetActive(true);
            nextRoomButton.SetActive(false);

            if (roomNameText != null)
            {
                roomNameText.text = room.roomName;
                roomNameText.gameObject.SetActive(true);
            }

            Debug.Log("Going to room: " + room.roomName);
        }
        else
        {
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

        if (currentIndex < 0 || currentIndex >= randomizedRooms.Count)
            return;

        logger?.StopRecordingAndSave();

        stopButton.SetActive(false);
        nextRoomButton.SetActive(true);
    }
}

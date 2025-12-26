using System.Collections.Generic;
using UnityEngine;

public class ExperimentManager : MonoBehaviour
{
    public Transform xrOrigin;

    public RoomController testingRoom;
    public List<RoomController> experimentalRooms;

    private List<RoomController> randomizedRooms;
    private int currentIndex = -1;
    public GameObject stopButton;
    public GameObject nextRoomButton;

    void Start()
    {
        randomizedRooms = new List<RoomController>(experimentalRooms);
        Shuffle(randomizedRooms);

        EnterTestingRoom();
        stopButton.SetActive(true);
        nextRoomButton.SetActive(false);
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
        if (currentIndex == -1)
        {
            testingRoom.Deactivate();
        }
        else
        {
            randomizedRooms[currentIndex].Deactivate();
        }

        currentIndex++;

        if (currentIndex < randomizedRooms.Count)
        {
            randomizedRooms[currentIndex].Activate(xrOrigin);

            stopButton.SetActive(true);
            nextRoomButton.SetActive(false);

        }
        else
        {
            Debug.Log("Experiment finished");
        }
    }

    public string GetNextRoomName()
    {
        if (currentIndex + 1 < randomizedRooms.Count)
            return randomizedRooms[currentIndex + 1].roomName;

        return "End";
    }

        public void StopRoom()
    {
        RoomController currentRoom;

        if (currentIndex == -1)
            currentRoom = testingRoom;
        else
            currentRoom = randomizedRooms[currentIndex];

        currentRoom.StopRoom();

        stopButton.SetActive(false);
        nextRoomButton.SetActive(true);
    }



}


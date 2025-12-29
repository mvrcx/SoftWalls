using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ExperimentManager : MonoBehaviour
{
    public Transform xrOrigin;

    public RoomController testingRoom;
    public List<RoomController> experimentalRooms;

    private List<RoomController> randomizedRooms;
    private int currentIndex = -1;

    public GameObject stopButton;
    public GameObject nextRoomButton;


    public Canvas endMessageCanvas;
    public TextMeshProUGUI endMessageText;


    public TextMeshProUGUI roomNameText;

    void Start()
    {

        randomizedRooms = new List<RoomController>(experimentalRooms);
        Shuffle(randomizedRooms);


        EnterTestingRoom();

        stopButton.SetActive(true);
        nextRoomButton.SetActive(false);


        if (endMessageCanvas != null)
            endMessageCanvas.gameObject.SetActive(false);

  
        if (roomNameText != null)
        {
            roomNameText.text = testingRoom.roomName;
            roomNameText.gameObject.SetActive(true);
        }
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
        // Desativa room atual
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


            if (roomNameText != null)
            {
                roomNameText.text = randomizedRooms[currentIndex].roomName;
                roomNameText.gameObject.SetActive(true);
            }

            Debug.Log("Going to room: " + randomizedRooms[currentIndex].roomName);
        }
        else
        {
 
            if (randomizedRooms.Count > 0)
                randomizedRooms[randomizedRooms.Count - 1].Deactivate();


            testingRoom.Activate(xrOrigin);

    
            if (roomNameText != null)
                roomNameText.gameObject.SetActive(false);


            if (endMessageCanvas != null && endMessageText != null)
            {
                endMessageCanvas.gameObject.SetActive(true);
                endMessageText.text = "The end of the experiment! Thank you!";
            }

            stopButton.SetActive(false);
            nextRoomButton.SetActive(false);

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

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
        // Randomiza salas experimentais
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

        // Começa a gravar métricas para a sala de teste
        if (logger != null && !string.IsNullOrEmpty(participantId))
        {
            logger.SetRoom(testingRoom.gameObject, testingRoom.roomName);
            logger.StartRecording(participantId);
        }
    }

    public void GoToNextRoom()
    {
        if (experimentFinished) return;

        // Para a gravação da sala atual
        if (currentIndex >= 0)
            logger?.StopRecordingAndSave();

        // Desativa a sala anterior
        if (currentIndex == -1)
            testingRoom.Deactivate();
        else if (currentIndex < randomizedRooms.Count)
            randomizedRooms[currentIndex].Deactivate();

        currentIndex++;

        // Se houver salas restantes
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
        }
        else
        {
            // Experimento terminado
            experimentFinished = true;

            // Reativa a sala de teste para mostrar fim
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
        }
    }

    // Chamado pelo botão STOP
    public void StopRoom()
    {
        if (experimentFinished || currentIndex < -1 || currentIndex > randomizedRooms.Count) return;

        // Para a gravação do logger atual
        logger?.StopRecordingAndSave();

        stopButton.SetActive(false);
        nextRoomButton.SetActive(true);
    }
}

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class ObjectiveMetricsLogger : MonoBehaviour
{
    [Header("Scene References")]
    public Transform player;
    private CharacterController characterController;

    [Header("Room Info")]
    public GameObject room;
    public string roomName;

    private List<BoxCollider> walls = new List<BoxCollider>();
    private float distanceAtStop;
    private float startTime;
    private float timeUntilStop;
    private bool recording = false;
    private string participantId;

    void Awake()
    {
        characterController = player.GetComponent<CharacterController>();
    }

    public void SetRoom(GameObject newRoom, string newRoomName)
    {
        room = newRoom;
        roomName = newRoomName;
        walls.Clear();
        
        BoxCollider[] allBoxColliders = room.GetComponentsInChildren<BoxCollider>(true);
        
        foreach (BoxCollider c in allBoxColliders)
        {
            if (c.CompareTag("Wall"))
            {
                walls.Add(c);
            }
        }
    }

    public void StartRecording(string id)
    {
        participantId = id;
        distanceAtStop = 0f;
        recording = true;
        startTime = Time.time;
    }

    public void StopRecordingAndSave()
    {
        if (!recording) return;
        
        recording = false;
        timeUntilStop = Time.time - startTime;
        distanceAtStop = CalculateMinDistanceToWalls();
        SaveCSV();
    }

    private float CalculateMinDistanceToWalls()
    {
        if (walls.Count == 0)
        {
            return 0f;
        }

        float minDist = float.MaxValue;
        Vector3 playerCenter = player.position + characterController.center;

        foreach (BoxCollider wall in walls)
        {
            if (wall == null || !wall.gameObject.activeInHierarchy) continue;
            
            Vector3 closestPoint = wall.ClosestPoint(playerCenter);
            float distance = Vector3.Distance(playerCenter, closestPoint);

            if (distance < minDist)
            {
                minDist = distance;
            }
        }

        if (minDist == float.MaxValue)
        {
            return 0f;
        }

        return minDist;
    }

    private void SaveCSV()
    {
        string folderPath = Path.Combine(Application.persistentDataPath, "Metrics");
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string filePath = Path.Combine(folderPath, $"participant_{participantId}.csv");
        bool fileExists = File.Exists(filePath);

        StringBuilder sb = new StringBuilder();
        
        if (!fileExists)
        {
            sb.AppendLine("ParticipantID,Room,DistanceAtStop,TimeUntilStop");
        }

        sb.AppendLine($"{participantId},{roomName},{distanceAtStop:F3},{timeUntilStop:F2}");

        File.AppendAllText(filePath, sb.ToString());
    }
}
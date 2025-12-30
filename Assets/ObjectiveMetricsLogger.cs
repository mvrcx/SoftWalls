using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ObjectiveMetricsLogger : MonoBehaviour
{
    [Header("Scene References")]
    public Transform player;

    [Header("Room Info")]
    public GameObject room; // room atual
    public string roomName;

    [Header("Parameters")]
    public float approachDistance = 1.0f; // distância <1m para primeira abordagem

    private List<Collider> walls = new List<Collider>();
    private float minDistance = float.MaxValue;
    private int collisionCount = 0;
    private float startTime;
    private float timeToFirstApproach = -1f;
    private float timeUntilStop = -1f;

    private bool approached = false;
    private bool recording = false;
    private string participantId;

    // Inicializa a room atual
    public void SetRoom(GameObject newRoom, string newRoomName)
    {
        room = newRoom;
        roomName = newRoomName;

        walls.Clear();
        foreach (Collider c in room.GetComponentsInChildren<Collider>())
            walls.Add(c);
    }

    public void StartRecording(string id)
    {
        participantId = id;

        minDistance = float.MaxValue;
        collisionCount = 0;
        timeToFirstApproach = -1f;
        timeUntilStop = -1f;
        approached = false;
        recording = true;
        startTime = Time.time;
    }

    void Update()
    {
        if (!recording || room == null) return;

        foreach (Collider wall in walls)
        {
            float distance = Vector3.Distance(player.position, wall.ClosestPoint(player.position));
            if (distance < minDistance)
                minDistance = distance;

            if (!approached && distance <= approachDistance)
            {
                approached = true;
                timeToFirstApproach = Time.time - startTime;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (!recording || room == null) return;

        if (walls.Contains(collision.collider))
            collisionCount++;
    }

    public void StopRecordingAndSave()
    {
        if (!recording) return;

        recording = false;
        timeUntilStop = Time.time - startTime;

        SaveCSV();
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
            sb.AppendLine("ParticipantID,Room,MinDistance,Collisions,TimeToFirstApproach,TimeUntilStop");

        sb.AppendLine($"{participantId},{roomName},{minDistance:F3},{collisionCount},{timeToFirstApproach:F2},{timeUntilStop:F2}");

        File.AppendAllText(filePath, sb.ToString());
        Debug.Log("CSV saved: " + filePath);
    }
}

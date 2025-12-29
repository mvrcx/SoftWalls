using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ObjectiveMetricsLogger : MonoBehaviour
{
    [Header("Participant")]
    public string playerId;

    [Header("Scene References")]
    public Transform player;
    public GameObject room;

    [Header("Room Info")]
    public string roomName;
    public string wallType;

    private List<Collider> walls = new List<Collider>();

    private float minDistance = float.MaxValue;
    private int collisionCount = 0;
    private float timeToFirstApproach = -1f;
    private bool approached = false;

    private float startTime;

    void Start()
    {
        startTime = Time.time;

        Collider[] roomColliders = room.GetComponentsInChildren<Collider>();

        foreach (Collider c in roomColliders)
        {
            if (c.CompareTag("Wall"))
                walls.Add(c);
        }
    }

    void Update()
    {
        foreach (Collider wall in walls)
        {
            float distance = Vector3.Distance(
                player.position,
                wall.ClosestPoint(player.position)
            );

            if (distance < minDistance)
                minDistance = distance;

            if (!approached && distance < 1.0f)
            {
                approached = true;
                timeToFirstApproach = Time.time - startTime;
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall") &&
            collision.transform.IsChildOf(room.transform))
        {
            collisionCount++;
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("App paused → saving CSV");
            SaveCSV();
        }
    }

    void OnDestroy()
    {
        Debug.Log("App destroyed → saving CSV");
        SaveCSV();
    }


    void SaveCSV()
    {

    string folderPath =
    Path.Combine(Application.persistentDataPath, "Metrics");
        Debug.Log("SaveCSV() called");
        Debug.Log("Saving to: " + folderPath);
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string fileName =
            $"P{playerId}_{roomName}_{System.DateTime.Now:yyyyMMdd_HHmmss}.csv";

        string filePath = Path.Combine(folderPath, fileName);

        StringBuilder sb = new StringBuilder();
        sb.AppendLine("PlayerID,Room,WallType,MinDistance,Collisions,TimeToFirstApproach");
        sb.AppendLine($"{playerId},{roomName},{wallType},{minDistance:F3},{collisionCount},{timeToFirstApproach:F2}");

        File.WriteAllText(filePath, sb.ToString());

        Debug.Log($"CSV saved at: {filePath}");
    }
}

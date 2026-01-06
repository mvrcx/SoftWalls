using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class ObjectiveMetricsLogger : MonoBehaviour
{
    [Header("Scene References")]
    public Transform player;                 // XR Rig ROOT (not camera)

    [Header("Room Info")]
    public GameObject room;
    public string roomName;

    [Header("Parameters")]
    public float approachDistance = 1.0f;
    public float collisionThreshold = 0.15f; // VR-appropriate

    private List<Collider> walls = new List<Collider>();
    private float minDistance = float.MaxValue;
    private int collisionCount = 0;
    private float startTime;
    private float timeToFirstApproach = -1f;
    private float timeUntilStop = -1f;

    private bool approached = false;
    private bool isCurrentlyColliding = false;
    private bool recording = false;
    private string participantId;

    private Collider playerCollider;

    // Diagnostic Timer
    private float logTimer = 0f;

    void Awake()
    {
        playerCollider = player.GetComponent<Collider>();

        if (playerCollider == null)
        {
            Debug.LogError("PLAYER HAS NO COLLIDER — distance will not work!");
        }
    }

    public void SetRoom(GameObject newRoom, string newRoomName)
    {
        room = newRoom;
        roomName = newRoomName;
        walls.Clear();

        foreach (Collider c in room.GetComponentsInChildren<Collider>(true))
        {
            if (!c.isTrigger)
                walls.Add(c);
        }

        Debug.Log($"Room '{newRoomName}' set. Found {walls.Count} wall colliders.");
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

        Debug.Log($"Recording Started for {id}");
    }

    void Update()
    {
        if (!recording || playerCollider == null || room == null) return;

        float currentClosest = float.MaxValue;
        bool collisionDetectedThisFrame = false;

        foreach (Collider wall in walls)
        {
            if (!wall || !wall.gameObject.activeInHierarchy) continue;

            Vector3 direction;
            float distance;

            bool overlapping = Physics.ComputePenetration(
                playerCollider, player.position, player.rotation,
                wall, wall.transform.position, wall.transform.rotation,
                out direction,
                out distance
            );

            float effectiveDistance = overlapping ? 0f : distance;

            currentClosest = Mathf.Min(currentClosest, effectiveDistance);
            minDistance    = Mathf.Min(minDistance, effectiveDistance);
        }

        // Collision detection
        if (currentClosest <= collisionThreshold)
        {
            collisionDetectedThisFrame = true;
        }

        if (collisionDetectedThisFrame && !isCurrentlyColliding)
        {
            collisionCount++;
            isCurrentlyColliding = true;
            Debug.Log("<color=red><b>VR WALL HIT REGISTERED</b></color>");
        }
        else if (!collisionDetectedThisFrame)
        {
            isCurrentlyColliding = false;
        }

        // Heartbeat log (every 2s)
        logTimer += Time.deltaTime;
        if (logTimer > 2f)
        {
            Debug.Log($"STATUS | Closest: {currentClosest:F2}m | Min: {minDistance:F2}m");
            logTimer = 0;
        }
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
        if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

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

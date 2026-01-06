using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System;

public class CSVDataManager : MonoBehaviour
{
    public static CSVDataManager Instance;

    private string currentRoomName;
    private float roomStartTime;
    private List<string> logEntries = new List<string>();
    private Dictionary<string, int> collisionCounts = new Dictionary<string, int>();

    void Awake() 
    { 
        Instance = this; 
        logEntries.Add("Timestamp,Room,EventType,Details"); // CSV Header
    }

    public void StartRoomTracking(string roomName)
    {
        currentRoomName = roomName;
        roomStartTime = Time.time;
        if (!collisionCounts.ContainsKey(roomName)) 
            collisionCounts[roomName] = 0;
        
        logEntries.Add($"{DateTime.Now:HH:mm:ss},{roomName},Started,0");
    }

    public void RecordCollision()
    {
        if (string.IsNullOrEmpty(currentRoomName))
        {
            Debug.LogError("[CSV Manager] FAILED: RecordCollision called but currentRoomName is NULL!");
            return;
        }

        collisionCounts[currentRoomName]++;
        Debug.Log($"[CSV Manager] SUCCESS: Collision logged for {currentRoomName}. Total hits now: {collisionCounts[currentRoomName]}");

        string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        logEntries.Add($"{timestamp},{currentRoomName},Collision,Hit#{collisionCounts[currentRoomName]}");
    }
    public void EndRoomTracking()
    {
        if (string.IsNullOrEmpty(currentRoomName)) return;

        float timeSpent = Time.time - roomStartTime;
        logEntries.Add($"{DateTime.Now:HH:mm:ss},{currentRoomName},Summary,Time:{timeSpent:F2}s|TotalHits:{collisionCounts[currentRoomName]}");
        
        SaveToCSV(); // Save frequently to prevent data loss
    }

    public void SaveToCSV()
    {
        string filePath = Path.Combine(Application.persistentDataPath, "RoomMetrics.csv");
        File.WriteAllLines(filePath, logEntries);
        Debug.Log("CSV Saved to: " + filePath);
    }
}
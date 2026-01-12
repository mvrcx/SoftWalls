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

    [Header("Parameters")]
    public float collisionCooldown = 0.1f;
    
    [Header("Debug")]
    public bool showDebugInfo = true;

    private List<BoxCollider> walls = new List<BoxCollider>();
    private float currentMinDistance; // distância NESTE frame
    private float distanceAtStop; // distância quando clica Stop
    private int collisionCount;
    private float startTime;
    private float timeUntilStop;
    private bool recording = false;
    private string participantId;

    private HashSet<BoxCollider> recentlyCollidedWalls = new HashSet<BoxCollider>();
    private Dictionary<BoxCollider, float> lastCollisionTime = new Dictionary<BoxCollider, float>();

    void Awake()
    {
        characterController = player.GetComponent<CharacterController>();
        if (characterController == null)
        {
            Debug.LogError("[Metrics] XR Origin MUST have a CharacterController!");
        }
    }

    public void SetRoom(GameObject newRoom, string newRoomName)
    {
        room = newRoom;
        roomName = newRoomName;

        walls.Clear();
        
        Debug.Log($"========================================");
        Debug.Log($"[Metrics] 🔍 Setting room: '{roomName}'");
        Debug.Log($"[Metrics] Room GameObject: {room.name}");
        Debug.Log($"[Metrics] Room active? {room.activeInHierarchy}");
        
        // Primeiro, conta TODOS os BoxColliders (com ou sem tag)
        BoxCollider[] allBoxColliders = room.GetComponentsInChildren<BoxCollider>(true);
        Debug.Log($"[Metrics] Total BoxColliders found in room: {allBoxColliders.Length}");
        
        if (allBoxColliders.Length == 0)
        {
            Debug.LogError($"[Metrics] ❌ CRITICAL: No BoxColliders found in room '{roomName}'!");
            Debug.LogError($"[Metrics] Make sure the room has child objects with BoxCollider components!");
            Debug.Log($"========================================");
            return;
        }
        
        // Agora procura só os que têm a tag "Wall"
        int boxCollidersWithWrongTag = 0;
        foreach (BoxCollider c in allBoxColliders)
        {
            if (c.CompareTag("Wall"))
            {
                walls.Add(c);
                if (showDebugInfo)
                    Debug.Log($"[Metrics]   ✅ Wall found: {c.name} (tag: {c.tag})");
            }
            else
            {
                boxCollidersWithWrongTag++;
                if (showDebugInfo)
                    Debug.Log($"[Metrics]   ⚠️ BoxCollider without 'Wall' tag: {c.name} (tag: '{c.tag}')");
            }
        }

        Debug.Log($"[Metrics] 📊 SUMMARY:");
        Debug.Log($"[Metrics]   • Total BoxColliders: {allBoxColliders.Length}");
        Debug.Log($"[Metrics]   • With 'Wall' tag: {walls.Count}");
        Debug.Log($"[Metrics]   • Without 'Wall' tag: {boxCollidersWithWrongTag}");
        
        if (walls.Count == 0)
        {
            Debug.LogError($"[Metrics] ❌ CRITICAL: No walls with tag 'Wall' found in room '{roomName}'!");
            Debug.LogError($"[Metrics] SOLUTION:");
            Debug.LogError($"[Metrics]   1. Select each wall GameObject in the Hierarchy");
            Debug.LogError($"[Metrics]   2. In the Inspector (top), change Tag to 'Wall'");
            Debug.LogError($"[Metrics]   3. Or use RoomDiagnostics script to auto-fix");
        }
        else
        {
            Debug.Log($"[Metrics] ✅ Room '{roomName}' configured with {walls.Count} walls.");
        }
        
        Debug.Log($"========================================");
    }

    public void StartRecording(string id)
    {
        participantId = id;
        currentMinDistance = float.MaxValue;
        distanceAtStop = 0f;
        collisionCount = 0;
        recentlyCollidedWalls.Clear();
        lastCollisionTime.Clear();
        recording = true;
        startTime = Time.time;
        
        Debug.Log($"[Metrics] ✅ Recording started for participant {id} in room '{roomName}'");
    }

    void Update()
    {
        if (!recording || room == null || walls.Count == 0) return;

        // Reset current min distance cada frame
        currentMinDistance = float.MaxValue;

        // Posição central do CharacterController
        Vector3 playerCenter = player.position + characterController.center;

        // Parâmetros da cápsula do CharacterController
        float halfHeight = characterController.height / 2f;
        float radius = characterController.radius;
        
        Vector3 capsuleBottom = playerCenter - Vector3.up * (halfHeight - radius);
        Vector3 capsuleTop = playerCenter + Vector3.up * (halfHeight - radius);

        foreach (BoxCollider wall in walls)
        {
            if (wall == null || !wall.gameObject.activeInHierarchy) continue;

            // ========================================
            // 1. CÁLCULO DA DISTÂNCIA MÍNIMA
            // ========================================
            Vector3 closestPointOnWall = wall.ClosestPoint(playerCenter);
            float distanceToWall = Vector3.Distance(playerCenter, closestPointOnWall);
            
            // Atualiza a distância mínima ATUAL (este frame)
            if (distanceToWall < currentMinDistance)
            {
                currentMinDistance = distanceToWall;
            }

            // ========================================
            // 2. DETECÇÃO DE COLISÃO - MÉTODO MELHORADO
            // ========================================
            
            // Método 1: Verifica se o bounds intersecta com a cápsula
            Vector3 closestPointOnBounds = wall.bounds.ClosestPoint(playerCenter);
            float distanceToBounds = Vector3.Distance(playerCenter, closestPointOnBounds);
            bool boundsCollision = distanceToBounds < radius;
            
            // Método 2: Verifica se a distância à superfície é menor que o raio
            bool surfaceCollision = distanceToWall < radius;
            
            // Considera colisão se QUALQUER um dos métodos detetar
            bool isColliding = boundsCollision || surfaceCollision;
            
            if (isColliding)
            {
                // Cooldown system: só conta se não colidiu recentemente
                if (!lastCollisionTime.ContainsKey(wall) || 
                    Time.time - lastCollisionTime[wall] > collisionCooldown)
                {
                    collisionCount++;
                    lastCollisionTime[wall] = Time.time;
                    
                    Debug.Log($"[Metrics] 💥 Collision #{collisionCount} with {wall.name} (distance: {distanceToWall:F3}m, bounds dist: {distanceToBounds:F3}m)");
                }
            }
        }
    }

    public void StopRecordingAndSave()
    {
        if (!recording) return;
        
        recording = false;
        timeUntilStop = Time.time - startTime;
        
        // Captura a distância NESTE MOMENTO (quando clicou Stop)
        distanceAtStop = currentMinDistance;
        
        // Se nunca atualizou a distância, põe 0
        if (distanceAtStop == float.MaxValue)
        {
            distanceAtStop = 0f;
            Debug.LogWarning("[Metrics] ⚠️ Distance was never updated! Using 0.");
        }
        
        Debug.Log($"[Metrics] 🛑 Recording stopped.");
        Debug.Log($"[Metrics]    Distance at Stop: {distanceAtStop:F3}m");
        Debug.Log($"[Metrics]    Collisions: {collisionCount}");
        Debug.Log($"[Metrics]    Time: {timeUntilStop:F2}s");
        
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
        {
            sb.AppendLine("ParticipantID,Room,DistanceAtStop,Collisions,TimeUntilStop");
        }

        sb.AppendLine($"{participantId},{roomName},{distanceAtStop:F3},{collisionCount},{timeUntilStop:F2}");

        File.AppendAllText(filePath, sb.ToString());
        
        Debug.Log($"[Metrics] 💾 CSV saved: {filePath}");
    }

    // ========================================
    // DEBUG VISUAL (Opcional)
    // ========================================
    void OnDrawGizmos()
    {
        if (!recording || player == null || characterController == null) return;

        // Desenha a cápsula do CharacterController
        Vector3 playerCenter = player.position + characterController.center;
        float halfHeight = characterController.height / 2f;
        float radius = characterController.radius;

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(playerCenter + Vector3.up * (halfHeight - radius), radius);
        Gizmos.DrawWireSphere(playerCenter - Vector3.up * (halfHeight - radius), radius);

        // Desenha linha para a parede mais próxima
        if (walls != null && walls.Count > 0)
        {
            float closestDist = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;

            foreach (BoxCollider wall in walls)
            {
                if (wall == null) continue;
                Vector3 point = wall.ClosestPoint(playerCenter);
                float dist = Vector3.Distance(playerCenter, point);
                
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closestPoint = point;
                }
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(playerCenter, closestPoint);
            Gizmos.DrawSphere(closestPoint, 0.1f);
        }
    }
}
using UnityEngine;

public class RoomController : MonoBehaviour
{
    public string roomName;
    public Transform spawnPoint;
    public ObjectiveMetricsLogger recorder;

    public void Activate(Transform xrOrigin)
    {
        gameObject.SetActive(true);

        xrOrigin.position = spawnPoint.position;
        xrOrigin.rotation = spawnPoint.rotation;

        if (recorder != null)
            recorder.roomName = roomName;  
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
        
    }

    public void StopRoom()
    {
        Debug.Log($"Room {roomName} stopped");
    }
}

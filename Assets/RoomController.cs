using UnityEngine;

public class RoomController : MonoBehaviour
{
    public string roomName;
    public Transform spawnPoint;

    public void Activate(Transform xrOrigin)
    {
        gameObject.SetActive(true);
        xrOrigin.position = spawnPoint.position;
        xrOrigin.rotation = spawnPoint.rotation;
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
